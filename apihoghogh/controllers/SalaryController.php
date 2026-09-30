<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';
require_once __DIR__ . '/../jdf.php';

class SalaryController {

    private $persianMonths = [
        1=>'فروردین',2=>'اردیبهشت',3=>'خرداد',4=>'تیر',5=>'مرداد',6=>'شهریور',
        7=>'مهر',8=>'آبان',9=>'آذر',10=>'دی',11=>'بهمن',12=>'اسفند',
    ];

    // POST /Salary/Rebuild
    public function rebuild() {
        requireMethod('POST');

        $body  = getRequestBody();
        $year  = trim($body['year']        ?? '');
        $month = (int)($body['monthNumber'] ?? 0);

        if (!$year || !$month) errorResponse('سال و ماه الزامی است');

        $db = getDB();

        $monthName = $this->persianMonths[$month] ?? '';

        // بازه ماه به صورت عدد صحیح برای مقایسه مطمئن (مستقل از zero-padding)
        $firstInt = (int)$year * 10000 + $month * 100 + 1;
        $lastInt  = (int)$year * 10000 + $month * 100 + $this->daysInMonth($month);

        $allContracts = $db->query("
            SELECT pc.*,
                   p.id           AS p_id,
                   p.first_name,
                   p.last_name,
                   cp_sum.extra_payments
            FROM personnel_contracts pc
            JOIN personnel p ON p.id = pc.personnel_id
            LEFT JOIN (
                SELECT contract_id, SUM(monthly_amount) AS extra_payments
                FROM contract_payments
                GROUP BY contract_id
            ) cp_sum ON cp_sum.contract_id = pc.id
        ")->fetchAll();

        // فیلتر در PHP: قراردادی که بازه‌اش با ماه درخواستی همپوشانی داشته باشد
        $contracts = array_filter($allContracts, function($c) use ($firstInt, $lastInt) {
            $startInt = $this->dateToInt($c['start_date'] ?? '');
            $endInt   = $this->dateToInt($c['end_date']   ?? '');
            if ($startInt === 0 || $endInt === 0) return false;
            return $startInt <= $lastInt && $endInt >= $firstInt;
        });

        $daysInMonth = $this->daysInMonth($month);

        // پاک کردن فیش‌های قبلی همین ماه — بازسازی کامل از صفر
        $db->prepare("DELETE FROM salary_slips WHERE year = :year AND month_number = :month")
           ->execute([':year' => $year, ':month' => $month]);

        $count = 0;

        $stmt = $db->prepare("
            INSERT INTO salary_slips
                (personnel_id, contract_id, year, month_number, month_name,
                 base_amount, total_payments,
                 overtime_hours, overtime_amount,
                 custom_deductions,
                 insurance_deduction, tax_deduction,
                 total_deductions, net_salary)
            VALUES
                (:pid, :cid, :year, :month, :month_name,
                 :base, :gross,
                 :ot_hours, :ot_amount,
                 :custom_ded,
                 :ins, :tax,
                 :ded, :net)
        ");

        $otStmt = $db->prepare("
            SELECT COALESCE(SUM(hours), 0) AS hrs, COALESCE(SUM(amount), 0) AS amt
            FROM overtime_records
            WHERE personnel_id = :pid AND year = :year AND month = :month
        ");

        // کسورات اختصاصی پرسنل
        $customDedStmt = $db->prepare("
            SELECT * FROM personnel_deductions
            WHERE personnel_id = :pid AND status = 'active'
        ");

        // پرداختی ویژه
        $spStmt = $db->prepare("
            SELECT COALESCE(SUM(amount), 0) AS total
            FROM special_payments
            WHERE personnel_id = :pid AND year = :year AND month = :month
        ");

        $currentInt = (int)$year * 100 + $month;

        // بارگذاری روزهای کاری برای تمامی پرسنل از جدول day_jobs
        $dayJobStmt = $db->prepare("
            SELECT personnel_id, working_days
            FROM day_jobs
            WHERE year = :year AND month = :month
        ");
        $dayJobStmt->execute([':year' => $year, ':month' => $month]);
        $dayJobsByPersonnel = [];
        foreach ($dayJobStmt->fetchAll() as $dj) {
            $dayJobsByPersonnel[(int)$dj['personnel_id']] = (int)$dj['working_days'];
        }

        foreach ($contracts as $c) {
            $pid   = (int)$c['p_id'];
            // اگر روزکاری ثبت شده بود، استفاده کن، وگرنه حداکثر روز ماه
            $workingDays = $dayJobsByPersonnel[$pid] ?? $daysInMonth;
            $base  = $this->calcBase($c, $workingDays);
            $extra = (float)($c['extra_payments'] ?? 0);

            // اضافه کاری
            $otStmt->execute([':pid' => $pid, ':year' => $year, ':month' => $month]);
            $ot       = $otStmt->fetch();
            $otHours  = (float)($ot['hrs'] ?? 0);
            $otAmount = (float)($ot['amt'] ?? 0);

            // کسورات اختصاصی
            $customDedStmt->execute([':pid' => $pid]);
            $customDeds    = $customDedStmt->fetchAll();
            $customDedTotal = 0;

            foreach ($customDeds as $ded) {
                $startInt = (int)$ded['start_year'] * 100 + (int)$ded['start_month'];
                if ($currentInt < $startInt) continue;

                $dedAmt = (float)$ded['amount'];

                if ($ded['end_type'] === 'untilDate') {
                    if ($ded['end_year'] && $ded['end_month']) {
                        $endInt = (int)$ded['end_year'] * 100 + (int)$ded['end_month'];
                        if ($currentInt > $endInt) continue;
                    }
                }

                if ($ded['end_type'] === 'untilAmount') {
                    $maxAmt       = (float)$ded['max_amount'];
                    $totalDeducted = (float)$ded['total_deducted'];
                    if ($totalDeducted >= $maxAmt) continue;
                    $dedAmt = min($dedAmt, $maxAmt - $totalDeducted);
                    // ثبت مبلغ کسر شده
                    $db->prepare("UPDATE personnel_deductions SET total_deducted = total_deducted + :amt WHERE id = :id")
                       ->execute([':amt' => $dedAmt, ':id' => $ded['id']]);
                }

                $customDedTotal += $dedAmt;
            }

            // پرداختی ویژه این پرسنل برای این ماه
            $spStmt->execute([':pid' => $pid, ':year' => $year, ':month' => $month]);
            $spTotal = (float)($spStmt->fetchColumn() ?? 0);

            $gross = $base + $extra + $otAmount + $spTotal;
            $ins   = $c['has_insurance'] ? round($gross * (float)($c['personnel_insurance_percent'] ?? 7) / 100) : 0;
            $tax   = $this->calcTax($gross);
            $ded   = $ins + $tax + $customDedTotal;

            $stmt->execute([
                ':pid'        => $pid,
                ':cid'        => (int)$c['id'],
                ':year'       => $year,
                ':month'      => $month,
                ':month_name' => $monthName,
                ':base'       => $base,
                ':gross'      => $gross,
                ':ot_hours'   => $otHours,
                ':ot_amount'  => $otAmount,
                ':custom_ded' => $customDedTotal,
                ':ins'        => $ins,
                ':tax'        => $tax,
                ':ded'        => $ded,
                ':net'        => $gross - $ded,
            ]);
            $count++;
        }

        $debugContracts = array_map(function($c) {
            return [
                'id'         => $c['id'],
                'start_date' => $c['start_date'],
                'end_date'   => $c['end_date'],
                'start_int'  => $this->dateToInt($c['start_date'] ?? ''),
                'end_int'    => $this->dateToInt($c['end_date']   ?? ''),
            ];
        }, $allContracts);

        jsonResponse([
            'status'   => true,
            'inserted' => $count,
            'message'  => $count . ' فیش حقوقی ساخته/بروزرسانی شد',
            'debug'    => [
                'year'               => $year,
                'month'              => $month,
                'firstInt'           => $firstInt,
                'lastInt'            => $lastInt,
                'total_contracts'    => count($allContracts),
                'filtered_contracts' => count($contracts),
                'contracts'          => $debugContracts,
            ],
        ]);
    }

    // DELETE /Salary/DeleteProcessing?year=1405&monthNumber=2
    public function deleteProcessing() {
        requireMethod('DELETE');

        $year  = trim(getQueryParam('year')        ?? '');
        $month = (int)(getQueryParam('monthNumber') ?? 0);

        if (!$year || !$month) errorResponse('سال و ماه الزامی است');

        $db   = getDB();
        $stmt = $db->prepare("DELETE FROM salary_slips WHERE year = :year AND month_number = :month");
        $stmt->execute([':year' => $year, ':month' => $month]);

        jsonResponse([
            'status'  => true,
            'deleted' => $stmt->rowCount(),
            'message' => 'پردازش حقوق حذف شد',
        ]);
    }

    // GET /Salary/GetSlips?year=1405&monthNumber=2
    public function getSlips() {
        requireMethod('GET');

        $year  = trim(getQueryParam('year')        ?? '');
        $month = (int)(getQueryParam('monthNumber') ?? 0);

        if (!$year || !$month) errorResponse('سال و ماه الزامی است');

        $db = getDB();

        $schoolId    = (int)(getQueryParam('schoolId') ?? 0);
        $schoolWhere = $schoolId > 0 ? ' AND p.school_id = :school_id' : '';

        $stmt = $db->prepare("
            SELECT
                ss.id,
                ss.personnel_id,
                ss.year,
                ss.month_number,
                ss.month_name,
                ss.contract_id,
                ss.base_amount                                            AS base_salary,
                (ss.total_payments - ss.base_amount - ss.overtime_amount) AS extra_payments,
                ss.overtime_hours,
                ss.overtime_amount,
                ss.custom_deductions,
                ss.total_payments                                         AS gross_salary,
                ss.insurance_deduction                                    AS insurance,
                ss.tax_deduction                                          AS tax,
                ss.total_deductions,
                ss.net_salary,
                ss.status,
                p.first_name, p.last_name, p.national_code, p.bank,
                pc.workplace, pc.job_title, pc.employment_type,
                s.name AS school_name
            FROM salary_slips ss
            JOIN personnel p ON p.id = ss.personnel_id
            LEFT JOIN personnel_contracts pc ON pc.id = ss.contract_id
            LEFT JOIN schools s ON s.id = p.school_id
            WHERE ss.year = :year AND ss.month_number = :month$schoolWhere
            ORDER BY p.last_name, p.first_name
        ");

        $params = [':year' => $year, ':month' => $month];
        if ($schoolId > 0) $params[':school_id'] = $schoolId;
        $stmt->execute($params);
        $slips = $stmt->fetchAll();

        // بارگذاری آیتم‌های پرداختی هر قرارداد با عنوان واقعی
        $contractIds = array_values(array_unique(array_filter(
            array_column($slips, 'contract_id')
        )));

        $paymentsByContract = [];
        if (!empty($contractIds)) {
            $ph = implode(',', array_fill(0, count($contractIds), '?'));
            $payStmt = $db->prepare("
                SELECT cp.contract_id, pt.title, cp.monthly_amount
                FROM contract_payments cp
                JOIN payment_types pt ON pt.code = cp.payment_code
                WHERE cp.contract_id IN ($ph) AND cp.monthly_amount > 0
                ORDER BY cp.contract_id, pt.title
            ");
            $payStmt->execute($contractIds);
            foreach ($payStmt->fetchAll() as $p) {
                $paymentsByContract[(int)$p['contract_id']][] = [
                    'title'  => $p['title'],
                    'amount' => (float)$p['monthly_amount'],
                ];
            }
        }

        // بارگذاری کسورات اختصاصی هر پرسنل با عنوان دقیق
        $personnelIds = array_values(array_unique(array_filter(
            array_column($slips, 'personnel_id')
        )));

        $customDedByPersonnel = [];
        if (!empty($personnelIds)) {
            $ph2 = implode(',', array_fill(0, count($personnelIds), '?'));
            $dedStmt = $db->prepare("
                SELECT personnel_id, deduction_name, amount,
                       start_year, start_month, end_type, end_year, end_month,
                       max_amount, total_deducted
                FROM personnel_deductions
                WHERE personnel_id IN ($ph2) AND status = 'active'
            ");
            $dedStmt->execute($personnelIds);

            $currentInt = (int)$year * 100 + $month;

            foreach ($dedStmt->fetchAll() as $ded) {
                $startInt = (int)$ded['start_year'] * 100 + (int)$ded['start_month'];
                if ($currentInt < $startInt) continue;

                if ($ded['end_type'] === 'untilDate' && $ded['end_year'] && $ded['end_month']) {
                    if ($currentInt > (int)$ded['end_year'] * 100 + (int)$ded['end_month']) continue;
                }
                if ($ded['end_type'] === 'untilAmount') {
                    if ((float)$ded['total_deducted'] >= (float)$ded['max_amount']) continue;
                }

                $pid = (int)$ded['personnel_id'];
                $customDedByPersonnel[$pid][] = [
                    'name'   => $ded['deduction_name'],
                    'amount' => (float)$ded['amount'],
                ];
            }
        }

        // بارگذاری پرداختی‌های ویژه
        $specialByPersonnel = [];
        if (!empty($personnelIds)) {
            $ph3 = implode(',', array_fill(0, count($personnelIds), '?'));
            $spStmt2 = $db->prepare("
                SELECT personnel_id, payment_name, payment_code, amount
                FROM special_payments
                WHERE personnel_id IN ($ph3) AND year = ? AND month = ?
                ORDER BY payment_name
            ");
            $spStmt2->execute(array_merge($personnelIds, [$year, $month]));
            foreach ($spStmt2->fetchAll() as $sp) {
                $specialByPersonnel[(int)$sp['personnel_id']][] = [
                    'name'   => $sp['payment_name'],
                    'code'   => (int)$sp['payment_code'],
                    'amount' => (float)$sp['amount'],
                ];
            }
        }

        foreach ($slips as &$slip) {
            $slip['payment_items']          = $paymentsByContract[(int)$slip['contract_id']] ?? [];
            $slip['custom_deduction_items'] = $customDedByPersonnel[(int)$slip['personnel_id']] ?? [];
            $slip['special_payment_items']  = $specialByPersonnel[(int)$slip['personnel_id']]  ?? [];

            // محاسبه مجدد با مقادیر زنده
            $liveCustom   = array_sum(array_column($slip['custom_deduction_items'], 'amount'));
            $liveSpecial  = array_sum(array_column($slip['special_payment_items'],  'amount'));
            $slip['custom_deductions'] = $liveCustom;
            $slip['gross_salary']      = (float)($slip['base_salary']   ?? 0)
                                       + (float)($slip['extra_payments'] ?? 0)
                                       + (float)($slip['overtime_amount'] ?? 0)
                                       + $liveSpecial;
            $slip['total_deductions']  = (float)($slip['insurance'] ?? 0)
                                       + (float)($slip['tax']       ?? 0)
                                       + $liveCustom;
            $slip['net_salary']        = $slip['gross_salary'] - $slip['total_deductions'];
        }

        jsonResponse(['status' => true, 'data' => $slips]);
    }

    // GET /Salary/GetAnnualSlips?year=1404
    public function getAnnualSlips() {
        requireMethod('GET');

        $year = trim(getQueryParam('year') ?? '');
        if (!$year) errorResponse('سال الزامی است');

        $db = getDB();
        $stmt = $db->prepare("
            SELECT
                ss.personnel_id,
                ss.month_number,
                ss.month_name,
                ss.base_amount        AS base_salary,
                ss.total_payments     AS gross_salary,
                ss.insurance_deduction AS insurance,
                ss.tax_deduction      AS tax,
                ss.total_deductions,
                ss.net_salary,
                ss.overtime_amount,
                p.first_name, p.last_name, p.national_code,
                s.name AS school_name
            FROM salary_slips ss
            JOIN personnel p ON p.id = ss.personnel_id
            LEFT JOIN schools s ON s.id = p.school_id
            WHERE ss.year = :year
            ORDER BY ss.month_number, p.last_name, p.first_name
        ");
        $stmt->execute([':year' => $year]);
        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // تبدیل "1404/07/01" یا "1404/7/1" به عدد 14040701
    private function dateToInt(string $date): int {
        $parts = array_map('intval', explode('/', trim($date)));
        if (count($parts) !== 3 || $parts[0] < 1300) return 0;
        return $parts[0] * 10000 + $parts[1] * 100 + $parts[2];
    }

    private function daysInMonth(int $month): int {
        if ($month <= 6)  return 31;
        if ($month <= 11) return 30;
        return 29;
    }

    private function calcBase(array $c, int $days): float {
        $type = $c['employment_type'] ?? 'labor_law';
        if ($type === 'labor_law')       return (float)($c['daily_base_salary'] ?? 0) * $days;
        if ($type === 'decree_teaching') return (float)($c['hag_shogh'] ?? 0) + (float)($c['hag_shaghel'] ?? 0);
        if ($type === 'hourly_teaching') return (float)($c['hourly_rate'] ?? 0) * 160;
        return 0;
    }

    private function calcTax(float $gross): float {
        $taxable = max(0, $gross - 14000000);
        return $taxable > 0 ? round($taxable * 0.10) : 0;
    }
}
