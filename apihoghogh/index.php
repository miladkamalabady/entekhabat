<?php

require_once __DIR__ . '/helpers.php';
require_once __DIR__ . '/controllers/ContractController.php';
require_once __DIR__ . '/controllers/PersonnelController.php';
require_once __DIR__ . '/controllers/NewContractController.php';
require_once __DIR__ . '/controllers/SalaryTypeController.php';
require_once __DIR__ . '/controllers/ParamController.php';
require_once __DIR__ . '/controllers/SalaryController.php';
require_once __DIR__ . '/controllers/OvertimeController.php';
require_once __DIR__ . '/controllers/SchoolController.php';
require_once __DIR__ . '/controllers/DeductionController.php';
require_once __DIR__ . '/controllers/SpecialPaymentController.php';
require_once __DIR__ . '/controllers/DayJobController.php';

setCorsHeaders();

$uri = strtok($_SERVER['REQUEST_URI'], '?');
$uri = preg_replace('#^/apihoghogh#', '', $uri);
$uri = rtrim($uri, '/');

try {
    $contract    = new ContractController();
    $personnel   = new PersonnelController();
    $newContract = new NewContractController();
    $salaryType  = new SalaryTypeController();
    $salary      = new SalaryController();
    $param       = new ParamController();
    $overtime    = new OvertimeController();
    $school      = new SchoolController();
    $deduction       = new DeductionController();
    $specialPayment  = new SpecialPaymentController();
    $dayJob      = new DayJobController();

    switch ($uri) {

        // ===== قراردادهای استخدام =====
        case '/Contract/GetList':       $contract->getList();       break;
        case '/Contract/GetById':       $contract->getById();       break;
        case '/Contract/Delete':        $contract->delete();        break;
        case '/Contract/Copy':          $contract->copy();          break;
        case '/Contract/UpdateStatus':  $contract->updateStatus();  break;

        // ===== لیست کارکنان =====
        case '/Personnel/GetList':      $personnel->getList();      break;
        case '/Personnel/GetById':      $personnel->getById();      break;
        case '/Personnel/Save':         $personnel->save();         break;
        case '/Personnel/Delete':       $personnel->delete();       break;
        case '/Personnel/Import':       $personnel->import();       break;
        case '/Personnel/GetSchools':   $personnel->getSchools();   break;

        // ===== ثبت قرارداد جدید =====
        case '/NewContract/GetDetails':            $newContract->getDetails();            break;
        case '/NewContract/GetPersonnelList':      $newContract->getPersonnelList();      break;
        case '/NewContract/GetPaymentTypes':       $newContract->getPaymentTypes();       break;
        case '/NewContract/GetNextContractNumber': $newContract->getNextContractNumber(); break;
        case '/NewContract/Save':                  $newContract->save();                  break;

        // ===== انواع پرداختی =====
        case '/SalaryType/GetPaymentList':   $salaryType->getPaymentList();   break;
        case '/SalaryType/SavePayment':      $salaryType->savePayment();      break;
        case '/SalaryType/DeletePayment':    $salaryType->deletePayment();    break;

        // ===== انواع کسورات =====
        case '/SalaryType/GetDeductionList': $salaryType->getDeductionList(); break;
        case '/SalaryType/SaveDeduction':    $salaryType->saveDeduction();    break;
        case '/SalaryType/DeleteDeduction':  $salaryType->deleteDeduction();  break;

        // ===== حقوق و دستمزد =====
        case '/Salary/Rebuild':          $salary->rebuild();          break;
        case '/Salary/DeleteProcessing': $salary->deleteProcessing(); break;
        case '/Salary/GetSlips':         $salary->getSlips();         break;
        case '/Salary/GetAnnualSlips':   $salary->getAnnualSlips();   break;

        // ===== پارامترهای حقوق =====
        case '/Param/GetSalaryParams':  $param->getSalaryParams();  break;
        case '/Param/SaveSalaryParams': $param->saveSalaryParams(); break;

        // ===== اضافه کاری =====
        case '/Overtime/GetList': $overtime->getList(); break;
        case '/Overtime/Save':    $overtime->save();    break;
        case '/Overtime/Delete':  $overtime->delete();  break;

        // ===== مدارس =====
        case '/School/GetList': $school->getList(); break;
        case '/School/Save':    $school->save();    break;
        case '/School/Delete':  $school->delete();  break;

        // ===== پرداختی ویژه =====
        case '/SpecialPayment/GetList': $specialPayment->getList(); break;
        case '/SpecialPayment/Save':    $specialPayment->save();    break;
        case '/SpecialPayment/Delete':  $specialPayment->delete();  break;

        // ===== کسورات پرسنل =====
        case '/PersonnelDeduction/GetList': $deduction->getList(); break;
        case '/PersonnelDeduction/Save':    $deduction->save();    break;
        case '/PersonnelDeduction/Delete':  $deduction->delete();  break;

        // ===== روزکاری =====
        case '/DayJob/GetList':    $dayJob->getList();    break;
        case '/DayJob/GetSchools': $dayJob->getSchools(); break;
        case '/DayJob/Save':       $dayJob->save();       break;
        case '/DayJob/SaveBatch':  $dayJob->saveBatch();  break;
        case '/DayJob/Delete':     $dayJob->delete();     break;

        default:
            errorResponse('مسیر یافت نشد', 404);
    }
} catch (PDOException $e) {
    errorResponse('خطای پایگاه داده: ' . $e->getMessage(), 500);
} catch (Throwable $e) {
    errorResponse('خطای سرور: ' . $e->getMessage(), 500);
}
