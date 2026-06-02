<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'liveChatHelpers.php';

header('Content-Type: application/json; charset=utf-8');
$db->connect();
ensureLiveChatTables($db);

$user = getLiveChatUser($db, $nationalId);
if (!$user) {
    http_response_code(404);
    echo json_encode(['status' => false, 'message' => 'کاربر یافت نشد'], JSON_UNESCAPED_UNICODE);
    exit;
}

$nid = $db->escape($nationalId);
$limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 30;
if ($limit < 1) $limit = 30;
if ($limit > 100) $limit = 100;

$where = "s.user_national_id = '{$nid}'";
if (isLiveChatSupportAgent($user)) {
    $where = "(s.status IN ('waiting','active') OR s.assigned_agent_national_id = '{$nid}')";
}

$sql = "SELECT s.*,
        (SELECT message FROM live_chat_messages m WHERE m.session_id = s.id ORDER BY m.id DESC LIMIT 1) AS last_message,
        (SELECT COUNT(*) FROM live_chat_messages m WHERE m.session_id = s.id AND m.sender_national_id <> '{$nid}' AND m.is_read = 0) AS unread_count
    FROM live_chat_sessions s
    WHERE {$where}
    ORDER BY FIELD(s.status, 'waiting', 'active', 'closed'), s.updated_at DESC
    LIMIT {$limit}";
$res = $db->query($sql);
$sessions = [];
if ($res) {
    while ($row = $res->fetch_assoc()) {
        $item = formatLiveChatSession($row);
        $item['lastMessage'] = $row['last_message'];
        $item['unreadCount'] = (int)$row['unread_count'];
        $sessions[] = $item;
    }
}

echo json_encode([
    'status' => true,
    'data' => $sessions,
    'isSupportAgent' => isLiveChatSupportAgent($user),
    'onlineAgents' => getLiveChatOnlineAgentsCount($db)
], JSON_UNESCAPED_UNICODE);
