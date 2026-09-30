<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

/**
 * مدیریت انواع پرداختی (payment_types) و انواع کسورات (deduction_types)
 */
class SalaryTypeController {

    // ==================== انواع پرداختی ====================

    // GET /SalaryType/GetPaymentList
    public function getPaymentList() {
        requireMethod('GET');

        $search = getQueryParam('search');
        $where  = '1=1';
        $params = [];
        if (!empty($search)) {
            $where = 'title LIKE :s OR CAST(code AS CHAR) LIKE :s';
            $params[':s'] = '%' . $search . '%';
        }

        $stmt = getDB()->prepare(
            "SELECT code, title, account, sub_code, payment_type, cash_type,
                    continuity_type1, continuity_type2, insurance, tax,
                    taxable, fixed_monthly, is_benefit, show_in_salary_list,
                    tax_exempt_percent
             FROM payment_types WHERE $where ORDER BY code"
        );
        $stmt->execute($params);
        $rows = $stmt->fetchAll();

        jsonResponse(['status' => true, 'data' => $rows]);
    }

    // POST /SalaryType/SavePayment
    public function savePayment() {
        requireMethod('POST');

        $body = getRequestBody();
        if (empty($body['code']))  errorResponse('کد پرداخت الزامی است');
        if (empty($body['title'])) errorResponse('عنوان پرداختی الزامی است');

        $db   = getDB();
        $code = (int) $body['code'];

        $fields = [
            'title'              => $body['title'],
            'account'            => $body['account']           ?? '',
            'sub_code'           => $body['subCode']           ?? '',
            'payment_type'       => $body['paymentType']       ?? 'حقوق پایه',
            'cash_type'          => $body['cashType']          ?? 'نقدی',
            'continuity_type1'   => $body['continuityType1']   ?? 'حقوق پایه',
            'continuity_type2'   => $body['continuityType2']   ?? 'حقوق پایه',
            'insurance'          => (int)($body['insurance']   ?? 1),
            'tax'                => (int)($body['tax']         ?? 1),
            'taxable'            => (int)($body['taxable']     ?? 0),
            'fixed_monthly'      => (int)($body['fixedMonthly'] ?? 0),
            'is_benefit'         => (int)($body['isBenefit']   ?? 0),
            'show_in_salary_list'=> (int)($body['showInSalaryList'] ?? 0),
            'tax_exempt_percent' => (float)($body['taxExemptPercent'] ?? 0),
        ];

        // بررسی وجود
        $exists = $db->prepare("SELECT code FROM payment_types WHERE code = :code");
        $exists->execute([':code' => $code]);

        if ($exists->fetch()) {
            // UPDATE
            $setParts = array();
            foreach (array_keys($fields) as $k) $setParts[] = "$k = :$k";
            $sets = implode(', ', $setParts);
            $stmt = $db->prepare("UPDATE payment_types SET $sets WHERE code = :code");
            $bound = array(':code' => $code);
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            jsonResponse(['status' => true, 'message' => 'نوع پرداخت ویرایش شد', 'data' => ['code' => $code]]);
        } else {
            // INSERT
            $fields['code'] = $code;
            $cols = implode(', ', array_keys($fields));
            $valParts = array();
            foreach (array_keys($fields) as $k) $valParts[] = ":$k";
            $vals = implode(', ', $valParts);
            $stmt = $db->prepare("INSERT INTO payment_types ($cols) VALUES ($vals)");
            $bound = array();
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            jsonResponse(['status' => true, 'message' => 'نوع پرداخت جدید ثبت شد', 'data' => ['code' => $code]]);
        }
    }

    // DELETE /SalaryType/DeletePayment   body: { code: X }
    public function deletePayment() {
        requireMethod('DELETE');

        $body = getRequestBody();
        $code = (int) ($body['code'] ?? 0);
        if (!$code) errorResponse('کد پرداخت الزامی است');

        $db = getDB();
        $stmt = $db->prepare("SELECT code FROM payment_types WHERE code = :code");
        $stmt->execute([':code' => $code]);
        if (!$stmt->fetch()) errorResponse('نوع پرداخت یافت نشد', 404);

        $db->prepare("DELETE FROM payment_types WHERE code = :code")->execute([':code' => $code]);

        jsonResponse(['status' => true, 'message' => 'نوع پرداخت حذف شد']);
    }

    // ==================== انواع کسورات ====================

    // GET /SalaryType/GetDeductionList
    public function getDeductionList() {
        requireMethod('GET');

        $search = getQueryParam('search');
        $where  = '1=1';
        $params = [];
        if (!empty($search)) {
            $where = 'name LIKE :s OR account_code LIKE :s';
            $params[':s'] = '%' . $search . '%';
        }

        $stmt = getDB()->prepare(
            "SELECT id, code, name, account_code, deduction_type, default_amount,
                    account, is_creditor, show_in_salary_list
             FROM deduction_types WHERE $where ORDER BY code"
        );
        $stmt->execute($params);
        $rows = $stmt->fetchAll();

        jsonResponse(['status' => true, 'data' => $rows]);
    }

    // POST /SalaryType/SaveDeduction
    public function saveDeduction() {
        requireMethod('POST');

        $body = getRequestBody();
        if (empty($body['name'])) errorResponse('نام کسر الزامی است');

        $db = getDB();
        $id = (int) ($body['id'] ?? 0);

        $fields = [
            'code'                => (int)($body['code']              ?? 0),
            'name'                => $body['name'],
            'account_code'        => $body['accountCode']             ?? '',
            'deduction_type'      => $body['deductionType']           ?? '',
            'default_amount'      => (float)($body['defaultAmount']   ?? 0),
            'account'             => $body['account']                 ?? '',
            'is_creditor'         => (int)($body['isCreditor']        ?? 0),
            'show_in_salary_list' => (int)($body['showInSalaryList']  ?? 0),
        ];

        if ($id > 0) {
            $stmt = $db->prepare("SELECT id FROM deduction_types WHERE id = :id");
            $stmt->execute([':id' => $id]);
            if (!$stmt->fetch()) errorResponse('کسر یافت نشد', 404);

            $setParts = array();
            foreach (array_keys($fields) as $k) $setParts[] = "$k = :$k";
            $stmt = $db->prepare("UPDATE deduction_types SET " . implode(', ', $setParts) . " WHERE id = :id");
            $bound = array(':id' => $id);
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            jsonResponse(['status' => true, 'message' => 'کسر ویرایش شد', 'data' => ['id' => $id]]);
        } else {
            $cols = implode(', ', array_keys($fields));
            $valParts = array();
            foreach (array_keys($fields) as $k) $valParts[] = ":$k";
            $stmt = $db->prepare("INSERT INTO deduction_types (" . $cols . ") VALUES (" . implode(', ', $valParts) . ")");
            $bound = array();
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            $newId = (int) $db->lastInsertId();
            jsonResponse(['status' => true, 'message' => 'کسر جدید ثبت شد', 'data' => ['id' => $newId]]);
        }
    }

    // DELETE /SalaryType/DeleteDeduction   body: { id: X }
    public function deleteDeduction() {
        requireMethod('DELETE');

        $body = getRequestBody();
        $id   = (int) ($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه کسر الزامی است');

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM deduction_types WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) errorResponse('کسر یافت نشد', 404);

        $db->prepare("DELETE FROM deduction_types WHERE id = :id")->execute([':id' => $id]);

        jsonResponse(['status' => true, 'message' => 'کسر حذف شد']);
    }
}
