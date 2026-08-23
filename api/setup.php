<?php
/**
 * یک‌بار اجرا کنید: https://yourdomain.com/api/setup.php
 * بعد از اجرا این فایل را حذف کنید!
 */
require_once __DIR__ . '/config.php';

try {
    $pdo = new PDO(
        'mysql:host=' . DB_HOST . ';dbname=' . DB_NAME . ';charset=utf8mb4',
        DB_USER, DB_PASS,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );

    $hash = hash('sha256', '1234' . 'tt-salt-2024');

    $stmt = $pdo->prepare(
        'INSERT IGNORE INTO users (username, password_hash, role) VALUES (?,?,?)'
    );
    $stmt->execute(['admin', $hash, 'admin']);

    if ($stmt->rowCount() > 0) {
        echo '<p style="color:green">✓ کاربر admin با رمز 1234 ساخته شد.</p>';
    } else {
        echo '<p style="color:orange">⚠ کاربر admin از قبل وجود داشت.</p>';
    }
    echo '<p><strong>این فایل را از سرور حذف کنید!</strong></p>';

} catch (Exception $e) {
    echo '<p style="color:red">خطا: ' . htmlspecialchars($e->getMessage()) . '</p>';
}
