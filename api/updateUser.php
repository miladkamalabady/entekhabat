<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true) ?: [];
if (!is_array($input)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'داده ارسالی معتبر نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
$db->connect();

$allowedRoles = ['ADMIN', 'SUPERVISOR', 'EXECUTIVE', 'CANDIDATE', 'VOTER'];
$targetNationalId = isset($input['national_id']) ? $db->escape($input['national_id']) : '';
$newRegionId = isset($input['region_id']) ? (int)$input['region_id'] : 0;
$newRole = isset($input['roles']) ? $db->escape($input['roles']) : '';

if ($targetNationalId === '' || $newRegionId <= 0 || !in_array($newRole, $allowedRoles, true)) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'اطلاعات ارسالی معتبر نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

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
$newRegionRes = $db->query("SELECT id, ProvinceCode, Name FROM region WHERE id = {$newRegionId} LIMIT 1");
$newRegion = $newRegionRes ? $newRegionRes->fetch_assoc() : null;
if (!$newRegion) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'منطقه انتخاب شده معتبر نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if ($isProvinceSupervisor) {
    $provinceCode = (int)$user['ProvinceCode'];
    $targetRes = $db->query("SELECT u.national_id, r.ProvinceCode
        FROM users u
        JOIN region r ON r.id = u.region_id
        WHERE u.national_id = '{$targetNationalId}'
        LIMIT 1");
    $target = $targetRes ? $targetRes->fetch_assoc() : null;

    if (!$target || (int)$target['ProvinceCode'] !== $provinceCode || (int)$newRegion['ProvinceCode'] !== $provinceCode) {
        http_response_code(403);
        echo json_encode([
            'status' => false,
            'message' => 'امکان ویرایش کاربران خارج از استان شما وجود ندارد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }
}

$newRegionName = $db->escape($newRegion['Name']);
$sql = "UPDATE users
SET region_id = {$newRegionId}, regionName = '{$newRegionName}', roles = '{$newRole}'
WHERE national_id = '{$targetNationalId}'";
$res = $db->query($sql);


echo json_encode([
    'status' => true,
    'message' => 'ویرایش با موفقیت انجام شد.',
    'data' =>true
], JSON_UNESCAPED_UNICODE);
