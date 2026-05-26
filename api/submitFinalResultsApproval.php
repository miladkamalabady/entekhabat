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

$input = json_decode(file_get_contents('php://input'), true) ?: [];
$role = strtoupper(trim($input['role'] ?? ''));
$passcode1 = trim($input['passcode1'] ?? '');
$passcode2 = trim($input['passcode2'] ?? '');

if (!in_array($role, ['EXECUTIVE', 'SUPERVISOR'], true)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'نقش تایید نامعتبر است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->connect();

$userRes = $db->query("SELECT roles,region_id FROM users WHERE national_id = '" . $db->escape($nationalId) . "' LIMIT 1");
$user = $db->fetch_assoc($userRes);
$userRoles = strtoupper($user['roles'] ?? '');

$result = $db->query("SELECT EXECUTIVEPass, SUPERVISORPass FROM final_results_approvals WHERE region_id = {$user['region_id']}");
$row = $result ? $db->fetch_assoc($result) : null;
$expectedCodes = [
    'EXECUTIVE' => $row['EXECUTIVEPass'],
    'SUPERVISOR' => $row['SUPERVISORPass']
];

if ($passcode1 === '' || $passcode2 === '' || $passcode1 !== $expectedCodes['EXECUTIVE'] || $passcode2 !== $expectedCodes['SUPERVISOR']) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'رمز وارد شده صحیح نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}


if ($userRoles === '' || strpos($userRoles, $role) === false) {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما مجوز تایید با این نقش را ندارید.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

//$db->query("INSERT IGNORE INTO final_results_approvals (region_id) VALUES ({$user['region_id']} )");

$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

try {
    $nationalIdEsc = $db->escape($nationalId);

    if ($role === 'EXECUTIVE' || $role === 'SUPERVISOR') {
        $db->query("UPDATE final_results_approvals
                   SET executive_approved = 1,
                   supervisor_approved = 1,
                       executive_approved_by = '{$nationalIdEsc}',
                       executive_approved_at = NOW(),
                       supervisor_approved_by = '{$nationalIdEsc}',
                       supervisor_approved_at = NOW()
                   WHERE region_id = {$user['region_id']} ");
    }

    // if ($role === 'SUPERVISOR') {
    //     $db->query("UPDATE final_results_approvals
    //                SET supervisor_approved = 1,
    //                    supervisor_approved_by = '{$nationalIdEsc}',
    //                    supervisor_approved_at = NOW()
    //                WHERE id = 1");
    // }

    $rowRes = $db->query("SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE region_id = {$user['region_id']} FOR UPDATE");
    $row = $db->fetch_assoc($rowRes);

    $executiveApproved = (int)($row['executive_approved'] ?? 0);
    $supervisorApproved = (int)($row['supervisor_approved'] ?? 0);
    $isActive = ($executiveApproved === 1 && $supervisorApproved === 1) ? 0 : 0;

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
