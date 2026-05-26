<?php
require_once 'database.php';
require_once 'jdf.php';
require_once 'config.php';

header('Content-Type: application/json; charset=utf-8');
function base64UrlEncode($data)
{
    return rtrim(strtr(base64_encode($data), '+/', '-_'), '=');
}

function generateJWT($payload)
{

    $header = [
        'alg' => 'HS256',
        'typ' => 'JWT'
    ];

    $payload['iss'] = JWT_ISSUER;
    $payload['iat'] = time();
    $payload['exp'] = time() + JWT_EXPIRE;

    $base64Header  = base64UrlEncode(json_encode($header));
    $base64Payload = base64UrlEncode(json_encode($payload));

    $signature = hash_hmac(
        'sha256',
        $base64Header . "." . $base64Payload,
        JWT_SECRET,
        true
    );

    $base64Signature = base64UrlEncode($signature);

    return $base64Header . "." . $base64Payload . "." . $base64Signature;
}

$db->connect();
$db->query("SET autocommit=0");
$db->query("START TRANSACTION");

/* =========================
   1. Validate input
========================= */
$code = $_REQUEST['code'] ?? null;
$role = $_REQUEST['role'] ?? null;

if (!$code) {
    http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'خطای دریافت کد!']);
    exit;
}


////////////
$data['roles'] = "VOTER";
$data['ozvsandogh'] = 1;
$data['sabegheO'] = 1;
$data['madrak'] = 1;

/* =========================
   3. Check user existence
========================= */
$nationalId = $db->escape($code);
// $nationalId = $data['nationalID'];

if($role){
    $check = $db->query("update users set roles='{$role}' WHERE national_id = '{$code}'");
    // if($role=='VOTER')
    // $check = $db->query("delete from final_submissions WHERE nationalId = '{$code}'");
}
$check = $db->query("SELECT * FROM users WHERE national_id = '{$nationalId}'");
$user  = $check->fetch_assoc();

/* =========================
   4. Insert or Update user
========================= */
if ($user) {

    // UPDATE
    $userId = $user['id'];
    if (!$role){
        $data['roles'] = $user['roles'];
    }
    else
        $data['roles'] = $role;

    $action = 'updated';
    $regionName = $user['regionName'];
    
} else {
      http_response_code(400);
    echo json_encode(['status' => false, 'message' => 'خطای دریافت کاربر!']);
    exit;
}


/* =========================
   6. userstatus 
========================= */
$data['ozvsandogh'] = 1;
$data['sabegheO'] = 1;
$data['madrak'] = 1;
if (!empty($data['ozvsandogh']) && !empty($userId)) {

    $chkAddr = $db->query("SELECT user_id FROM userstatus WHERE user_id = {$userId}");

    if ($chkAddr->num_rows) {
        $db->query("
            UPDATE userstatus
            SET nationalId  = '{$nationalId}',ozvsandogh = {$data['ozvsandogh']}, sabegheO = {$data['sabegheO']}, madrak = {$data['madrak']}
            WHERE user_id = {$userId}
        ");
    } else {
        $db->query("
            INSERT INTO userstatus (user_Id,nationalId ,ozvsandogh, sabegheO, madrak)
            VALUES ({$userId},'{$nationalId}',{$data['ozvsandogh']}, '{$data['sabegheO']}', '{$data['madrak']}')
        ");
    }
}
/////jwt////////
$jwtPayload = [
    'national_id'   => $nationalId,
    'roles'  => $data['roles'],
    'regionName' => $regionName,
];

$token = generateJWT($jwtPayload);


////////////////
/* =========================
   6. API Response
========================= */
$db->query("COMMIT");
echo json_encode([
    'status' => true,
    'action' => $action, // inserted | updated
    'user' => [
        'id' => $nationalId,
        'national_id' => $nationalId,
        'personnel_code' => $user['personnel_code'],
        'orgPositionDesc' => $user['org_position_desc'],
        'full_name' => trim($user['first_name'] . ' ' . $user['last_name']),
        'roles' => [$data['roles']],
        'userType' => [$user['user_type']],
        'regionName' => $regionName,
        'regionId' => $user['region_id']
    ],
    'token' => $token
], JSON_UNESCAPED_UNICODE);
