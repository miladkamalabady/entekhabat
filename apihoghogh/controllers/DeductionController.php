<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class DeductionController {

    // GET /PersonnelDeduction/GetList?personnelId=X&status=active
    public function getList() {
        requireMethod('GET');

        $personnelId = (int)(getQueryParam('personnelId') ?? 0);
        $status      = trim(getQueryParam('status')      ?? '');

        $where  = ['1=1'];
        $params = [];

        if ($personnelId > 0) {
            $where[]        = 'pd.personnel_id = :pid';
            $params[':pid'] = $personnelId;
        }
        if ($status !== '') {
            $where[]           = 'pd.status = :status';
            $params[':status'] = $status;
        }

        $whereSQL = implode(' AND ', $where);
        $stmt = getDB()->prepare("
            SELECT pd.*,
                   p.first_name, p.last_name, p.national_code, p.detail_code
            FROM personnel_deductions pd
            JOIN personnel p ON p.id = pd.personnel_id
            WHERE $whereSQL
            ORDER BY p.last_name, p.first_name, pd.id
        ");
        $stmt->execute($params);
        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // POST /PersonnelDeduction/Save
    public function save() {
        requireMethod('POST');

        $body          = getRequestBody();
        $id            = (int)($body['id']            ?? 0);
        $personnelId   = (int)($body['personnelId']   ?? 0);
        $deductionName = trim($body['deductionName']  ?? '');
        $deductionCode = (int)($body['deductionCode'] ?? 0);
        $amount        = (float)($body['amount']      ?? 0);
        $startYear     = trim($body['startYear']      ?? '');
        $startMonth    = (int)($body['startMonth']    ?? 1);
        $endType       = trim($body['endType']        ?? 'always');
        $endYear       = trim($body['endYear']        ?? '');
        $endMonth      = (int)($body['endMonth']      ?? 0);
        $maxAmount     = (float)($body['maxAmount']   ?? 0);
        $status        = trim($body['status']         ?? 'active');

        if (!$personnelId)   errorResponse('پرسنل الزامی است');
        if (!$deductionName) errorResponse('نوع کسر الزامی است');
        if ($amount <= 0)    errorResponse('مبلغ کسر باید بیشتر از صفر باشد');
        if (!$startYear)     errorResponse('سال شروع الزامی است');

        $db = getDB();

        $pCheck = $db->prepare("SELECT id FROM personnel WHERE id = :id");
        $pCheck->execute([':id' => $personnelId]);
        if (!$pCheck->fetch()) errorResponse('پرسنل یافت نشد', 404);

        $fields = [
            'personnel_id'   => $personnelId,
            'deduction_name' => $deductionName,
            'deduction_code' => $deductionCode,
            'amount'         => $amount,
            'start_year'     => $startYear,
            'start_month'    => $startMonth,
            'end_type'       => $endType,
            'end_year'       => $endYear,
            'end_month'      => $endMonth,
            'max_amount'     => $maxAmount,
            'status'         => $status,
        ];

        if ($id > 0) {
            $sets = implode(', ', array_map(fn($k) => "$k = :$k", array_keys($fields)));
            $stmt = $db->prepare("UPDATE personnel_deductions SET $sets WHERE id = :id");
            $fields['id'] = $id;
            $bound = [];
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            jsonResponse(['status' => true, 'message' => 'کسر ویرایش شد', 'data' => ['id' => $id]]);
        } else {
            $cols  = implode(', ', array_keys($fields));
            $vals  = implode(', ', array_map(fn($k) => ":$k", array_keys($fields)));
            $stmt  = $db->prepare("INSERT INTO personnel_deductions ($cols) VALUES ($vals)");
            $bound = [];
            foreach ($fields as $k => $v) $bound[":$k"] = $v;
            $stmt->execute($bound);
            jsonResponse(['status' => true, 'message' => 'کسر ثبت شد', 'data' => ['id' => (int)$db->lastInsertId()]]);
        }
    }

    // DELETE /PersonnelDeduction/Delete   body: { id }
    public function delete() {
        requireMethod('DELETE');

        $body = getRequestBody();
        $id   = (int)($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه الزامی است');

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM personnel_deductions WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) errorResponse('کسر یافت نشد', 404);

        $db->prepare("DELETE FROM personnel_deductions WHERE id = :id")->execute([':id' => $id]);
        jsonResponse(['status' => true, 'message' => 'کسر حذف شد']);
    }
}
