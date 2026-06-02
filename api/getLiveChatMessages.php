<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'liveChatHelpers.php';

header('Content-Type: application/json; charset=utf-8');
$db->connect();
ensureLiveChatTables($db);

$user = getLiveChatUser($db, $nationalId);
$sessionId = isset($_GET['sessionId']) ? (int)$_GET['sessionId'] : 0;
$afterId = isset($_GET['afterId']) ? (int)$_GET['afterId'] : 0;
if (!$sessionId || !$user) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'پارامترهای گفتگو کامل نیست'], JSON_UNESCAPED_UNICODE);
    exit;
}
$session = canAccessLiveChatSession($db, $sessionId, $nationalId, $user);
if (!$session) {
    http_response_code(403);
    echo json_encode(['status' => false, 'message' => 'دسترسی به این گفتگو مجاز نیست'], JSON_UNESCAPED_UNICODE);
    exit;
}

$nid = $db->escape($nationalId);
$whereAfter = $afterId > 0 ? "AND id > {$afterId}" : '';
$res = $db->query("SELECT * FROM live_chat_messages WHERE session_id = {$sessionId} {$whereAfter} ORDER BY id ASC LIMIT 200");
$messages = [];
if ($res) {
    while ($row = $res->fetch_assoc()) {
        $messages[] = formatLiveChatMessage($row);
    }
}
$db->query("UPDATE live_chat_messages SET is_read = 1 WHERE session_id = {$sessionId} AND (sender_national_id IS NULL OR sender_national_id <> '{$nid}')");

$resSession = $db->query("SELECT * FROM live_chat_sessions WHERE id = {$sessionId} LIMIT 1");
$session = $resSession->fetch_assoc();

echo json_encode([
    'status' => true,
    'data' => $messages,
    'session' => formatLiveChatSession($session)
], JSON_UNESCAPED_UNICODE);
