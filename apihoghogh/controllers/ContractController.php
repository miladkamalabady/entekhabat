<?php

require_once __DIR__ . '/../db.php';
require_once __DIR__ . '/../helpers.php';

class ContractController {

    // GET /Contract/GetList
    // params: fromDate, toDate, workStage, contractNumber, page, pageSize
    public function getList(): void {
        requireMethod('GET');

        $page     = max(1, (int) getQueryParam('page', 1));
        $pageSize = min(100, max(1, (int) getQueryParam('pageSize', 10)));
        $offset   = ($page - 1) * $pageSize;

        $fromDate       = getQueryParam('fromDate');
        $toDate         = getQueryParam('toDate');
        $workStage      = getQueryParam('workStage');
        $contractNumber = getQueryParam('contractNumber');

        $where  = ['1=1'];
        $params = [];

        if (!empty($fromDate)) {
            $where[]  = 'date1 >= :fromDate';
            $params[':fromDate'] = $fromDate;
        }
        if (!empty($toDate)) {
            $where[]  = 'date1 <= :toDate';
            $params[':toDate'] = $toDate;
        }
        if ($workStage !== null && $workStage !== '') {
            $where[]  = 'work_stage = :workStage';
            $params[':workStage'] = (int) $workStage;
        }
        if (!empty($contractNumber)) {
            $where[]  = 'contract_number LIKE :contractNumber';
            $params[':contractNumber'] = '%' . $contractNumber . '%';
        }

        $whereSQL = implode(' AND ', $where);

        $db = getDB();

        $countStmt = $db->prepare("SELECT COUNT(*) FROM contracts WHERE $whereSQL");
        $countStmt->execute($params);
        $total = (int) $countStmt->fetchColumn();

        $stmt = $db->prepare(
            "SELECT id, contract_number, national_code, number, title,
                    work_action, workplace, job,
                    date1, date2, date3, date4,
                    first_name, last_name, work_stage, status
             FROM contracts
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

        jsonResponse([
            'status' => true,
            'data'   => $items,
            'total'  => $total,
            'page'   => $page,
            'pageSize' => $pageSize,
        ]);
    }

    // GET /Contract/GetById?id=X
    public function getById(): void {
        requireMethod('GET');

        $id = (int) getQueryParam('id');
        if (!$id) {
            errorResponse('شناسه قرارداد الزامی است');
        }

        $stmt = getDB()->prepare(
            "SELECT c.*,
                    cs.include_in_insurance, cs.start_year, cs.start_month, cs.start_status,
                    cs.end_day, cs.end_month, cs.end_year,
                    cs.include_end_in_insurance, cs.end_insurance_year,
                    cs.end_insurance_month, cs.end_status
             FROM contracts c
             LEFT JOIN contract_status cs ON cs.contract_id = c.id
                 AND cs.id = (SELECT MAX(id) FROM contract_status WHERE contract_id = c.id)
             WHERE c.id = :id"
        );
        $stmt->execute([':id' => $id]);
        $contract = $stmt->fetch();

        if (!$contract) {
            errorResponse('قرارداد یافت نشد', 404);
        }

        jsonResponse(['status' => true, 'data' => $contract]);
    }

    // DELETE /Contract/Delete
    // body: { id: X }
    public function delete(): void {
        requireMethod('DELETE');

        $body = getRequestBody();
        $id   = (int) ($body['id'] ?? 0);
        if (!$id) {
            errorResponse('شناسه قرارداد الزامی است');
        }

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM contracts WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) {
            errorResponse('قرارداد یافت نشد', 404);
        }

        $db->prepare("DELETE FROM contract_status WHERE contract_id = :id")->execute([':id' => $id]);
        $db->prepare("DELETE FROM contracts WHERE id = :id")->execute([':id' => $id]);

        jsonResponse(['status' => true, 'message' => 'قرارداد با موفقیت حذف شد']);
    }

