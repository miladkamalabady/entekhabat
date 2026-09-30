<?php
require_once __DIR__ . '/config.php';

// ── CORS ──
header('Access-Control-Allow-Origin: ' . ALLOWED_ORIGIN);
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204);
    exit;
}

// ── Database ──
function db(): PDO {
    static $pdo = null;
    if (!$pdo) {
        $pdo = new PDO(
            'mysql:host=' . DB_HOST . ';dbname=' . DB_NAME . ';charset=utf8mb4',
            DB_USER, DB_PASS,
            [
                PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
                PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            ]
        );
    }
    return $pdo;
}

// ── Helpers ──
function body(): array {
    return json_decode(file_get_contents('php://input'), true) ?? [];
}

function ok($data, int $code = 200): void {
    http_response_code($code);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function err(string $msg, int $code = 400): void {
    http_response_code($code);
    echo json_encode(['error' => $msg], JSON_UNESCAPED_UNICODE);
    exit;
}

// ── Auth ──
function hash_pwd(string $pwd): string {
    return hash('sha256', $pwd . PWD_SALT);
}

function make_token(): string {
    return bin2hex(random_bytes(32));
}

function get_bearer_token(): string {
    $auth = '';
    if (!empty($_SERVER['HTTP_AUTHORIZATION'])) {
        $auth = $_SERVER['HTTP_AUTHORIZATION'];
    } elseif (function_exists('apache_request_headers')) {
        $headers = apache_request_headers();
        $auth = $headers['Authorization'] ?? $headers['authorization'] ?? '';
    }
    return trim(str_replace('Bearer', '', $auth));
}

function auth_user(): ?array {
    $token = get_bearer_token();
    if (!$token) return null;
    $stmt = db()->prepare(
        'SELECT u.id, u.username, u.role
         FROM sessions s JOIN users u ON s.user_id = u.id
         WHERE s.token = ? AND s.expires_at > NOW()'
    );
    $stmt->execute([$token]);
    return $stmt->fetch() ?: null;
}

function require_auth(): array {
    $user = auth_user();
    if (!$user) err('Unauthorized', 401);
    return $user;
}

function auth_for_writes(): ?array {
    if ($_SERVER['REQUEST_METHOD'] === 'GET') return null;
    return require_auth();
}

function require_admin(): array {
    $user = require_auth();
    if ($user['role'] !== 'admin') err('Forbidden', 403);
    return $user;
}

// ── Settings table helpers ──
function get_setting(string $key, $default = null) {
    $stmt = db()->prepare('SELECT v FROM settings WHERE k = ?');
    $stmt->execute([$key]);
    $row = $stmt->fetch();
    return $row ? json_decode($row['v'], true) : $default;
}

function set_setting(string $key, $value): void {
    $v = json_encode($value, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    $stmt = db()->prepare(
        'INSERT INTO settings (k,v) VALUES (?,?) ON DUPLICATE KEY UPDATE v=?'
    );
    $stmt->execute([$key, $v, $v]);
}

// ── Row → object (BIGINT tables) ──
function to_obj(array $row): array {
    $data = json_decode($row['json_data'], true);
    $data['id'] = (int)$row['id'];
    return $data;
}

// ── CRUD handler (publications / journal_issues / events / team_members) ──
function handle_crud(string $method, ?string $id, string $table): void {
    auth_for_writes();

    if ($method === 'GET') {
        $order = ($table === 'journal_issues') ? 'ASC' : 'DESC';
        $rows = db()->query("SELECT id, json_data FROM $table ORDER BY id $order")->fetchAll();
        ok(array_map('to_obj', $rows));
    }

    if ($method === 'POST') {
        $data = body();
        $newId = isset($data['id']) ? (int)$data['id'] : (int)(microtime(true) * 1000);
        unset($data['id']);
        $json = json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        db()->prepare("INSERT INTO $table (id, json_data) VALUES (?,?)")->execute([$newId, $json]);
        ok(array_merge(['id' => $newId], $data), 201);
    }

    if ($method === 'PUT') {
        if (!$id) err('ID required');
        $data = body();
        unset($data['id']);
        $json = json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        db()->prepare("UPDATE $table SET json_data=? WHERE id=?")->execute([$json, (int)$id]);
        ok(array_merge(['id' => (int)$id], $data));
    }

    if ($method === 'DELETE') {
        if (!$id) err('ID required');
        db()->prepare("DELETE FROM $table WHERE id=?")->execute([(int)$id]);
        ok(['ok' => true]);
    }

    err('Method not allowed', 405);
}
