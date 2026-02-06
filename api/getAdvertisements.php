<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
$roles =  $jwtData['roles'];

// if ($roles !== 'CANDIDATE') {
//     http_response_code(403);
//     echo json_encode([
//         'status' => false,
//         'message' => 'شما دسترسی لازم را ندارید'
//     ], JSON_UNESCAPED_UNICODE);
//     exit;
// }

if ($roles === 'CANDIDATE')
    $sql = "SELECT * FROM advertisements where nationalId='{$nationalId}' ORDER BY create_date DESC;";
else
    $sql = "SELECT ad.*,u.first_name,u.last_name,u.id as code FROM advertisements as ad join users as u on u.national_id=ad.nationalId ORDER BY create_date DESC;";
$res = $db->query($sql);

$list = [];

while ($row = $res->fetch_assoc()) {

    $row['create_atsh'] = jdate('H:i Y-n-j ', strtotime($row['create_date']), '', '', 'en');
    $row['updated_atsh'] = jdate('H:i Y-n-j', strtotime($row['updated_at']), '', '', 'en');
    $list[] = $row;
}

/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data'   => $list
], JSON_UNESCAPED_UNICODE);
