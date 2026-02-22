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

$decisionType = isset($input['decisionType']) ? trim($input['decisionType']) : '';
$caseNumber = isset($input['caseNumber']) ? trim($input['caseNumber']) : '';
$candidateName = isset($input['candidateName']) ? trim($input['candidateName']) : '';
$candidateRegion = isset($input['candidateRegion']) ? trim($input['candidateRegion']) : '';
$candidatePosition = isset($input['candidatePosition']) ? trim($input['candidatePosition']) : '';
$subject = isset($input['subject']) ? trim($input['subject']) : '';
$description = isset($input['description']) ? trim($input['description']) : '';
$reasons = isset($input['reasons']) && is_array($input['reasons']) ? $input['reasons'] : [];
$urgency = isset($input['urgency']) ? trim($input['urgency']) : 'normal';
$declaration = !empty($input['declaration']) ? 1 : 0;

if ($decisionType === '' || $caseNumber === '' || $subject === '' || $description === '' || !$declaration) {
    $db->query("ROLLBACK");
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'پارامترهای الزامی ناقص است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$safeDecisionType = $db->escape($decisionType);
$safeCaseNumber = $db->escape($caseNumber);
$safeCandidateName = $db->escape($candidateName);
$safeCandidateRegion = $db->escape($candidateRegion);
$safeCandidatePosition = $db->escape($candidatePosition);
$safeSubject = $db->escape($subject);
$safeDescription = $db->escape($description);
$safeReasons = $db->escape(json_encode($reasons, JSON_UNESCAPED_UNICODE));
$safeUrgency = $db->escape($urgency);
$safeNationalId = $db->escape($nationalId);

$trackingCode = date('YmdHis') . str_pad((string)mt_rand(1, 9999), 4, '0', STR_PAD_LEFT);
$safeTrackingCode = $db->escape($trackingCode);

$sql = "INSERT INTO objections (
        tracking_code, national_id, decision_type, case_number, candidate_name, candidate_region, candidate_position,
        subject, description, reasons, urgency, status, declaration
    ) VALUES (
        '{$safeTrackingCode}', '{$safeNationalId}', '{$safeDecisionType}', '{$safeCaseNumber}', '{$safeCandidateName}', '{$safeCandidateRegion}', '{$safeCandidatePosition}',
        '{$safeSubject}', '{$safeDescription}', '{$safeReasons}', '{$safeUrgency}', 'pending', {$declaration}
    )";

$res = $db->query($sql);
if (!$res) {
    $db->query("ROLLBACK");
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'message' => 'ثبت اعتراض ناموفق بود.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->query("INSERT INTO logs (nationalId, action, description) VALUES ('{$safeNationalId}', 'ثبت اعتراض', 'ثبت اعتراض با کد {$safeTrackingCode}')");
$db->query("COMMIT");

echo json_encode([
    'status' => true,
    'message' => 'اعتراض با موفقیت ثبت شد.',
    'data' => [
        'trackingCode' => $trackingCode
    ]
], JSON_UNESCAPED_UNICODE);
