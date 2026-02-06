<?php

/* =========================
   JWT Helpers
========================= */
define('JWT_SECRET', 'CHANGE_THIS_SECRET_KEY_!@#');

function base64UrlDecode($data) {
    return base64_decode(strtr($data, '-_', '+/'));
}

function verifyJWT($jwt) {

    $parts = explode('.', $jwt);
    if (count($parts) !== 3) return false;

    [$header, $payload, $signature] = $parts;

    $check = rtrim(strtr(
        base64_encode(
            hash_hmac('sha256', "$header.$payload", JWT_SECRET, true)
        ),
        '+/',
        '-_'
    ), '=');

    if (!hash_equals($check, $signature)) return false;

    $payloadData = json_decode(base64UrlDecode($payload), true);

    if (!$payloadData || $payloadData['exp'] < time()) return false;

    return $payloadData;
}

function getBearerToken() {
    $headers = apache_request_headers();
    if (!isset($headers['Authorization'])) return null;

    if (preg_match('/Bearer\s(\S+)/', $headers['Authorization'], $m)) {
        return $m[1];
    }
    return null;
}

/* =========================
   1. Auth
========================= */
$token = getBearerToken();

if (!$token) {
    http_response_code(401);
    echo json_encode(['status' => false, 'message' => 'Token missing']);
    exit;
}

$jwtData = verifyJWT($token);

if (!$jwtData) {
    http_response_code(401);
    echo json_encode(['status' => false, 'message' => 'Invalid or expired token']);
    exit;
}
/* =========================
   2. Read identity from JWT
========================= */
$nationalId =  $jwtData['national_id'];
if ($nationalId === '') {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'کدملی در توکن یافت نشد.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}