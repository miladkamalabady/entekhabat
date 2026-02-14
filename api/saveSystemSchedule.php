<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode([
        'status' => false,
        'message' => 'فقط درخواست POST مجاز است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$input = json_decode(file_get_contents('php://input'), true) ?: [];
$events = $input['events'] ?? null;

if (!is_array($events) || count($events) === 0) {
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'لیست رویدادها ارسال نشده است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->connect();
$db->query("CREATE TABLE IF NOT EXISTS election_schedule_events (
    id INT(11) NOT NULL AUTO_INCREMENT,
    event_key VARCHAR(100) NOT NULL,
    event_name VARCHAR(255) NOT NULL,
    start_date DATETIME NULL,
    end_date DATETIME NULL,
    sort_order INT(11) NOT NULL DEFAULT 0,
    updated_by VARCHAR(20) DEFAULT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uniq_event_key (event_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

try {
    foreach ($events as $event) {
        $eventKey = isset($event['key']) ? trim($event['key']) : '';
        $eventName = isset($event['name']) ? trim($event['name']) : '';
        $startDate = isset($event['startDate']) && $event['startDate'] !== '' ? trim($event['startDate']) : null;
        $endDate = isset($event['endDate']) && $event['endDate'] !== '' ? trim($event['endDate']) : null;
        $sortOrder = isset($event['id']) ? intval($event['id']) : 0;

        if ($eventKey === '' || $eventName === '') {
            continue;
        }

        $eventKeySql = $db->escape($eventKey);
        $eventNameSql = $db->escape($eventName);
        $updatedBySql = $db->escape($nationalId);

        $startDateSql = $startDate ? "'" . $db->escape($startDate) . "'" : "NULL";
        $endDateSql = $endDate ? "'" . $db->escape($endDate) . "'" : "NULL";

        $sql = "INSERT INTO election_schedule_events (event_key, event_name, start_date, end_date, sort_order, updated_by)
                VALUES ('{$eventKeySql}', '{$eventNameSql}', {$startDateSql}, {$endDateSql}, {$sortOrder}, '{$updatedBySql}')
                ON DUPLICATE KEY UPDATE
                  event_name = VALUES(event_name),
                  start_date = VALUES(start_date),
                  end_date = VALUES(end_date),
                  sort_order = VALUES(sort_order),
                  updated_by = VALUES(updated_by)";

        $db->query($sql);
    }

    $db->query("COMMIT");

    echo json_encode([
        'status' => true,
        'message' => 'زمان‌بندی با موفقیت ذخیره شد.'
    ], JSON_UNESCAPED_UNICODE);
} catch (Exception $e) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'خطا در ذخیره‌سازی زمان‌بندی.'
    ], JSON_UNESCAPED_UNICODE);
}
