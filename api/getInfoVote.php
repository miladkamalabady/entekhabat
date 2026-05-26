<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$db->connect();

$userRes = $db->query("SELECT roles,region_id FROM users WHERE national_id = '" . $db->escape($nationalId) . "' LIMIT 1");
$user = $db->fetch_assoc($userRes);
$userRole = strtoupper(trim($user['roles'] ?? ''));
$isAdmin = strpos($userRole, 'ADMIN') !== false;
$regionFilter = $isAdmin ? '' : " AND u.region_id={$user['region_id']}";
/* ---------------------------
   تعداد کل رکوردهای رأی
--------------------------- */
$resVotes = $db->query("SELECT COUNT(id) as totalVotes FROM votes");
$totalVotes = $resVotes->fetch_assoc()['totalVotes'];

/* ---------------------------
   تعداد رأی‌دهندگان یکتا
--------------------------- */
$resUnique = $db->query("SELECT COUNT(DISTINCT v.national_id ) as totalParticipants FROM votes as v JOIN users as u ON u.national_id = v.national_Id " . ($isAdmin ? "" : " AND u.region_id={$user['region_id']}"));
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
JOIN users as u ON u.national_id = f.nationalId 

LEFT JOIN votes as v ON v.candidate_id = f.id 
LEFT JOIN user_documents as d ON d.nationalId = f.nationalId
WHERE f.requestStatus='SUPERVISION_APPROVED' {$regionFilter}
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
$res2 = $db->query("SELECT COUNT(f.id) as totalCandidates FROM final_submissions as f JOIN users as u ON u.national_id = f.nationalId WHERE 1=1 {$regionFilter}");
$totalCandidates = $res2->fetch_assoc()['totalCandidates'];
/* ---------------------------
   آمار کاندیداها بر اساس منطقه رأی‌دهندگان (برای ادمین)
--------------------------- */
$candidateRegionStats = [];
if ($isAdmin) {
    $sqlCandidateRegion = "
    SELECT
        vu.region_id,
        f.id as candidate_id,
        u.first_name,
        u.last_name,
        COUNT(v.id) as vote_count
    FROM votes as v
    JOIN users as vu ON vu.national_id = v.national_id
    JOIN final_submissions as f ON f.id = v.candidate_id
    JOIN users as u ON u.national_id = f.nationalId
    WHERE f.requestStatus = 'SUPERVISION_APPROVED'
    GROUP BY vu.region_id, f.id, u.first_name, u.last_name
    ORDER BY vu.region_id ASC, vote_count DESC
    ";
    $resRegionStats = $db->query($sqlCandidateRegion);
    while ($row = $resRegionStats->fetch_assoc()) {
        $regionId = (int)$row['region_id'];
        if (!isset($candidateRegionStats[$regionId])) {
            $candidateRegionStats[$regionId] = [];
        }
        $candidateRegionStats[$regionId][] = [
            'candidate_id' => (int)$row['candidate_id'],
            'first_name' => $row['first_name'],
            'last_name' => $row['last_name'],
            'vote_count' => (int)$row['vote_count']
        ];
    }
}
/* ---------------------------
   خروجی
--------------------------- */

$totalEligible = 0;
if ($isAdmin) {
    $result = $db->query("SELECT SUM(totalEligible) as totalEligible FROM final_results_approvals");
    $totalEligible = (int)($result->fetch_assoc()['totalEligible'] ?? 0);
} else {
    $result = $db->query("SELECT totalEligible FROM final_results_approvals WHERE region_id = {$user['region_id']}");
    $totalEligible = (int)($result->fetch_assoc()['totalEligible'] ?? 0);
}
$data = [
    'totalVoters'        => $totalEligible,                 // واجدین
    'totalVotes'         => $totalVotes,               // تعداد رأی (رکورد)
    'participants'       => $totalParticipants,        // 👈 تعداد افراد رأی‌دهنده
    'voterParticipation' => $totalEligible > 0 ? ($totalParticipants / $totalEligible) : 0, // درصد واقعی مشارکت
    'activeCandidates'   => count($list),
    'Candidates'         => $totalCandidates,
    'listCan'            => $list,
    'candidateRegionStats' => $candidateRegionStats
];

echo json_encode([
    'status' => true,
    'message' => 'دریافت اطلاعات با موفقیت انجام شد.',
    'data' => $data
], JSON_UNESCAPED_UNICODE);
