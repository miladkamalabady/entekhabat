<?php
require_once 'database.php';
require_once 'jdf.php';

header("Access-Control-Allow-Origin: https://shababalmoqawama.com");
header("Access-Control-Allow-Methods: GET, POST, OPTIONS, PUT, DELETE");
header("Access-Control-Allow-Headers: *");

// $form_data['success'] = false;
// $form_data['posted']  = "زمان ثبت مشخصات به اتمام رسیده است. در صورت نیاز با شماره 09920126985 تماس حاصل فرمایید";

    $db->connect();
    $sql = "select * from cities where state=0 and country_id=2";
    $res0 = $db->query($sql);
    $return=array();
    while($rowco = $db->fetch_assoc($res0))
    {
        array_push($return,$rowco);
    }
    $form_data['success'] = true;
    $form_data['posted']  = $return;

    $db->disconnect();


//////////


echo json_encode($form_data);
