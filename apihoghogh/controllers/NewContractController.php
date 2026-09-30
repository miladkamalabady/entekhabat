<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';
require_once __DIR__ . '/../jdf.php';

class NewContractController {

    // GET /NewContract/GetPersonnelList  — لیست ساده برای dropdown
    public function getPersonnelList() {
        requireMethod('GET');

        $stmt = getDB()->query(
            "SELECT id, first_name, last_name, national_code
             FROM personnel ORDER BY last_name, first_name"
        );
        $rows = $stmt->fetchAll();

        jsonResponse(['status' => true, 'data' => $rows]);
    }

    // GET /NewContract/GetPaymentTypes  — عناوین و کدهای آیتم‌های سایر پرداختی‌ها
    public function getPaymentTypes() {
        requireMethod('GET');

        $stmt = getDB()->query("SELECT code, title FROM payment_types ORDER BY code");
        $rows = $stmt->fetchAll();

        jsonResponse(['status' => true, 'data' => $rows]);
    }

    // GET /NewContract/GetNextContractNumber  — شماره قرارداد بعدی
    public function getNextContractNumber() {
        requireMethod('GET');

        $row = getDB()->query(
            "SELECT MAX(CAST(contract_number AS UNSIGNED)) AS max_num FROM personnel_contracts"
        )->fetch();

        $next = ($row['max_num'] ?? 10000) + 1;

        jsonResponse(['status' => true, 'data' => ['contractNumber' => (string)$next]]);
    }

    // GET /NewContract/GetDetails?id=X  — بارگذاری قرارداد برای ویرایش/چاپ
    // id = شناسه از جدول contracts (لیست قراردادهای استخدام)
    public function getDetails() {
        requireMethod('GET');

        $id = (int) getQueryParam('id');
        if (!$id) errorResponse('شناسه قرارداد الزامی است');

        $db = getDB();

        // بارگذاری اطلاعات اصلی از جدول contracts
        $stmt = $db->prepare("SELECT * FROM contracts WHERE id = :id");
        $stmt->execute([':id' => $id]);
        $contract = $stmt->fetch();
        if (!$contract) errorResponse('قرارداد یافت نشد', 404);

        $contractNumber = $contract['contract_number'];

        // بارگذاری اطلاعات تفصیلی از personnel_contracts (اگر موجود باشد)
        $stmt = $db->prepare(
            "SELECT pc.*, p.first_name, p.last_name, p.national_code
             FROM personnel_contracts pc
             JOIN personnel p ON p.id = pc.personnel_id
             WHERE pc.contract_number = :cn"
        );
        $stmt->execute([':cn' => $contractNumber]);
        $pc = $stmt->fetch();

        // اگر رکورد تفصیلی نداشت، اطلاعات پایه را از contracts برمی‌گردانیم
        if (!$pc) {
            $pc = [
                'id'              => null,
                'contract_number' => $contractNumber,
                'personnel_id'    => null,
                'first_name'      => $contract['first_name'],
                'last_name'       => $contract['last_name'],
                'national_code'   => $contract['national_code'],
                'workplace'       => $contract['workplace'],
                'employment_type' => $contract['work_action'],
                'job_title'       => $contract['job'],
                'contract_type'   => 'permanent',
                'contract_date'   => $contract['date2'],
                'start_date'      => $contract['date3'],
                'end_date'        => $contract['date1'],
                'description'     => '',
                'daily_base_salary'   => '',
                'hag_shogh'       => 0,
                'hag_shaghel'     => 0,
                'hourly_rate'     => 0,
                'shift_type'      => 'fixed',
                'is_married'      => 'no',
            ];
        }

        // بارگذاری سایر پرداختی‌ها
        $payments = [];
        if ($pc['id']) {
            $stmt = $db->prepare(
                "SELECT payment_code AS code, daily_amount, monthly_amount
                 FROM contract_payments WHERE contract_id = :cid"
            );
            $stmt->execute([':cid' => $pc['id']]);
            $payments = $stmt->fetchAll();
        }

        jsonResponse([
            'status'   => true,
            'data'     => $pc,
            'payments' => $payments,
        ]);
    }

