<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';

header('Content-Type: application/json; charset=utf-8');

$db->connect();

$sqlUser = "SELECT roles FROM users WHERE national_id = '{$nationalId}' LIMIT 1";
$resUser = $db->query($sqlUser);
$user = $resUser ? $resUser->fetch_assoc() : null;

if (!$user || $user['roles'] !== 'ADMIN') {
    http_response_code(403);
    echo json_encode([
        'status' => false,
        'message' => 'شما دسترسی لازم را ندارید'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 200;
if ($limit <= 0) {
    $limit = 200;
}
if ($limit > 1000) {
    $limit = 1000;
}

$sql = "SELECT id, nationalId, action, description, create_date FROM logs ORDER BY id DESC LIMIT {$limit}";
$res = $db->query($sql);

$list = [];
if ($res) {
    while ($row = $res->fetch_assoc()) {
        $row['create_date_shamsi'] = jdate('H:i Y-n-j', strtotime($row['create_date']), '', '', 'en');
        $list[] = $row;
    }
}

echo json_encode([
    'status' => true,
    'data' => $list
], JSON_UNESCAPED_UNICODE);
