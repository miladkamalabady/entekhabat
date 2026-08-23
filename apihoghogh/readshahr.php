<?php
require_once 'database.php';
require_once 'jdf.php';

header("Access-Control-Allow-Origin: https://shababalmoqawama.com");
header("Access-Control-Allow-Methods: GET, POST, OPTIONS, PUT, DELETE");
header("Access-Control-Allow-Headers: *");

// $form_data['success'] = false;
// $form_data['posted']  = "زمان ثبت مشخصات به اتمام رسیده است. در صورت نیاز با شماره 09920126985 تماس حاصل فرمایید";
$find = array("۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹");
$replace = array("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
$row = array();
foreach ($_GET as $key => $value) {
    $row[$key] = $value;
    $row[$key] = str_replace($find, $replace, $row[$key]);
}
if (isset($row['ostan'])) {
    $db->connect();
    $sql = "select * from cities where state={$row['ostan']} and country_id=2";
    $res0 = $db->query($sql);
    $return=array();
    while($rowco = $db->fetch_assoc($res0))
    {
        array_push($return,$rowco);
    }
    $form_data['success'] = true;
    $form_data['posted']  = $return;

    $db->disconnect();
}

//////////


echo json_encode($form_data);
