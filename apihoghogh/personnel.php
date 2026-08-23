<?php
// Headers
header('Access-Control-Allow-Origin: *');
header('Content-Type: application/json; charset=UTF-8');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With');

// Handle preflight requests
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit();
}

// Include database
require_once '../config/database.php';

// Initialize database
$database = new Database();
$db = $database->getConnection();

// Get request method
$method = $_SERVER['REQUEST_METHOD'];

// Get ID from URL if exists
$id = isset($_GET['id']) ? $_GET['id'] : null;

// Router
switch($method) {
    case 'GET':
        if ($id) {
            getPersonnel($db, $id);
        } else {
            getPersonnelList($db);
        }
        break;
    case 'POST':
        createPersonnel($db);
        break;
    case 'PUT':
        updatePersonnel($db, $id);
        break;
    case 'DELETE':
        deletePersonnel($db, $id);
        break;
    default:
        http_response_code(405);
        echo json_encode(['message' => 'Method not allowed']);
        break;
}

/**
 * دریافت لیست تمام پرسنل
 */
function getPersonnelList($db) {
    try {
        // پارامترهای جستجو و فیلتر
        $lastName = isset($_GET['lastName']) ? $_GET['lastName'] : '';
        $nationalCode = isset($_GET['nationalCode']) ? $_GET['nationalCode'] : '';
        $school = isset($_GET['school']) ? $_GET['school'] : '';
        $gender = isset($_GET['gender']) ? $_GET['gender'] : '';
        $page = isset($_GET['page']) ? (int)$_GET['page'] : 1;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 25;
        $offset = ($page - 1) * $limit;

        // ساخت query پایه
        $query = "SELECT p.*, s.name as school_name 
                  FROM personnel p 
                  LEFT JOIN schools s ON p.school_id = s.id 
                  WHERE 1=1";
        
        $params = [];

        // اعمال فیلترها
        if (!empty($lastName)) {
            $query .= " AND p.last_name LIKE :lastName";
            $params[':lastName'] = "%{$lastName}%";
        }

        if (!empty($nationalCode)) {
            $query .= " AND p.national_code LIKE :nationalCode";
            $params[':nationalCode'] = "%{$nationalCode}%";
        }

        if (!empty($school)) {
            $query .= " AND p.school_id = :school";
            $params[':school'] = $school;
        }

        if (!empty($gender)) {
            $query .= " AND p.gender = :gender";
            $params[':gender'] = $gender;
        }

        // دریافت تعداد کل رکوردها
        $countQuery = str_replace("SELECT p.*, s.name as school_name", "SELECT COUNT(*) as total", $query);
        $countStmt = $db->prepare($countQuery);
        $countStmt->execute($params);
        $totalRecords = $countStmt->fetch()['total'];

        // اضافه کردن مرتب‌سازی و صفحه‌بندی
        $query .= " ORDER BY p.id DESC LIMIT :limit OFFSET :offset";
        $params[':limit'] = $limit;
        $params[':offset'] = $offset;

        $stmt = $db->prepare($query);
        
        // Bind parameters
        foreach ($params as $key => $value) {
            if ($key === ':limit' || $key === ':offset') {
                $stmt->bindValue($key, $value, PDO::PARAM_INT);
            } else {
                $stmt->bindValue($key, $value, PDO::PARAM_STR);
            }
        }
        
        $stmt->execute();
        $personnel = $stmt->fetchAll();

        // فرمت‌دهی داده‌ها
        $formattedData = array_map(function($item) {
            return [
                'id' => (int)$item['id'],
                'detailCode' => $item['detail_code'],
                'nationalCode' => $item['national_code'],
                'firstName' => $item['first_name'],
                'lastName' => $item['last_name'],
                'fatherName' => $item['father_name'],
                'mobile' => $item['mobile'],
                'gender' => $item['gender'],
                'birthCertificateNumber' => $item['birth_certificate_number'],
                'birthDate' => [
                    'day' => $item['birth_date_day'],
                    'month' => $item['birth_date_month'],
                    'year' => $item['birth_date_year']
                ],
                'issueDate' => [
                    'day' => $item['issue_date_day'],
                    'month' => $item['issue_date_month'],
                    'year' => $item['issue_date_year']
                ],
                'taxStatus' => $item['tax_status'],
                'employeeStatus' => $item['employee_status'],
                'insuranceType' => $item['insurance_type'],
                'hasTaxHistory' => (bool)$item['has_tax_history'],
                'showInTaxFile' => (bool)$item['show_in_tax_file'],
                'address' => $item['address'],
                'postalCode' => $item['postal_code'],
                'salaryRowNumber' => $item['salary_row_number'],
                'insuranceNumber' => $item['insurance_number'],
                'bank' => $item['bank'],
                'industry' => $item['industry'],
                'schoolId' => $item['school_id'] ? (int)$item['school_id'] : '',
                'schoolName' => $item['school_name'],
                'schoolStaff' => $item['school_staff'],
                'connection' => $item['connection_info'],
                'createdAt' => $item['created_at'],
                'updatedAt' => $item['updated_at']
            ];
        }, $personnel);

        http_response_code(200);
        echo json_encode([
            'success' => true,
            'data' => $formattedData,
            'pagination' => [
                'total' => (int)$totalRecords,
                'page' => $page,
                'limit' => $limit,
                'totalPages' => ceil($totalRecords / $limit)
            ]
        ], JSON_UNESCAPED_UNICODE);

    } catch(PDOException $e) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'message' => 'خطا در دریافت اطلاعات: ' . $e->getMessage()
        ], JSON_UNESCAPED_UNICODE);
    }
}

