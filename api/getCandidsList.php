<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

function normalizeDigits($value)
{
    return tr_num((string)$value, 'en');
}
function normalizeToGregorianDateTime($value)
{
    if ($value === null) {
        return null;
    }

    $value = trim((string)$value);
    if ($value === '') {
        return null;
    }

    $value = normalizeDigits(str_replace('T', ' ', $value));
    $parts = preg_split('/\s+/', $value);
    $datePart = $parts[0] ?? '';
    $timePart = $parts[1] ?? '00:00:00';

    if (!preg_match('/^(\d{2,4})-(\d{1,2})-(\d{1,2})$/', $datePart, $m)) {
        return null;
    }

    $year = (int)$m[1];
    $month = (int)$m[2];
    $day = (int)$m[3];

    if (!preg_match('/^\d{2}:\d{2}(:\d{2})?$/', $timePart)) {
        $timePart = '00:00:00';
    }
    if (strlen($timePart) === 5) {
        $timePart .= ':00';
    }

    if ($year < 1700) {
        list($gy, $gm, $gd) = jalali_to_gregorian($year, $month, $day);
        return sprintf('%04d-%02d-%02d %s', $gy, $gm, $gd, $timePart);
    }

    return sprintf('%04d-%02d-%02d %s', $year, $month, $day, $timePart);
}

$roles = $jwtData['roles'];
$startDate = null;
$endDate = null;

$tableCheck = $db->query("SHOW TABLES LIKE 'election_schedule_events'");
if ($tableCheck && $db->num_rows($tableCheck) > 0) {
    $scheduleSql = "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1";
    $scheduleRes = $db->query($scheduleSql);
    if ($scheduleRes && $db->num_rows($scheduleRes) > 0) {
        $scheduleRow = $db->fetch_assoc($scheduleRes);
        $startDate = normalizeToGregorianDateTime($scheduleRow['start_date'] ?? null);
        $endDate = normalizeToGregorianDateTime($scheduleRow['end_date'] ?? null);
    }
}

if (empty($startDate) || empty($endDate)) {
    $sql = "SELECT startDate, EndDate FROM config WHERE id=1";
    $res = $db->query($sql);
    if ($res && $db->num_rows($res) > 0) {
        $row = $db->fetch_assoc($res);
        $startDate = normalizeToGregorianDateTime($row['startDate'] ?? null);
        $endDate = normalizeToGregorianDateTime($row['EndDate'] ?? null);
    }
}

// بررسی زمان انتخابات
$now = time();
$startTime = $startDate ? strtotime($startDate) : 0;
$endTime = $endDate ? strtotime($endDate) : 0;

// اگر تاریخ‌ها تنظیم نشده باشند
if ($startTime === 0 || $endTime === 0) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'زمان‌بندی انتخابات تنظیم نشده است'.$startDate
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

$sql = "SELECT tracking_code,f.create_date,u.id,u.national_Id,u.first_name,u.last_name,u.persian_birth_date,u.personnel_code,u.gender,u.father_name,u.org_position_desc,u.regionName,ud.user_photo,ua.post_code,ua.address FROM final_submissions as f join users as u on u.national_id=f.nationalId join user_documents as ud on ud.nationalId=f.nationalId left join user_addresses as ua on ua.user_id=u.id where requestStatus='SUPERVISION_APPROVED' ORDER BY create_date DESC;";
$res = $db->query($sql);

$list = [];
while ($row = $db->fetch_assoc($res)) {
    $row['create_datesh'] = jdate('H:i Y-n-j ', strtotime($row['create_date']), '', '', 'en');
    $list[] = $row;
}

/* =========================
   4. Response
========================= */
echo json_encode([
    'status' => true,
    'data'   => $list
], JSON_UNESCAPED_UNICODE);