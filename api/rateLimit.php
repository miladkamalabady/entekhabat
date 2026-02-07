<?php
require_once 'database.php';
$db->connect();
if(!defined('API_CALL')){
    http_response_code(403);
    exit;
}

function clientIP() {

    if (!empty($_SERVER['HTTP_CF_CONNECTING_IP'])) 
        return $_SERVER['HTTP_CF_CONNECTING_IP'];

    if (!empty($_SERVER['HTTP_X_FORWARDED_FOR']))
        return explode(',', $_SERVER['HTTP_X_FORWARDED_FOR'])[0];

    return $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';
}

function checkRateLimit($route, $maxRequests = 5, $perSeconds = 60, $blockSeconds = 120){
global $db;
    $ip = clientIP();
    $now = date('Y-m-d H:i:s');

    // رکورد قبلی
    $result = $db->query("
        SELECT * FROM api_rate_limits 
        WHERE ip='{$ip}' AND route='{$route}'
        LIMIT 1
    ");
    if($row = $result->fetch_assoc()){

        // اگر بلاک است
        if($row['blocked_until'] && $row['blocked_until'] > $now){
            http_response_code(429);
            echo json_encode([
                "status"=>false,
                "message"=>"تعداد درخواست‌ها بیش از حد مجاز است. چند دقیقه دیگر تلاش کنید."
            ], JSON_UNESCAPED_UNICODE);
            exit;
        }

        $last = strtotime($row['last_request']);
        $diff = time() - $last;

        if($diff <= $perSeconds){

            $count = $row['request_count'] + 1;

            // اگر از حد گذشت → بلاک
            if($count > $maxRequests){

                $blockedUntil = date('Y-m-d H:i:s', time()+$blockSeconds);

                $update = $db->query("
                    UPDATE api_rate_limits
                    SET request_count='{$count}', blocked_until='{$blockedUntil}', last_request='{$now}'
                    WHERE id='{$row['id']}'
                ");
                http_response_code(429);
                echo json_encode([
                    "status"=>false,
                    "message"=>"به دلیل ارسال درخواست‌های مکرر، موقتاً مسدود شدید."
                ], JSON_UNESCAPED_UNICODE);
                exit;
            }

            // افزایش شمارنده
            $update = $db->query("
                UPDATE api_rate_limits
                SET request_count='{$count}', last_request='{$now}'
                WHERE id='{$row['id']}'
            ");

        } else {
            // ریست شمارنده
            $update = $db->query("
                UPDATE api_rate_limits
                SET request_count=1, last_request='{$now}', blocked_until=NULL
                WHERE id='{$row['id']}'
            ");
        }

    } else {
        // اولین درخواست
        $insert = $db->query("
            INSERT INTO api_rate_limits (ip, route, request_count, last_request)
            VALUES ('{$ip}','{$route}', 1, '{$now}')
        ");
    }
}