/**
 * دریافت یک پرسنل با ID
 */
function getPersonnel($db, $id) {
    try {
        $query = "SELECT p.*, s.name as school_name 
                  FROM personnel p 
                  LEFT JOIN schools s ON p.school_id = s.id 
                  WHERE p.id = :id";
        
        $stmt = $db->prepare($query);
        $stmt->bindParam(':id', $id, PDO::PARAM_INT);
        $stmt->execute();
        
        $item = $stmt->fetch();

        if ($item) {
            $formattedData = [
                'id' => (int)$item['id'],
                'detailCode' => $item['detail_code'],
                'nationalCode' => $item['national_code'],
                'firstName' => $item['first_name'],
                'lastName' => $item['last_name'],
                'fatherName' => $item['father_name'],
                'mobile' => $item['mobile'],
                'gender' => $item['gender'],
                'birthCertificateNumber' => $item['birth_certificate_number'],
                'birthDate' => [
                    'day' => $item['birth_date_day'],
                    'month' => $item['birth_date_month'],
                    'year' => $item['birth_date_year']
                ],
                'issueDate' => [
                    'day' => $item['issue_date_day'],
                    'month' => $item['issue_date_month'],
                    'year' => $item['issue_date_year']
                ],
                'taxStatus' => $item['tax_status'],
                'employeeStatus' => $item['employee_status'],
                'insuranceType' => $item['insurance_type'],
                'hasTaxHistory' => (bool)$item['has_tax_history'],
                'showInTaxFile' => (bool)$item['show_in_tax_file'],
                'address' => $item['address'],
                'postalCode' => $item['postal_code'],
                'salaryRowNumber' => $item['salary_row_number'],
                'insuranceNumber' => $item['insurance_number'],
                'bank' => $item['bank'],
                'industry' => $item['industry'],
                'schoolId' => $item['school_id'] ? (int)$item['school_id'] : '',
                'schoolName' => $item['school_name'],
                'schoolStaff' => $item['school_staff'],
                'connection' => $item['connection_info']
            ];

            http_response_code(200);
            echo json_encode([
                'success' => true,
                'data' => $formattedData
            ], JSON_UNESCAPED_UNICODE);
        } else {
            http_response_code(404);
            echo json_encode([
                'success' => false,
                'message' => 'پرسنل مورد نظر یافت نشد'
            ], JSON_UNESCAPED_UNICODE);
        }

    } catch(PDOException $e) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'message' => 'خطا در دریافت اطلاعات: ' . $e->getMessage()
        ], JSON_UNESCAPED_UNICODE);
    }
}

/**
 * ایجاد پرسنل جدید
 */
