<?php
require_once 'database.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

$sql = "SELECT r.id, r.ProvinceCode, r.Name AS name, COALESCE(mv.maxVotes, 1) AS maxVotes
FROM region r
LEFT JOIN (
    SELECT region_id, MAX(maxVotes) AS maxVotes
    FROM maxvotes
    GROUP BY region_id
) mv ON mv.region_id = r.id
ORDER BY r.ProvinceCode ASC, r.id ASC, r.Name ASC;";
$res = $db->query($sql);

$provincesMap = [];
$areasByProvince = [];
while ($row = $res->fetch_assoc()) {
    $provinceCode = (int)$row['ProvinceCode'];
    $regionId = (string)$row['id'];
    $regionName = $row['name'];

    // Province rows usually end with 00
    if (substr($regionId, -2) === '00') {
        // if (!isset($provincesMap[$provinceCode])) {
            $provincesMap[$provinceCode] = [
                'id' => $provinceCode,
                'name' => $regionName
            ];
        // }
        // continue;
    }

    if (!isset($areasByProvince[$provinceCode])) {
        $areasByProvince[$provinceCode] = [];
    }

    $areasByProvince[$provinceCode][] = [
        'id' => $regionId,
        'name' => $regionName,
                'maxVotes' => (int)$row['maxVotes']
    ];
}
$provinces = array_values($provincesMap);
echo json_encode([
    'status' => true,
    'data' => $provinces,
    'areasByProvince' => $areasByProvince
], JSON_UNESCAPED_UNICODE);
