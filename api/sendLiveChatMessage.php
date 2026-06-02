<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'liveChatHelpers.php';

header('Content-Type: application/json; charset=utf-8');
$db->connect();
ensureLiveChatTables($db);

$user = getLiveChatUser($db, $nationalId);
$input = json_decode(file_get_contents('php://input'), true) ?: [];
$sessionId = isset($input['sessionId']) ? (int)$input['sessionId'] : 0;
$message = trim($input['message'] ?? '');
if (!$sessionId || !$user || $message === '') {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'پیام یا شناسه گفتگو معتبر نیست'], JSON_UNESCAPED_UNICODE);
    exit;
}
if (strlen($message) > 1000) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'حداکثر طول پیام ۱۰۰۰ کاراکتر است'], JSON_UNESCAPED_UNICODE);
    exit;
}
$session = canAccessLiveChatSession($db, $sessionId, $nationalId, $user);
if (!$session || $session['status'] === 'closed') {
    http_response_code(403);
    echo json_encode(['status' => false, 'message' => 'امکان ارسال پیام در این گفتگو وجود ندارد'], JSON_UNESCAPED_UNICODE);
    exit;
}

$isAgent = isLiveChatSupportAgent($user) && $session['user_national_id'] !== $nationalId;
$senderType = $isAgent ? 'support' : 'user';
$nid = $db->escape($nationalId);
$name = $db->escape($user['full_name']);
$role = $db->escape($user['roles']);
$text = $db->escape($message);

if ($isAgent) {
    $db->query("UPDATE live_chat_sessions SET status = 'active', assigned_agent_national_id = '{$nid}', assigned_agent_name = '{$name}', updated_at = NOW() WHERE id = {$sessionId}");
} else {
    $db->query("UPDATE live_chat_sessions SET updated_at = NOW() WHERE id = {$sessionId}");
}

$db->query("INSERT INTO live_chat_messages (session_id, sender_national_id, sender_name, sender_role, sender_type, message)
    VALUES ({$sessionId}, '{$nid}', '{$name}', '{$role}', '{$senderType}', '{$text}')");
$messageId = (int)$db->insert_id(null);
$res = $db->query("SELECT * FROM live_chat_messages WHERE id = {$messageId} LIMIT 1");
$row = $res->fetch_assoc();

echo json_encode([
    'status' => true,
    'data' => formatLiveChatMessage($row)
], JSON_UNESCAPED_UNICODE);
