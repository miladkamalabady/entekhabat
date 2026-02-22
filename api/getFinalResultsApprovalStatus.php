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
$db->query("CREATE TABLE IF NOT EXISTS final_results_approvals (
    id INT(11) NOT NULL,
    executive_approved TINYINT(1) NOT NULL DEFAULT 0,
    supervisor_approved TINYINT(1) NOT NULL DEFAULT 0,
    is_active TINYINT(1) NOT NULL DEFAULT 0,
    executive_approved_by VARCHAR(20) DEFAULT NULL,
    executive_approved_at DATETIME DEFAULT NULL,
    supervisor_approved_by VARCHAR(20) DEFAULT NULL,
    supervisor_approved_at DATETIME DEFAULT NULL,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

$db->query("INSERT IGNORE INTO final_results_approvals (id) VALUES (1)");

$result = $db->query("SELECT executive_approved, supervisor_approved, is_active FROM final_results_approvals WHERE id = 1 LIMIT 1");
$row = $db->fetch_assoc($result);

$data = [
    'executiveApproved' => (bool)($row['executive_approved'] ?? 0),
    'supervisorApproved' => (bool)($row['supervisor_approved'] ?? 0),
    'isActive' => (bool)($row['is_active'] ?? 0)
];

echo json_encode([
    'status' => true,
    'data' => $data
], JSON_UNESCAPED_UNICODE);
