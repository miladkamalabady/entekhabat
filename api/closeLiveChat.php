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
$session = $sessionId && $user ? canAccessLiveChatSession($db, $sessionId, $nationalId, $user) : false;
if (!$session) {
    http_response_code(403);
    echo json_encode(['status' => false, 'message' => 'دسترسی به این گفتگو مجاز نیست'], JSON_UNESCAPED_UNICODE);
    exit;
}

$db->query("UPDATE live_chat_sessions SET status = 'closed', closed_at = NOW(), updated_at = NOW() WHERE id = {$sessionId}");
$name = $db->escape($user['full_name']);
$role = $db->escape($user['roles']);
$nid = $db->escape($nationalId);
$text = $db->escape('گفتگو بسته شد.');
$db->query("INSERT INTO live_chat_messages (session_id, sender_national_id, sender_name, sender_role, sender_type, message, is_read)
    VALUES ({$sessionId}, '{$nid}', '{$name}', '{$role}', 'system', '{$text}', 0)");

echo json_encode(['status' => true, 'message' => 'گفتگو بسته شد'], JSON_UNESCAPED_UNICODE);
