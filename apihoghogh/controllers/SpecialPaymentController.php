<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class SpecialPaymentController {

    // GET /SpecialPayment/GetList?year=1405&monthNumber=2&paymentCode=110&schoolId=0
    public function getList() {
        requireMethod('GET');

        $year        = trim(getQueryParam('year')        ?? '');
        $month       = (int)(getQueryParam('monthNumber') ?? 0);
        $paymentCode = (int)(getQueryParam('paymentCode') ?? 0);
        $schoolId    = (int)(getQueryParam('schoolId')    ?? 0);
        $personnelId = (int)(getQueryParam('personnelId') ?? 0);

        $where  = ['1=1'];
        $params = [];

        if ($year)        { $where[] = 'sp.year = :year';            $params[':year']  = $year; }
        if ($month)       { $where[] = 'sp.month = :month';          $params[':month'] = $month; }
        if ($paymentCode) { $where[] = 'sp.payment_code = :pc';      $params[':pc']    = $paymentCode; }
        if ($personnelId) { $where[] = 'sp.personnel_id = :pid';     $params[':pid']   = $personnelId; }
        if ($schoolId)    { $where[] = 'p.school_id = :school_id';   $params[':school_id'] = $schoolId; }

        $whereSQL = implode(' AND ', $where);

        $stmt = getDB()->prepare("
            SELECT sp.*, p.first_name, p.last_name, p.national_code, p.detail_code,
                   s.name AS school_name
            FROM special_payments sp
            JOIN personnel p ON p.id = sp.personnel_id
            LEFT JOIN schools s ON s.id = p.school_id
            WHERE $whereSQL
            ORDER BY p.last_name, p.first_name
        ");
        $stmt->execute($params);
        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // POST /SpecialPayment/Save   body: { personnelId, year, monthNumber, paymentCode, paymentName, amount }
    // or batch: { items: [{personnelId, amount}, ...], year, monthNumber, paymentCode, paymentName }
    public function save() {
        requireMethod('POST');

        $body        = getRequestBody();
        $year        = trim($body['year']        ?? '');
        $month       = (int)($body['monthNumber'] ?? 0);
        $paymentCode = (int)($body['paymentCode'] ?? 0);
        $paymentName = trim($body['paymentName']  ?? '');
        $items       = $body['items'] ?? null;

        if (!$year)  errorResponse('سال الزامی است');
        if (!$month) errorResponse('ماه الزامی است');

        $db = getDB();

        // اگر batch
        if (is_array($items)) {
            $stmt = $db->prepare("
                INSERT INTO special_payments (personnel_id, year, month, payment_code, payment_name, amount)
                VALUES (:pid, :year, :month, :pc, :pname, :amount)
                ON DUPLICATE KEY UPDATE payment_name = VALUES(payment_name), amount = VALUES(amount)
            ");
            $count = 0;
            foreach ($items as $item) {
                $pid    = (int)($item['personnelId'] ?? 0);
                $amount = (float)($item['amount']    ?? 0);
                if (!$pid || $amount <= 0) continue;
                $stmt->execute([':pid'=>$pid,':year'=>$year,':month'=>$month,
                                ':pc'=>$paymentCode,':pname'=>$paymentName,':amount'=>$amount]);
                $count++;
            }
            jsonResponse(['status'=>true,'message'=>"$count پرداختی ثبت/بروزرسانی شد",'count'=>$count]);
        }

        // single
        $personnelId = (int)($body['personnelId'] ?? 0);
        $amount      = (float)($body['amount']    ?? 0);
        if (!$personnelId) errorResponse('پرسنل الزامی است');
        if ($amount <= 0)  errorResponse('مبلغ باید بیشتر از صفر باشد');

        $stmt = $db->prepare("
            INSERT INTO special_payments (personnel_id, year, month, payment_code, payment_name, amount)
            VALUES (:pid, :year, :month, :pc, :pname, :amount)
            ON DUPLICATE KEY UPDATE payment_name = VALUES(payment_name), amount = VALUES(amount)
        ");
        $stmt->execute([':pid'=>$personnelId,':year'=>$year,':month'=>$month,
                        ':pc'=>$paymentCode,':pname'=>$paymentName,':amount'=>$amount]);

        jsonResponse(['status'=>true,'message'=>'پرداختی ثبت شد','data'=>['id'=>(int)$db->lastInsertId()]]);
    }

    // DELETE /SpecialPayment/Delete   body: { id }
    public function delete() {
        requireMethod('DELETE');
        $body = getRequestBody();
        $id   = (int)($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه الزامی است');
        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM special_payments WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) errorResponse('رکورد یافت نشد', 404);
        $db->prepare("DELETE FROM special_payments WHERE id = :id")->execute([':id' => $id]);
        jsonResponse(['status'=>true,'message'=>'پرداختی حذف شد']);
    }
}