function createPersonnel($db) {
    try {
        $data = json_decode(file_get_contents("php://input"), true);

        // اعتبارسنجی
        if (empty($data['nationalCode']) || empty($data['firstName']) || empty($data['lastName'])) {
            http_response_code(400);
            echo json_encode([
                'success' => false,
                'message' => 'کد ملی، نام و نام خانوادگی الزامی است'
            ], JSON_UNESCAPED_UNICODE);
            return;
        }

        // بررسی تکراری بودن کد ملی
        $checkQuery = "SELECT id FROM personnel WHERE national_code = :nationalCode";
        $checkStmt = $db->prepare($checkQuery);
        $checkStmt->bindParam(':nationalCode', $data['nationalCode']);
        $checkStmt->execute();
        
        if ($checkStmt->fetch()) {
            http_response_code(409);
            echo json_encode([
                'success' => false,
                'message' => 'کد ملی تکراری است'
            ], JSON_UNESCAPED_UNICODE);
            return;
        }

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

        // Bind parameters
        $stmt->bindParam(':detailCode', $data['detailCode']);
        $stmt->bindParam(':nationalCode', $data['nationalCode']);
        $stmt->bindParam(':firstName', $data['firstName']);
        $stmt->bindParam(':lastName', $data['lastName']);
        $stmt->bindParam(':fatherName', $data['fatherName']);
        $stmt->bindParam(':mobile', $data['mobile']);
        $stmt->bindParam(':gender', $data['gender']);
        $stmt->bindParam(':birthCertificateNumber', $data['birthCertificateNumber']);
        $stmt->bindParam(':birthDateDay', $data['birthDate']['day']);
        $stmt->bindParam(':birthDateMonth', $data['birthDate']['month']);
        $stmt->bindParam(':birthDateYear', $data['birthDate']['year']);
        $stmt->bindParam(':issueDateDay', $data['issueDate']['day']);
        $stmt->bindParam(':issueDateMonth', $data['issueDate']['month']);
        $stmt->bindParam(':issueDateYear', $data['issueDate']['year']);
        $stmt->bindParam(':taxStatus', $data['taxStatus']);
        $stmt->bindParam(':employeeStatus', $data['employeeStatus']);
        $stmt->bindParam(':insuranceType', $data['insuranceType']);
        $hasTaxHistory = $data['hasTaxHistory'] ? 1 : 0;
        $stmt->bindParam(':hasTaxHistory', $hasTaxHistory, PDO::PARAM_INT);
        $showInTaxFile = $data['showInTaxFile'] ? 1 : 0;
        $stmt->bindParam(':showInTaxFile', $showInTaxFile, PDO::PARAM_INT);
        $stmt->bindParam(':address', $data['address']);
        $stmt->bindParam(':postalCode', $data['postalCode']);
        $stmt->bindParam(':salaryRowNumber', $data['salaryRowNumber']);
        $stmt->bindParam(':insuranceNumber', $data['insuranceNumber']);
        $stmt->bindParam(':bank', $data['bank']);
        $stmt->bindParam(':industry', $data['industry']);
        $schoolId = !empty($data['schoolId']) ? $data['schoolId'] : null;
        $stmt->bindParam(':schoolId', $schoolId, PDO::PARAM_INT);
        $stmt->bindParam(':schoolStaff', $data['schoolStaff']);
        $stmt->bindParam(':connection', $data['connection']);

        if ($stmt->execute()) {
            $newId = $db->lastInsertId();
            
            http_response_code(201);
            echo json_encode([
                'success' => true,
                'message' => 'پرسنل جدید با موفقیت ثبت شد',
                'id' => (int)$newId
            ], JSON_UNESCAPED_UNICODE);
        }

    } catch(PDOException $e) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'message' => 'خطا در ثبت اطلاعات: ' . $e->getMessage()
        ], JSON_UNESCAPED_UNICODE);
    }
}

/**
 * بروزرسانی اطلاعات پرسنل
 */
