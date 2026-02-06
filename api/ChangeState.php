<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */
$national_Id = isset($input['national_Id']) ? trim($input['national_Id']) : '';
$requestStatus = isset($input['requestStatus']) ? trim($input['requestStatus']) : '';
$reson = isset($input['reason']) ? trim($input['reason']) : '';

$roles =  $jwtData['roles'];
if ($roles !== 'EXECUTIVE') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
if ($national_Id === '' || $requestStatus==='') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر کد و وضعیت الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$sql = "update final_submissions set requestStatus='{$requestStatus}',reson='{$reson}'  WHERE nationalId = '{$national_Id}'";

$res = $db->query($sql);


echo json_encode([
    'status' => true,
    'message' => ' با موفقیت انجام شد.',
    'data' => [
        'nationalId' => $national_Id,
        'requestStatus' => $requestStatus
    ]
], JSON_UNESCAPED_UNICODE);