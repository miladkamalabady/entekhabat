<?php

function setCorsHeaders() {
    header('Access-Control-Allow-Origin: *');
    header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
    header('Access-Control-Allow-Headers: Content-Type, Authorization');
    header('Content-Type: application/json; charset=utf-8');

    if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
        http_response_code(204);
        exit;
    }
}

function jsonResponse($data, int $status = 200) {
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE);
    exit;
}

function errorResponse(string $message, int $status = 400) {
    jsonResponse(['status' => false, 'message' => $message], $status);
}

function getRequestBody(): array {
    $body = file_get_contents('php://input');
    if (empty($body)) {
        return $_REQUEST;
    }
    $data = json_decode($body, true);
    return is_array($data) ? $data : $_REQUEST;
}

function getQueryParam(string $key, $default = null) {
    return isset($_GET[$key]) ? $_GET[$key] : $default;
}

function requireMethod(string $method) {
    if ($_SERVER['REQUEST_METHOD'] !== strtoupper($method)) {
        errorResponse('Method not allowed', 405);
    }
}
