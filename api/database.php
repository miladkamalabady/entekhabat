<?php
header("Access-Control-Allow-Origin: http://localhost:2000");
header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
header("Access-Control-Allow-Headers: Content-Type, Authorization, X-Requested-With");
header("Access-Control-Allow-Credentials: true");

if ($_SERVER['REQUEST_METHOD'] == 'OPTIONS') {
    http_response_code(200);
    exit();
}
define("DB_SERVR", "localhost");
define("DB_USER", "root");
define("DB_PASS", "");
define("DB_NAME", "entekhabat");
date_default_timezone_set('Asia/Tehran');


class DB
{
    private $connection;

    private $magic_quotes_active;

    private $real_escape_string_exist;

    function __construct()
    {
        $this->connect();
        //$this->magic_quotes_active=get_magic_quotes_gpc();
        $this->real_escape_string_exist = function_exists("mysql_real_escape_string");
    }

    public function connect()
    {
        $this->connection = mysqli_connect(DB_SERVR, DB_USER, DB_PASS);
        if (!$this->connection) {
            die('Database Connection failed. <br>' . mysqli_error($this->connection));
        } else {
            $db_select = mysqli_select_db($this->connection, DB_NAME);
            if (!$db_select) {
                die('Datbase Selecttion Failed. <br>' . mysqli_error($this->connection));
            } else {
                mysqli_set_charset($this->connection, "utf8");
            }
        }
    }
    public function escape($value)
    {
        return mysqli_real_escape_string($this->connection, $value);
    }
    public function disconnect()
    {
        if (isset($this->connection)) {
            mysqli_close($this->connection);
            unset($this->connection);
        }
    }

    public function query($sql)
    {
        $result = mysqli_query($this->connection, $sql);
        $this->confirm_query($result, $sql);
        return $result;
    }

    public function escape_value($value)
    {
        if ($this->real_escape_string_exist) {
            if ($this->magic_quotes_active) {
                $value = stripslashes($value);
            }
            $value = mysqli_real_escape_string($value);
        } else {
            if (!$this->magic_quotes_active) {
                $value = addslashes($value);
            }
        }
        return $value;
    }

    public function fetch_array($result)
    {
        return mysqli_fetch_array($result);
    }
    public function fetch_assoc($result)
    {
        return mysqli_fetch_assoc($result);
    }

    public function get_row($result)
    {
        return mysqli_fetch_row($result);
    }
    public function result($result, $row, $field)
    {
        return mysqli_result($result, $row, $field);
    }
    public function num_rows($result)
    {
        return mysqli_num_rows($result);
    }

    public function insert_id($result)
    {
        //get the last inserted id over the current db connection.
        return mysqli_insert_id($this->connection);
    }

    public function affected_rows()
    {
        return mysqli_affected_rows($this->connection);
    }

    private function confirm_query($result, $sql)
    {
        if (!$result) {
            $api = "675688333:AAH6zFP4xC86f9CjWbP1TEsrQ0-edn9qW0A";
            $url = 'https://tapi.bale.ai/bot' . $api . "/sendMessage";
            $params = array(
                'chat_id'   => 1098618258,
                'text'     => "خطای  سامانه انتخابات: " . mysqli_error($this->connection) .
                    $sql
            );
            $ch = curl_init();
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, $params);
            curl_setopt($ch, CURLOPT_URL, $url);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
            $result = curl_exec($ch);
            die("Database Query Failed. " . $sql);
        }
    }
}
$db = new DB();
