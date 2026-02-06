<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
$sql = "SELECT requestStatus  FROM final_submissions WHERE nationalId = '{$nationalId}'";

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
    'data' => $row['requestStatus']
    
], JSON_UNESCAPED_UNICODE);
