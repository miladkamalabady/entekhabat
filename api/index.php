<?php
require_once 'database.php';
require_once 'jdf.php';

header('Content-Type: application/json; charset=utf-8');

$db->connect();

/* =========================
   1. Validate input
========================= */
$code = $_REQUEST['code'] ?? null;

if (!$code) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'code is required']);
    exit;
}

/* =========================
   2. Call SSO API
========================= */
$curl = curl_init();

curl_setopt_array($curl, [
    CURLOPT_URL => 'http://192.168.13.60:5002/api/sso/UserInfo',
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_FOLLOWLOCATION => true,
    CURLOPT_TIMEOUT => 10,
    CURLOPT_CUSTOMREQUEST => 'POST',
    CURLOPT_POSTFIELDS => '',
    CURLOPT_HTTPHEADER => [
        'code: ' . $code,
        'Authorization: Basic MTExOjIyMg==',
        'Content-Length: 0'
    ],
]);

$response = curl_exec($curl);

if ($response === false) {
    http_response_code(500);
    echo json_encode(['status' => false, 'message' => curl_error($curl)]);
    exit;
}

curl_close($curl);

$result = json_decode($response, true);

if (!$result || $result['resultCode'] != 200) {
    http_response_code(401);
    echo json_encode(['status' => false, 'message' => $result['data']]);
    exit;
}

$data = $result['data'];

/* =========================
   3. Check user existence
========================= */
$nationalId = $db->escape($data['nationalID']);

$check = $db->query("SELECT id FROM users WHERE national_id = '{$nationalId}'");
$user  = $check->fetch_assoc();

/* =========================
   4. Insert or Update user
========================= */
if ($user) {

    // UPDATE
    $userId = $user['id'];

    $sql = "
        UPDATE users SET
            first_name = '{$db->escape($data['firstName'])}',
            last_name = '{$db->escape($data['lastName'])}',
            father_name = '{$db->escape($data['father'])}',
            gender = {$data['gender']},
            birth_date = '" . substr($data['birthDate'], 0, 10) . "',
            persian_birth_date = '{$data['persianBirthDate']}',
            mobile = '{$data['mobile']}',
            user_type = {$data['userType']},
            verified = " . ($data['verified'] ? 1 : 0) . ",
            employee_key = '{$data['employeeKey']}',
            personnel_code = {$data['personnelCode']},
            org_position_code = {$data['orgPositionCode']},
            org_position_desc = '{$db->escape($data['orgPositionDesc'])}',
            org_position_type_code = {$data['orgPositionTypeCode']},
            org_position_type_desc = '{$db->escape($data['orgPositionTypeDesc'])}',
            region_id = {$data['regionId']},
            is_foreigner = " . ($data['isForeigner'] ? 1 : 0) . ",
            ip_address = '{$data['ip']}',
            updated_at = NOW()
        WHERE id = {$userId}
    ";
    $db->query($sql);
    $action = 'updated';

} else {

    // INSERT
    $sql = "
        INSERT INTO users (
            national_id, first_name, last_name, father_name, gender,
            birth_date, persian_birth_date, mobile, user_type, verified,
            employee_key, personnel_code, org_position_code, org_position_desc,
            org_position_type_code, org_position_type_desc, region_id,
            is_foreigner, ip_address, created_at
        ) VALUES (
            '{$nationalId}',
            '{$db->escape($data['firstName'])}',
            '{$db->escape($data['lastName'])}',
            '{$db->escape($data['father'])}',
            {$data['gender']},
            '" . substr($data['birthDate'], 0, 10) . "',
            '{$data['persianBirthDate']}',
            '{$data['mobile']}',
            {$data['userType']},
            " . ($data['verified'] ? 1 : 0) . ",
            '{$data['employeeKey']}',
            {$data['personnelCode']},
            {$data['orgPositionCode']},
            '{$db->escape($data['orgPositionDesc'])}',
            {$data['orgPositionTypeCode']},
            '{$db->escape($data['orgPositionTypeDesc'])}',
            {$data['regionId']},
            " . ($data['isForeigner'] ? 1 : 0) . ",
            '{$data['ip']}',
            NOW()
        )
    ";

    $res=$db->query($sql);
    $userId = $db->insert_id($res);
    $action = 'inserted';
}

/* =========================
   5. Address (upsert)
========================= */
if (!empty($data['homeAddress'])) {

    $postCode = $db->escape($data['homeAddress']['postCode']);
    $address  = $db->escape($data['homeAddress']['address']);

    $chkAddr = $db->query("SELECT id FROM user_addresses WHERE user_id = {$userId}");

    if ($chkAddr->num_rows) {
        $db->query("
            UPDATE user_addresses
            SET post_code = '{$postCode}', address = '{$address}'
            WHERE user_id = {$userId}
        ");
    } else {
        $db->query("
            INSERT INTO user_addresses (user_id, post_code, address)
            VALUES ({$userId}, '{$postCode}', '{$address}')
        ");
    }
}

/* =========================
   6. API Response
========================= */
echo json_encode([
    'status' => true,
    'action' => $action,
    'user_id' => $userId
], JSON_UNESCAPED_UNICODE);
