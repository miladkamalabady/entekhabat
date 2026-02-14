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
        $row['startDates'] = $startValue;
        $row['startTime'] = jdate('H:i', strtotime($startValue), '', '', 'en');
    } else {
        $row['startDates'] = null;
        $row['startTime'] = null;
    }

    if ($endValue) {
        $row['endDates'] = $endValue;
        $row['endTime'] = jdate('H:i', strtotime($endValue), '', '', 'en');
    } else {
        $row['endDates'] = null;
        $row['endTime'] = null;
    }

    return $row;
}

$hasScheduleTable = false;
$tableCheck = $db->query("SHOW TABLES LIKE 'election_schedule_events'");
if ($tableCheck && $db->num_rows($tableCheck) > 0) {
    $hasScheduleTable = true;
}

if ($hasScheduleTable) {
    $sqlSchedule = "SELECT start_date, end_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1";
    $scheduleResult = $db->query($sqlSchedule);

    if ($scheduleResult && $db->num_rows($scheduleResult) > 0) {
        $scheduleRow = $db->fetch_assoc($scheduleResult);

        $now = date('Y-m-d H:i:s');
        $startDate = $scheduleRow['start_date'];
        $endDate = $scheduleRow['end_date'];

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
}

$sql = "SELECT * FROM config WHERE id=1";
$res = $db->query($sql);

if (!$res || $db->num_rows($res) === 0) {
    http_response_code(404);
    echo json_encode([
        'status' => false,
        'message' => 'وضعیت یافت نشد!'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
$row = $res->fetch_assoc();
// $row['endDates'] = jdate('l j F Y', strtotime($row['EndDate']), '', '', 'en');
// $row['endTime'] = jdate('H:i', strtotime($row['EndDate']), '', '', 'en');
// $row['startDates'] = jdate('l j F Y', strtotime($row['startDate']), '', '', 'en');
// $row['startTime'] = jdate('H:i', strtotime($row['startDate']), '', '', 'en');
/* =========================
   4. Response
========================= */

if (isset($row['active']) && (int)$row['active'] === 1) {
    $now = date('Y-m-d H:i:s');
    if (!empty($row['startDate']) && !empty($row['EndDate'])) {
        $row['active'] = ($row['startDate'] <= $now && $now <= $row['EndDate']) ? 1 : 0;
    }
}
echo json_encode([
    'status' => true,
    'data' => formatConfigDates($row)

], JSON_UNESCAPED_UNICODE);
