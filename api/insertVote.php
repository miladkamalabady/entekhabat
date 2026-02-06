<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */
$candidateId = isset($input['usernationalid']) ? trim($input['usernationalid']) : '';
$trackingCode = isset($input['trackingCode']) ? trim($input['trackingCode']) : '';
if ($trackingCode === '' || $candidateId=='') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر tracking_code الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$sql = "SELECT usernationalid  FROM voters WHERE usernationalid = '{$nationalId}'";

$res = $db->query($sql);

if ($res->num_rows) {
    http_response_code(404);
    echo json_encode([
        'status' => false,
        'message' => 'شما قبلا رای داده‌اید!'
    ]);
    exit;
}
$trackingCodeSql = $db->escape($trackingCode);


$db->query("INSERT INTO voters (usernationalid, trackingCode,candidateId)
VALUES ('{$nationalId}', '{$trackingCodeSql}','{$candidateId}')");

$db->query("INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$nationalId}','ثبت رای',' کد {$nationalId} به {$candidateId} رای داد')");
echo json_encode([
    'status' => true,
    'message' => 'ثبت رای با موفقیت انجام شد.',
    'data' => [
        'nationalId' => $nationalId,
        'tracking_code' => $trackingCode
    ]
], JSON_UNESCAPED_UNICODE);