    // POST /Contract/Copy
    // body: { id: X }
    public function copy() {
        requireMethod('POST');

        $body = getRequestBody();
        $id   = (int) ($body['id'] ?? 0);
        if (!$id) errorResponse('شناسه قرارداد الزامی است');

        $db = getDB();

        // اصل قرارداد از جدول contracts
        $stmt = $db->prepare("SELECT * FROM contracts WHERE id = :id");
        $stmt->execute([':id' => $id]);
        $original = $stmt->fetch();
        if (!$original) errorResponse('قرارداد یافت نشد', 404);

        // شماره قرارداد جدید — بزرگترین عدد موجود + 1
        $maxRow = $db->query(
            "SELECT MAX(CAST(contract_number AS UNSIGNED)) AS m FROM contracts"
        )->fetch();
        $newNumber = (string)(($maxRow['m'] ?? 10000) + 1);

        // درج در contracts
        $db->prepare(
            "INSERT INTO contracts
                (contract_number, national_code, number, title,
                 work_action, workplace, job, date1, date2, date3, date4,
                 first_name, last_name, work_stage, status)
             VALUES
                (:cn, :nc, :num, :title, :wa, :wp, :job, :d1, :d2, :d3, :d4, :fn, :ln, :ws, :st)"
        )->execute([
            ':cn'  => $newNumber,
            ':nc'  => $original['national_code'],
            ':num' => $newNumber,
            ':title' => $original['title'],
            ':wa'  => $original['work_action'],
            ':wp'  => $original['workplace'],
            ':job' => $original['job'],
            ':d1'  => $original['date1'],
            ':d2'  => $original['date2'],
            ':d3'  => $original['date3'],
            ':d4'  => $original['date4'],
            ':fn'  => $original['first_name'],
            ':ln'  => $original['last_name'],
            ':ws'  => $original['work_stage'],
            ':st'  => 'active',
        ]);
        $newContractsId = (int) $db->lastInsertId();

        // کپی از personnel_contracts اگر وجود دارد
        $pcStmt = $db->prepare("SELECT * FROM personnel_contracts WHERE contract_number = :cn");
        $pcStmt->execute([':cn' => $original['contract_number']]);
        $pc = $pcStmt->fetch();

        if ($pc) {
            $oldPcId = $pc['id'];
            unset($pc['id'], $pc['created_at']);
            $pc['contract_number'] = $newNumber;

            $cols = implode(', ', array_keys($pc));
            $valParts = array();
            foreach (array_keys($pc) as $k) $valParts[] = ":$k";
            $vals = implode(', ', $valParts);

            $pcInsert = $db->prepare("INSERT INTO personnel_contracts ($cols) VALUES ($vals)");
            $bound = array();
            foreach ($pc as $k => $v) $bound[":$k"] = $v;
            $pcInsert->execute($bound);
            $newPcId = (int) $db->lastInsertId();

            // کپی contract_payments
            $payStmt = $db->prepare(
                "SELECT payment_code, daily_amount, monthly_amount
                 FROM contract_payments WHERE contract_id = :cid"
            );
            $payStmt->execute([':cid' => $oldPcId]);
            $payments = $payStmt->fetchAll();

            if (!empty($payments)) {
                $payInsert = $db->prepare(
                    "INSERT INTO contract_payments
                        (contract_id, payment_code, daily_amount, monthly_amount)
                     VALUES (:cid, :code, :da, :ma)"
                );
                foreach ($payments as $pay) {
                    $payInsert->execute([
                        ':cid'  => $newPcId,
                        ':code' => $pay['payment_code'],
                        ':da'   => $pay['daily_amount'],
                        ':ma'   => $pay['monthly_amount'],
                    ]);
                }
            }
        }

        jsonResponse([
            'status'  => true,
            'message' => 'قرارداد با موفقیت کپی شد',
            'data'    => ['id' => $newContractsId],
        ]);
    }

    // POST /Contract/UpdateStatus
    // body: { id, includeInInsurance, startYear, startMonth, startStatus,
    //         endDay, endMonth, endYear, includeEndInInsurance,
    //         endInsuranceYear, endInsuranceMonth, endStatus }
    public function updateStatus(): void {
        requireMethod('POST');

        $body = getRequestBody();
        $id   = (int) ($body['id'] ?? 0);
        if (!$id) {
            errorResponse('شناسه قرارداد الزامی است');
        }

        $db   = getDB();
        $stmt = $db->prepare("SELECT id FROM contracts WHERE id = :id");
        $stmt->execute([':id' => $id]);
        if (!$stmt->fetch()) {
            errorResponse('قرارداد یافت نشد', 404);
        }

        $insert = $db->prepare(
            "INSERT INTO contract_status
                (contract_id, include_in_insurance, start_year, start_month, start_status,
                 end_day, end_month, end_year,
                 include_end_in_insurance, end_insurance_year, end_insurance_month, end_status)
             VALUES
                (:contract_id, :include_in_insurance, :start_year, :start_month, :start_status,
                 :end_day, :end_month, :end_year,
                 :include_end_in_insurance, :end_insurance_year, :end_insurance_month, :end_status)"
        );
        $insert->execute([
            ':contract_id'              => $id,
            ':include_in_insurance'     => (int) ($body['includeInInsurance']    ?? 0),
            ':start_year'               => $body['startYear']               ?? '',
            ':start_month'              => $body['startMonth']              ?? '',
            ':start_status'             => $body['startStatus']             ?? 'pending',
            ':end_day'                  => $body['endDay']                  ?? '',
            ':end_month'                => $body['endMonth']                ?? '',
            ':end_year'                 => $body['endYear']                 ?? '',
            ':include_end_in_insurance' => (int) ($body['includeEndInInsurance'] ?? 0),
            ':end_insurance_year'       => $body['endInsuranceYear']        ?? '',
            ':end_insurance_month'      => $body['endInsuranceMonth']       ?? '',
            ':end_status'               => $body['endStatus']               ?? 'pending',
        ]);

        // وضعیت اصلی قرارداد را هم آپدیت کن
        $db->prepare("UPDATE contracts SET status = :status WHERE id = :id")
           ->execute([':status' => $body['startStatus'] ?? 'pending', ':id' => $id]);

        jsonResponse(['status' => true, 'message' => 'وضعیت قرارداد با موفقیت ثبت شد']);
    }
}
