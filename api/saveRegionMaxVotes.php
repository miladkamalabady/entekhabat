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
$regionId = isset($input['region_id']) ? (int)$input['region_id'] : 0;
$maxVotes = isset($input['maxVotes']) ? (int)$input['maxVotes'] : 0;

if ($regionId <= 0 || $maxVotes <= 0 || $maxVotes > 50) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'منطقه یا تعداد رأی مجاز معتبر نیست.'
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
        'message' => 'شما دسترسی لازم را ندارید.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$regionRes = $db->query("SELECT id, ProvinceCode, Name FROM region WHERE id = {$regionId} LIMIT 1");
$region = $regionRes ? $regionRes->fetch_assoc() : null;
if (!$region) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'منطقه انتخاب شده معتبر نیست.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if ($isProvinceSupervisor && (int)$region['ProvinceCode'] !== (int)$user['ProvinceCode']) {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'امکان ویرایش مناطق خارج از استان شما وجود ندارد.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$existing = $db->query("SELECT id FROM maxvotes WHERE region_id = {$regionId} LIMIT 1");
if ($existing && $existing->num_rows > 0) {
    $db->query("UPDATE maxvotes SET maxVotes = {$maxVotes} WHERE region_id = {$regionId}");
} else {
    $db->query("INSERT INTO maxvotes (maxVotes, region_id) VALUES ({$maxVotes}, {$regionId})");
}

$safeRegionName = $db->escape($region['Name']);
$safeNationalId = $db->escape($nationalId);
$db->query("INSERT INTO logs (nationalId, action, description)
VALUES ('{$safeNationalId}', 'تنظیم تعداد رأی منطقه', 'تعداد رأی مجاز منطقه {$safeRegionName} ({$regionId}) به {$maxVotes} تغییر کرد')");

echo json_encode([
    'status' => true,
    'message' => 'تعداد رأی مجاز منطقه با موفقیت ذخیره شد.',
    'data' => [
        'region_id' => $regionId,
        'maxVotes' => $maxVotes
    ]
], JSON_UNESCAPED_UNICODE);
