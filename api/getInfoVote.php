<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */

$sql = "SELECT * FROM voters";
$res = $db->query($sql);
$row['totalVoters']=150000;
$row['totalVotes']=$res->num_rows;
$row['participationRate']=58.4;
$row['voterParticipation']=65.2;
$row['activeCandidates']=8;
echo json_encode([
    'status' => true,
    'message' => 'دریافت اطلاعات با موفقیت انجام شد.',
    'data' => $row
], JSON_UNESCAPED_UNICODE);