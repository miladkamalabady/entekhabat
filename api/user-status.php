<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   2. Read identity from JWT
========================= */
$nationalId =  $jwtData['national_id'];

/* =========================
   3. Query status
========================= */
$sql = "
    SELECT
        ozvsandogh,
        sabegheO,
        madrak,
        region,
        create_date
    FROM userstatus
    WHERE nationalId = '{$nationalId}'
    ORDER BY create_date DESC
    LIMIT 1
";

$res = $db->query($sql);

if (!$res || !$res->num_rows) {
    http_response_code(404);
    echo json_encode([
        'status' => false,
        'message' => 'وضعیت یافت نشد!'
    ]);
    exit;
}

$row = $res->fetch_assoc();

/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data' => [
        'membershipActive' => (bool)$row['ozvsandogh'],
        'membershipYears'   => (bool)$row['sabegheO'],
        'degree'     => (bool)$row['madrak'],
        'alreadyRegistered'     => (bool)0,
    ]
], JSON_UNESCAPED_UNICODE);
