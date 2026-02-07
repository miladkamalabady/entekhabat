<?php
require_once 'database.php';
require_once 'readToken.php';

$db->connect();



/* 2 — آیا قبلا رأی داده؟ */
$check = $db->query("SELECT usernationalid  FROM voters WHERE usernationalid = '{$nationalId}'");

if ($check->num_rows > 0) {
    http_response_code(403);
    echo json_encode([
        "status" => false,
        "message" => "شما قبلا رأی داده‌اید"
    ]);
    exit;
}

/* 3 — ساخت توکن امن */
$rawToken = bin2hex(random_bytes(32));
$tokenHash = hash('sha256', $rawToken);

$expire = date("Y-m-d H:i:s", time() + 120); // 2 دقیقه

$db->query(
    "INSERT INTO voting_tokens (user_id, token_hash, expires_at)
     VALUES ('{$nationalId}','$tokenHash', '$expire')");

echo json_encode([
    "status" => true,
    "vote_token" => $rawToken,
    "expires_in" => 120
]);
