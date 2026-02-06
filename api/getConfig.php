<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
$sql = "SELECT *  FROM config where id=1";
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
$row['endDates'] = jdate('l j F Y', strtotime($row['EndDate']), '', '', 'en');
$row['endTime'] = jdate('H:i', strtotime($row['EndDate']), '', '', 'en');
$row['startDates'] = jdate('l j F Y', strtotime($row['startDate']), '', '', 'en');
$row['startTime'] = jdate('H:i', strtotime($row['startDate']), '', '', 'en');
/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data' => $row

], JSON_UNESCAPED_UNICODE);
