<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode([
        'status' => false,
        'message' => 'فقط درخواست POST مجاز است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
$db->connect();

/* =========================
   2. Read identity from JWT
========================= */
$nationalId =  $jwtData['national_id'];

if ($nationalId === '') {
    $userInfo = $db->query("SELECT national_id FROM users WHERE national_id = {$nationalId} LIMIT 1");
    $userRow = $userInfo->fetch_assoc();
    if ($userRow && !empty($userRow['national_id'])) {
        $nationalId = $db->escape(trim($userRow['national_id']));
    }
}

if ($nationalId === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر  nationalId الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$requiredFiles = [
    'user_photo' => [
        'label' => 'عکس کاربر',
        'allowed' => ['image/jpeg', 'image/png', 'image/jpg']
    ],
    'education_doc' => [
        'label' => 'مدرک تحصیلی',
        'allowed' => ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf']
    ],
    'employment_cert' => [
        'label' => 'گواهی اشتغال',
        'allowed' => ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf']
    ]
];

$maxSize = 1 * 1024 * 1024; // 1MB
$baseUploadDir = __DIR__ . '/uploads/user_documents';

if (!is_dir($baseUploadDir) && !mkdir($baseUploadDir, 0775, true)) {
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ایجاد پوشه آپلود ممکن نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$targetUserKey ='nid_' . preg_replace('/[^0-9]/', '', $nationalId);
$userUploadDir = $baseUploadDir . '/' . $targetUserKey;

if (!is_dir($userUploadDir) && !mkdir($userUploadDir, 0775, true)) {
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ایجاد پوشه کاربر برای آپلود ممکن نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$finfo = new finfo(FILEINFO_MIME_TYPE);
$storedPaths = [];

foreach ($requiredFiles as $field => $meta) {
    if (!isset($_FILES[$field]) || $_FILES[$field]['error'] !== UPLOAD_ERR_OK) {
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => $meta['label'] . ' ارسال نشده یا خطا در آپلود دارد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    if ($_FILES[$field]['size'] > $maxSize) {
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => $meta['label'] . ' نباید بیشتر از 5 مگابایت باشد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $tmpName = $_FILES[$field]['tmp_name'];
    $mimeType = $finfo->file($tmpName);

    if (!in_array($mimeType, $meta['allowed'], true)) {
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'فرمت فایل برای ' . $meta['label'] . ' مجاز نیست.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $originalName = $_FILES[$field]['name'];
    $extension = strtolower(pathinfo($originalName, PATHINFO_EXTENSION));
    if ($extension === '') {
        $extension = $mimeType === 'application/pdf' ? 'pdf' : 'jpg';
    }

    $fileName = $field . '_' . date('Ymd_His') . '_' . bin2hex(random_bytes(4)) . '.' . $extension;
    $serverPath = $userUploadDir . '/' . $fileName;
    $relativePath = 'uploads/user_documents/' . $targetUserKey . '/' . $fileName;

    if (!move_uploaded_file($tmpName, $serverPath)) {
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'ذخیره فایل ' . $meta['label'] . ' روی سرور انجام نشد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $storedPaths[$field] = $relativePath;
}


$nationalIdSql = $db->escape($nationalId);
$userPhotoSql = $db->escape($storedPaths['user_photo']);
$educationDocSql = $db->escape($storedPaths['education_doc']);
$employmentCertSql = $db->escape($storedPaths['employment_cert']);

$db->query("INSERT INTO user_documents ( nationalId, user_photo, education_doc, employment_cert)
VALUES ( '{$nationalIdSql}', '{$userPhotoSql}', '{$educationDocSql}', '{$employmentCertSql}')
ON DUPLICATE KEY UPDATE
user_photo = VALUES(user_photo),
education_doc = VALUES(education_doc),
employment_cert = VALUES(employment_cert),
updated_at = NOW()");

echo json_encode([
    'status' => true,
    'message' => 'فایل‌ها با موفقیت ذخیره شدند.',
    'data' => [
        'nationalId' => $nationalId,
        'user_photo' => $storedPaths['user_photo'],
        'education_doc' => $storedPaths['education_doc'],
        'employment_cert' => $storedPaths['employment_cert']
    ]
], JSON_UNESCAPED_UNICODE);