<?php
require_once 'database.php';
require_once 'jdf.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */

$sql = "SELECT fi.id as codeentekhabati,v.candidate_id,v.created_at,f.tracking_code,u.first_name,u.last_name FROM `votes` as v join `final_submissions` as fi on fi.id=v.candidate_id join `users` as u on fi.nationalid=u.national_id join `election_participants` as f on f.national_id=v.national_id WHERE v.national_id  = '{$nationalId}'";
$res = $db->query($sql);

$votes = [];
if ($res->num_rows > 0) {
    while ($row = $res->fetch_assoc()) {
        $row['date1'] = jdate('l j F Y', strtotime($row['created_at']), '', '', 'en');
        $row['Time1'] = jdate('H:i', strtotime($row['created_at']), '', '', 'en');
        $votes[] = $row;
    }
} else {
    $sql = "SELECT maxVotes  FROM `users` join maxvotes on maxvotes.region_id=users.region_id WHERE national_id = '{$nationalId}'";
    $res = $db->query($sql);
    if ($res->num_rows > 0) {
        $row = $res->fetch_assoc();
        $votes =intval($row['maxVotes']);
    }
    else $votes =1;
}

echo json_encode([
    'status' => true,
    'message' => 'دریافت لیست رای‌ها با موفقیت انجام شد.',
    'data' => $votes
], JSON_UNESCAPED_UNICODE);
