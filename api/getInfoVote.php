<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$db->connect();

/* ---------------------------
   تعداد کل رکوردهای رأی
--------------------------- */
$resVotes = $db->query("SELECT COUNT(id) as totalVotes FROM votes");
$totalVotes = $resVotes->fetch_assoc()['totalVotes'];

/* ---------------------------
   تعداد رأی‌دهندگان یکتا
--------------------------- */
$resUnique = $db->query("SELECT COUNT(DISTINCT national_id ) as totalParticipants FROM votes");
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
    d.user_photo,
    COUNT(v.id) as vote_count
FROM final_submissions as f
JOIN users as u ON u.national_id = f.nationalId
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
$res2 = $db->query("SELECT COUNT(id) as totalCandidates FROM final_submissions");
$totalCandidates = $res2->fetch_assoc()['totalCandidates'];

/* ---------------------------
   خروجی
--------------------------- */

$totalEligible = 150000; // واجدین شرایط

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