    // POST /NewContract/Save  — ذخیره کامل قرارداد (همه 4 مرحله)
    public function save() {
        requireMethod('POST');

        $body = getRequestBody();

        $personnelId = (int) ($body['personnelId'] ?? 0);
        if (!$personnelId) errorResponse('انتخاب پرسنل الزامی است');

        $db = getDB();

        // بررسی وجود پرسنل
        $chk = $db->prepare("SELECT id FROM personnel WHERE id = :id");
        $chk->execute([':id' => $personnelId]);
        if (!$chk->fetch()) errorResponse('پرسنل یافت نشد', 404);

        // بررسی قرارداد فعال — اگر end_date >= امروز باشد، قرارداد فعال است
        $today = jdate('Y/m/d');
        $activeStmt = $db->prepare("
            SELECT contract_number, end_date
            FROM personnel_contracts
            WHERE personnel_id = :pid AND end_date >= :today
            LIMIT 1
        ");
        $activeStmt->execute([':pid' => $personnelId, ':today' => $today]);
        $activeContract = $activeStmt->fetch();
        if ($activeContract) {
            errorResponse(
                'این پرسنل دارای قرارداد فعال شماره ' . $activeContract['contract_number'] .
                ' تا تاریخ ' . $activeContract['end_date'] . ' می‌باشد'
            );
        }

        // گرفتن شماره قرارداد بعدی
        $row = $db->query(
            "SELECT MAX(CAST(contract_number AS UNSIGNED)) AS max_num FROM personnel_contracts"
        )->fetch();
        $contractNumber = (string)(($row['max_num'] ?? 10000) + 1);

        // ساخت رشته تاریخ از آرایه  {year, month, day}
        $joinDate = function($d) {
            if (is_array($d)) return ($d['year']??'') . '/' . ($d['month']??'') . '/' . ($d['day']??'');
            return is_string($d) ? $d : '';
        };

        // --- مرحله ۱: اطلاعات پرسنل/قرارداد ---
        $p = $body['personnel'] ?? [];

        // --- مرحله ۲: شغل ---
        $j = $body['job'] ?? [];

        // --- مرحله ۳: پارامترهای حقوق ---
        $pr = $body['params'] ?? [];

        $stmt = $db->prepare(
            "INSERT INTO personnel_contracts (
                contract_number, personnel_id,
                workplace, contract_type, workplace_status, employment_type,
                include_job_classification, experience_days, experience_months, years_of_service,
                major, education, contract_date, start_date, end_date, description,
                job_classification, job_title, job_class, job_group, hazardous_job,
                tax_job_title, tax_type,
                insurance_job_title, has_insurance,
                employer_insurance_percent, unified_employer_insurance_percent, personnel_insurance_percent,
                daily_base_salary, per_child_amount, is_married, shift_type,
                holiday_overtime_rate, shayana_overtime_rate,
                hormozgan_special, overtime_taxable, monthly_bonus, monthly_leave, monthly_severance,
                hag_shogh, hag_shaghel, hourly_rate
            ) VALUES (
                :contract_number, :personnel_id,
                :workplace, :contract_type, :workplace_status, :employment_type,
                :include_job_classification, :experience_days, :experience_months, :years_of_service,
                :major, :education, :contract_date, :start_date, :end_date, :description,
                :job_classification, :job_title, :job_class, :job_group, :hazardous_job,
                :tax_job_title, :tax_type,
                :insurance_job_title, :has_insurance,
                :employer_insurance_percent, :unified_employer_insurance_percent, :personnel_insurance_percent,
                :daily_base_salary, :per_child_amount, :is_married, :shift_type,
                :holiday_overtime_rate, :shayana_overtime_rate,
                :hormozgan_special, :overtime_taxable, :monthly_bonus, :monthly_leave, :monthly_severance,
                :hag_shogh, :hag_shaghel, :hourly_rate
            )"
        );

        $stmt->execute([
            ':contract_number'                    => $contractNumber,
            ':personnel_id'                       => $personnelId,
            ':workplace'                          => $p['workplace']                  ?? '',
            ':contract_type'                      => $p['contractType']               ?? 'normal',
            ':workplace_status'                   => $p['workplaceStatus']            ?? 'vertical',
            ':employment_type'                    => $p['employmentType']             ?? 'labor_law',
            ':include_job_classification'         => (int)($p['includeJobClassification'] ?? 0),
            ':experience_days'                    => (int)($p['experienceDays']       ?? 0),
            ':experience_months'                  => (int)($p['experienceMonths']     ?? 0),
            ':years_of_service'                   => (int)($p['yearsOfService']       ?? 0),
            ':major'                              => $p['major']                      ?? '',
            ':education'                          => $p['education']                  ?? '',
            ':contract_date'                      => $joinDate($p['contractDate']     ?? ''),
            ':start_date'                         => $joinDate($p['startDate']        ?? ''),
            ':end_date'                           => $joinDate($p['endDate']          ?? ''),
            ':description'                        => $p['description']                ?? '',
            ':job_classification'                 => $j['jobClassification']          ?? '',
            ':job_title'                          => $j['jobTitle']                   ?? '',
            ':job_class'                          => (int)($j['jobClass']             ?? 0),
            ':job_group'                          => (int)($j['jobGroup']             ?? 1),
            ':hazardous_job'                      => (int)($j['hazardousJob']         ?? 0),
            ':tax_job_title'                      => $j['taxJobTitle']                ?? '',
            ':tax_type'                           => $j['taxType']                    ?? 'full',
            ':insurance_job_title'                => $j['insuranceJobTitle']          ?? '',
            ':has_insurance'                      => (int)($j['hasInsurance']         ?? 0),
            ':employer_insurance_percent'         => (float)($j['employerInsurancePercent']        ?? 0),
            ':unified_employer_insurance_percent' => (float)($j['unifiedEmployerInsurancePercent'] ?? 0),
            ':personnel_insurance_percent'        => (float)($j['personnelInsurancePercent']       ?? 0),
            ':daily_base_salary'                  => $pr['dailyBaseSalary']           ?? '',
            ':per_child_amount'                   => $pr['perChildAmount']            ?? '',
            ':is_married'                         => $pr['isMarried']                 ?? 'no',
            ':shift_type'                         => $pr['shiftType']                 ?? 'fixed',
            ':holiday_overtime_rate'              => $pr['holidayOvertimeRate']       ?? '',
            ':shayana_overtime_rate'              => $pr['shayanaOvertimeRate']        ?? '',
            ':hormozgan_special'                  => (int)($pr['hormozganSpecial']    ?? 0),
            ':overtime_taxable'                   => (int)($pr['overtimeTaxable']     ?? 0),
            ':monthly_bonus'                      => (int)($pr['monthlyBonus']        ?? 0),
            ':monthly_leave'                      => (int)($pr['monthlyLeave']        ?? 0),
            ':monthly_severance'                  => (int)($pr['monthlySeverance']    ?? 0),
            ':hag_shogh'                          => (float)($pr['hagShogh']          ?? 0),
            ':hag_shaghel'                        => (float)($pr['hagShaghel']        ?? 0),
            ':hourly_rate'                        => (float)($pr['hourlyRate']         ?? 0),
        ]);

        $contractId = (int) $db->lastInsertId();

        // --- مرحله ۴: سایر پرداختی‌ها ---
        $payments = $body['payments'] ?? [];
        if (!empty($payments) && is_array($payments)) {
            $pStmt = $db->prepare(
                "INSERT INTO contract_payments (contract_id, payment_code, daily_amount, monthly_amount)
                 VALUES (:contract_id, :payment_code, :daily_amount, :monthly_amount)"
            );
            foreach ($payments as $pay) {
                if ((float)($pay['dailyAmount'] ?? 0) > 0 || (float)($pay['monthlyAmount'] ?? 0) > 0) {
                    $pStmt->execute([
                        ':contract_id'    => $contractId,
                        ':payment_code'   => (int)($pay['code']          ?? 0),
                        ':daily_amount'   => (float)($pay['dailyAmount']  ?? 0),
                        ':monthly_amount' => (float)($pay['monthlyAmount'] ?? 0),
                    ]);
                }
            }
        }

        // همچنین یک ردیف در جدول contracts (برای لیست قراردادهای استخدام) درج کن
        $personStmt = $db->prepare("SELECT first_name, last_name, national_code FROM personnel WHERE id = :id");
        $personStmt->execute([':id' => $personnelId]);
        $person = $personStmt->fetch();

        $db->prepare(
            "INSERT INTO contracts
                (contract_number, national_code, number, title, work_action,
                 workplace, job, date1, date2, date3, date4,
                 first_name, last_name, work_stage, status)
             VALUES
                (:cn, :nc, :num, :title, :wa, :wp, :job, :d1, :d2, :d3, :d4, :fn, :ln, :ws, :st)"
        )->execute([
            ':cn'  => $contractNumber,
            ':nc'  => $person['national_code'] ?? '',
            ':num' => $contractNumber,
            ':title' => $joinDate($p['contractDate'] ?? ''),
            ':wa'  => $p['employmentType'] ?? 'contract',
            ':wp'  => $p['workplace'] ?? '',
            ':job' => $j['jobTitle'] ?? '',
            ':d1'  => $joinDate($p['endDate']      ?? ''),
            ':d2'  => $joinDate($p['contractDate']  ?? ''),
            ':d3'  => $joinDate($p['startDate']     ?? ''),
            ':d4'  => $joinDate($p['startDate']     ?? ''),
            ':fn'  => $person['first_name'] ?? '',
            ':ln'  => $person['last_name']  ?? '',
            ':ws'  => 1,
            ':st'  => 'active',
        ]);

        jsonResponse([
            'status'          => true,
            'message'         => 'قرارداد با موفقیت ثبت شد',
            'data'            => [
                'id'             => $contractId,
                'contractNumber' => $contractNumber,
            ],
        ]);
    }
}
