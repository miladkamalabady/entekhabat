<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   3. Query status
========================= */
function formatConfigDates($row)
{
    $startValue = $row['startDate'] ?? null;
    $endValue = $row['EndDate'] ?? null;

    if ($startValue) {
        $row['startDates'] = jdate('l j F Y', strtotime($startValue), '', '', 'en');
        $row['startTime'] = jdate('H:i', strtotime($startValue), '', '', 'en');
    } else {
        $row['startDates'] = null;
        $row['startTime'] = null;
    }

    if ($endValue) {
        $row['endDates'] = jdate('l j F Y', strtotime($endValue), '', '', 'en');
        $row['endTime'] = jdate('H:i', strtotime($endValue), '', '', 'en');
    } else {
        $row['endDates'] = null;
        $row['endTime'] = null;
    }

    return $row;
}


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


    $sqlSchedule = "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1";
    $scheduleResult = $db->query($sqlSchedule);

    if ($scheduleResult && $db->num_rows($scheduleResult) > 0) {
        $scheduleRow = $db->fetch_assoc($scheduleResult);

        $now = date('Y-m-d H:i:s');
        $startDate = normalizeToGregorianDateTime($scheduleRow['start_date']);
        $endDate = normalizeToGregorianDateTime($scheduleRow['end_date']);

        $isActive = 0;
        if (!empty($startDate) && !empty($endDate) && $startDate <= $now && $now <= $endDate) {
            $isActive = 1;
        }

        $configFromSchedule = [
            'id' => 1,
            'startDate' => $startDate,
            'EndDate' => $endDate,
            'create_date' => null,
            'active' => $isActive
        ];

        echo json_encode([
            'status' => true,
            'data' => formatConfigDates($configFromSchedule)
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'زمان‌بندی انتخابات تنظیم نشده است'
    ], JSON_UNESCAPED_UNICODE);
    exit;
