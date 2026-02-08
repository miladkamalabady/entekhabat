<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

session_start();
$key = 'vote_' . $nationalId;
if (!isset($_SESSION[$key])) $_SESSION[$key] = 0;
$_SESSION[$key]++;
if ($_SESSION[$key] > 10) {
    http_response_code(429);
    echo json_encode(['status' => false, 'message' => 'درخواست بیش از حد']);
    exit;
}

/* دریافت آرایه کاندیداها و توکن */
$candidateIds = $input['candidateIds'] ?? [];
$voteToken    = $input['vote_token'] ?? '';

if (!$candidateIds || !$voteToken) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'پارامتر الزامی است.'], JSON_UNESCAPED_UNICODE);
    exit;
}

/* تولید کد رهگیری */
function generateTrackingCode($length = 6)
{
    $bytes = random_bytes($length);
    return strtoupper(bin2hex($bytes));
}
do {
    $trackingCode = generateTrackingCode();
    $check = $db->query("SELECT id FROM election_participants WHERE tracking_code='$trackingCode' LIMIT 1");
} while ($check->num_rows > 0);

/* بررسی توکن */
$tokenHash = hash('sha256', $voteToken);
$stmt = $db->query("
    SELECT * FROM voting_tokens
    WHERE token_hash='{$tokenHash}' 
    AND user_id='{$nationalId}'
    LIMIT 1
    FOR UPDATE
");
$token = $stmt->fetch_assoc();
if (!$token) {
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode(["status" => false, "message" => "توکن نامعتبر"]);
    exit;
}
if ($token['used']) {
    $db->query("ROLLBACK");
    http_response_code(409);
    echo json_encode(["status" => false, "message" => "شما قبلا رأی خود را ثبت کرده‌اید"]);
    exit;
}
if (strtotime($token['expires_at']) < time()) {
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode(["status" => false, "message" => "زمان رأی‌گیری منقضی شد"]);
    exit;
}

$sql = "SELECT maxVotes  FROM `users` join maxvotes on maxvotes.region_id=users.region_id WHERE national_id = '{$nationalId}'";
$res = $db->query($sql);
if ($res->num_rows > 0) {
    $row = $res->fetch_assoc();
    $votes = intval($row['maxVotes']);
} else $votes = 1;
if ($votes<count($candidateIds)) {
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode(["status" => false, "message" => "تعداد کاندیداهای انتخابی صحیح نمی‌باشد"]);
    exit;
}
/* ثبت شرکت‌کننده در جدول election_participants در صورت نیاز */
$participant = $db->query("
    SELECT id FROM election_participants
    WHERE national_id='{$nationalId}'
    LIMIT 1
");
if (!$participant->num_rows) {
    $db->query("
        INSERT INTO election_participants (national_id, tracking_code)
        VALUES ('{$nationalId}','{$trackingCode}')
    ");
}

/* حلقه روی همه کاندیداها */
foreach ($candidateIds as $candidateId) {
    // جلوگیری از رأی دوباره به همان کاندیدا
    $checkVote = $db->query("
        SELECT id FROM votes
        WHERE national_id='{$nationalId}'
        AND candidate_id='{$candidateId}'
        LIMIT 1
    ");
    if ($checkVote->num_rows) continue;

    $db->query("
        INSERT INTO votes (national_id, candidate_id)
        VALUES ('{$nationalId}','{$candidateId}')
    ");

    $db->query("
        INSERT INTO logs (nationalId, action, description)
        VALUES ('{$nationalId}','ثبت رای',' کد {$nationalId} به {$candidateId} رای داد')
    ");
}

/* مصرف توکن */
$db->query("
    UPDATE voting_tokens
    SET used=1, used_at=NOW()
    WHERE id={$token['id']}
");

/* کامیت تراکنش */
$db->query("COMMIT");

/* خروجی */
echo json_encode([
    'status' => true,
    'message' => 'ثبت رای با موفقیت انجام شد.',
    'data' => ['tracking_code' => $trackingCode]
], JSON_UNESCAPED_UNICODE);
