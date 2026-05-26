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

$userRes = $db->query("SELECT roles,region_id FROM users WHERE national_id = '" . $db->escape($nationalId) . "' LIMIT 1");
$user = $db->fetch_assoc($userRes);
$userRoles = strtoupper($user['roles'] ?? '');


if (!in_array($userRoles, ['EXECUTIVE', 'SUPERVISOR'], true)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'نقش تایید نامعتبر است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

try {
    $nationalIdEsc = $db->escape($nationalId);


    $rowRes = $db->query("SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE region_id = {$user['region_id']} FOR UPDATE");
    $row = $db->fetch_assoc($rowRes);

    $executiveApproved = (int)($row['executive_approved'] ?? 0);
    $supervisorApproved = (int)($row['supervisor_approved'] ?? 0);
    $isActive = ($executiveApproved === 1 && $supervisorApproved === 1) ? 1 : 0;

    $db->query("UPDATE final_results_approvals SET is_active = {$isActive} WHERE region_id = {$user['region_id']} ");

    $db->query("COMMIT");

    echo json_encode([
        'status' => true,
        'message' => ' با موفقیت ثبت شد.',
        'data' => [
            'executiveApproved' => (bool)$executiveApproved,
            'supervisorApproved' => (bool)$supervisorApproved,
            'isActive' => (bool)$isActive
        ]
    ], JSON_UNESCAPED_UNICODE);
} catch (Exception $e) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'خطا در ثبت تایید نهایی.'
    ], JSON_UNESCAPED_UNICODE);
}
