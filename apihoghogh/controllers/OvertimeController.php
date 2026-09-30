<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class OvertimeController {

    // GET /Overtime/GetList?year=1404&monthNumber=7&personnelId=X
    public function getList() {
        requireMethod('GET');

        $year        = trim(getQueryParam('year')        ?? '');
        $monthNumber = (int)(getQueryParam('monthNumber') ?? 0);
        $personnelId = (int)(getQueryParam('personnelId') ?? 0);

        $where  = ['1=1'];
        $params = [];

        if ($year) {
            $where[]         = 'o.year = :year';
            $params[':year'] = $year;
        }
        if ($monthNumber) {
            $where[]          = 'o.month = :month';
            $params[':month'] = $monthNumber;
        }
        if ($personnelId) {
            $where[]        = 'o.personnel_id = :pid';
            $params[':pid'] = $personnelId;
        }

        $whereSQL = implode(' AND ', $where);

        $stmt = getDB()->prepare("
            SELECT o.id, o.personnel_id, o.year, o.month, o.hours, o.amount, o.description,
                   p.first_name, p.last_name, p.national_code
            FROM overtime_records o
            JOIN personnel p ON p.id = o.personnel_id
            WHERE $whereSQL
            ORDER BY o.year DESC, o.month DESC, p.last_name, p.first_name
        ");
        $stmt->execute($params);

        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // POST /Overtime/Save
    // body: { id?, personnelId, year, monthNumber, hours, amount, description? }
    public function save() {
        requireMethod('POST');

        $body        = getRequestBody();
        $id          = (int)($body['id']          ?? 0);
        $personnelId = (int)($body['personnelId']  ?? 0);
        $year        = trim($body['year']          ?? '');
        $monthNumber = (int)($body['monthNumber']  ?? 0);
        $hours       = (float)($body['hours']      ?? 0);
        $amount      = (float)($body['amount']     ?? 0);
        $description = trim($body['description']   ?? '');

        if (!$personnelId) errorResponse('پرسنل الزامی است');
        if (!$year)        errorResponse('سال الزامی است');
        if (!$monthNumber) errorResponse('ماه الزامی است');
        if ($hours < 0)    errorResponse('ساعت نمی‌تواند منفی باشد');
        if ($amount < 0)   errorResponse('مبلغ نمی‌تواند منفی باشد');

        $db = getDB();

        // بررسی وجود پرسنل
        $pCheck = $db->prepare("SELECT id FROM personnel WHERE id = :id");
        $pCheck->execute([':id' => $personnelId]);
        if (!$pCheck->fetch()) errorResponse('پرسنل یافت نشد', 404);

        if ($id > 0) {
            $stmt = $db->prepare("
                UPDATE overtime_records
                SET personnel_id = :pid, year = :year, month = :month,
                    hours = :hours, amount = :amount, description = :desc
                WHERE id = :id
            ");
            $stmt->execute([
                ':pid'   => $personnelId,
                ':year'  => $year,
                ':month' => $monthNumber,
                ':hours' => $hours,
                ':amount'=> $amount,
                ':desc'  => $description,
                ':id'    => $id,
            ]);
            jsonResponse(['status' => true, 'message' => 'اضافه کاری با موفقیت ویرایش شد', 'data' => ['id' => $id]]);
        } else {
            $stmt = $db->prepare("
                INSERT INTO overtime_records (personnel_id, year, month, hours, amount, description)
                VALUES (:pid, :year, :month, :hours, :amount, :desc)
            ");
            $stmt->execute([
                ':pid'   => $personnelId,
                ':year'  => $year,
                ':month' => $monthNumber,
                ':hours' => $hours,
                ':amount'=> $amount,
                ':desc'  => $description,
            ]);
            $newId = (int)$db->lastInsertId();
            jsonResponse(['status' => true, 'message' => 'اضافه کاری با موفقیت ثبت شد', 'data' => ['id' => $newId]]);
        }
    }

    // DELETE /Overtime/Delete   body: { id: X }
    public function delete() {
        requireMethod('DELETE');

        $body = getRequestBody();
        $id   = (int)($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه الزامی است');

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM overtime_records WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) errorResponse('رکورد اضافه کاری یافت نشد', 404);

        $db->prepare("DELETE FROM overtime_records WHERE id = :id")->execute([':id' => $id]);

        jsonResponse(['status' => true, 'message' => 'اضافه کاری با موفقیت حذف شد']);
    }
}
