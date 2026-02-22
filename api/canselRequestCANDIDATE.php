<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");


function jalaliScheduleToTimestamp($dateStr)
{
    list($date,$time) = explode(' ',$dateStr);
    list($jy,$jm,$jd) = explode('-',$date);

    // تبدیل جلالی → میلادی
    list($gy,$gm,$gd) = jalali_to_gregorian($jy,$jm,$jd);

    return strtotime("$gy-$gm-$gd $time");
}
/* =========================
   3. Query status
========================= */

$sql = "SELECT nationalId  FROM final_submissions WHERE nationalId = '{$nationalId}'";

$res = $db->query($sql);
if (!$res->num_rows) {
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}


$eventSql = "SELECT start_date,end_date 
             FROM election_schedule_events 
             WHERE event_key='voting' 
             LIMIT 1";

$eventRes = $db->query($eventSql);

if(!$eventRes || !$eventRes->num_rows){
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status'=>false,
        'message'=>'زمان انتخابات در سیستم تعریف نشده است.'
    ],JSON_UNESCAPED_UNICODE);
    exit;
}

$event = $eventRes->fetch_assoc();
$start = jalaliScheduleToTimestamp($event['start_date']);
$end   = jalaliScheduleToTimestamp($event['end_date']);
$now   = time();
$withdrawDeadline = $start - (48 * 60 * 60);

// هنوز شروع نشده
if ($now > $withdrawDeadline) {
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status'=>false,
        'message'=>'زمان مجاز انصراف، 48 ساعت قبل شروع انتخابات می‌باشد.'
    ],JSON_UNESCAPED_UNICODE);
   exit;
}
$db->query("delete from final_submissions WHERE nationalId  = '{$nationalId}'");
$db->query("update users set roles='VOTER' where national_id='{$nationalId}'");
$sql = "INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$nationalId}','حذف کاندید','حذف کاندید توسط خودش')";
$res = $db->query($sql);
$db->query("COMMIT");
echo json_encode([
    'status' => true,
    'message' => ' با موفقیت انجام شد.'.$nationalId,
], JSON_UNESCAPED_UNICODE);
