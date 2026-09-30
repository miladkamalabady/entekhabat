<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class PersonnelController {

    // GET /Personnel/GetList
    // params: lastName, nationalCode, schoolId, gender, page, pageSize
    public function getList() {
        requireMethod('GET');

        $page     = max(1, (int) getQueryParam('page', 1));
        $pageSize = min(100, max(1, (int) getQueryParam('pageSize', 25)));
        $offset   = ($page - 1) * $pageSize;

        $lastName     = getQueryParam('lastName');
        $nationalCode = getQueryParam('nationalCode');
        $schoolId     = getQueryParam('schoolId');
        $gender       = getQueryParam('gender');

        $where  = ['1=1'];
        $params = [];

        if (!empty($lastName)) {
            $where[]  = 'last_name LIKE :lastName';
            $params[':lastName'] = '%' . $lastName . '%';
        }
        if (!empty($nationalCode)) {
            $where[]  = 'national_code LIKE :nationalCode';
            $params[':nationalCode'] = '%' . $nationalCode . '%';
        }
        if (!empty($schoolId)) {
            $where[]  = 'school_id = :schoolId';
            $params[':schoolId'] = (int) $schoolId;
        }
        if (!empty($gender)) {
            $where[]  = 'gender = :gender';
            $params[':gender'] = $gender;
        }

        $whereSQL = implode(' AND ', $where);
        $db = getDB();

        $countStmt = $db->prepare("SELECT COUNT(*) FROM personnel WHERE $whereSQL");
        $countStmt->execute($params);
        $total = (int) $countStmt->fetchColumn();

        $stmt = $db->prepare(
            "SELECT id, detail_code, national_code, first_name, last_name,
                    father_name, mobile, gender, school_id, salary_row_number
             FROM personnel
             WHERE $whereSQL
             ORDER BY id DESC
             LIMIT :limit OFFSET :offset"
        );
        foreach ($params as $key => $val) {
            $stmt->bindValue($key, $val);
        }
        $stmt->bindValue(':limit',  $pageSize, PDO::PARAM_INT);
        $stmt->bindValue(':offset', $offset,   PDO::PARAM_INT);
        $stmt->execute();
        $items = $stmt->fetchAll();

        jsonResponse(['status' => true, 'data' => $items, 'total' => $total]);
    }

    // GET /Personnel/GetById?id=X
    public function getById() {
        requireMethod('GET');

        $id = (int) getQueryParam('id');
        if (!$id) errorResponse('شناسه پرسنل الزامی است');

        $stmt = getDB()->prepare("SELECT * FROM personnel WHERE id = :id");
        $stmt->execute([':id' => $id]);
        $row = $stmt->fetch();

        if (!$row) errorResponse('پرسنل یافت نشد', 404);

        jsonResponse(['status' => true, 'data' => $row]);
    }

    // POST /Personnel/Save
    // body: همه فیلدهای پرسنل — اگر id وجود داشت: update، وگرنه: insert
    public function save() {
        requireMethod('POST');

        $body = getRequestBody();

        if (empty($body['nationalCode']))  errorResponse('کد ملی الزامی است');
        if (empty($body['firstName']))     errorResponse('نام الزامی است');
        if (empty($body['lastName']))      errorResponse('نام خانوادگی الزامی است');

        $id = (int) ($body['id'] ?? 0);
        $db = getDB();

        $fields = [
            'detail_code'               => $body['detailCode']             ?? '',
            'national_code'             => $body['nationalCode'],
            'first_name'                => $body['firstName'],
            'last_name'                 => $body['lastName'],
            'father_name'               => $body['fatherName']             ?? '',
            'mobile'                    => $body['mobile']                 ?? '',
            'gender'                    => $body['gender']                 ?? 'male',
            'school_id'                 => (int)($body['schoolId']         ?? 0),
            'school_staff'              => $body['schoolStaff']            ?? 'teacher',
            'salary_row_number'         => $body['salaryRowNumber']        ?? '',
            'insurance_number'          => $body['insuranceNumber']        ?? '',
            'birth_certificate_number'  => $body['birthCertificateNumber'] ?? '',
            'birth_date'                => $this->joinDate($body['birthDate']  ?? []),
            'issue_date'                => $this->joinDate($body['issueDate']  ?? []),
            'tax_status'                => $body['taxStatus']              ?? '',
            'employee_status'           => $body['employeeStatus']         ?? 'ایران',
            'insurance_type'            => $body['insuranceType']          ?? 'normal',
            'has_tax_history'           => (int)($body['hasTaxHistory']    ?? 0),
            'show_in_tax_file'          => (int)($body['showInTaxFile']    ?? 0),
            'address'                   => $body['address']                ?? '',
            'postal_code'               => $body['postalCode']             ?? '',
            'bank'                      => $body['bank']                   ?? 'melli',
            'industry'                  => $body['industry']               ?? '',
            'connection'                => $body['connection']             ?? '',
        ];

        if ($id > 0) {
            // UPDATE
            $stmt = $db->prepare("SELECT id FROM personnel WHERE id = :id");
            $stmt->execute([':id' => $id]);
            if (!$stmt->fetch()) errorResponse('پرسنل یافت نشد', 404);

            $setParts = [];
            foreach (array_keys($fields) as $k) $setParts[] = "$k = :$k";
            $sets = implode(', ', $setParts);
            $stmt = $db->prepare("UPDATE personnel SET $sets WHERE id = :id");
            $fields['id'] = $id;
            $bound = [];
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);

            jsonResponse(['status' => true, 'message' => 'اطلاعات پرسنل با موفقیت ویرایش شد', 'data' => ['id' => $id]]);
        } else {
            // INSERT
            $cols  = implode(', ', array_keys($fields));
            $valParts = [];
            foreach (array_keys($fields) as $k) $valParts[] = ":$k";
            $vals  = implode(', ', $valParts);
            $stmt  = $db->prepare("INSERT INTO personnel ($cols) VALUES ($vals)");
            $bound = [];
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            $newId = (int) $db->lastInsertId();

            jsonResponse(['status' => true, 'message' => 'پرسنل جدید با موفقیت ثبت شد', 'data' => ['id' => $newId]]);
        }
    }

    // DELETE /Personnel/Delete   body: { id: X }
    public function delete() {
        requireMethod('DELETE');

        $body = getRequestBody();
        $id   = (int) ($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه پرسنل الزامی است');

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM personnel WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) errorResponse('پرسنل یافت نشد', 404);

        $db->prepare("DELETE FROM personnel WHERE id = :id")->execute([':id' => $id]);

        jsonResponse(['status' => true, 'message' => 'پرسنل با موفقیت حذف شد']);
    }

    // POST /Personnel/Import   body: { items: [...] }
    public function import() {
        requireMethod('POST');

        $body  = getRequestBody();
        $items = $body['items'] ?? [];

        if (empty($items) || !is_array($items)) {
            errorResponse('داده‌ای برای درج وجود ندارد');
        }

        $db = getDB();
        $inserted = 0;
        $errors   = [];

        $stmt = $db->prepare(
            "INSERT INTO personnel
                (detail_code, national_code, first_name, last_name, father_name,
                 mobile, gender, school_id, salary_row_number, school_staff)
             VALUES
                (:detail_code, :national_code, :first_name, :last_name, :father_name,
                 :mobile, :gender, :school_id, :salary_row_number, :school_staff)
             ON DUPLICATE KEY UPDATE
                first_name = VALUES(first_name),
                last_name  = VALUES(last_name),
                mobile     = VALUES(mobile)"
        );

        foreach ($items as $i => $item) {
            if (empty($item['nationalCode']) || empty($item['firstName']) || empty($item['lastName'])) {
                $errors[] = "ردیف " . ($i + 1) . ": اطلاعات ضروری ناقص است";
                continue;
            }
            $stmt->execute([
                ':detail_code'       => $item['detailCode']       ?? '',
                ':national_code'     => $item['nationalCode'],
                ':first_name'        => $item['firstName'],
                ':last_name'         => $item['lastName'],
                ':father_name'       => $item['fatherName']       ?? '',
                ':mobile'            => $item['mobile']           ?? '',
                ':gender'            => $item['gender']           ?? 'male',
                ':school_id'         => (int)($item['schoolId']   ?? 0),
                ':salary_row_number' => $item['salaryRowNumber']  ?? '',
                ':school_staff'      => $item['schoolStaff']      ?? 'teacher',
            ]);
            $inserted++;
        }

        jsonResponse([
            'status'   => true,
            'message'  => "$inserted پرسنل با موفقیت درج شد",
            'inserted' => $inserted,
            'errors'   => $errors,
        ]);
    }

    // GET /Personnel/GetSchools
    public function getSchools() {
        requireMethod('GET');

        $stmt = getDB()->query("SELECT id, name FROM schools ORDER BY id");
        $rows = $stmt->fetchAll();

        jsonResponse(['status' => true, 'data' => $rows]);
    }

    // ---- helper ----
    private function joinDate($d) {
        if (is_array($d) && isset($d['year'], $d['month'], $d['day'])) {
            return $d['year'] . '/' . $d['month'] . '/' . $d['day'];
        }
        return is_string($d) ? $d : '';
    }
}
