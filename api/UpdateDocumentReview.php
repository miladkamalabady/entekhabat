<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$input = json_decode(file_get_contents('php://input'), true);

$national_Id = isset($input['national_Id']) ? trim($input['national_Id']) : '';
$documentKey = isset($input['documentKey']) ? trim($input['documentKey']) : '';
$reviewStatus = isset($input['reviewStatus']) ? trim($input['reviewStatus']) : '';

$allowedDocumentKeys = ['user_photo', 'education_doc', 'employment_cert', 'soPishine_cert', 'ravan_cert'];
$allowedStatuses = ['approved', 'rejected', 'pending'];
$roles = $jwtData['roles'] ?? '';

if ($roles !== 'SUPERVISOR' && $roles !== 'EXECUTIVE') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if ($national_Id === '' || $documentKey === '' || $reviewStatus === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامترهای کد ملی، نوع مدرک و وضعیت بررسی الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if (!in_array($documentKey, $allowedDocumentKeys, true)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'نوع مدرک نامعتبر است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if (!in_array($reviewStatus, $allowedStatuses, true)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'وضعیت بررسی نامعتبر است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

$ensureColumn = $db->query("ALTER TABLE user_documents ADD COLUMN IF NOT EXISTS document_reviews JSON NULL");
if (!$ensureColumn) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'خطا در آماده‌سازی جدول مدارک کاربران.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$safeNationalId = $db->escape($national_Id);
$safeReviewer = $db->escape($nationalId);
$safeDocumentKey = $db->escape($documentKey);
$safeStatus = $db->escape($reviewStatus);

$sql = "UPDATE user_documents
        SET document_reviews = JSON_SET(
            COALESCE(document_reviews, JSON_OBJECT()),
            '$.{$safeDocumentKey}',
            JSON_OBJECT(
                'status', '{$safeStatus}',
                'reviewed_by', '{$safeReviewer}',
                'reviewed_at', DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s')
            )
        )
        WHERE nationalId = '{$safeNationalId}'";
$res = $db->query($sql);

if (!$res) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ثبت وضعیت بررسی مدرک ناموفق بود.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$sqlLog = "INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$safeReviewer}','بررسی مدرک','بررسی {$safeDocumentKey} برای {$safeNationalId} با وضعیت {$safeStatus}')";
$logRes = $db->query($sqlLog);
if (!$logRes) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ثبت لاگ بررسی مدرک ناموفق بود.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->query("COMMIT");

echo json_encode([
    'status' => true,
    'message' => 'نظر بررسی مدرک ثبت شد.',
    'data' => [
        'nationalId' => $national_Id,
        'documentKey' => $documentKey,
        'reviewStatus' => $reviewStatus,
        'reviewedBy' => $nationalId
    ]
], JSON_UNESCAPED_UNICODE);
