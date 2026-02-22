<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');

$input = json_decode(file_get_contents('php://input'), true);
$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

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

$id = isset($input['id']) ? (int)$input['id'] : 0;
$status = isset($input['status']) ? trim($input['status']) : '';
$responseText = isset($input['responseText']) ? trim($input['responseText']) : '';

$allowedStatuses = ['pending', 'under_review', 'approved', 'rejected', 'cancelled'];
if (!$id || !in_array($status, $allowedStatuses, true)) {
    $db->query("ROLLBACK");
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'پارامترها نامعتبر است.'], JSON_UNESCAPED_UNICODE);
    exit;
}

$roles = isset($jwtData['roles']) ? $jwtData['roles'] : null;
$isReviewer = is_array($roles)
    ? (in_array('SUPERVISOR', $roles) || in_array('EXECUTIVE', $roles) || in_array('ADMIN', $roles))
    : ($roles === 'SUPERVISOR' || $roles === 'EXECUTIVE' || $roles === 'ADMIN');
if (($status === 'approved' || $status === 'rejected' || $status === 'under_review') && !$isReviewer) {
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode(['status' => false, 'message' => 'دسترسی لازم برای تایید/رد اعتراض را ندارید.'], JSON_UNESCAPED_UNICODE);
    exit;
}

$safeStatus = $db->escape($status);
$safeResponseText = $db->escape($responseText);
$safeReviewer = $db->escape($nationalId);

$whereOwner = '';
if (!$isReviewer) {
    $safeOwner = $db->escape($nationalId);
    $whereOwner = " AND national_id = '{$safeOwner}'";
}

$sql = "UPDATE objections
        SET status = '{$safeStatus}',
            response_text = CASE WHEN '{$safeResponseText}' <> '' THEN '{$safeResponseText}' ELSE response_text END,
            response_by = CASE WHEN '{$safeStatus}' IN ('approved','rejected','under_review') THEN '{$safeReviewer}' ELSE response_by END,
            response_at = CASE WHEN '{$safeStatus}' IN ('approved','rejected','under_review') THEN NOW() ELSE response_at END,
            cancelled_at = CASE WHEN '{$safeStatus}' = 'cancelled' THEN NOW() ELSE cancelled_at END,
            updated_at = NOW()
        WHERE id = {$id}{$whereOwner}";

$db->query($sql);
if ($db->affected_rows() <= 0) {
    $db->query("ROLLBACK");
    http_response_code(404);
    echo json_encode(['status' => false, 'message' => 'اعتراضی برای بروزرسانی یافت نشد.'], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->query("INSERT INTO logs (nationalId, action, description) VALUES ('{$safeReviewer}', 'بروزرسانی اعتراض', 'اعتراض {$id} به وضعیت {$safeStatus} تغییر یافت')");
$db->query("COMMIT");

echo json_encode([
    'status' => true,
    'message' => 'وضعیت اعتراض با موفقیت بروزرسانی شد.',
    'data' => [
        'id' => $id,
        'status' => $status
    ]
], JSON_UNESCAPED_UNICODE);
