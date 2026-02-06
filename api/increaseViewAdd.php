<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */
$id = isset($input['id']) ? trim($input['id']) : '';

$roles =  $jwtData['roles'];
if (!$roles) {
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
$sql = "select views from advertisements  WHERE id = {$id}";
$res = $db->query($sql);
$row = $res->fetch_assoc();
$c=$row['views']+1;
$sql = "update advertisements set views={$c} WHERE id = {$id}";
$res = $db->query($sql);


echo json_encode([
    'status' => true,
    'message' => ' با موفقیت انجام شد.',
    'data' => [
        'id' => $id
    ]
], JSON_UNESCAPED_UNICODE);