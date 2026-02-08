<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

session_start();
$key = 'vote_' . $nationalId;
if (!isset($_SESSION[$key])) $_SESSION[$key] = 0;
$_SESSION[$key]++;
if ($_SESSION[$key] > 10) {
    http_response_code(429);
    echo json_encode(['status' => false, 'message' => 'درخواست بیش از حد']);
    exit;
}

$rating = isset($input['rating']) ? (int)$input['rating'] : 0;
$comment = isset($input['comment']) ? trim($input['comment']) : '';

if (!$rating) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'پارامتر الزامی است.'], JSON_UNESCAPED_UNICODE);
    exit;
}


try {
    $participant = $db->query("
    SELECT id FROM feedback
    WHERE national_id='{$nationalId}'
    LIMIT 1
");
    $mess = 'نظر شما با موفقیت ویرایش شد.';
    if (!$participant->num_rows) {
        $mess = 'نظر شما با موفقیت ثبت شد.';
        $sql = ("INSERT INTO feedback (national_id, rating, comment) VALUES ('{$nationalId}',{$rating}, '{$comment}')");
    } else
        $sql = ("update feedback set rating={$rating}, comment='{$comment}' where national_id='{$nationalId}'");
    $res = $db->query($sql);
/* کامیت تراکنش */
$db->query("COMMIT");

    echo json_encode([
        'status' => true,
        'data' => $mess
    ], JSON_UNESCAPED_UNICODE);
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'status' => false,
        'data' => 'خطا در ثبت نظر',
        'error' => $e->getMessage()
    ], JSON_UNESCAPED_UNICODE);
}
