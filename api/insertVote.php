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
$voteToken   = $input['vote_token'] ?? null;
if ($trackingCode === '' || $candidateId=='' || !$voteToken) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر tracking_code الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
/* 1 — پیدا کردن توکن */
$tokenHash = hash('sha256', $voteToken);

$stmt = $db->query(
    "SELECT * FROM voting_tokens
     WHERE token_hash = '{$tokenHash}' AND user_id ='{$nationalId}'
     LIMIT 1"
);

$token = $stmt->fetch_assoc();

if (!$token) {
    http_response_code(403);
    echo json_encode(["status"=>false,"message"=>"توکن نامعتبر"]);
    exit;
}
/* 2 — بررسی مصرف شدن */
if ($token['used']) {
    http_response_code(403);
    echo json_encode(["status"=>false,"message"=>"این رأی قبلا ثبت شده"]);
    exit;
}

/* 3 — بررسی انقضا */
if (strtotime($token['expires_at']) < time()) {
    http_response_code(403);
    echo json_encode(["status"=>false,"message"=>"زمان رأی‌گیری شما منقضی شد"]);
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

 // مصرف توکن
    $db->query(
        "UPDATE voting_tokens
         SET used=1, used_at=NOW()
         WHERE id={$token['id']}"
    );
    
$db->query("INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$nationalId}','ثبت رای',' کد {$nationalId} به {$candidateId} رای داد')");
echo json_encode([
    'status' => true,
    'message' => 'ثبت رای با موفقیت انجام شد.',
    'data' => [
        'nationalId' => $nationalId,
        'tracking_code' => $trackingCode
    ]
], JSON_UNESCAPED_UNICODE);