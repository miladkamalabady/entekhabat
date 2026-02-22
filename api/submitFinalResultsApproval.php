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
$passcode = trim($input['passcode'] ?? '');

if (!in_array($role, ['EXECUTIVE', 'SUPERVISOR'], true)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'نقش تایید نامعتبر است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$expectedCodes = [
    'EXECUTIVE' => 'EXEC-1404',
    'SUPERVISOR' => 'SUP-1404'
];

if ($passcode === '' || $passcode !== $expectedCodes[$role]) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'رمز وارد شده صحیح نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->connect();

$userRes = $db->query("SELECT roles FROM users WHERE national_id = '" . $db->escape($nationalId) . "' LIMIT 1");
$user = $db->fetch_assoc($userRes);
$userRoles = strtoupper($user['roles'] ?? '');

if ($userRoles === '' || strpos($userRoles, $role) === false) {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما مجوز تایید با این نقش را ندارید.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->query("CREATE TABLE IF NOT EXISTS final_results_approvals (
    id INT(11) NOT NULL,
    executive_approved TINYINT(1) NOT NULL DEFAULT 0,
    supervisor_approved TINYINT(1) NOT NULL DEFAULT 0,
    is_active TINYINT(1) NOT NULL DEFAULT 0,
    executive_approved_by VARCHAR(20) DEFAULT NULL,
    executive_approved_at DATETIME DEFAULT NULL,
    supervisor_approved_by VARCHAR(20) DEFAULT NULL,
    supervisor_approved_at DATETIME DEFAULT NULL,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

$db->query("INSERT IGNORE INTO final_results_approvals (id) VALUES (1)");

$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

try {
    $nationalIdEsc = $db->escape($nationalId);

    if ($role === 'EXECUTIVE') {
        $db->query("UPDATE final_results_approvals
                   SET executive_approved = 1,
                       executive_approved_by = '{$nationalIdEsc}',
                       executive_approved_at = NOW()
                   WHERE id = 1");
    }

    if ($role === 'SUPERVISOR') {
        $db->query("UPDATE final_results_approvals
                   SET supervisor_approved = 1,
                       supervisor_approved_by = '{$nationalIdEsc}',
                       supervisor_approved_at = NOW()
                   WHERE id = 1");
    }

    $rowRes = $db->query("SELECT executive_approved, supervisor_approved FROM final_results_approvals WHERE id = 1 FOR UPDATE");
    $row = $db->fetch_assoc($rowRes);

    $executiveApproved = (int)($row['executive_approved'] ?? 0);
    $supervisorApproved = (int)($row['supervisor_approved'] ?? 0);
    $isActive = ($executiveApproved === 1 && $supervisorApproved === 1) ? 1 : 0;

    $db->query("UPDATE final_results_approvals SET is_active = {$isActive} WHERE id = 1");

    $db->query("COMMIT");

    echo json_encode([
        'status' => true,
        'message' => 'تایید با موفقیت ثبت شد.',
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
