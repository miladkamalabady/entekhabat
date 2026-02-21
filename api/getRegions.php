<?php
require_once 'database.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

$sql = "SELECT DISTINCT ProvinceCode, Name AS name 
FROM region 
WHERE id LIKE '%00'
ORDER BY ProvinceCode ASC, Name ASC;";
$res = $db->query($sql);

$regions = [];
while ($row = $res->fetch_assoc()) {
    $regions[] = [
        'id' => (int)$row['ProvinceCode'],
        'name' => $row['name']
    ];
}

echo json_encode([
    'status' => true,
    'data' => $regions
], JSON_UNESCAPED_UNICODE);
