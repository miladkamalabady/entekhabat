<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$db->connect();

$userRes = $db->query("SELECT roles,region_id FROM users WHERE national_id = '" . $db->escape($nationalId) . "' LIMIT 1");
$user = $db->fetch_assoc($userRes);
/* ---------------------------
   تعداد کل رکوردهای رأی
--------------------------- */
$resVotes = $db->query("SELECT COUNT(id) as totalVotes FROM votes");
$totalVotes = $resVotes->fetch_assoc()['totalVotes'];

/* ---------------------------
   تعداد رأی‌دهندگان یکتا
--------------------------- */
$resUnique = $db->query("SELECT COUNT(DISTINCT v.national_id ) as totalParticipants FROM votes as v JOIN users as u ON u.national_id = v.national_Id and u.region_id={$user['region_id']}");
$totalParticipants = $resUnique->fetch_assoc()['totalParticipants'];

/* ---------------------------
   لیست کاندیداها + تعداد رأی
--------------------------- */
$sql = "
SELECT 
    f.id,
    f.id as candidate_id,
    u.first_name,
    u.last_name,
    u.org_position_desc,
    u.region_id,
    d.user_photo,
    COUNT(v.id) as vote_count
FROM final_submissions as f
JOIN users as u ON u.national_id = f.nationalId and u.region_id={$user['region_id']}

LEFT JOIN votes as v ON v.candidate_id = f.id 
LEFT JOIN user_documents as d ON d.nationalId = f.nationalId
WHERE requestStatus='SUPERVISION_APPROVED'
GROUP BY f.id
ORDER BY vote_count DESC, f.id
";
$res1 = $db->query($sql);

$list = [];
while ($rowres1 = $res1->fetch_assoc()) {
    $list[] = $rowres1;
}

/* ---------------------------
   تعداد کل کاندیداها
--------------------------- */
$res2 = $db->query("SELECT COUNT(f.id) as totalCandidates FROM final_submissions as f JOIN users as u ON u.national_id = f.nationalId and u.region_id={$user['region_id']}");
$totalCandidates = $res2->fetch_assoc()['totalCandidates'];

/* ---------------------------
   خروجی
--------------------------- */

$result = $db->query("SELECT totalEligible FROM final_results_approvals WHERE region_id = {$user['region_id']}");
$totalEligible =  $result->fetch_assoc()['totalEligible']; // واجدین شرایط

$data = [
    'totalVoters'        => $totalEligible,                 // واجدین
    'totalVotes'         => $totalVotes,               // تعداد رأی (رکورد)
    'participants'       => $totalParticipants,        // 👈 تعداد افراد رأی‌دهنده
    'voterParticipation' => $totalParticipants / $totalEligible, // درصد واقعی مشارکت
    'activeCandidates'   => count($list),
    'Candidates'         => $totalCandidates,
    'listCan'            => $list
];

echo json_encode([
    'status' => true,
    'message' => 'دریافت اطلاعات با موفقیت انجام شد.',
    'data' => $data
], JSON_UNESCAPED_UNICODE);