function updatePersonnel($db, $id) {
    try {
        if (!$id) {
            http_response_code(400);
            echo json_encode([
                'success' => false,
                'message' => 'شناسه پرسنل الزامی است'
            ], JSON_UNESCAPED_UNICODE);
            return;
        }

        $data = json_decode(file_get_contents("php://input"), true);

        // بررسی وجود پرسنل
        $checkQuery = "SELECT id FROM personnel WHERE id = :id";
        $checkStmt = $db->prepare($checkQuery);
        $checkStmt->bindParam(':id', $id, PDO::PARAM_INT);
        $checkStmt->execute();
        
        if (!$checkStmt->fetch()) {
            http_response_code(404);
            echo json_encode([
                'success' => false,
                'message' => 'پرسنل مورد نظر یافت نشد'
            ], JSON_UNESCAPED_UNICODE);
            return;
        }

        // بررسی تکراری بودن کد ملی
        if (!empty($data['nationalCode'])) {
            $dupQuery = "SELECT id FROM personnel WHERE national_code = :nationalCode AND id != :id";
            $dupStmt = $db->prepare($dupQuery);
            $dupStmt->bindParam(':nationalCode', $data['nationalCode']);
            $dupStmt->bindParam(':id', $id, PDO::PARAM_INT);
            $dupStmt->execute();
            
            if ($dupStmt->fetch()) {
                http_response_code(409);
                echo json_encode([
                    'success' => false,
                    'message' => 'کد ملی تکراری است'
                ], JSON_UNESCAPED_UNICODE);
                return;
            }
        }

        $query = "UPDATE personnel SET
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
            connection_info = :connection
            WHERE id = :id";

        $stmt = $db->prepare($query);

        // Bind parameters
        $stmt->bindParam(':detailCode', $data['detailCode']);
        $stmt->bindParam(':nationalCode', $data['nationalCode']);
        $stmt->bindParam(':firstName', $data['firstName']);
        $stmt->bindParam(':lastName', $data['lastName']);
        $stmt->bindParam(':fatherName', $data['fatherName']);
        $stmt->bindParam(':mobile', $data['mobile']);
        $stmt->bindParam(':gender', $data['gender']);
        $stmt->bindParam(':birthCertificateNumber', $data['birthCertificateNumber']);
        $stmt->bindParam(':birthDateDay', $data['birthDate']['day']);
        $stmt->bindParam(':birthDateMonth', $data['birthDate']['month']);
        $stmt->bindParam(':birthDateYear', $data['birthDate']['year']);
        $stmt->bindParam(':issueDateDay', $data['issueDate']['day']);
        $stmt->bindParam(':issueDateMonth', $data['issueDate']['month']);
        $stmt->bindParam(':issueDateYear', $data['issueDate']['year']);
        $stmt->bindParam(':taxStatus', $data['taxStatus']);
        $stmt->bindParam(':employeeStatus', $data['employeeStatus']);
        $stmt->bindParam(':insuranceType', $data['insuranceType']);
        $hasTaxHistory = $data['hasTaxHistory'] ? 1 : 0;
        $stmt->bindParam(':hasTaxHistory', $hasTaxHistory, PDO::PARAM_INT);
        $showInTaxFile = $data['showInTaxFile'] ? 1 : 0;
        $stmt->bindParam(':showInTaxFile', $showInTaxFile, PDO::PARAM_INT);
        $stmt->bindParam(':address', $data['address']);
        $stmt->bindParam(':postalCode', $data['postalCode']);
        $stmt->bindParam(':salaryRowNumber', $data['salaryRowNumber']);
        $stmt->bindParam(':insuranceNumber', $data['insuranceNumber']);
        $stmt->bindParam(':bank', $data['bank']);
        $stmt->bindParam(':industry', $data['industry']);
        $schoolId = !empty($data['schoolId']) ? $data['schoolId'] : null;
        $stmt->bindParam(':schoolId', $schoolId, PDO::PARAM_INT);
        $stmt->bindParam(':schoolStaff', $data['schoolStaff']);
        $stmt->bindParam(':connection', $data['connection']);
        $stmt->bindParam(':id', $id, PDO::PARAM_INT);

        if ($stmt->execute()) {
            http_response_code(200);
            echo json_encode([
                'success' => true,
                'message' => 'اطلاعات پرسنل با موفقیت بروزرسانی شد'
            ], JSON_UNESCAPED_UNICODE);
        }

    } catch(PDOException $e) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'message' => 'خطا در بروزرسانی اطلاعات: ' . $e->getMessage()
        ], JSON_UNESCAPED_UNICODE);
    }
}

/**
 * حذف پرسنل
 */
function deletePersonnel($db, $id) {
    try {
        if (!$id) {
            http_response_code(400);
            echo json_encode([
                'success' => false,
                'message' => 'شناسه پرسنل الزامی است'
            ], JSON_UNESCAPED_UNICODE);
            return;
        }

        // بررسی وجود پرسنل
        $checkQuery = "SELECT id, first_name, last_name FROM personnel WHERE id = :id";
        $checkStmt = $db->prepare($checkQuery);
        $checkStmt->bindParam(':id', $id, PDO::PARAM_INT);
        $checkStmt->execute();
        
        $personnel = $checkStmt->fetch();
        if (!$personnel) {
            http_response_code(404);
            echo json_encode([
                'success' => false,
                'message' => 'پرسنل مورد نظر یافت نشد'
            ], JSON_UNESCAPED_UNICODE);
            return;
        }

        $query = "DELETE FROM personnel WHERE id = :id";
        $stmt = $db->prepare($query);
        $stmt->bindParam(':id', $id, PDO::PARAM_INT);

        if ($stmt->execute()) {
            http_response_code(200);
            echo json_encode([
                'success' => true,
                'message' => "پرسنل {$personnel['first_name']} {$personnel['last_name']} با موفقیت حذف شد"
            ], JSON_UNESCAPED_UNICODE);
        }

    } catch(PDOException $e) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'message' => 'خطا در حذف پرسنل: ' . $e->getMessage()
        ], JSON_UNESCAPED_UNICODE);
    }
}
?>