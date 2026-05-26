<?php
require_once 'database.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

$sql = "SELECT id, ProvinceCode, Name AS name
FROM region
ORDER BY ProvinceCode ASC, id ASC, Name ASC;";
$res = $db->query($sql);

$provincesMap = [];
$areasByProvince = [];

while ($row = $res->fetch_assoc()) {
    $provinceCode = (int)$row['ProvinceCode'];
    $regionId = (string)$row['id'];
    $regionName = $row['name'];

    // Province rows usually end with 00
    if (substr($regionId, -2) === '00') {
        if (!isset($provincesMap[$provinceCode])) {
            $provincesMap[$provinceCode] = [
                'id' => $provinceCode,
                'name' => $regionName
            ];
        }
        continue;
    }

    if (!isset($areasByProvince[$provinceCode])) {
        $areasByProvince[$provinceCode] = [];
    }

    $areasByProvince[$provinceCode][] = [
        'id' => $regionId,
        'name' => $regionName
    ];
}

$provinces = array_values($provincesMap);

echo json_encode([
    'status' => true,
    'data' => $provinces,
    'areasByProvince' => $areasByProvince
], JSON_UNESCAPED_UNICODE);
