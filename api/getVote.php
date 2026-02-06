<?php
require_once 'database.php';
require_once 'readToken.php';
header('Content-Type: application/json; charset=utf-8');
$input = json_decode(file_get_contents('php://input'), true);
$db->connect();

/* =========================
   3. Query status
========================= */

$sql = "SELECT *  FROM voters WHERE usernationalid = '{$nationalId}'";

$res = $db->query($sql);
$row = $res->fetch_assoc();

echo json_encode([
    'status' => true,
    'message' => 'دریافت رای با موفقیت انجام شد.',
    'data' => $row
], JSON_UNESCAPED_UNICODE);