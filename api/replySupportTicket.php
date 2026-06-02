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

$ticketId = (int)($input['ticketId'] ?? 0);
$message = trim($input['message'] ?? '');
$closeTicket = !empty($input['closeTicket']);
if ($ticketId <= 0 || ($message === '' && !$closeTicket)) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'شناسه تیکت یا متن پاسخ نامعتبر است.'], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->connect();
ensureSupportTicketTables($db);
$user = getSupportCurrentUser($db, $nationalId);
if (!$user) {
    http_response_code(404);
    echo json_encode(['status' => false, 'message' => 'کاربر یافت نشد'], JSON_UNESCAPED_UNICODE);
    exit;
}

$res = $db->query("SELECT * FROM support_tickets WHERE id = {$ticketId} LIMIT 1");
$ticket = $res ? $res->fetch_assoc() : null;
if (!$ticket || !canAccessSupportTicket($ticket, $user)) {
    http_response_code(403);
    echo json_encode(['status' => false, 'message' => 'شما دسترسی پاسخ به این تیکت را ندارید.'], JSON_UNESCAPED_UNICODE);
    exit;
}
if ($ticket['status'] === 'closed') {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'این تیکت بسته شده است.'], JSON_UNESCAPED_UNICODE);
    exit;
}

$senderRole = normalizeSupportRole($user['roles'] ?? '');
if ((string)$ticket['requester_national_id'] === (string)$nationalId && $senderRole !== 'ADMIN') {
    $senderRole = 'USER';
}
$senderName = trim(($user['first_name'] ?? '') . ' ' . ($user['last_name'] ?? ''));

if ($message !== '') {
    $db->query("INSERT INTO support_ticket_messages
        (ticket_id, sender_national_id, sender_name, sender_role, message, attachments)
        VALUES (
            {$ticketId},
            '" . $db->escape($nationalId) . "',
            '" . $db->escape($senderName) . "',
            '" . $db->escape($senderRole) . "',
            '" . $db->escape($message) . "',
            '[]'
        )");
}

$statusSql = $closeTicket ? ", status = 'closed', closed_at = NOW(), closed_by = '" . $db->escape($nationalId) . "'" : ", status = 'open'";
$db->query("UPDATE support_tickets SET updated_at = NOW() {$statusSql} WHERE id = {$ticketId}");

echo json_encode(['status' => true, 'message' => 'پاسخ تیکت ثبت شد.'], JSON_UNESCAPED_UNICODE);
