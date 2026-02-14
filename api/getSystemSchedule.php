<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    http_response_code(405);
    echo json_encode([
        'status' => false,
        'message' => 'فقط درخواست GET مجاز است.'
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

$result = $db->query("SELECT event_key, event_name, start_date, end_date, sort_order FROM election_schedule_events ORDER BY sort_order ASC, id ASC");
$data = [];
while ($row = $db->fetch_assoc($result)) {
    $data[] = $row;
}

echo json_encode([
    'status' => true,
    'data' => $data
], JSON_UNESCAPED_UNICODE);
