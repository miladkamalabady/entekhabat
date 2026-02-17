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
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");
/* =========================
   2. Read identity from JWT
========================= */

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
        'message' => 'پارامتر nationalId الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$requiredFiles = [
    'user_photo' => [
        'label' => 'عکس کاربر',
        'allowed' => ['image/jpeg', 'image/png', 'image/jpg']
    ],
    'soPishine_cert' => [
        'label' => 'گواهی سوء پشینیه',
        'allowed' => ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf']
    ],
    'ravan_cert' => [
        'label' => 'گواهی سلامت روانی و جسمی ',
        'allowed' => ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf']
    ]
];

// فایل اختیاری
$optionalFiles = [
    'education_doc' => [
        'label' => 'مدرک تحصیلی',
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

$targetUserKey = 'nid_' . preg_replace('/[^0-9]/', '', $nationalId);
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

// پردازش فایل‌های اجباری
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
            'message' => $meta['label'] . ' نباید بیشتر از 1 مگابایت باشد.'
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

    $fileName = $field . '_' . date('Ymd_Hi') . '_' . bin2hex(random_bytes(4)) . '.' . $extension;
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

// پردازش فایل‌های اختیاری
foreach ($optionalFiles as $field => $meta) {
    if (isset($_FILES[$field]) && $_FILES[$field]['error'] === UPLOAD_ERR_OK) {
        if ($_FILES[$field]['size'] > $maxSize) {
            http_response_code(400);
            echo json_encode([
                'status' => false,
                'message' => $meta['label'] . ' نباید بیشتر از 1 مگابایت باشد.'
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

        $fileName = $field . '_' . date('Ymd_Hi') . '_' . bin2hex(random_bytes(4)) . '.' . $extension;
        $serverPath = $userUploadDir . '/' . $fileName;
        $relativePath = 'uploads/user_documents/' . $targetUserKey . '/' . $fileName;

        if (move_uploaded_file($tmpName, $serverPath)) {
            $storedPaths[$field] = $relativePath;
        }
    }
}

$nationalIdSql = $db->escape($nationalId);
$userPhotoSql = $db->escape($storedPaths['user_photo']);
$educationDocSql = isset($storedPaths['education_doc']) ? $db->escape($storedPaths['education_doc']) : 'NULL';
$soPishineCertSql = $db->escape($storedPaths['soPishine_cert']);
$ravanCertSql = $db->escape($storedPaths['ravan_cert']);

// ساخت کوئری با توجه به اختیاری بودن education_doc
if (isset($storedPaths['education_doc'])) {
    $db->query("INSERT INTO user_documents (nationalId, user_photo, education_doc, employment_cert, soPishine_cert, ravan_cert)
    VALUES ('{$nationalIdSql}', '{$userPhotoSql}', '{$educationDocSql}', '', '{$soPishineCertSql}', '{$ravanCertSql}')
    ON DUPLICATE KEY UPDATE
    user_photo = VALUES(user_photo),
    education_doc = VALUES(education_doc),
    soPishine_cert = VALUES(soPishine_cert),
    ravan_cert = VALUES(ravan_cert),
    updated_at = NOW()");
} else {
    $db->query("INSERT INTO user_documents (nationalId, user_photo, employment_cert, soPishine_cert, ravan_cert)
    VALUES ('{$nationalIdSql}', '{$userPhotoSql}', '', '{$soPishineCertSql}', '{$ravanCertSql}')
    ON DUPLICATE KEY UPDATE
    user_photo = VALUES(user_photo),
    soPishine_cert = VALUES(soPishine_cert),
    ravan_cert = VALUES(ravan_cert),
    updated_at = NOW()");
}

$responseData = [
    'status' => true,
    'message' => 'فایل‌ها با موفقیت ذخیره شدند.',
    'data' => [
        'nationalId' => $nationalId,
        'user_photo' => $storedPaths['user_photo'],
        'soPishine_cert' => $storedPaths['soPishine_cert'],
        'ravan_cert' => $storedPaths['ravan_cert']
    ]
];

// اضافه کردن education_doc به پاسخ در صورت وجود
if (isset($storedPaths['education_doc'])) {
    $responseData['data']['education_doc'] = $storedPaths['education_doc'];
}
/* کامیت تراکنش */
$db->query("COMMIT");
echo json_encode($responseData, JSON_UNESCAPED_UNICODE);