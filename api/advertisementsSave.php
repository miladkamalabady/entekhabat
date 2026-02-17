<?php
require_once 'database.php';
require_once 'readToken.php';
require_once 'jdf.php';
date_default_timezone_set('Asia/Tehran');

function jalaliScheduleToTimestamp($dateStr)
{
    list($date,$time) = explode(' ',$dateStr);
    list($jy,$jm,$jd) = explode('-',$date);

    // تبدیل جلالی → میلادی
    list($gy,$gm,$gd) = jalali_to_gregorian($jy,$jm,$jd);

    return strtotime("$gy-$gm-$gd $time");
}
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    $db->query("ROLLBACK");
    http_response_code(405);
    echo json_encode([
        'status' => false,
        'message' => 'فقط درخواست POST مجاز است.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// شروع مهلت قانونی تبلیغات
$resStart = $db->query("SELECT start_date FROM election_schedule_events WHERE event_key='ads_upload_start' LIMIT 1");
$adsStart = jalaliScheduleToTimestamp($resStart->fetch_assoc()['start_date']);
$adsEndMax7Days = $adsStart + (7 * 24 * 60 * 60); // 7 روز بعد
// زمان شروع رأی‌گیری
$resVoting = $db->query("SELECT start_date FROM election_schedule_events WHERE event_key='voting' LIMIT 1");
$votingStart = jalaliScheduleToTimestamp($resVoting->fetch_assoc()['start_date']);
$adsEndBeforeVoting = $votingStart - (24 * 60 * 60); // 24 ساعت قبل از شروع رأی‌گیری
$adsEnd = min($adsEndMax7Days, $adsEndBeforeVoting);
$now = time();
if($now < $adsStart || $now > $adsEnd){
    $db->query("ROLLBACK");
    http_response_code(403);
    echo json_encode([
        'status'=>false,
        'message'=>'امکان ارسال تبلیغ خارج از بازه قانونی وجود ندارد.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}


$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

$input = [];
if (!empty($_POST)) {
    $input = $_POST;
} else {
    $input = json_decode(file_get_contents('php://input'), true) ?: [];
}

$id = isset($input['id']) ? intval($input['id']) : 0;
$title = isset($input['title']) ? trim($input['title']) : '';
$description = isset($input['description']) ? trim($input['description']) : '';
$type = isset($input['type']) ? trim($input['type']) : '';
$status = isset($input['status']) ? trim($input['status']) : '';
$targetLink = isset($input['targetLink']) ? trim($input['targetLink']) : '';
$imagePath = isset($input['imagePath']) ? trim($input['imagePath']) : '';
$managerialRecords = isset($input['managerialRecords']) ? trim($input['managerialRecords']) : '';
$academicRecords = isset($input['academicRecords']) ? trim($input['academicRecords']) : '';
$honors = isset($input['honors']) ? trim($input['honors']) : '';
$plans = isset($input['plans']) ? trim($input['plans']) : '';
$slogan = isset($input['slogan']) ? trim($input['slogan']) : '';

if ($title === '' || $description === '' || $type === '' || $status === '') {
    $db->query("ROLLBACK");
    http_response_code(400);
    echo json_encode([
        'status' => false,
        'message' => 'تمام فیلدهای ضروری باید ارسال شوند.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$storedImagePath = $imagePath !== '' ? $imagePath : null;

if (isset($_FILES['image']) && $_FILES['image']['error'] !== UPLOAD_ERR_NO_FILE) {
    if ($_FILES['image']['error'] !== UPLOAD_ERR_OK) {
        $db->query("ROLLBACK");
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'خطا در آپلود تصویر.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    if ($_FILES['image']['size'] > 2 * 1024 * 1024) {
        $db->query("ROLLBACK");
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'حجم تصویر نباید بیشتر از 2 مگابایت باشد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $finfo = new finfo(FILEINFO_MIME_TYPE);
    $tmpName = $_FILES['image']['tmp_name'];
    $mimeType = $finfo->file($tmpName);
    $allowed = ['image/jpeg', 'image/png', 'image/jpg'];

    if (!in_array($mimeType, $allowed, true)) {
        $db->query("ROLLBACK");
        http_response_code(400);
        echo json_encode([
            'status' => false,
            'message' => 'فرمت تصویر مجاز نیست.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $baseUploadDir = __DIR__ . '/uploads/advertisements';
    if (!is_dir($baseUploadDir) && !mkdir($baseUploadDir, 0775, true)) {
        $db->query("ROLLBACK");
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'ایجاد پوشه آپلود ممکن نیست.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $originalName = $_FILES['image']['name'];
    $extension = strtolower(pathinfo($originalName, PATHINFO_EXTENSION));
    if ($extension === '') {
        $extension = $mimeType === 'image/png' ? 'png' : 'jpg';
    }

    $fileName = 'ad_' . date('Ymd_His') . '_' . bin2hex(random_bytes(4)) . '.' . $extension;
    $serverPath = $baseUploadDir . '/' . $fileName;
    $relativePath = 'uploads/advertisements/' . $fileName;

    if (!move_uploaded_file($tmpName, $serverPath)) {
        $db->query("ROLLBACK");
        http_response_code(500);
        echo json_encode([
            'status' => false,
            'message' => 'ذخیره تصویر روی سرور انجام نشد.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $storedImagePath = $relativePath;
}

$titleSql = $db->escape($title);
$descriptionSql = $db->escape($description);
$typeSql = $db->escape($type);
$statusSql = $db->escape($status);
$targetLinkSql = $db->escape($targetLink);
$imageSql = $storedImagePath !== null ? "'" . $db->escape($storedImagePath) . "'" : "NULL";
$managerialRecords = $db->escape($managerialRecords);
$academicRecords = $db->escape($academicRecords);
$honors = $db->escape($honors);
$plans = $db->escape($plans);
$slogan = $db->escape($slogan);

$db->query("INSERT INTO advertisements (id,nationalId, title, description, type,image, target_link, status,managerialRecords,academicRecords,honors,plans,slogan)
VALUES ($id,'{$nationalId}', '{$titleSql}', '{$descriptionSql}', '{$typeSql}', {$imageSql}, '{$targetLinkSql}', '{$statusSql}', '{$managerialRecords}', '{$academicRecords}', '{$honors}', '{$plans}', '{$slogan}')
ON DUPLICATE KEY UPDATE
title = VALUES(title),
description = VALUES(description),
type = VALUES(type),
image = IF(VALUES(image) IS NULL, image, VALUES(image)),
target_link = VALUES(target_link),
managerialRecords = VALUES(managerialRecords),academicRecords = VALUES(academicRecords),honors = VALUES(honors),plans = VALUES(plans),slogan = VALUES(slogan),
status = VALUES(status)");

$insertId = $id > 0 ? $id : $db->insert_id(null);
$db->query("COMMIT");
echo json_encode([
    'status' => true,
    'message' => 'تبلیغ با موفقیت ذخیره شد.',
    'data' => [
        'id' => $insertId,
        'image' => $storedImagePath
    ]
], JSON_UNESCAPED_UNICODE);