<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
require_once 'supportTicketHelpers.php';

header('Content-Type: application/json; charset=utf-8');

$db->connect();
ensureSupportTicketTables($db);
$user = getSupportCurrentUser($db, $nationalId);
if (!$user) {
    http_response_code(404);
    echo json_encode(['status' => false, 'message' => 'کاربر یافت نشد'], JSON_UNESCAPED_UNICODE);
    exit;
}

$where = buildSupportTicketWhere($db, $user);
$status = isset($_GET['status']) ? trim($_GET['status']) : '';
if ($status !== '' && in_array($status, ['open', 'closed', 'pending'], true)) {
    $where .= " AND t.status = '" . $db->escape($status) . "'";
}

$sql = "SELECT t.*
        FROM support_tickets t
        WHERE {$where}
        ORDER BY t.updated_at DESC, t.id DESC
        LIMIT 300";
$res = $db->query($sql);
$tickets = [];
$ids = [];
while ($row = $res->fetch_assoc()) {
    $row['conversation'] = [];
    $row['canReply'] = canAccessSupportTicket($row, $user);
    $row['targetRoleLabel'] = supportRoleLabel($row['target_role']);
    $row['supportLevelLabel'] = supportLevelLabel($row['support_level']);
    $row['date'] = jdate('Y/m/d', strtotime($row['created_at']), '', '', 'en');
    $row['lastReply'] = jdate('H:i Y/m/d', strtotime($row['updated_at']), '', '', 'en');
    $tickets[$row['id']] = $row;
    $ids[] = (int)$row['id'];
}

if (count($ids)) {
    $messageSql = "SELECT * FROM support_ticket_messages WHERE ticket_id IN (" . implode(',', $ids) . ") ORDER BY id ASC";
    $messageRes = $db->query($messageSql);
    while ($message = $messageRes->fetch_assoc()) {
        $message['text'] = $message['message'];
        $message['sender'] = ((string)$message['sender_national_id'] === (string)$user['national_id']) ? 'user' : 'support';
        $message['senderLabel'] = $message['sender_role'] === 'USER' ? 'کاربر' : supportRoleLabel($message['sender_role']);
        $message['time'] = jdate('H:i Y/m/d', strtotime($message['created_at']), '', '', 'en');
        $message['attachments'] = $message['attachments'] ? json_decode($message['attachments'], true) : [];
        $tickets[$message['ticket_id']]['conversation'][] = $message;
    }
}

$list = array_values($tickets);
$stats = [
    'total' => count($list),
    'open' => 0,
    'closed' => 0,
    'pending' => 0
];
foreach ($list as $ticket) {
    if (isset($stats[$ticket['status']])) {
        $stats[$ticket['status']]++;
    }
}

echo json_encode([
    'status' => true,
    'data' => $list,
    'stats' => $stats,
    'scope' => [
        'role' => normalizeSupportRole($user['roles'] ?? ''),
        'roleLabel' => supportRoleLabel(normalizeSupportRole($user['roles'] ?? '')),
        'level' => getSupportLevel($user),
        'levelLabel' => supportLevelLabel(getSupportLevel($user)),
        'regionId' => $user['region_id'],
        'regionName' => $user['regionName'],
        'provinceCode' => $user['provinceCode']
    ]
], JSON_UNESCAPED_UNICODE);
