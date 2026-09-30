<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class ParamController {

    // GET /Param/GetSalaryParams
    // بارگذاری همه پارامترها + لیست انواع پرداختی
    public function getSalaryParams() {
        requireMethod('GET');

        $db = getDB();

        // لیست انواع پرداختی از جدول payment_types
        $payStmt = $db->query(
            "SELECT code, title, is_benefit, fixed_monthly
             FROM payment_types ORDER BY code"
        );
        $paymentTypes = $payStmt->fetchAll();

        // بارگذاری پارامترها از جدول salary_params
        $paramStmt = $db->query("SELECT param_key, param_value FROM salary_params");
        $rows = $paramStmt->fetchAll();

        $params = [];
        foreach ($rows as $row) {
            $params[$row['param_key']] = json_decode($row['param_value'], true);
        }

        jsonResponse([
            'status'       => true,
            'paymentTypes' => $paymentTypes,
            'params'       => $params,
        ]);
    }

    // POST /Param/SaveSalaryParams
    // body: { paramKey: 'vacation', codes: [109, 110, ...] }
    public function saveSalaryParams() {
        requireMethod('POST');

        $body = getRequestBody();
        $key  = $body['paramKey'] ?? '';
        $codes = $body['codes'] ?? [];

        $validKeys = ['vacation','yearlyBonus','severance','overtime','mission','other','hormozgan'];
        if (!in_array($key, $validKeys)) {
            errorResponse('کلید پارامتر نامعتبر است');
        }

        $db   = getDB();
        $json = json_encode(array_values($codes), JSON_UNESCAPED_UNICODE);

        $stmt = $db->prepare(
            "INSERT INTO salary_params (param_key, param_value)
             VALUES (:key, :val)
             ON DUPLICATE KEY UPDATE param_value = :val2"
        );
        $stmt->execute([':key' => $key, ':val' => $json, ':val2' => $json]);

        jsonResponse(['status' => true, 'message' => 'تنظیمات با موفقیت ذخیره شد']);
    }
}
