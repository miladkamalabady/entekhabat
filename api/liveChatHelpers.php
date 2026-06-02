<?php

function ensureLiveChatTables($db) {
    $db->query("CREATE TABLE IF NOT EXISTS live_chat_sessions (
        id INT(11) NOT NULL AUTO_INCREMENT,
        session_code VARCHAR(32) NOT NULL,
        user_national_id VARCHAR(20) NOT NULL,
        user_name VARCHAR(255) DEFAULT NULL,
        user_role VARCHAR(50) DEFAULT NULL,
        user_region_id VARCHAR(20) DEFAULT NULL,
        assigned_agent_national_id VARCHAR(20) DEFAULT NULL,
        assigned_agent_name VARCHAR(255) DEFAULT NULL,
        status ENUM('waiting','active','closed') NOT NULL DEFAULT 'waiting',
        subject VARCHAR(255) DEFAULT NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        closed_at TIMESTAMP NULL DEFAULT NULL,
        PRIMARY KEY (id),
        UNIQUE KEY session_code (session_code),
        KEY user_national_id (user_national_id),
        KEY assigned_agent_national_id (assigned_agent_national_id),
        KEY status (status),
        KEY updated_at (updated_at)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8");

    $db->query("CREATE TABLE IF NOT EXISTS live_chat_messages (
        id INT(11) NOT NULL AUTO_INCREMENT,
        session_id INT(11) NOT NULL,
        sender_national_id VARCHAR(20) DEFAULT NULL,
        sender_name VARCHAR(255) DEFAULT NULL,
        sender_role VARCHAR(50) DEFAULT NULL,
        sender_type ENUM('user','support','system') NOT NULL DEFAULT 'user',
        message TEXT NOT NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        is_read TINYINT(1) NOT NULL DEFAULT 0,
        PRIMARY KEY (id),
        KEY session_id (session_id),
        KEY created_at (created_at),
        CONSTRAINT live_chat_messages_ibfk_1 FOREIGN KEY (session_id) REFERENCES live_chat_sessions (id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8");
}

function getLiveChatUser($db, $nationalId) {
    $nid = $db->escape($nationalId);
    $res = $db->query("SELECT national_id, first_name, last_name, roles, region_id, regionName FROM users WHERE national_id = '{$nid}' LIMIT 1");
    if (!$res || !$res->num_rows) return null;
    $user = $res->fetch_assoc();
    $user['full_name'] = trim(($user['first_name'] ?? '') . ' ' . ($user['last_name'] ?? ''));
    return $user;
}

function isLiveChatSupportAgent($user) {
    return $user && in_array($user['roles'], ['ADMIN', 'SUPERVISOR', 'EXECUTIVE']);
}

function canAccessLiveChatSession($db, $sessionId, $nationalId, $user) {
    $sessionId = (int)$sessionId;
    $nid = $db->escape($nationalId);
    $res = $db->query("SELECT * FROM live_chat_sessions WHERE id = {$sessionId} LIMIT 1");
    if (!$res || !$res->num_rows) return false;
    $session = $res->fetch_assoc();

    if ($session['user_national_id'] === $nationalId) return $session;
    if (isLiveChatSupportAgent($user)) return $session;
    if ($session['assigned_agent_national_id'] === $nationalId) return $session;
    return false;
}

function formatLiveChatSession($row) {
    return [
        'id' => (int)$row['id'],
        'sessionCode' => $row['session_code'],
        'userNationalId' => $row['user_national_id'],
        'userName' => $row['user_name'],
        'userRole' => $row['user_role'],
        'userRegionId' => $row['user_region_id'],
        'assignedAgentNationalId' => $row['assigned_agent_national_id'],
        'assignedAgentName' => $row['assigned_agent_name'],
        'status' => $row['status'],
        'subject' => $row['subject'],
        'createdAt' => $row['created_at'],
        'updatedAt' => $row['updated_at'],
        'closedAt' => $row['closed_at']
    ];
}

function formatLiveChatMessage($row) {
    return [
        'id' => (int)$row['id'],
        'sessionId' => (int)$row['session_id'],
        'senderNationalId' => $row['sender_national_id'],
        'senderName' => $row['sender_name'],
        'senderRole' => $row['sender_role'],
        'senderType' => $row['sender_type'],
        'text' => $row['message'],
        'createdAt' => $row['created_at'],
        'isRead' => (int)$row['is_read'] === 1
    ];
}

function getLiveChatOnlineAgentsCount($db) {
    $res = $db->query("SELECT COUNT(*) AS total FROM users WHERE roles IN ('ADMIN','SUPERVISOR','EXECUTIVE')");
    $row = $res ? $res->fetch_assoc() : ['total' => 0];
    return (int)$row['total'];
}
