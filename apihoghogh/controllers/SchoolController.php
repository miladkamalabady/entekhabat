<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class SchoolController {

    // GET /School/GetList
    public function getList() {
        requireMethod('GET');
        $stmt = getDB()->query("SELECT id, name, phone, manager_phone, bank_account FROM schools ORDER BY id");
        jsonResponse(['status' => true, 'data' => $stmt->fetchAll()]);
    }

    // POST /School/Save   body: { id?, name, phone?, managerPhone?, bankAccount? }
    public function save() {
        requireMethod('POST');

        $body        = getRequestBody();
        $id          = (int)($body['id']           ?? 0);
        $name        = trim($body['name']          ?? '');
        $phone       = trim($body['phone']         ?? '');
        $managerPhone= trim($body['managerPhone']  ?? '');
        $bankAccount = trim($body['bankAccount']   ?? '');

        if (!$name) errorResponse('نام مدرسه الزامی است');

        $db = getDB();

        if ($id > 0) {
            $stmt = $db->prepare("
                UPDATE schools SET name=:name, phone=:phone, manager_phone=:mp, bank_account=:ba
                WHERE id=:id
            ");
            $stmt->execute([':name'=>$name,':phone'=>$phone,':mp'=>$managerPhone,':ba'=>$bankAccount,':id'=>$id]);
            jsonResponse(['status'=>true,'message'=>'مدرسه ویرایش شد','data'=>['id'=>$id]]);
        } else {
            $stmt = $db->prepare("
                INSERT INTO schools (name, phone, manager_phone, bank_account)
                VALUES (:name, :phone, :mp, :ba)
            ");
            $stmt->execute([':name'=>$name,':phone'=>$phone,':mp'=>$managerPhone,':ba'=>$bankAccount]);
            jsonResponse(['status'=>true,'message'=>'مدرسه اضافه شد','data'=>['id'=>(int)$db->lastInsertId()]]);
        }
    }

    // DELETE /School/Delete   body: { id }
    public function delete() {
        requireMethod('DELETE');

        $body = getRequestBody();
        $id   = (int)($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه الزامی است');

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM schools WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) errorResponse('مدرسه یافت نشد', 404);

        $db->prepare("DELETE FROM schools WHERE id = :id")->execute([':id' => $id]);
        jsonResponse(['status' => true, 'message' => 'مدرسه حذف شد']);
    }
}
