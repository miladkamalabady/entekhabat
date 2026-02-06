<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
$trackingCode = isset($_POST['tracking_code']) ? trim($_POST['tracking_code']) : '';
if ($trackingCode === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر tracking_code الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$sql = "SELECT nationalId  FROM final_submissions WHERE nationalId = '{$nationalId}'";

$res = $db->query($sql);

if ($res->num_rows) {
    http_response_code(404);
    echo json_encode([
        'status' => false,
        'message' => 'این کاربر قبلا ثبت نام کرده است.!'
    ]);
    exit;
}
$trackingCodeSql = $db->escape($trackingCode);


$db->query("INSERT INTO final_submissions (nationalId, tracking_code)
VALUES ('{$nationalId}', '{$trackingCodeSql}')
ON DUPLICATE KEY UPDATE
tracking_code = VALUES(tracking_code),
create_date = NOW()");

echo json_encode([
    'status' => true,
    'message' => 'ثبت نهایی با موفقیت انجام شد.',
    'data' => [
        'nationalId' => $nationalId,
        'tracking_code' => $trackingCode
    ]
], JSON_UNESCAPED_UNICODE);