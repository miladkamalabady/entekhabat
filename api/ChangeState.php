<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");
/* =========================
   3. Query status
========================= */
$national_Id = isset($input['national_Id']) ? trim($input['national_Id']) : '';
$requestStatus = isset($input['requestStatus']) ? trim($input['requestStatus']) : '';
$reson = isset($input['reason']) ? trim($input['reason']) : '';

$roles =  $jwtData['roles'];
if ($roles !== 'EXECUTIVE' && $roles !== 'SUPERVISOR') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
if ($national_Id === '' || $requestStatus === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر کد و وضعیت الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
$alterFinalSubmissions = $db->query("ALTER TABLE final_submissions ADD COLUMN IF NOT EXISTS `edited_at` DATETIME NULL DEFAULT NULL");
if (!$alterFinalSubmissions) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'خطا در آماده‌سازی جدول ثبت نهایی.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
$safeReason = $db->escape($reson);
$safeRequestStatus = $db->escape($requestStatus);
$safeNationalId = $db->escape($national_Id);

$sql = "update final_submissions set requestStatus='{$safeRequestStatus}',reson='{$safeReason}',edited_at=NOW() WHERE nationalId = '{$safeNationalId}'";
$res = $db->query($sql);
if (!$res) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ثبت وضعیت در جدول درخواست‌ها ناموفق بود.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if ($roles === 'SUPERVISOR' && ($requestStatus === 'SUPERVISION_APPROVED' || $requestStatus === 'SUPERVISION_REJECTED')) {
    $alterUserDocuments = $db->query("ALTER TABLE user_documents
        ADD COLUMN IF NOT EXISTS supervision_status VARCHAR(50) NULL AFTER ravan_cert,
        ADD COLUMN IF NOT EXISTS supervision_reason TEXT NULL AFTER supervision_status,
        ADD COLUMN IF NOT EXISTS supervision_reviewed_by VARCHAR(20) NULL AFTER supervision_reason,
        ADD COLUMN IF NOT EXISTS supervision_reviewed_at DATETIME NULL AFTER supervision_reviewed_by");
    if (!$alterUserDocuments) {
        $db->query("ROLLBACK");
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'خطا در آماده‌سازی جدول مدارک کاربران.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $safeReviewerNationalId = $db->escape($nationalId);
    $sql = "UPDATE user_documents
            SET supervision_status = '{$requestStatus}',
                 supervision_reason = '{$safeReason}',
                supervision_reviewed_by = '{$safeReviewerNationalId}',
                supervision_reviewed_at = NOW()
              WHERE nationalId = '{$safeNationalId}'";
    $res = $db->query($sql);

    if (!$res) {
        $db->query("ROLLBACK");
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'ثبت نتیجه بررسی مدارک در جدول user_documents ناموفق بود.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }
}
$sql = "INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$nationalId}','تغییر وضعیت','تغییر کدملی {$national_Id} به {$requestStatus}')";
$res = $db->query($sql);
if (!$res) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ثبت لاگ عملیات ناموفق بود.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
$db->query("COMMIT");
echo json_encode([
    'status' => true,
    'message' => ' با موفقیت انجام شد.',
    'data' => [
        'nationalId' => $national_Id,
        'requestStatus' => $requestStatus
    ]
], JSON_UNESCAPED_UNICODE);
