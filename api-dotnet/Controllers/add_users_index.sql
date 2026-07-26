-- اجرا روی دیتابیس MySQL/MariaDB پروژه (entekhabat)
-- این ایندکس‌ها JOIN و فیلتر روی users.region_id را در getUsers سریع‌تر می‌کنند.

ALTER TABLE `users` ADD INDEX `idx_users_region_id` (`region_id`);

-- اگر جستجوی زیاد روی این ستون‌ها انجام می‌شود و حجم users بزرگ است،
-- چون کوئری از LIKE '%...%' استفاده می‌کند (wildcard دو طرفه)، ایندکس معمولی
-- کمکی نمی‌کند مگر الگوی جستجو به‌جای '%value%' به 'value%' (فقط ابتدای رشته) تغییر کند:
-- ALTER TABLE `users` ADD INDEX `idx_users_personnel_code` (`personnel_code`);
