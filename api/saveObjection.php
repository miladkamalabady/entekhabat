<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

// ایجاد جدول objections اگر وجود نداشت
$db->query("CREATE TABLE IF NOT EXISTS objections (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    tracking_code VARCHAR(30) NOT NULL,
    national_id VARCHAR(20) NOT NULL,
    decision_type VARCHAR(80) NULL,
    case_number VARCHAR(80) NULL,
    candidate_name VARCHAR(255) NULL,
    candidate_region VARCHAR(255) NULL,
    candidate_position VARCHAR(255) NULL,
    subject VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    reasons TEXT NULL,
    urgency VARCHAR(20) DEFAULT 'normal',
    status VARCHAR(30) DEFAULT 'pending',
    declaration TINYINT(1) DEFAULT 1,
    response_text TEXT NULL,
    response_by VARCHAR(20) NULL,
    response_at DATETIME NULL,
    cancelled_at DATETIME NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_national_id (national_id),
    INDEX idx_status (status),
    UNIQUE KEY uq_tracking_code (tracking_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

// ایجاد جدول برای ذخیره فایل‌های اعتراض
$db->query("CREATE TABLE IF NOT EXISTS objection_documents (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    objection_id BIGINT UNSIGNED NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size INT UNSIGNED DEFAULT 0,
    file_type VARCHAR(100) NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_objection_id (objection_id),
    FOREIGN KEY (objection_id) REFERENCES objections(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

// بررسی نوع محتوا (JSON یا FormData)
$isFormData = strpos($_SERVER['CONTENT_TYPE'] ?? '', 'multipart/form-data') !== false;
$input = [];

if ($isFormData) {
    // دریافت داده‌ها از FormData
    $decisionType = isset($_POST['decisionType']) ? trim($_POST['decisionType']) : '';
    $caseNumber = isset($_POST['caseNumber']) ? trim($_POST['caseNumber']) : '';
    $candidateName = isset($_POST['candidateName']) ? trim($_POST['candidateName']) : '';
    $candidateRegion = isset($_POST['candidateRegion']) ? trim($_POST['candidateRegion']) : '';
    $candidatePosition = isset($_POST['candidatePosition']) ? trim($_POST['candidatePosition']) : '';
    $subject = isset($_POST['subject']) ? trim($_POST['subject']) : '';
    $description = isset($_POST['description']) ? trim($_POST['description']) : '';
    $reasons = isset($_POST['reasons']) ? json_decode($_POST['reasons'], true) : [];
    $urgency = isset($_POST['urgency']) ? trim($_POST['urgency']) : 'normal';
    $declaration = !empty($_POST['declaration']) ? 1 : 0;
} else {
    // دریافت داده‌ها از JSON
    $input = json_decode(file_get_contents('php://input'), true);
    $decisionType = isset($input['decisionType']) ? trim($input['decisionType']) : '';
    $caseNumber = isset($input['caseNumber']) ? trim($input['caseNumber']) : '';
    $candidateName = isset($input['candidateName']) ? trim($input['candidateName']) : '';
    $candidateRegion = isset($input['candidateRegion']) ? trim($input['candidateRegion']) : '';
    $candidatePosition = isset($input['candidatePosition']) ? trim($input['candidatePosition']) : '';
    $subject = isset($input['subject']) ? trim($input['subject']) : '';
    $description = isset($input['description']) ? trim($input['description']) : '';
    $reasons = isset($input['reasons']) && is_array($input['reasons']) ? $input['reasons'] : [];
    $urgency = isset($input['urgency']) ? trim($input['urgency']) : 'normal';
    $declaration = !empty($input['declaration']) ? 1 : 0;
}

// اعتبارسنجی
if ($decisionType === '' || $description === '' || !$declaration) {
    $db->query("ROLLBACK");
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامترهای الزامی ناقص است. (decisionType, description, declaration)'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// تنظیم subject پیش‌فرض در صورت خالی بودن
if ($subject === '') {
    $subject = '-';
}

$safeDecisionType = $db->escape($decisionType);
$safeCaseNumber = $db->escape($caseNumber);
$safeCandidateName = $db->escape($candidateName);
$safeCandidateRegion = $db->escape($candidateRegion);
$safeCandidatePosition = $db->escape($candidatePosition);
$safeSubject = $db->escape($subject);
$safeDescription = $db->escape($description);
$safeReasons = $db->escape(json_encode($reasons, JSON_UNESCAPED_UNICODE));
$safeUrgency = $db->escape($urgency);
$safeNationalId = $db->escape($nationalId);

// ایجاد کد رهگیری یکتا
do {
    $trackingCode = date('YmdHis') . str_pad((string)mt_rand(1, 9999), 4, '0', STR_PAD_LEFT);
    $checkSql = "SELECT id FROM objections WHERE tracking_code = '{$trackingCode}'";
    $checkResult = $db->query($checkSql);
    $exists = $checkResult && $checkResult->num_rows > 0;
} while ($exists);

$safeTrackingCode = $db->escape($trackingCode);

// ذخیره در جدول objections
$sql = "INSERT INTO objections (
    tracking_code, national_id, decision_type, case_number, candidate_name, 
    candidate_region, candidate_position, subject, description, reasons, 
    urgency, status, declaration
) VALUES (
    '{$safeTrackingCode}', '{$safeNationalId}', '{$safeDecisionType}', '{$safeCaseNumber}', 
    '{$safeCandidateName}', '{$safeCandidateRegion}', '{$safeCandidatePosition}',
    '{$safeSubject}', '{$safeDescription}', '{$safeReasons}', 
    '{$safeUrgency}', 'pending', {$declaration}
)";

$res = $db->query($sql);
if (!$res) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ثبت اعتراض ناموفق بود. خطا: ' . $db->error()
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// دریافت آخرین ID درج شده - اصلاح شده
$objectionId = $db->insert_id('objections');  // ارسال نام جدول به تابع

if (!$objectionId) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'دریافت شناسه اعتراض ناموفق بود.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// ========== ذخیره فایل‌های آپلود شده ==========
$uploadedFiles = [];
$uploadDir = __DIR__ . '/uploads/objections/';

// ایجاد پوشه اگر وجود نداشت
if (!is_dir($uploadDir)) {
    mkdir($uploadDir, 0777, true);
}

if ($isFormData && isset($_FILES) && !empty($_FILES)) {
    // پردازش فایل‌های آپلود شده
    foreach ($_FILES as $key => $file) {
        // پشتیبانی از آرایه فایل‌ها (documents[0], documents[1], ...)
        if (is_array($file['name'])) {
            for ($i = 0; $i < count($file['name']); $i++) {
                if ($file['error'][$i] === UPLOAD_ERR_OK) {
                    $fileName = basename($file['name'][$i]);
                    $fileTmp = $file['tmp_name'][$i];
                    $fileSize = $file['size'][$i];
                    $fileType = $file['type'][$i];
                    
                    // نام یکتا برای فایل
                    $uniqueName = time() . '_' . uniqid() . '_' . $fileName;
                    $filePath = $uploadDir . $uniqueName;
                    
                    if (move_uploaded_file($fileTmp, $filePath)) {
                        // ذخیره در دیتابیس
                        $safeFileName = $db->escape($fileName);
                        $safeFilePath = $db->escape('uploads/objections/' . $uniqueName);
                        $safeFileSize = (int)$fileSize;
                        $safeFileType = $db->escape($fileType);
                        
                        $docSql = "INSERT INTO objection_documents 
                            (objection_id, file_name, file_path, file_size, file_type) 
                            VALUES ({$objectionId}, '{$safeFileName}', '{$safeFilePath}', {$safeFileSize}, '{$safeFileType}')";
                        $db->query($docSql);
                        
                        $uploadedFiles[] = [
                            'name' => $fileName,
                            'path' => 'uploads/objections/' . $uniqueName,
                            'size' => $fileSize
                        ];
                    }
                }
            }
        } else {
            // فایل تکی
            if ($file['error'] === UPLOAD_ERR_OK) {
                $fileName = basename($file['name']);
                $fileTmp = $file['tmp_name'];
                $fileSize = $file['size'];
                $fileType = $file['type'];
                
                $uniqueName = time() . '_' . uniqid() . '_' . $fileName;
                $filePath = $uploadDir . $uniqueName;
                
                if (move_uploaded_file($fileTmp, $filePath)) {
                    $safeFileName = $db->escape($fileName);
                    $safeFilePath = $db->escape('uploads/objections/' . $uniqueName);
                    $safeFileSize = (int)$fileSize;
                    $safeFileType = $db->escape($fileType);
                    
                    $docSql = "INSERT INTO objection_documents 
                        (objection_id, file_name, file_path, file_size, file_type) 
                        VALUES ({$objectionId}, '{$safeFileName}', '{$safeFilePath}', {$safeFileSize}, '{$safeFileType}')";
                    $db->query($docSql);
                    
                    $uploadedFiles[] = [
                        'name' => $fileName,
                        'path' => 'uploads/objections/' . $uniqueName,
                        'size' => $fileSize
                    ];
                }
            }
        }
    }
}

// ثبت لاگ
$db->query("INSERT INTO logs (nationalId, action, description) 
    VALUES ('{$safeNationalId}', 'ثبت اعتراض', 'ثبت اعتراض با کد {$safeTrackingCode} با " . count($uploadedFiles) . " فایل ضمیمه')");

$db->query("COMMIT");

// پاسخ موفقیت‌آمیز
echo json_encode([
    'status' => true,
    'message' => 'اعتراض با موفقیت ثبت شد.',
    'data' => [
        'trackingCode' => $trackingCode,
        'objectionId' => $objectionId,
        'filesCount' => count($uploadedFiles),
        'uploadedFiles' => $uploadedFiles
    ]
], JSON_UNESCAPED_UNICODE);