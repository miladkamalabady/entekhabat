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
$trackingCode = isset($input['tracking_code']) ? trim($input['tracking_code']) : '';
if ($trackingCode === '') {
    $db->query("ROLLBACK");
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامتر tracking_code الزامی است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

if(!$nationalId){
    $db->query("ROLLBACK");
    http_response_code(401);
    echo json_encode([
        'status'=>false,
        'message'=>'کاربر احراز هویت نشده است.'
    ],JSON_UNESCAPED_UNICODE);
    exit;
}
/* =========================
   Election time validation
========================= */

$eventSql = "SELECT start_date,end_date 
             FROM election_schedule_events 
             WHERE event_key='candidate_registration' 
             LIMIT 1";

$eventRes = $db->query($eventSql);

if(!$eventRes || !$eventRes->num_rows){
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status'=>false,
        'message'=>'زمان ثبت‌نام انتخابات در سیستم تعریف نشده است.'
    ],JSON_UNESCAPED_UNICODE);
    exit;
}

$event = $eventRes->fetch_assoc();

$start = jalaliScheduleToTimestamp($event['start_date']);
$end   = jalaliScheduleToTimestamp($event['end_date']);
$now   = time();

// هنوز شروع نشده
if($now < $start){
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status'=>false,
        'message'=>'هنوز مهلت ثبت‌نام داوطلبان آغاز نشده است.'
    ],JSON_UNESCAPED_UNICODE);
    exit;
}

// پایان یافته
if($now > $end){
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status'=>false,
        'message'=>'مهلت ثبت‌نام داوطلبان به پایان رسیده است.'
    ],JSON_UNESCAPED_UNICODE);
    exit;
}

$sql = "SELECT nationalId  FROM final_submissions WHERE nationalId = '{$nationalId}'";

$res = $db->query($sql);

if ($res->num_rows) {
    $db->query("ROLLBACK");
    http_response_code(404);
    echo json_encode([
        'status' => false,
        'message' => 'این کاربر قبلا ثبت نام کرده است.!'
    ]);
    exit;
}
$trackingCodeSql = $db->escape($trackingCode);


$db->query("INSERT INTO final_submissions (nationalId, tracking_code,requestStatus)
VALUES ('{$nationalId}', '{$trackingCodeSql}','SUBMITTED')
ON DUPLICATE KEY UPDATE
tracking_code = VALUES(tracking_code),
create_date = NOW()");

$db->query("update users set roles='CANDIDATE' where national_id='{$nationalId}'");

$db->query("INSERT INTO `logs`(`nationalId`, `action`, `description`) VALUES ('{$nationalId}','ثبت کاندید','تغییر کد {$nationalId} ثبت نام کرد')");
$db->query("COMMIT");
echo json_encode([
    'status' => true,
    'message' => 'ثبت نهایی با موفقیت انجام شد.',
    'data' => [
        'nationalId' => $nationalId,
        'tracking_code' => $trackingCode
    ]
], JSON_UNESCAPED_UNICODE);