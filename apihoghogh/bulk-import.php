<?php
header('Access-Control-Allow-Origin: *');
header('Content-Type: application/json; charset=UTF-8');
header('Access-Control-Allow-Methods: POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit();
}

require_once '../config/database.php';

$database = new Database();
$db = $database->getConnection();

try {
    $data = json_decode(file_get_contents("php://input"), true);

    if (empty($data['personnel']) || !is_array($data['personnel'])) {
        http_response_code(400);
        echo json_encode([
            'success' => false,
            'message' => 'داده‌ای برای ثبت وجود ندارد'
        ], JSON_UNESCAPED_UNICODE);
        return;
    }

    $db->beginTransaction();
    
    $successCount = 0;
    $errors = [];

    $query = "INSERT INTO personnel SET
        detail_code = :detailCode,
        national_code = :nationalCode,
        first_name = :firstName,
        last_name = :lastName,
        father_name = :fatherName,
        mobile = :mobile,
        gender = :gender,
        birth_certificate_number = :birthCertificateNumber,
        birth_date_day = :birthDateDay,
        birth_date_month = :birthDateMonth,
        birth_date_year = :birthDateYear,
        issue_date_day = :issueDateDay,
        issue_date_month = :issueDateMonth,
        issue_date_year = :issueDateYear,
        tax_status = :taxStatus,
        employee_status = :employeeStatus,
        insurance_type = :insuranceType,
        has_tax_history = :hasTaxHistory,
        show_in_tax_file = :showInTaxFile,
        address = :address,
        postal_code = :postalCode,
        salary_row_number = :salaryRowNumber,
        insurance_number = :insuranceNumber,
        bank = :bank,
        industry = :industry,
        school_id = :schoolId,
        school_staff = :schoolStaff,
        connection_info = :connection";

    $stmt = $db->prepare($query);

    foreach ($data['personnel'] as $index => $person) {
        // اعتبارسنجی
        if (empty($person['nationalCode']) || empty($person['firstName']) || empty($person['lastName'])) {
            $errors[] = "ردیف " . ($index + 1) . ": کد ملی، نام و نام خانوادگی الزامی است";
            continue;
        }

        // بررسی تکراری بودن کد ملی
        $checkQuery = "SELECT id FROM personnel WHERE national_code = :nationalCode";
        $checkStmt = $db->prepare($checkQuery);
        $checkStmt->bindParam(':nationalCode', $person['nationalCode']);
        $checkStmt->execute();
        
        if ($checkStmt->fetch()) {
            $errors[] = "ردیف " . ($index + 1) . ": کد ملی {$person['nationalCode']} تکراری است";
            continue;
        }

        try {
            $stmt->bindParam(':detailCode', $person['detailCode']);
            $stmt->bindParam(':nationalCode', $person['nationalCode']);
            $stmt->bindParam(':firstName', $person['firstName']);
            $stmt->bindParam(':lastName', $person['lastName']);
            $stmt->bindParam(':fatherName', $person['fatherName']);
            $stmt->bindParam(':mobile', $person['mobile']);
            $stmt->bindParam(':gender', $person['gender']);
            $stmt->bindParam(':birthCertificateNumber', $person['birthCertificateNumber']);
            $stmt->bindParam(':birthDateDay', $person['birthDate']['day']);
            $stmt->bindParam(':birthDateMonth', $person['birthDate']['month']);
            $stmt->bindParam(':birthDateYear', $person['birthDate']['year']);
            $stmt->bindParam(':issueDateDay', $person['issueDate']['day']);
            $stmt->bindParam(':issueDateMonth', $person['issueDate']['month']);
            $stmt->bindParam(':issueDateYear', $person['issueDate']['year']);
            $stmt->bindParam(':taxStatus', $person['taxStatus']);
            $stmt->bindParam(':employeeStatus', $person['employeeStatus']);
            $stmt->bindParam(':insuranceType', $person['insuranceType']);
            $hasTaxHistory = $person['hasTaxHistory'] ? 1 : 0;
            $stmt->bindParam(':hasTaxHistory', $hasTaxHistory, PDO::PARAM_INT);
            $showInTaxFile = $person['showInTaxFile'] ? 1 : 0;
            $stmt->bindParam(':showInTaxFile', $showInTaxFile, PDO::PARAM_INT);
            $stmt->bindParam(':address', $person['address']);
            $stmt->bindParam(':postalCode', $person['postalCode']);
            $stmt->bindParam(':salaryRowNumber', $person['salaryRowNumber']);
            $stmt->bindParam(':insuranceNumber', $person['insuranceNumber']);
            $stmt->bindParam(':bank', $person['bank']);
            $stmt->bindParam(':industry', $person['industry']);
            $schoolId = !empty($person['schoolId']) ? $person['schoolId'] : null;
            $stmt->bindParam(':schoolId', $schoolId, PDO::PARAM_INT);
            $stmt->bindParam(':schoolStaff', $person['schoolStaff']);
            $stmt->bindParam(':connection', $person['connection']);
            
            $stmt->execute();
            $successCount++;
        } catch (PDOException $e) {
            $errors[] = "ردیف " . ($index + 1) . ": خطا در ثبت - " . $e->getMessage();
        }
    }

    if ($successCount > 0) {
        $db->commit();
        
        http_response_code(200);
        echo json_encode([
            'success' => true,
            'message' => "{$successCount} رکورد با موفقیت ثبت شد",
            'totalSuccess' => $successCount,
            'totalErrors' => count($errors),
            'errors' => $errors
        ], JSON_UNESCAPED_UNICODE);
    } else {
        $db->rollBack();
        
        http_response_code(400);
        echo json_encode([
            'success' => false,
            'message' => 'هیچ رکوردی ثبت نشد',
            'errors' => $errors
        ], JSON_UNESCAPED_UNICODE);
    }

} catch(PDOException $e) {
    $db->rollBack();
    
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'message' => 'خطا در ثبت گروهی اطلاعات: ' . $e->getMessage()
    ], JSON_UNESCAPED_UNICODE);
}
?>