<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
$sql = "SELECT *  FROM config";

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
$row['EndDates']=jdate('H:i Y-n-j ', strtotime($row['EndDate']), '', '', 'en');
    $row['startDates']=jdate('H:i Y-n-j', strtotime($row['startDate']), '', '', 'en');
/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data' => $row
    
], JSON_UNESCAPED_UNICODE);
