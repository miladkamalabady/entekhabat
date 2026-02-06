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

$input = [];
if (!empty($_POST)) {
    $input = $_POST;
} else {
    $input = json_decode(file_get_contents('php://input'), true) ?: [];
}

$id = isset($input['id']) ? intval($input['id']) : 0;
$title = isset($input['title']) ? trim($input['title']) : '';
$description = isset($input['description']) ? trim($input['description']) : '';
$type = isset($input['type']) ? trim($input['type']) : '';
$status = isset($input['status']) ? trim($input['status']) : '';
$targetLink = isset($input['targetLink']) ? trim($input['targetLink']) : '';
$imagePath = isset($input['imagePath']) ? trim($input['imagePath']) : '';

if ($title === '' || $description === '' || $type === '' || $status === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'تمام فیلدهای ضروری باید ارسال شوند.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$storedImagePath = $imagePath !== '' ? $imagePath : null;

if (isset($_FILES['image']) && $_FILES['image']['error'] !== UPLOAD_ERR_NO_FILE) {
    if ($_FILES['image']['error'] !== UPLOAD_ERR_OK) {
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'خطا در آپلود تصویر.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    if ($_FILES['image']['size'] > 2 * 1024 * 1024) {
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'حجم تصویر نباید بیشتر از 2 مگابایت باشد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $finfo = new finfo(FILEINFO_MIME_TYPE);
    $tmpName = $_FILES['image']['tmp_name'];
    $mimeType = $finfo->file($tmpName);
    $allowed = ['image/jpeg', 'image/png', 'image/jpg'];

    if (!in_array($mimeType, $allowed, true)) {
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'فرمت تصویر مجاز نیست.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $baseUploadDir = __DIR__ . '/uploads/advertisements';
    if (!is_dir($baseUploadDir) && !mkdir($baseUploadDir, 0775, true)) {
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'ایجاد پوشه آپلود ممکن نیست.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $originalName = $_FILES['image']['name'];
    $extension = strtolower(pathinfo($originalName, PATHINFO_EXTENSION));
    if ($extension === '') {
        $extension = $mimeType === 'image/png' ? 'png' : 'jpg';
    }

    $fileName = 'ad_' . date('Ymd_His') . '_' . bin2hex(random_bytes(4)) . '.' . $extension;
    $serverPath = $baseUploadDir . '/' . $fileName;
    $relativePath = 'uploads/advertisements/' . $fileName;

    if (!move_uploaded_file($tmpName, $serverPath)) {
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'ذخیره تصویر روی سرور انجام نشد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $storedImagePath = $relativePath;
}

$titleSql = $db->escape($title);
$descriptionSql = $db->escape($description);
$typeSql = $db->escape($type);
$statusSql = $db->escape($status);
$targetLinkSql = $db->escape($targetLink);
$imageSql = $storedImagePath !== null ? "'" . $db->escape($storedImagePath) . "'" : "NULL";


$db->query("INSERT INTO advertisements (id,nationalId, title, description, type,image, target_link, status)
VALUES ($id,'{$nationalId}', '{$titleSql}', '{$descriptionSql}', '{$typeSql}', {$imageSql}, '{$targetLinkSql}', '{$statusSql}')
ON DUPLICATE KEY UPDATE
title = VALUES(title),
description = VALUES(description),
type = VALUES(type),
image = IF(VALUES(image) IS NULL, image, VALUES(image)),
target_link = VALUES(target_link),
status = VALUES(status)");

$insertId = $id > 0 ? $id : $db->insert_id(null);

echo json_encode([
    'status' => true,
    'message' => 'تبلیغ با موفقیت ذخیره شد.',
    'data' => [
        'id' => $insertId,
        'image' => $storedImagePath
    ]
], JSON_UNESCAPED_UNICODE);