<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

$sqlUser = "SELECT roles FROM users WHERE national_id = '{$nationalId}' LIMIT 1";
$resUser = $db->query($sqlUser);
$user = $resUser ? $resUser->fetch_assoc() : null;

if (!$user || ($user['roles'] !== 'ADMIN' && $user['roles'] !== 'SUPERVISOR')) {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$sql = "update users set region_id={$input['region_id']}, roles='{$input['roles']}'
where national_id='{$input['national_id']}'";
$res = $db->query($sql);


echo json_encode([
    'status' => true,
    'message' => 'ویرایش با موفقیت انجام شد.',
    'data' =>true
], JSON_UNESCAPED_UNICODE);
