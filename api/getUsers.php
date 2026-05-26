<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';

header('Content-Type: application/json; charset=utf-8');

$db->connect();

$sqlUser = "SELECT roles FROM users WHERE national_id = '{$nationalId}' LIMIT 1";
$resUser = $db->query($sqlUser);
$user = $resUser ? $resUser->fetch_assoc() : null;

if (!$user || $user['roles'] !== 'ADMIN') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 200;
if ($limit <= 0) {
    $limit = 200;
}
if ($limit > 1000) {
    $limit = 1000;
}

$sql = "SELECT 
    u.id,
    u.national_id,
    u.personnel_code,
    u.region_id,
    u.roles,
    u.education,
    u.yearsOfService,
    u.created_at,
    r.name as regionName,
    -- استخراج کد استان (دو رقم اول + 00)
    CONCAT(LEFT(u.region_id, LENGTH(u.region_id) - 2), '00') as province_code,
    -- دریافت نام استان
    p.name as provinceName
FROM users as u 
JOIN region as r ON r.id = u.region_id
LEFT JOIN region as p ON p.id = CONCAT(LEFT(u.region_id, LENGTH(u.region_id) - 2), '00')
ORDER BY u.id DESC 
LIMIT {$limit}";
$res = $db->query($sql);

$list = [];
if ($res) {
    while ($row = $res->fetch_assoc()) {
        
        $row['created_at'] = jdate('H:i Y-n-j', strtotime($row['created_at']), '', '', 'en');
        $list[] = $row;
    }
}

echo json_encode([
    'status' => true,
    'data' => $list
], JSON_UNESCAPED_UNICODE);
