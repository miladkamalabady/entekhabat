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
$sql = "SELECT f.id,u.first_name,u.last_name,u.org_position_desc,d.user_photo,COUNT(v.id) as vote_count FROM final_submissions as f JOIN users as u ON u.national_id = f.nationalId LEFT JOIN voters as v ON v.candidateId = f.id LEFT JOIN     user_documents as d ON d.nationalId = f.nationalId where requestStatus='SUPERVISION_APPROVED' GROUP BY f.id, u.first_name, u.last_name ORDER BY vote_count DESC, f.id";
$res1 = $db->query($sql);
$list = [];
while ($rowres1 = $res1->fetch_assoc()) {
    $list[] = $rowres1;
}

$sql = "SELECT * FROM final_submissions ";
$res2 = $db->query($sql);

$row['totalVoters'] = 150000;
$row['totalVotes'] = $res->num_rows;
$row['voterParticipation'] = $res->num_rows / 150000;
$row['activeCandidates'] = $res1->num_rows;
$row['listCan'] = $list;
$row['Candidates'] = $res2->num_rows;

echo json_encode([
    'status' => true,
    'message' => 'دریافت اطلاعات با موفقیت انجام شد.',
    'data' => $row
], JSON_UNESCAPED_UNICODE);
