<?php

function ensureSupportTicketTables($db) {
    $db->query("CREATE TABLE IF NOT EXISTS support_tickets (
        id INT(11) NOT NULL AUTO_INCREMENT,
        ticket_code VARCHAR(30) NOT NULL,
        requester_national_id VARCHAR(20) NOT NULL,
        requester_name VARCHAR(120) DEFAULT NULL,
        requester_region_id INT(11) DEFAULT NULL,
        requester_region_name VARCHAR(120) DEFAULT NULL,
        requester_province_code INT(11) DEFAULT NULL,
        target_role VARCHAR(20) NOT NULL DEFAULT 'EXECUTIVE',
        support_level VARCHAR(20) NOT NULL DEFAULT 'region',
        subject VARCHAR(255) NOT NULL,
        category VARCHAR(60) NOT NULL,
        priority VARCHAR(20) NOT NULL DEFAULT 'medium',
        status VARCHAR(20) NOT NULL DEFAULT 'open',
        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        closed_at DATETIME DEFAULT NULL,
        closed_by VARCHAR(20) DEFAULT NULL,
        PRIMARY KEY (id),
        UNIQUE KEY support_tickets_code_idx (ticket_code),
        KEY support_tickets_requester_idx (requester_national_id),
        KEY support_tickets_scope_idx (target_role, support_level, requester_region_id, requester_province_code),
        KEY support_tickets_status_idx (status)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_persian_ci");

    $db->query("CREATE TABLE IF NOT EXISTS support_ticket_messages (
        id INT(11) NOT NULL AUTO_INCREMENT,
        ticket_id INT(11) NOT NULL,
        sender_national_id VARCHAR(20) NOT NULL,
        sender_name VARCHAR(120) DEFAULT NULL,
        sender_role VARCHAR(20) NOT NULL DEFAULT 'USER',
        message TEXT NOT NULL,
        attachments TEXT DEFAULT NULL,
        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (id),
        KEY support_ticket_messages_ticket_idx (ticket_id)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_persian_ci");
}

function getSupportCurrentUser($db, $nationalId) {
    $sql = "SELECT u.national_id, u.first_name, u.last_name, u.roles, u.region_id,
                   r.Name AS regionName, r.ProvinceCode AS provinceCode
            FROM users u
            LEFT JOIN region r ON r.id = u.region_id
            WHERE u.national_id = '" . $db->escape($nationalId) . "'
            LIMIT 1";
    $res = $db->query($sql);
    return $res ? $res->fetch_assoc() : null;
}

function normalizeSupportRole($role) {
    $role = strtoupper(trim((string)$role));
    if ($role === 'ADMIN' || $role === 'SUPERVISOR' || $role === 'EXECUTIVE') {
        return $role;
    }
    return 'USER';
}

function getSupportLevel($user) {
    $role = normalizeSupportRole($user['roles'] ?? '');
    if ($role === 'ADMIN') {
        return 'headquarters';
    }
    $regionId = (string)($user['region_id'] ?? '');
    if ($regionId !== '' && substr($regionId, -2) === '00') {
        return 'province';
    }
    return 'region';
}

function canAccessSupportTicket($ticket, $user) {
    $role = normalizeSupportRole($user['roles'] ?? '');
    $nationalId = (string)($user['national_id'] ?? '');

    if ((string)$ticket['requester_national_id'] === $nationalId) {
        return true;
    }
    if ($role === 'ADMIN') {
        return true;
    }
    if ($role !== 'SUPERVISOR' && $role !== 'EXECUTIVE') {
        return false;
    }
    if ((string)$ticket['target_role'] !== $role) {
        return false;
    }

    $userRegionId = (string)($user['region_id'] ?? '');
    $ticketRegionId = (string)($ticket['requester_region_id'] ?? '');
    if ($userRegionId !== '' && substr($userRegionId, -2) === '00') {
        return (int)($ticket['requester_province_code'] ?? -1) === (int)($user['provinceCode'] ?? -2);
    }

    return $ticketRegionId !== '' && $ticketRegionId === $userRegionId;
}

function buildSupportTicketWhere($db, $user) {
    $role = normalizeSupportRole($user['roles'] ?? '');
    $nationalId = $db->escape($user['national_id'] ?? '');

    if ($role === 'ADMIN') {
        return "1=1";
    }

    $own = "t.requester_national_id = '{$nationalId}'";
    if ($role !== 'SUPERVISOR' && $role !== 'EXECUTIVE') {
        return $own;
    }

    $roleSql = $db->escape($role);
    $regionId = (int)($user['region_id'] ?? 0);
    if ($regionId > 0 && substr((string)$regionId, -2) === '00') {
        $provinceCode = (int)($user['provinceCode'] ?? 0);
        $staff = "(t.target_role = '{$roleSql}' AND t.requester_province_code = {$provinceCode})";
    } else {
        $staff = "(t.target_role = '{$roleSql}' AND t.requester_region_id = {$regionId})";
    }

    return "({$own} OR {$staff})";
}

function supportRoleLabel($role) {
    $labels = [
        'ADMIN' => 'ستاد/ادمین',
        'SUPERVISOR' => 'نظارت',
        'EXECUTIVE' => 'اجرایی',
        'USER' => 'کاربر'
    ];
    return $labels[$role] ?? $role;
}

function supportLevelLabel($level) {
    $labels = [
        'headquarters' => 'ستاد',
        'province' => 'استان',
        'region' => 'منطقه'
    ];
    return $labels[$level] ?? $level;
}
