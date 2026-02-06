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

$sql = "SELECT *  FROM config where id=1";
$res = $db->query($sql);
$row = $res->fetch_assoc();


// بررسی زمان انتخابات
$now = time();
$startTime = isset($row['startDate']) ? strtotime($row['startDate']) : 0;
$endTime = isset($row['EndDate']) ? strtotime($row['EndDate']) : 0;

// اگر تاریخ‌ها تنظیم نشده باشند
if ($startTime === 0 || $endTime === 0) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'زمان‌بندی انتخابات تنظیم نشده است'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// اگر هنوز زمان شروع انتخابات نرسیده
if ($now < $startTime) {
    $remainingSeconds = $startTime - $now;
    $days = floor($remainingSeconds / (60 * 60 * 24));
    $hours = floor(($remainingSeconds % (60 * 60 * 24)) / (60 * 60));
    $minutes = floor(($remainingSeconds % (60 * 60)) / 60);
    
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'زمان انتخابات فرا نرسیده است',
        'remaining_time' => [
            'days' => $days,
            'hours' => $hours,
            'minutes' => $minutes,
            'total_seconds' => $remainingSeconds
        ]
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// اگر زمان انتخابات تمام شده
// if ($now > $endTime) {
//     http_response_code(400);
//     echo json_encode([
//         'status' => false,
//         'message' => 'زمان انتخابات به پایان رسیده است'
//     ], JSON_UNESCAPED_UNICODE);
//     exit;
// }

$sql = "SELECT tracking_code,f.create_date,u.id,u.national_Id,u.first_name,u.last_name,u.persian_birth_date,u.personnel_code,u.gender,u.father_name,u.org_position_desc,u.region_id,ud.user_photo,ua.post_code,ua.address FROM final_submissions as f join users as u on u.national_id=f.nationalId join user_documents as ud on ud.nationalId=f.nationalId left join user_addresses as ua on ua.user_id=u.id where requestStatus='SUPERVISION_APPROVED' ORDER BY create_date DESC;";
$res = $db->query($sql);

$list = [];

while ($row = $res->fetch_assoc()) {

    $row['create_datesh']=jdate('H:i Y-n-j ', strtotime($row['create_date']), '', '', 'en');
    $list[] = $row;
}

/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data'   => $list
], JSON_UNESCAPED_UNICODE);