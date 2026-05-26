<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    http_response_code(405);
    echo json_encode([
        'status' => false,
        'message' => 'فقط درخواست GET مجاز است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// تابع ساخت پسورد رندوم ۶ کاراکتری (حروف بزرگ + اعداد)
function generateRandomPassword($region_id) {
    $characters = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    $password = '';
    for ($i = 0; $i < 6; $i++) {
        $password .= $characters[rand(0, strlen($characters) - 1)];
    }
    // ترکیب region_id به صورت دلخواه، مثلاً ابتدا یا انتها یا درهم
    // اینجا یک نمونه: قرار دادن region_id در ابتدا و سپس ۶ کاراکتر رندوم
    return $region_id . $password;
}

$db->connect();

$userRes = $db->query("SELECT roles,region_id FROM users WHERE national_id = '" . $db->escape($nationalId) . "' LIMIT 1");
$user = $db->fetch_assoc($userRes);
$userRoles = strtoupper($user['roles'] ?? '');

$EXECUTIVEPass1 = generateRandomPassword($user['region_id']);
$SUPERVISORPass1 = generateRandomPassword($user['region_id']);

$result = $db->query("SELECT executive_approved, supervisor_approved, is_active FROM final_results_approvals WHERE region_id = {$user['region_id']}");
$row = $result ? $db->fetch_assoc($result) : null;
if(!($row))
$db->query("INSERT IGNORE INTO final_results_approvals (region_id,EXECUTIVEPass,SUPERVISORPass) VALUES ({$user['region_id']},'{$EXECUTIVEPass1}','{$SUPERVISORPass1}')");
// if (in_array($userRoles, ['EXECUTIVE', 'SUPERVISOR'], true)) 
//     $row['is_active']=true;
$data = [
    'executiveApproved' => (bool)($row['executive_approved'] ?? 0),
    'supervisorApproved' => (bool)($row['supervisor_approved'] ?? 0),
    'isActive' => (bool)($row['is_active'] ?? 0)
];

echo json_encode([
    'status' => true,
    'data' => $data
], JSON_UNESCAPED_UNICODE);
