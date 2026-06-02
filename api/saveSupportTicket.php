<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'supportTicketHelpers.php';

header('Content-Type: application/json; charset=utf-8');
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['status' => false, 'message' => 'فقط درخواست POST مجاز است.'], JSON_UNESCAPED_UNICODE);
    exit;
}

$input = json_decode(file_get_contents('php://input'), true);
if (!$input) {
    $input = $_POST;
}

$db->connect();
ensureSupportTicketTables($db);
$user = getSupportCurrentUser($db, $nationalId);
if (!$user) {
    http_response_code(404);
    echo json_encode(['status' => false, 'message' => 'کاربر یافت نشد'], JSON_UNESCAPED_UNICODE);
    exit;
}

$subject = trim($input['subject'] ?? '');
$category = trim($input['category'] ?? '');
$priority = trim($input['priority'] ?? 'medium');
$description = trim($input['description'] ?? '');
$targetRole = normalizeSupportRole($input['targetRole'] ?? 'EXECUTIVE');
if ($targetRole === 'USER') {
    $targetRole = 'EXECUTIVE';
}

if ($subject === '' || $category === '' || $description === '') {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'موضوع، دسته‌بندی و شرح درخواست الزامی است.'], JSON_UNESCAPED_UNICODE);
    exit;
}
if (!in_array($priority, ['low', 'medium', 'high', 'urgent'], true)) {
    $priority = 'medium';
}

$regionId = (int)($user['region_id'] ?? 0);
$provinceCode = (int)($user['provinceCode'] ?? 0);
$supportLevel = $targetRole === 'ADMIN' ? 'headquarters' : ($regionId > 0 && substr((string)$regionId, -2) === '00' ? 'province' : 'region');
$requesterName = trim(($user['first_name'] ?? '') . ' ' . ($user['last_name'] ?? ''));
$ticketCode = 'T' . date('YmdHis') . random_int(100, 999);

$db->query("INSERT INTO support_tickets
    (ticket_code, requester_national_id, requester_name, requester_region_id, requester_region_name, requester_province_code, target_role, support_level, subject, category, priority, status)
    VALUES (
        '" . $db->escape($ticketCode) . "',
        '" . $db->escape($nationalId) . "',
        '" . $db->escape($requesterName) . "',
        {$regionId},
        '" . $db->escape($user['regionName'] ?? '') . "',
        {$provinceCode},
        '" . $db->escape($targetRole) . "',
        '" . $db->escape($supportLevel) . "',
        '" . $db->escape($subject) . "',
        '" . $db->escape($category) . "',
        '" . $db->escape($priority) . "',
        'open'
    )");
$ticketId = $db->insert_id(null);

$db->query("INSERT INTO support_ticket_messages
    (ticket_id, sender_national_id, sender_name, sender_role, message, attachments)
    VALUES (
        {$ticketId},
        '" . $db->escape($nationalId) . "',
        '" . $db->escape($requesterName) . "',
        'USER',
        '" . $db->escape($description) . "',
        '[]'
    )");

echo json_encode([
    'status' => true,
    'message' => 'تیکت با موفقیت ثبت شد.',
    'ticketId' => $ticketId,
    'ticketCode' => $ticketCode
], JSON_UNESCAPED_UNICODE);
