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

$input = json_decode(file_get_contents('php://input'), true) ?: [];
$subject = trim($input['subject'] ?? 'گفتگوی آنلاین پشتیبانی');
$subject = $db->escape(substr($subject, 0, 250));
$nid = $db->escape($nationalId);

$res = $db->query("SELECT * FROM live_chat_sessions WHERE user_national_id = '{$nid}' AND status IN ('waiting','active') ORDER BY id DESC LIMIT 1");
if ($res && $res->num_rows) {
    $session = $res->fetch_assoc();
} else {
    $sessionCode = 'LC' . date('YmdHis') . random_int(100, 999);
    $name = $db->escape($user['full_name']);
    $role = $db->escape($user['roles']);
    $regionId = $db->escape($user['region_id']);
    $db->query("INSERT INTO live_chat_sessions (session_code, user_national_id, user_name, user_role, user_region_id, subject)
        VALUES ('{$sessionCode}', '{$nid}', '{$name}', '{$role}', '{$regionId}', '{$subject}')");
    $sessionId = (int)$db->insert_id(null);
    $welcome = $db->escape('گفتگوی آنلاین شما شروع شد. لطفاً پیام خود را بنویسید تا اولین پشتیبان آنلاین پاسخ دهد.');
    $db->query("INSERT INTO live_chat_messages (session_id, sender_type, sender_name, sender_role, message, is_read)
        VALUES ({$sessionId}, 'system', 'سامانه', 'SYSTEM', '{$welcome}', 1)");
    $res = $db->query("SELECT * FROM live_chat_sessions WHERE id = {$sessionId} LIMIT 1");
    $session = $res->fetch_assoc();
}

echo json_encode([
    'status' => true,
    'data' => formatLiveChatSession($session),
    'onlineAgents' => getLiveChatOnlineAgentsCount($db)
], JSON_UNESCAPED_UNICODE);
