<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
// $sql = "SELECT requestStatus,tracking_code  FROM final_submissions WHERE nationalId = '{$nationalId}'";
$hasEditedAt = $db->query("SHOW COLUMNS FROM final_submissions LIKE 'edited_at'");
$editedAtExpr = ($hasEditedAt && $hasEditedAt->num_rows) ? "edited_at" : "NULL AS edited_at";

$sql = "SELECT requestStatus,tracking_code,reson,{$editedAtExpr} FROM final_submissions WHERE nationalId = '{$nationalId}'";
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
        'requestStatus' => $row['requestStatus'],
        'tracking_code' => $row['tracking_code'],
        'reson' => $row['reson'],
        'edited_at' => $row['edited_at'],
        'edited_at_sh' => $row['edited_at'] ? jdate('H:i Y-n-j', strtotime($row['edited_at']), '', '', 'en') : null
    ]
], JSON_UNESCAPED_UNICODE);
