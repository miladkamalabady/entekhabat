<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';

header('Content-Type: application/json; charset=utf-8');

$db->connect();

$sqlUser = "SELECT u.roles, u.region_id, r.ProvinceCode
FROM users u
LEFT JOIN region r ON r.id = u.region_id
WHERE u.national_id = '" . $db->escape($nationalId) . "'
LIMIT 1";
$resUser = $db->query($sqlUser);
$user = $resUser ? $resUser->fetch_assoc() : null;

$userRole = strtoupper(trim($user['roles'] ?? ''));
$isAdmin = strpos($userRole, 'ADMIN') !== false;
$isSupervisor = strpos($userRole, 'SUPERVISOR') !== false;

if (!$isAdmin && !$isSupervisor) {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$q = trim($_GET['q'] ?? '');
$qLength = function_exists('mb_strlen') ? mb_strlen($q, 'UTF-8') : strlen($q);
if ($q === '' || $qLength < 2) {
    echo json_encode([
        'status' => true,
        'data' => [],
        'message' => 'برای جستجو حداقل دو کاراکتر وارد کنید.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 200;
if ($limit <= 0) {
    $limit = 200;
}
if ($limit > 1000) {
    $limit = 1000;
}

$whereParts = [];
$whereParts[] = "(
    vu.national_id LIKE '%" . $db->escape($q) . "%' OR
    vu.personnel_code LIKE '%" . $db->escape($q) . "%' OR
    vu.first_name LIKE '%" . $db->escape($q) . "%' OR
    vu.last_name LIKE '%" . $db->escape($q) . "%' OR
    CONCAT(COALESCE(vu.first_name, ''), ' ', COALESCE(vu.last_name, '')) LIKE '%" . $db->escape($q) . "%'
)";

$scope = 'all';
if (!$isAdmin) {
    $currentRegionId = (int)($user['region_id'] ?? 0);
    $provinceCode = (int)($user['ProvinceCode'] ?? 0);
    if (substr((string)$currentRegionId, -2) === '00' && $provinceCode > 0) {
        $whereParts[] = "vr.ProvinceCode = {$provinceCode}";
        $scope = 'province';
    } else {
        $whereParts[] = "vu.region_id = {$currentRegionId}";
        $scope = 'region';
    }
}

$whereSql = implode(' AND ', $whereParts);

$sql = "SELECT
    vu.id as voter_user_id,
    vu.national_id as voter_national_id,
    vu.first_name as voter_first_name,
    vu.last_name as voter_last_name,
    vu.personnel_code as voter_personnel_code,
    vu.region_id as voter_region_id,
    vr.Name as voter_region_name,
    vr.ProvinceCode as voter_province_code,
    vp.Name as voter_province_name,
    ep.tracking_code,
    ep.created_at as participant_created_at,
    v.id as vote_id,
    v.created_at as voted_at,
    f.id as candidate_submission_id,
    f.tracking_code as candidate_tracking_code,
    cu.national_id as candidate_national_id,
    cu.first_name as candidate_first_name,
    cu.last_name as candidate_last_name,
    cu.org_position_desc as candidate_position,
    cu.region_id as candidate_region_id,
    cr.Name as candidate_region_name,
    cp.Name as candidate_province_name
FROM users vu
LEFT JOIN region vr ON vr.id = vu.region_id
LEFT JOIN region vp ON vp.id = (vr.ProvinceCode * 100)
LEFT JOIN election_participants ep ON ep.national_id = vu.national_id
LEFT JOIN votes v ON v.national_id = vu.national_id
LEFT JOIN final_submissions f ON f.id = v.candidate_id
LEFT JOIN users cu ON cu.national_id = f.nationalId
LEFT JOIN region cr ON cr.id = cu.region_id
LEFT JOIN region cp ON cp.id = (cr.ProvinceCode * 100)
WHERE {$whereSql}
ORDER BY vu.id DESC, v.created_at DESC
LIMIT {$limit}";

$res = $db->query($sql);

$list = [];
if ($res) {
    while ($row = $res->fetch_assoc()) {
        $row['voted_at_shamsi'] = $row['voted_at'] ? jdate('H:i Y-n-j', strtotime($row['voted_at']), '', '', 'en') : null;
        $row['participant_created_at_shamsi'] = $row['participant_created_at'] ? jdate('H:i Y-n-j', strtotime($row['participant_created_at']), '', '', 'en') : null;
        $list[] = $row;
    }
}

$summary = [];
foreach ($list as $row) {
    $voterNationalId = $row['voter_national_id'];
    if (!isset($summary[$voterNationalId])) {
        $summary[$voterNationalId] = [
            'national_id' => $voterNationalId,
            'first_name' => $row['voter_first_name'],
            'last_name' => $row['voter_last_name'],
            'personnel_code' => $row['voter_personnel_code'],
            'region_id' => $row['voter_region_id'],
            'region_name' => $row['voter_region_name'],
            'province_name' => $row['voter_province_name'],
            'tracking_code' => $row['tracking_code'],
            'vote_count' => 0
        ];
    }
    if (!empty($row['vote_id'])) {
        $summary[$voterNationalId]['vote_count']++;
    }
}

$db->query("INSERT INTO logs (nationalId, action, description) VALUES ('" . $db->escape($nationalId) . "','جستجوی آرای کاربر','جستجوی آرای کاربران با عبارت " . $db->escape($q) . "')");

echo json_encode([
    'status' => true,
    'data' => [
        'scope' => $scope,
        'items' => $list,
        'summary' => array_values($summary)
    ],
    'message' => 'جستجوی آرای کاربران با موفقیت انجام شد.'
], JSON_UNESCAPED_UNICODE);
