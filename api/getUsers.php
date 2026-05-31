<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';

header('Content-Type: application/json; charset=utf-8');

$db->connect();
$sqlUser = "SELECT u.roles, u.region_id, r.ProvinceCode
FROM users u
LEFT JOIN region r ON r.id = u.region_id
WHERE u.national_id = '" . $db->escape($nationalId) . "'
LIMIT 1";
$resUser = $db->query($sqlUser);
$user = $resUser ? $resUser->fetch_assoc() : null;

$isAdmin = $user && $user['roles'] === 'ADMIN';
$isProvinceSupervisor = $user
    && $user['roles'] === 'SUPERVISOR'
    && substr((string)$user['region_id'], -2) === '00'
    && $user['ProvinceCode'] !== null;

if (!$isAdmin && !$isProvinceSupervisor) {
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
$where = '';
if ($isProvinceSupervisor) {
    $provinceCode = (int)$user['ProvinceCode'];
    $where = "WHERE r.ProvinceCode = {$provinceCode}";
}
$sql = "SELECT 
    u.id,
    u.national_id,
    u.first_name,u.last_name,
    u.personnel_code,
    u.region_id,
    u.roles,
    u.education,
    u.yearsOfService,
    u.created_at,
    r.name as regionName,
    r.ProvinceCode as provinceCode,
    p.Name as provinceName
FROM users as u 
JOIN region as r ON r.id = u.region_id
LEFT JOIN region as p ON p.id = (r.ProvinceCode * 100)
{$where}
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
