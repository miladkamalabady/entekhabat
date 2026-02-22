<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$db->connect();

$db->query("CREATE TABLE IF NOT EXISTS objections (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    tracking_code VARCHAR(30) NOT NULL,
    national_id VARCHAR(20) NOT NULL,
    decision_type VARCHAR(80) NULL,
    case_number VARCHAR(80) NULL,
    candidate_name VARCHAR(255) NULL,
    candidate_region VARCHAR(255) NULL,
    candidate_position VARCHAR(255) NULL,
    subject VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    reasons TEXT NULL,
    urgency VARCHAR(20) DEFAULT 'normal',
    status VARCHAR(30) DEFAULT 'pending',
    declaration TINYINT(1) DEFAULT 1,
    response_text TEXT NULL,
    response_by VARCHAR(20) NULL,
    response_at DATETIME NULL,
    cancelled_at DATETIME NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_national_id (national_id),
    INDEX idx_status (status),
    UNIQUE KEY uq_tracking_code (tracking_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");

$roles = isset($jwtData['roles']) ? $jwtData['roles'] : null;
$isReviewer = is_array($roles)
    ? (in_array('SUPERVISOR', $roles) || in_array('EXECUTIVE', $roles) || in_array('ADMIN', $roles))
    : ($roles === 'SUPERVISOR' || $roles === 'EXECUTIVE' || $roles === 'ADMIN');

$trackingCode = isset($_GET['trackingCode']) ? trim($_GET['trackingCode']) : '';
$status = isset($_GET['status']) ? trim($_GET['status']) : '';

$where = [];
if (!$isReviewer) {
    $safeNationalId = $db->escape($nationalId);
    $where[] = "national_id = '{$safeNationalId}'";
}
if ($trackingCode !== '') {
    $safeTrackingCode = $db->escape($trackingCode);
    $where[] = "tracking_code = '{$safeTrackingCode}'";
}
if ($status !== '') {
    $safeStatus = $db->escape($status);
    $where[] = "status = '{$safeStatus}'";
}

$whereSql = count($where) ? ('WHERE ' . implode(' AND ', $where)) : '';

$sql = "SELECT * FROM objections {$whereSql} ORDER BY created_at DESC";
$result = $db->query($sql);

$rows = [];
while ($row = $db->fetch_assoc($result)) {
    $rows[] = [
        'id' => (int)$row['id'],
        'trackingCode' => $row['tracking_code'],
        'nationalId' => $row['national_id'],
        'decisionType' => $row['decision_type'],
        'caseNumber' => $row['case_number'],
        'candidateName' => $row['candidate_name'],
        'candidateRegion' => $row['candidate_region'],
        'candidatePosition' => $row['candidate_position'],
        'subject' => $row['subject'],
        'description' => $row['description'],
        'preview' => mb_substr($row['description'], 0, 150) . '...',
        'reasons' => $row['reasons'] ? json_decode($row['reasons'], true) : [],
        'urgency' => $row['urgency'] ?: 'normal',
        'status' => $row['status'],
        'declaration' => (bool)$row['declaration'],
        'submittedDate' => $row['created_at'],
        'lastUpdate' => $row['updated_at'],
        'documentsCount' => 0,
        'responseText' => $row['response_text'],
        'responseBy' => $row['response_by'],
        'responseAt' => $row['response_at']
    ];
}

echo json_encode([
    'status' => true,
    'data' => $rows
], JSON_UNESCAPED_UNICODE);
