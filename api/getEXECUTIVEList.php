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
$sql = "SELECT roles, region_id  FROM users where national_id ='$nationalId'";
$res = $db->query($sql);
$row = $res->fetch_assoc();
if ($row['roles'] !== 'EXECUTIVE' && $row['roles'] !== 'SUPERVISOR') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}


$currentUserRole = $row['roles'];
$currentUserRegionId = (int)$row['region_id'];


$sql = "SELECT tracking_code,requestStatus,f.create_date,u.id,u.national_Id,u.first_name,u.last_name,u.persian_birth_date,u.personnel_code,u.gender,u.father_name,u.org_position_desc,u.region_id,u.roles,ud.user_photo,ud.education_doc,ud.employment_cert,ud.soPishine_cert,ud.ravan_cert,ud.document_reviews,ud.updated_at as datepic,ua.post_code,ua.address,f.reson FROM final_submissions as f join users as u on u.national_id=f.nationalId join user_documents as ud on ud.nationalId=f.nationalId left join user_addresses as ua on ua.user_id=u.id WHERE u.region_id = {$currentUserRegionId} ORDER BY create_date DESC;";
$res = $db->query($sql);

$list = [];

while ($row = $res->fetch_assoc()) {

    $row['create_datesh']=jdate('H:i Y-n-j ', strtotime($row['create_date']), '', '', 'en');
    $row['datepic']=jdate('H:i Y-n-j', strtotime($row['datepic']), '', '', 'en');
    $list[] = $row;
}

/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data'   => $list
], JSON_UNESCAPED_UNICODE);