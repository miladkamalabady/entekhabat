<?php
header("Content-Type: application/json; charset=utf-8");

// دریافت route از query string
$route = $_GET['route'] ?? '';

// یا از PATH_INFO استفاده کنید (بهتر است)
$path = $_SERVER['PATH_INFO'] ?? $_SERVER['REQUEST_URI'] ?? '';
if (strpos($path, '/api/') === 0) {
    $path = substr($path, 5); // حذف '/api/'
}
$path = trim($path, '/');
if (empty($route) && !empty($path)) {
    $route = $path;
}

$method = $_SERVER['REQUEST_METHOD'];
define('API_CALL', true);

require_once 'rateLimit.php';
$route = $_GET['route'] ?? '';
checkRateLimit($route, 12, 60, 30);

// لیست routeهای مجاز
$allowed_routes = [
    'AccountLogin',
    'user-status',
    'FinalSubmit',
    'getstateCandid',
    'getEXECUTIVEList',
    'ChangeState',
    'advertisementsSave',
    'getAdvertisements',
    'increaseViewAdd',
    'deleteAdv',
    'getConfig',
    'getCandidsList',
    'insertVote',
    'getVote',
    'createVoteToken',
    'getInfoVote'
];

// امنیت: فقط routeهای مجاز
if (!in_array($route, $allowed_routes)) {
    http_response_code(404);
    echo json_encode([
        "status" => false,
        "message" => "API Not Found",
        "requested_route" => $route
    ]);
    exit;
}

// بارگذاری فایل مربوطه
$file_path = __DIR__ . '/' . $route . '.php';
if (file_exists($file_path)) {
    require $file_path;
} else {
    http_response_code(404);
    echo json_encode([
        "status" => false,
        "message" => "API File Not Found: " . $route
    ]);
}
?>