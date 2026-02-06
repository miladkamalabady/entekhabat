<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */
$id = isset($input['code']) ? trim($input['code']) : '';
$reson = isset($input['reson']) ? trim($input['reson']) : '';

$roles =  $jwtData['roles'];
if ($roles !== 'EXECUTIVE' && $roles !== 'SUPERVISOR' && $roles !== 'CANDIDATE') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
if ($id === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر کد الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
if ($roles == 'CANDIDATE')
    $sql = "select deleter from advertisements WHERE id = '{$id}'";
else
    $sql = "select deleter from advertisements WHERE nationalId='{$nationalId}' and id = '{$id}'";
$res = $db->query($sql);
$row = $res->fetch_assoc();
$c = $roles;
if ($row['deleter'] == $c)
    $c = NULL;
$sql = "update advertisements set deleter='{$c}',reson='{$reson}'  WHERE id = '{$id}'";
$res = $db->query($sql);

$sql = "INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$nationalId}','تغییر وضعیت تبلیغ','تغییر کد {$id} به {$c}')";
$res = $db->query($sql);

echo json_encode([
    'status' => true,
    'message' => ' با موفقیت انجام شد.' . $row['deleter'],
    'data' => [
        'id' => $id,
    ]
], JSON_UNESCAPED_UNICODE);
