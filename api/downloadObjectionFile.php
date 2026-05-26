<?php
require_once 'database.php';
require_once 'readToken.php';

$db->connect();

$fileId = isset($_GET['id']) ? (int)$_GET['id'] : 0;

if ($fileId <= 0) {
    http_response_code(400);
    echo "Invalid file ID";
    exit;
}

// دریافت اطلاعات فایل
$sql = "SELECT d.*, o.national_id 
        FROM objection_documents d
        JOIN objections o ON d.objection_id = o.id
        WHERE d.id = {$fileId}";

$result = $db->query($sql);
$file = $db->fetch_assoc($result);

if (!$file) {
    http_response_code(404);
    echo "File not found";
    exit;
}

// بررسی دسترسی (فقط صاحب اعتراض یا مدیر/ناظر)
$roles = isset($jwtData['roles']) ? $jwtData['roles'] : null;
$isReviewer = is_array($roles)
    ? (in_array('SUPERVISOR', $roles) || in_array('EXECUTIVE', $roles) || in_array('ADMIN', $roles))
    : ($roles === 'SUPERVISOR' || $roles === 'EXECUTIVE' || $roles === 'ADMIN');

if (!$isReviewer && $file['national_id'] !== $nationalId) {
    http_response_code(403);
    echo "Access denied";
    exit;
}

// ارسال فایل
$filePath = __DIR__ . '/' . $file['file_path'];

if (file_exists($filePath)) {
    header('Content-Type: ' . ($file['file_type'] ?: 'application/octet-stream'));
    header('Content-Disposition: inline; filename="' . $file['file_name'] . '"');
    header('Content-Length: ' . filesize($filePath));
    readfile($filePath);
    exit;
} else {
    http_response_code(404);
    echo "File not found on server";
    exit;
}