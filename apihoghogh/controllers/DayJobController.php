<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class DayJobController {

    // GET /DayJob/GetList?year=1404&monthNumber=10
    // لیست پرسنل برای تنظیم روزکاری در ماه خاص
    public function getList() {
        requireMethod('GET');

        $year  = trim(getQueryParam('year') ?? '');
        $month = (int)(getQueryParam('monthNumber') ?? 0);

        if (!$year || !$month) {
            errorResponse('سال و ماه الزامی است');
        }

        $db = getDB();

        $stmt = $db->prepare("
            SELECT
                p.id,
                p.detail_code,
                p.first_name,
                p.last_name,
                p.national_code,
                s.name AS school_name,
                s.id AS school_id,
                COALESCE(dj.working_days, 0) AS working_days,
                CASE WHEN dj.id IS NOT NULL THEN 1 ELSE 0 END AS registered
            FROM personnel p
            LEFT JOIN schools s ON s.id = p.school_id
            LEFT JOIN day_jobs dj ON dj.personnel_id = p.id AND dj.year = :year AND dj.month = :month
            ORDER BY s.name, p.last_name, p.first_name
        ");

        $stmt->execute([':year' => $year, ':month' => $month]);
        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // GET /DayJob/GetSchools
    // لیست مدارس برای فیلتر
    public function getSchools() {
        requireMethod('GET');

        $db = getDB();
        $stmt = $db->query("
            SELECT DISTINCT s.id, s.name
            FROM schools s
            ORDER BY s.name
        ");

        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // POST /DayJob/SaveBatch
    // body: { year, monthNumber, items: [{personnelId, workingDays}, ...] }
    public function saveBatch() {
        requireMethod('POST');

        $body  = getRequestBody();
        $year  = trim($body['year'] ?? '');
        $month = (int)($body['monthNumber'] ?? 0);
        $items = $body['items'] ?? [];

        error_log('SaveBatch called: year=' . $year . ', month=' . $month . ', items_count=' . count($items));

        if (!$year) errorResponse('سال الزامی است');
        if (!$month) errorResponse('ماه الزامی است');
        if (!is_array($items) || count($items) === 0) {
            errorResponse('لیست پرسنل الزامی است');
        }

        $db = getDB();

        try {
            $stmt = $db->prepare("
                INSERT INTO day_jobs (personnel_id, year, month, working_days)
                VALUES (:pid, :year, :month, :days)
                ON DUPLICATE KEY UPDATE working_days = VALUES(working_days)
            ");

            $count = 0;
            foreach ($items as $item) {
                $pid  = (int)($item['personnelId'] ?? 0);
                $days = (int)($item['workingDays'] ?? 0);

                error_log('Processing: pid=' . $pid . ', days=' . $days);

                if ($pid <= 0) continue;

                // اگر روزکاری تنظیم نشده بود، حداکثر 30 یا 31 روز
                if ($days <= 0) {
                    $days = $this->daysInMonth($month);
                }

                // محدود کردن به بیشترین روزهای ماه
                $maxDays = $this->daysInMonth($month);
                $days = min($days, $maxDays);

                error_log('Executing insert: pid=' . $pid . ', year=' . $year . ', month=' . $month . ', days=' . $days);

                $stmt->execute([
                    ':pid'   => $pid,
                    ':year'  => $year,
                    ':month' => $month,
                    ':days'  => $days
                ]);
                $count++;
            }
        } catch (Exception $e) {
            error_log('SaveBatch Error: ' . $e->getMessage());
            throw $e;
        }

        jsonResponse([
            'status'  => true,
            'message' => "$count پرسنل ثبت/بروزرسانی شد",
            'count'   => $count
        ]);
    }

    // POST /DayJob/Save
    // body: { personnelId, year, monthNumber, workingDays }
    public function save() {
        requireMethod('POST');

        $body = getRequestBody();
        $pid  = (int)($body['personnelId'] ?? 0);
        $year = trim($body['year'] ?? '');
        $month = (int)($body['monthNumber'] ?? 0);
        $days = (int)($body['workingDays'] ?? 0);

        if (!$pid) errorResponse('پرسنل الزامی است');
        if (!$year) errorResponse('سال الزامی است');
        if (!$month) errorResponse('ماه الزامی است');

        $db = getDB();

        // اگر روزکاری تنظیم نشده بود، حداکثر 30 یا 31 روز
        if ($days <= 0) {
            $days = $this->daysInMonth($month);
        }

        // محدود کردن به بیشترین روزهای ماه
        $maxDays = $this->daysInMonth($month);
        $days = min($days, $maxDays);

        $stmt = $db->prepare("
            INSERT INTO day_jobs (personnel_id, year, month, working_days)
            VALUES (:pid, :year, :month, :days)
            ON DUPLICATE KEY UPDATE working_days = VALUES(working_days)
        ");

        $stmt->execute([
            ':pid'   => $pid,
            ':year'  => $year,
            ':month' => $month,
            ':days'  => $days
        ]);

        jsonResponse([
            'status'  => true,
            'message' => 'روزکاری ثبت شد',
            'data'    => ['id' => (int)$db->lastInsertId()]
        ]);
    }

    // DELETE /DayJob/Delete
    // body: { personnelId, year, monthNumber }
    public function delete() {
        requireMethod('DELETE');

        $body  = getRequestBody();
        $pid   = (int)($body['personnelId'] ?? 0);
        $year  = trim($body['year'] ?? '');
        $month = (int)($body['monthNumber'] ?? 0);

        if (!$pid) errorResponse('پرسنل الزامی است');
        if (!$year) errorResponse('سال الزامی است');
        if (!$month) errorResponse('ماه الزامی است');

        $db = getDB();
        $db->prepare("
            DELETE FROM day_jobs
            WHERE personnel_id = :pid AND year = :year AND month = :month
        ")->execute([':pid' => $pid, ':year' => $year, ':month' => $month]);

        jsonResponse(['status' => true, 'message' => 'روزکاری حذف شد']);
    }

    private function daysInMonth($month) {
        $monthDays = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29]; // تقویم شمسی
        return isset($monthDays[$month - 1]) ? $monthDays[$month - 1] : 31;
    }
}
