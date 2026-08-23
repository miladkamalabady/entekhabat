<?php
// اجرا: http://localhost/apihoghogh/migrate.php
// پس از اجرا این فایل را حذف کنید

require_once __DIR__ . '/db.php';

$db = getDB();

// ===== جدول قراردادهای استخدام =====
$db->exec("
    CREATE TABLE IF NOT EXISTS contracts (
        id               INT AUTO_INCREMENT PRIMARY KEY,
        contract_number  VARCHAR(50)  NOT NULL,
        national_code    VARCHAR(20)  NOT NULL,
        number           VARCHAR(50)  DEFAULT '',
        title            VARCHAR(200) DEFAULT '',
        work_action      VARCHAR(100) DEFAULT '',
        workplace        VARCHAR(200) DEFAULT '',
        job              VARCHAR(100) DEFAULT '',
        date1            VARCHAR(20)  DEFAULT '',
        date2            VARCHAR(20)  DEFAULT '',
        date3            VARCHAR(20)  DEFAULT '',
        date4            VARCHAR(20)  DEFAULT '',
        first_name       VARCHAR(100) DEFAULT '',
        last_name        VARCHAR(100) DEFAULT '',
        work_stage       TINYINT      DEFAULT 0,
        status           VARCHAR(20)  DEFAULT 'active',
        created_at       TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

$db->exec("
    CREATE TABLE IF NOT EXISTS contract_status (
        id                       INT AUTO_INCREMENT PRIMARY KEY,
        contract_id              INT          NOT NULL,
        include_in_insurance     TINYINT      DEFAULT 0,
        start_year               VARCHAR(10)  DEFAULT '',
        start_month              VARCHAR(10)  DEFAULT '',
        start_status             VARCHAR(20)  DEFAULT 'pending',
        end_day                  VARCHAR(10)  DEFAULT '',
        end_month                VARCHAR(10)  DEFAULT '',
        end_year                 VARCHAR(10)  DEFAULT '',
        include_end_in_insurance TINYINT      DEFAULT 0,
        end_insurance_year       VARCHAR(10)  DEFAULT '',
        end_insurance_month      VARCHAR(10)  DEFAULT '',
        end_status               VARCHAR(20)  DEFAULT 'pending',
        created_at               TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== جدول مدارس =====
$db->exec("
    CREATE TABLE IF NOT EXISTS schools (
        id   INT AUTO_INCREMENT PRIMARY KEY,
        name VARCHAR(200) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== جدول پرسنل =====
$db->exec("
    CREATE TABLE IF NOT EXISTS personnel (
        id                      INT AUTO_INCREMENT PRIMARY KEY,
        detail_code             VARCHAR(50)  DEFAULT '',
        national_code           VARCHAR(20)  NOT NULL UNIQUE,
        first_name              VARCHAR(100) NOT NULL,
        last_name               VARCHAR(100) NOT NULL,
        father_name             VARCHAR(100) DEFAULT '',
        mobile                  VARCHAR(20)  DEFAULT '',
        gender                  VARCHAR(10)  DEFAULT 'male',
        school_id               INT          DEFAULT 0,
        school_staff            VARCHAR(20)  DEFAULT 'teacher',
        salary_row_number       VARCHAR(50)  DEFAULT '',
        insurance_number        VARCHAR(50)  DEFAULT '',
        birth_certificate_number VARCHAR(50) DEFAULT '',
        birth_date              VARCHAR(20)  DEFAULT '',
        issue_date              VARCHAR(20)  DEFAULT '',
        tax_status              VARCHAR(100) DEFAULT '',
        employee_status         VARCHAR(100) DEFAULT 'ایران',
        insurance_type          VARCHAR(20)  DEFAULT 'normal',
        has_tax_history         TINYINT      DEFAULT 0,
        show_in_tax_file        TINYINT      DEFAULT 0,
        address                 TEXT         DEFAULT '',
        postal_code             VARCHAR(20)  DEFAULT '',
        bank                    VARCHAR(20)  DEFAULT 'melli',
        industry                VARCHAR(100) DEFAULT '',
        connection              VARCHAR(200) DEFAULT '',
        created_at              TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== جدول انواع پرداختی =====
$db->exec("
    CREATE TABLE IF NOT EXISTS payment_types (
        code  INT          PRIMARY KEY,
        title VARCHAR(200) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== جدول قراردادهای پرسنل (ویزارد) =====
$db->exec("
    CREATE TABLE IF NOT EXISTS personnel_contracts (
        id                                INT AUTO_INCREMENT PRIMARY KEY,
        contract_number                   VARCHAR(50)  NOT NULL UNIQUE,
        personnel_id                      INT          NOT NULL,
        workplace                         VARCHAR(200) DEFAULT '',
        contract_type                     VARCHAR(20)  DEFAULT 'normal',
        workplace_status                  VARCHAR(20)  DEFAULT 'vertical',
        employment_type                   VARCHAR(20)  DEFAULT 'contract',
        include_job_classification        TINYINT      DEFAULT 0,
        experience_days                   INT          DEFAULT 0,
        experience_months                 INT          DEFAULT 0,
        years_of_service                  INT          DEFAULT 0,
        major                             VARCHAR(100) DEFAULT '',
        education                         VARCHAR(100) DEFAULT '',
        contract_date                     VARCHAR(20)  DEFAULT '',
        start_date                        VARCHAR(20)  DEFAULT '',
        end_date                          VARCHAR(20)  DEFAULT '',
        description                       TEXT,
        job_classification                VARCHAR(50)  DEFAULT '',
        job_title                         VARCHAR(100) DEFAULT '',
        job_class                         INT          DEFAULT 0,
        job_group                         INT          DEFAULT 1,
        hazardous_job                     TINYINT      DEFAULT 0,
        tax_job_title                     VARCHAR(100) DEFAULT '',
        tax_type                          VARCHAR(20)  DEFAULT 'full',
        insurance_job_title               VARCHAR(100) DEFAULT '',
        has_insurance                     TINYINT      DEFAULT 0,
        employer_insurance_percent        DECIMAL(5,2) DEFAULT 0,
        unified_employer_insurance_percent DECIMAL(5,2) DEFAULT 0,
        personnel_insurance_percent       DECIMAL(5,2) DEFAULT 0,
        daily_base_salary                 VARCHAR(50)  DEFAULT '',
        per_child_amount                  VARCHAR(50)  DEFAULT '',
        is_married                        VARCHAR(5)   DEFAULT 'no',
        shift_type                        VARCHAR(20)  DEFAULT 'fixed',
        holiday_overtime_rate             VARCHAR(50)  DEFAULT '',
        shayana_overtime_rate             VARCHAR(50)  DEFAULT '',
        hormozgan_special                 TINYINT      DEFAULT 0,
        overtime_taxable                  TINYINT      DEFAULT 0,
        monthly_bonus                     TINYINT      DEFAULT 0,
        monthly_leave                     TINYINT      DEFAULT 0,
        monthly_severance                 TINYINT      DEFAULT 0,
        hag_shogh                         DECIMAL(15,2) DEFAULT 0,
        hag_shaghel                       DECIMAL(15,2) DEFAULT 0,
        hourly_rate                       DECIMAL(15,2) DEFAULT 0,
        created_at                        TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (personnel_id) REFERENCES personnel(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// اگر جدول از قبل وجود دارد، ستون‌های جدید را اضافه کن
$cols = $db->query("SHOW COLUMNS FROM personnel_contracts LIKE 'hag_shogh'")->fetchAll();
if (empty($cols)) {
    $db->exec("ALTER TABLE personnel_contracts
        ADD COLUMN hag_shogh    DECIMAL(15,2) DEFAULT 0,
        ADD COLUMN hag_shaghel  DECIMAL(15,2) DEFAULT 0,
        ADD COLUMN hourly_rate  DECIMAL(15,2) DEFAULT 0");
}

// ===== جدول سایر پرداختی‌های قرارداد =====
$db->exec("
    CREATE TABLE IF NOT EXISTS contract_payments (
        id             INT AUTO_INCREMENT PRIMARY KEY,
        contract_id    INT          NOT NULL,
        payment_code   INT          NOT NULL,
        daily_amount   DECIMAL(15,2) DEFAULT 0,
        monthly_amount DECIMAL(15,2) DEFAULT 0,
        FOREIGN KEY (contract_id) REFERENCES personnel_contracts(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== گسترش جدول payment_types =====
$payColCheck = $db->query("SHOW COLUMNS FROM payment_types LIKE 'account'")->fetchAll();
if (empty($payColCheck)) {
    $db->exec("ALTER TABLE payment_types
        ADD COLUMN account             VARCHAR(50)   DEFAULT '',
        ADD COLUMN sub_code            VARCHAR(100)  DEFAULT '',
        ADD COLUMN payment_type        VARCHAR(50)   DEFAULT 'حقوق پایه',
        ADD COLUMN cash_type           VARCHAR(50)   DEFAULT 'نقدی',
        ADD COLUMN continuity_type1    VARCHAR(50)   DEFAULT 'حقوق پایه',
        ADD COLUMN continuity_type2    VARCHAR(50)   DEFAULT 'حقوق پایه',
        ADD COLUMN insurance           TINYINT       DEFAULT 1,
        ADD COLUMN tax                 TINYINT       DEFAULT 1,
        ADD COLUMN taxable             TINYINT       DEFAULT 0,
        ADD COLUMN fixed_monthly       TINYINT       DEFAULT 0,
        ADD COLUMN is_benefit          TINYINT       DEFAULT 0,
        ADD COLUMN show_in_salary_list TINYINT       DEFAULT 0,
        ADD COLUMN tax_exempt_percent  DECIMAL(5,2)  DEFAULT 0.00");
}

// ===== جدول انواع کسورات =====
$db->exec("
    CREATE TABLE IF NOT EXISTS deduction_types (
        id              INT AUTO_INCREMENT PRIMARY KEY,
        code            INT           DEFAULT 0,
        name            VARCHAR(200)  NOT NULL,
        account_code       VARCHAR(50)   DEFAULT '',
        deduction_type     VARCHAR(100)  DEFAULT '',
        default_amount     DECIMAL(15,2) DEFAULT 0,
        account            VARCHAR(100)  DEFAULT '',
        is_creditor        TINYINT       DEFAULT 0,
        show_in_salary_list TINYINT      DEFAULT 0
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// اضافه کردن ستون‌های جدید به جدول موجود
$dedColCheck = $db->query("SHOW COLUMNS FROM deduction_types LIKE 'is_creditor'")->fetchAll();
if (empty($dedColCheck)) {
    $db->exec("ALTER TABLE deduction_types
        ADD COLUMN is_creditor         TINYINT DEFAULT 0,
        ADD COLUMN show_in_salary_list TINYINT DEFAULT 0");
}

// داده نمونه کسورات
if ((int)$db->query("SELECT COUNT(*) FROM deduction_types")->fetchColumn() === 0) {
    $deductions = [
        [201, 'بیمه سهم کارمند',    '2110101', 'بیمه',   0,        ''],
        [202, 'مالیات',             '2110201', 'مالیات', 0,        ''],
        [203, 'وام مسکن',           '2110301', 'وام',    5000000,  ''],
        [204, 'غیبت',               '2110401', 'غیبت',   0,        ''],
        [205, 'مساعده',             '2110501', 'مساعده', 0,        ''],
    ];
    $dStmt = $db->prepare(
        "INSERT INTO deduction_types (code, name, account_code, deduction_type, default_amount, account)
         VALUES (:code, :name, :ac, :dt, :da, :acc)"
    );
    foreach ($deductions as $d) {
        $dStmt->execute([':code'=>$d[0],':name'=>$d[1],':ac'=>$d[2],
                         ':dt'=>$d[3],':da'=>$d[4],':acc'=>$d[5]]);
    }
}

// ===== داده‌های نمونه =====

// مدارس
if ((int)$db->query("SELECT COUNT(*) FROM schools")->fetchColumn() === 0) {
    $schools = [
        'مدرسه نمونه دولتی امام خمینی',
        'مدرسه شاهد شهید فهمیده',
        'مدرسه تیزهوشان علامه حلی',
        'مدرسه غیرانتفاعی مهر',
        'مدرسه عادی دکتر حسابی',
    ];
    $sStmt = $db->prepare("INSERT INTO schools (name) VALUES (:name)");
    foreach ($schools as $name) $sStmt->execute([':name' => $name]);
}

// انواع پرداختی
if ((int)$db->query("SELECT COUNT(*) FROM payment_types")->fetchColumn() === 0) {
    $types = [
        [109, 'کمک هزینه مصرف خانوار (خوار و بار)'],
        [110, 'حق مسکن'],
        [112, 'پاداش عادی'],
        [118, 'کلاس جبرانی'],
        [119, 'حق ایاب و ذهاب'],
        [120, 'فوق العاده ویژه'],
        [121, 'فوق العاده سوابق و تجارب'],
        [122, 'فوق العاده جذب'],
        [123, 'فوق العاده مدرک تحصیلی'],
        [124, 'فوق العاده مقطع'],
        [125, 'حق مدیریت، مسئولیت'],
        [126, 'کارانه'],
        [127, 'کمک رفاهی'],
        [128, 'معلمان برتر'],
        [129, 'حق تاهل'],
    ];
    $tStmt = $db->prepare("INSERT INTO payment_types (code, title) VALUES (:code, :title)");
    foreach ($types as $t) $tStmt->execute([':code' => $t[0], ':title' => $t[1]]);
}

// پرسنل نمونه
if ((int)$db->query("SELECT COUNT(*) FROM personnel")->fetchColumn() === 0) {
    $pStmt = $db->prepare("
        INSERT INTO personnel (detail_code, national_code, first_name, last_name,
            father_name, mobile, gender, school_id, salary_row_number)
        VALUES (:dc, :nc, :fn, :ln, :fa, :mob, :gen, :sch, :sal)
    ");
    $samples = [
        ['167','3251205846','آروز',  'جلیلیان',             'حسین','09376256178','male',1,''],
        ['150','3392642845','کریمی', 'آنسی',                'محمد','09391417125','female',2,''],
        ['157','3391598522','نیمه کاری','آنسی',             'حسین','09378123002','male',3,''],
        ['138','3391840547','چاشنی پور','آنسی',             'حسین','09171599514','female',1,''],
        ['174','6169892941','امیری', 'آنسی',                'شیرخان','09186168082','male',2,''],
        ['264','0945912188','پور ثانی','محمدحسین',          'علی', '09121234567','male',1,'264'],
        ['262','4231554315','مهودی مطلق','شهرام',           'حسن', '09122345678','male',2,'262'],
        ['263','4670101366','نجفی حاجی وند','ابوذر',        'رضا', '09123456789','male',3,'263'],
    ];
    foreach ($samples as $s) {
        $pStmt->execute([':dc'=>$s[0],':nc'=>$s[1],':fn'=>$s[2],':ln'=>$s[3],
            ':fa'=>$s[4],':mob'=>$s[5],':gen'=>$s[6],':sch'=>$s[7],':sal'=>$s[8]]);
    }
}

// قراردادهای نمونه
if ((int)$db->query("SELECT COUNT(*) FROM contracts")->fetchColumn() === 0) {
    $cStmt = $db->prepare("
        INSERT INTO contracts
            (contract_number, national_code, number, title, work_action,
             workplace, job, date1, date2, date3, date4,
             first_name, last_name, work_stage, status)
        VALUES (:cn,:nc,:num,:title,:wa,:wp,:job,:d1,:d2,:d3,:d4,:fn,:ln,:ws,:st)
    ");
    $cs = [
        ['10375','0945912188','264','1404/10/28','موسسه','مدرسه رازی',  'مشاور','1404/12/30','1404/10/10','1404/10/10','1404/10/01','محمدحسین','پور ثانی',     1,'active'],
        ['10373','4231554315','262','1404/10/28','موسسه','مدرسه امام',   'مشاور','1404/12/30','1404/10/10','1404/10/10','1404/10/01','شهرام',   'مهودی مطلق',  1,'active'],
        ['10374','4670101366','263','1404/10/28','موسسه','مدرسه فارابی','معلم', '1404/12/30','1404/10/10','1404/10/10','1404/10/01','ابوذر',    'نجفی حاجی وند',2,'active'],
    ];
    foreach ($cs as $s) {
        $cStmt->execute([':cn'=>$s[0],':nc'=>$s[1],':num'=>$s[2],':title'=>$s[3],
            ':wa'=>$s[4],':wp'=>$s[5],':job'=>$s[6],':d1'=>$s[7],':d2'=>$s[8],
            ':d3'=>$s[9],':d4'=>$s[10],':fn'=>$s[11],':ln'=>$s[12],':ws'=>$s[13],':st'=>$s[14]]);
    }
}

// ===== ۳ نمونه قرارداد — یکی برای هر نوع استخدام =====
// بررسی دقیق وجود هر شماره قرارداد (نه COUNT کل)
$existPc = $db->query(
    "SELECT contract_number FROM personnel_contracts
     WHERE contract_number IN ('10375','10373','10374')"
)->fetchAll(PDO::FETCH_COLUMN);

$toInsert = array_diff(['10375','10373','10374'], $existPc);

if (!empty($toInsert)) {
    $p1 = $db->query("SELECT id FROM personnel WHERE national_code='0945912188'")->fetch();
    $p2 = $db->query("SELECT id FROM personnel WHERE national_code='4231554315'")->fetch();
    $p3 = $db->query("SELECT id FROM personnel WHERE national_code='4670101366'")->fetch();

    if ($p1 && $p2 && $p3) {
        $pcStmt = $db->prepare("
            INSERT INTO personnel_contracts (
                contract_number, personnel_id,
                workplace, contract_type, employment_type,
                job_title, tax_type, has_insurance,
                daily_base_salary, is_married, shift_type,
                hag_shogh, hag_shaghel, hourly_rate,
                contract_date, start_date, end_date
            ) VALUES (
                :cn, :pid, :wp, :ct, :et,
                :jt, 'full', 0,
                :dbs, 'no', 'fixed',
                :hs, :hsh, :hr,
                :cd, :sd, :ed
            )
        ");

        $samples = [
            '10375' => [
                ':cn'=>'10375', ':pid'=>$p1['id'],
                ':wp'=>'مدرسه نمونه دولتی امام خمینی',
                ':ct'=>'permanent', ':et'=>'labor_law', ':jt'=>'معلم',
                ':dbs'=>'2500000', ':hs'=>0, ':hsh'=>0, ':hr'=>0,
                ':cd'=>'1404/07/01', ':sd'=>'1404/07/01', ':ed'=>'1404/12/29',
            ],
            '10373' => [
                ':cn'=>'10373', ':pid'=>$p2['id'],
                ':wp'=>'مدرسه شاهد شهید فهمیده',
                ':ct'=>'temporary', ':et'=>'decree_teaching', ':jt'=>'مشاور',
                ':dbs'=>'0', ':hs'=>18000000, ':hsh'=>12000000, ':hr'=>0,
                ':cd'=>'1404/07/01', ':sd'=>'1404/07/01', ':ed'=>'1404/12/29',
            ],
            '10374' => [
                ':cn'=>'10374', ':pid'=>$p3['id'],
                ':wp'=>'مدرسه تیزهوشان علامه حلی',
                ':ct'=>'temporary', ':et'=>'hourly_teaching', ':jt'=>'معلم',
                ':dbs'=>'0', ':hs'=>0, ':hsh'=>0, ':hr'=>850000,
                ':cd'=>'1404/07/01', ':sd'=>'1404/07/01', ':ed'=>'1404/12/29',
            ],
        ];

        foreach ($toInsert as $cn) {
            $pcStmt->execute($samples[$cn]);
            $newPcId = (int)$db->lastInsertId();

            // پرداختی‌های نمونه فقط برای قرارداد قانون کار
            if ($cn === '10375') {
                $payStmt = $db->prepare(
                    "INSERT INTO contract_payments (contract_id, payment_code, daily_amount, monthly_amount)
                     VALUES (:cid, :code, :da, :ma)"
                );
                foreach ([[109,0,2000000],[110,0,4000000],[119,0,1500000]] as $sp) {
                    $payStmt->execute([':cid'=>$newPcId,':code'=>$sp[0],':da'=>$sp[1],':ma'=>$sp[2]]);
                }
            }
        }
    }
}

// ===== جدول پارامترهای حقوق =====
$db->exec("
    CREATE TABLE IF NOT EXISTS salary_params (
        param_key   VARCHAR(50) PRIMARY KEY,
        param_value TEXT        NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== جدول فیش‌های حقوقی =====
$db->exec("
    CREATE TABLE IF NOT EXISTS salary_slips (
        id               INT AUTO_INCREMENT PRIMARY KEY,
        personnel_id     INT           NOT NULL,
        contract_id      INT           NOT NULL,
        year             VARCHAR(10)   NOT NULL,
        month            INT           NOT NULL,
        base_salary      DECIMAL(15,2) DEFAULT 0,
        extra_payments   DECIMAL(15,2) DEFAULT 0,
        overtime_hours   DECIMAL(8,2)  DEFAULT 0,
        overtime_amount  DECIMAL(15,2) DEFAULT 0,
        gross_salary     DECIMAL(15,2) DEFAULT 0,
        insurance        DECIMAL(15,2) DEFAULT 0,
        tax              DECIMAL(15,2) DEFAULT 0,
        total_deductions DECIMAL(15,2) DEFAULT 0,
        net_salary       DECIMAL(15,2) DEFAULT 0,
        created_at       TIMESTAMP     DEFAULT CURRENT_TIMESTAMP,
        UNIQUE KEY uq_slip (personnel_id, year, month),
        FOREIGN KEY (personnel_id) REFERENCES personnel(id)           ON DELETE CASCADE,
        FOREIGN KEY (contract_id)  REFERENCES personnel_contracts(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// اضافه کردن ستون‌های اضافه کاری به جدول موجود salary_slips
$otColCheck = $db->query("SHOW COLUMNS FROM salary_slips LIKE 'overtime_hours'")->fetchAll();
if (empty($otColCheck)) {
    $db->exec("ALTER TABLE salary_slips
        ADD COLUMN overtime_hours  DECIMAL(8,2)  DEFAULT 0,
        ADD COLUMN overtime_amount DECIMAL(15,2) DEFAULT 0");
}

// ===== جدول اضافه کاری پرسنل =====
$db->exec("
    CREATE TABLE IF NOT EXISTS overtime_records (
        id           INT AUTO_INCREMENT PRIMARY KEY,
        personnel_id INT           NOT NULL,
        year         VARCHAR(10)   NOT NULL,
        month        INT           NOT NULL,
        hours        DECIMAL(8,2)  DEFAULT 0,
        amount       DECIMAL(15,2) DEFAULT 0,
        description  TEXT,
        created_at   TIMESTAMP     DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (personnel_id) REFERENCES personnel(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

// ===== جدول روزکاری پرسنل =====
$db->exec("
    CREATE TABLE IF NOT EXISTS day_jobs (
        id           INT AUTO_INCREMENT PRIMARY KEY,
        personnel_id INT         NOT NULL,
        year         VARCHAR(10) NOT NULL,
        month        INT         NOT NULL,
        working_days INT         DEFAULT 0,
        created_at   TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
        updated_at   TIMESTAMP   DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        UNIQUE KEY uq_day_job (personnel_id, year, month),
        FOREIGN KEY (personnel_id) REFERENCES personnel(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");

echo json_encode([
    'status'  => true,
    'message' => 'همه جداول با موفقیت ساخته شدند'
], JSON_UNESCAPED_UNICODE);
