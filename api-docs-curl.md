# مستندات API — سامانه انتخابات
**Base URL:** `http://localhost/apiEntekhabat`

> برای endpointهای نیازمند Auth، مقدار `TOKEN` را از پاسخ AccountLogin بگیرید.

---

## احراز هویت

### AccountLogin — ورود ساده با کد ملی
```bash
curl -X GET "http://localhost/apiEntekhabat/AccountLogin?code=1234567890"
```
با تغییر نقش (فقط تست):
```bash
curl -X GET "http://localhost/apiEntekhabat/AccountLogin?code=1234567890&role=ADMIN"
```
**خروجی:**
```json
{
  "status": true,
  "action": "updated",
  "user": {
    "id": "1234567890",
    "national_id": "1234567890",
    "full_name": "نام خانوادگی",
    "roles": ["VOTER"],
    "regionName": "منطقه ۱ تهران",
    "regionId": 5
  },
  "token": "eyJ..."
}
```

---

### AccountLoginMain — ورود SSO آموزش و پرورش
```bash
curl -X POST "http://localhost/apiEntekhabat/AccountLoginMain" \
  -d "code=SSO_CODE_FROM_MEDU"
```
**خروجی:** مانند AccountLogin

---

## کاربر

### user-status — وضعیت شرایط احراز صلاحیت
```bash
curl -X GET "http://localhost/apiEntekhabat/user-status" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": {
    "membershipActive": true,
    "membershipYears": 5,
    "degree": "کارشناسی",
    "alreadyRegistered": false
  }
}
```

---

### getRegions — لیست استان‌ها و مناطق (بدون auth)
```bash
curl -X GET "http://localhost/apiEntekhabat/getRegions"
```
**خروجی:**
```json
{
  "status": true,
  "data": [{ "id": 1, "name": "تهران" }],
  "areasByProvince": {
    "1": [{ "id": 5, "name": "منطقه ۱", "maxVotes": 3 }]
  }
}
```

---

## ثبت‌نام کاندیدا

### FinalSubmit — ثبت نهایی کاندیداتوری
```bash
curl -X POST "http://localhost/apiEntekhabat/FinalSubmit" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tracking_code": "TRK123456"}'
```
**خروجی:**
```json
{
  "status": true,
  "message": "ثبت نام با موفقیت انجام شد",
  "data": { "nationalId": "1234567890", "tracking_code": "TRK123456" }
}
```

---

### getstateCandid — وضعیت درخواست کاندیداتوری
```bash
curl -X GET "http://localhost/apiEntekhabat/getstateCandid" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": {
    "requestStatus": "SUPERVISION_APPROVED",
    "tracking_code": "TRK123456",
    "reson": null,
    "edited_at_sh": "1403/10/15"
  }
}
```

---

### canselRequestCANDIDATE — انصراف از کاندیداتوری
```bash
curl -X POST "http://localhost/apiEntekhabat/canselRequestCANDIDATE" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{ "status": true, "message": "درخواست با موفقیت لغو شد" }
```

---

## مدارک

### UploadUserDocuments — آپلود مدارک
```bash
curl -X POST "http://localhost/apiEntekhabat/UploadUserDocuments" \
  -H "Authorization: Bearer TOKEN" \
  -F "user_photo=@/path/to/photo.jpg" \
  -F "soPishine_cert=@/path/to/cert.pdf" \
  -F "ravan_cert=@/path/to/ravan.pdf" \
  -F "education_doc=@/path/to/edu.pdf"
```
**خروجی:**
```json
{
  "status": true,
  "message": "مدارک با موفقیت آپلود شد",
  "data": {
    "nationalId": "1234567890",
    "user_photo": "uploads/photo.jpg",
    "soPishine_cert": "uploads/cert.pdf"
  }
}
```

---

## مدیریت کاندیداها (اجرایی/ناظر)

### getEXECUTIVEList — لیست کاندیداهای منطقه
```bash
curl -X GET "http://localhost/apiEntekhabat/getEXECUTIVEList" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": [
    {
      "national_id": "1234567890",
      "full_name": "نام کاندیدا",
      "requestStatus": "PENDING",
      "documents": { "user_photo": "...", "soPishine_cert": "..." }
    }
  ]
}
```

---

### ChangeState — تغییر وضعیت کاندیدا
```bash
curl -X POST "http://localhost/apiEntekhabat/ChangeState" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"national_Id": "1234567890", "requestStatus": "SUPERVISION_APPROVED", "reason": ""}'
```
مقادیر مجاز `requestStatus`: `SUPERVISION_APPROVED`, `SUPERVISION_REJECTED`, `EXECUTIVE_APPROVED`, `EXECUTIVE_REJECTED`

**خروجی:**
```json
{
  "status": true,
  "message": "وضعیت با موفقیت تغییر کرد",
  "data": { "nationalId": "1234567890", "requestStatus": "SUPERVISION_APPROVED" }
}
```

---

### UpdateDocumentReview — بررسی یک مدرک
```bash
curl -X POST "http://localhost/apiEntekhabat/UpdateDocumentReview" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "national_Id": "1234567890",
    "documentKey": "soPishine_cert",
    "reviewStatus": "approved"
  }'
```
مقادیر مجاز `documentKey`: `user_photo`, `education_doc`, `employment_cert`, `soPishine_cert`, `ravan_cert`

مقادیر مجاز `reviewStatus`: `approved`, `rejected`, `pending`

---

## تبلیغات

### advertisementsSave — ثبت/ویرایش تبلیغ
```bash
curl -X POST "http://localhost/apiEntekhabat/advertisementsSave" \
  -H "Authorization: Bearer TOKEN" \
  -F "title=عنوان تبلیغ" \
  -F "description=متن تبلیغ" \
  -F "type=1" \
  -F "slogan=شعار انتخاباتی" \
  -F "plans=برنامه‌ها" \
  -F "image=@/path/to/image.jpg"
```
برای ویرایش: `-F "id=5"`

---

### getAdvertisements — لیست تبلیغات
```bash
curl -X GET "http://localhost/apiEntekhabat/getAdvertisements" \
  -H "Authorization: Bearer TOKEN"
```

---

### increaseViewAdd — افزایش بازدید تبلیغ
```bash
curl -X POST "http://localhost/apiEntekhabat/increaseViewAdd" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 5}'
```

---

### deleteAdv — حذف/بازگردانی تبلیغ
```bash
curl -X POST "http://localhost/apiEntekhabat/deleteAdv" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"code": 5, "reson": "دلیل حذف"}'
```

---

## رأی‌گیری

### getConfig — زمان‌بندی رأی‌گیری
```bash
curl -X GET "http://localhost/apiEntekhabat/getConfig" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": {
    "active": true,
    "startDates": "1403/10/01",
    "startTime": "08:00",
    "endDates": "1403/10/02",
    "endTime": "18:00"
  }
}
```

---

### getCandidsList — لیست کاندیداها (صفحه رأی)
```bash
curl -X GET "http://localhost/apiEntekhabat/getCandidsList" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": [
    {
      "codeentekhabati": 101,
      "national_id": "...",
      "full_name": "نام کاندیدا",
      "user_photo": "uploads/..."
    }
  ]
}
```

---

### createVoteToken — دریافت توکن رأی (۲ دقیقه)
```bash
curl -X POST "http://localhost/apiEntekhabat/createVoteToken" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "vote_token": "abc123...",
  "expires_in": 120
}
```

---

### insertVote — ثبت رأی
```bash
curl -X POST "http://localhost/apiEntekhabat/insertVote" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"candidateIds": [101, 102, 103], "vote_token": "abc123..."}'
```
**خروجی:**
```json
{
  "status": true,
  "message": "رأی شما با موفقیت ثبت شد",
  "data": { "tracking_code": "VT20241201..." }
}
```

---

### getVote — رأی‌های ثبت‌شده کاربر
```bash
curl -X GET "http://localhost/apiEntekhabat/getVote" \
  -H "Authorization: Bearer TOKEN"
```

---

### getInfoVote — آمار کامل رأی‌گیری (ادمین)
```bash
curl -X GET "http://localhost/apiEntekhabat/getInfoVote" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": {
    "totalVoters": 1500,
    "totalVotes": 800,
    "voterParticipation": "53.33%",
    "Candidates": [{ "name": "نام", "voteCount": 200 }]
  }
}
```

---

### searchUserVotes — جستجوی رأی کاربر (ناظر/ادمین)
```bash
curl -X GET "http://localhost/apiEntekhabat/searchUserVotes?q=12345&limit=50" \
  -H "Authorization: Bearer TOKEN"
```

---

## نتایج نهایی

### getFinalResultsApprovalStatus — وضعیت تایید نتایج
```bash
curl -X GET "http://localhost/apiEntekhabat/getFinalResultsApprovalStatus" \
  -H "Authorization: Bearer TOKEN"
```

---

### submitFinalResultsApproval — تایید نهایی با رمز
```bash
curl -X POST "http://localhost/apiEntekhabat/submitFinalResultsApproval" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"role": "EXECUTIVE", "passcode1": "رمز-اجرایی", "passcode2": "رمز-ناظر"}'
```

---

### setFinalResultsApproval — فعال‌سازی نمایش نتایج
```bash
curl -X POST "http://localhost/apiEntekhabat/setFinalResultsApproval" \
  -H "Authorization: Bearer TOKEN"
```

---

## اطلاعیه‌ها

### getPublicAnnouncements — اطلاعیه‌های عمومی (بدون auth)
```bash
curl -X GET "http://localhost/apiEntekhabat/getPublicAnnouncements"
```
**خروجی:**
```json
{
  "status": true,
  "data": [
    { "id": 1, "title": "عنوان", "content": "متن", "created_at_shamsi": "1403/10/01" }
  ]
}
```

---

### getAnnouncements — اطلاعیه‌های کاربر
```bash
curl -X GET "http://localhost/apiEntekhabat/getAnnouncements" \
  -H "Authorization: Bearer TOKEN"
```

---

### getMyAnnouncements — اطلاعیه‌های من (ادمین/ناظر/اجرایی)
```bash
curl -X GET "http://localhost/apiEntekhabat/getMyAnnouncements" \
  -H "Authorization: Bearer TOKEN"
```

---

### saveAnnouncement — ثبت/ویرایش اطلاعیه
```bash
# اطلاعیه سراسری (فقط ادمین)
curl -X POST "http://localhost/apiEntekhabat/saveAnnouncement" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "عنوان", "content": "متن", "target_scope": "country", "target_ids": []}'

# اطلاعیه استانی
curl -X POST "http://localhost/apiEntekhabat/saveAnnouncement" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "عنوان", "content": "متن", "target_scope": "province", "target_ids": [23, 24]}'

# اطلاعیه منطقه‌ای
curl -X POST "http://localhost/apiEntekhabat/saveAnnouncement" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "عنوان", "content": "متن", "target_scope": "region", "target_ids": [5]}'

# ویرایش
curl -X POST "http://localhost/apiEntekhabat/saveAnnouncement" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 3, "title": "عنوان جدید", "content": "متن جدید", "target_scope": "country", "target_ids": []}'
```

---

### deleteAnnouncement — حذف اطلاعیه
```bash
curl -X POST "http://localhost/apiEntekhabat/deleteAnnouncement" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 3}'
```

---

## اعتراضات

### saveObjection — ثبت اعتراض
```bash
curl -X POST "http://localhost/apiEntekhabat/saveObjection" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "decisionType": "رد صلاحیت",
    "description": "متن اعتراض",
    "declaration": true,
    "subject": "موضوع",
    "reasons": ["دلیل اول", "دلیل دوم"],
    "urgency": "high"
  }'
```
با فایل ضمیمه:
```bash
curl -X POST "http://localhost/apiEntekhabat/saveObjection" \
  -H "Authorization: Bearer TOKEN" \
  -F "decisionType=رد صلاحیت" \
  -F "description=متن اعتراض" \
  -F "declaration=true" \
  -F "file[]=@/path/to/doc.pdf"
```

---

### getObjections — لیست اعتراضات
```bash
curl -X GET "http://localhost/apiEntekhabat/getObjections" \
  -H "Authorization: Bearer TOKEN"

# با فیلتر
curl -X GET "http://localhost/apiEntekhabat/getObjections?status=pending&trackingCode=OBJ123" \
  -H "Authorization: Bearer TOKEN"
```

---

### updateObjectionStatus — تغییر وضعیت اعتراض
```bash
curl -X POST "http://localhost/apiEntekhabat/updateObjectionStatus" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 2, "status": "approved", "responseText": "اعتراض پذیرفته شد"}'
```
مقادیر مجاز: `pending`, `under_review`, `approved`, `rejected`, `cancelled`

---

### downloadObjectionFile — دانلود فایل اعتراض
```bash
curl -X GET "http://localhost/apiEntekhabat/downloadObjectionFile?id=5" \
  -H "Authorization: Bearer TOKEN" \
  -o output_file.pdf
```

---

## زمان‌بندی سیستم

### getSystemSchedule — دریافت مراحل
```bash
curl -X GET "http://localhost/apiEntekhabat/getSystemSchedule" \
  -H "Authorization: Bearer TOKEN"
```
**خروجی:**
```json
{
  "status": true,
  "data": [
    { "event_key": "candidate_registration", "event_name": "ثبت‌نام کاندیدا", "start_date": "...", "end_date": "..." }
  ]
}
```

---

### saveSystemSchedule — ذخیره زمان‌بندی
```bash
curl -X POST "http://localhost/apiEntekhabat/saveSystemSchedule" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "events": [
      { "key": "candidate_registration", "name": "ثبت‌نام", "startDate": "2025-01-01 08:00:00", "endDate": "2025-01-10 18:00:00" }
    ]
  }'
```

---

## مدیریت کاربران

### getUsers — لیست کاربران (ادمین/ناظر)
```bash
curl -X GET "http://localhost/apiEntekhabat/getUsers?limit=100" \
  -H "Authorization: Bearer TOKEN"
```

---

### updateUser — ویرایش نقش و منطقه کاربر
```bash
curl -X POST "http://localhost/apiEntekhabat/updateUser" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"national_id": "1234567890", "region_id": 5, "roles": "EXECUTIVE"}'
```
مقادیر مجاز `roles`: `ADMIN`, `SUPERVISOR`, `EXECUTIVE`, `CANDIDATE`, `VOTER`

---

### saveRegionMaxVotes — تنظیم ظرفیت منطقه
```bash
curl -X POST "http://localhost/apiEntekhabat/saveRegionMaxVotes" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"region_id": 5, "maxVotes": 3}'
```

---

## پشتیبانی

### getSupportTickets — لیست تیکت‌ها
```bash
curl -X GET "http://localhost/apiEntekhabat/getSupportTickets" \
  -H "Authorization: Bearer TOKEN"

# با فیلتر وضعیت
curl -X GET "http://localhost/apiEntekhabat/getSupportTickets?status=open" \
  -H "Authorization: Bearer TOKEN"
```

---

### saveSupportTicket — ثبت تیکت جدید
```bash
curl -X POST "http://localhost/apiEntekhabat/saveSupportTicket" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "subject": "مشکل در ثبت مدارک",
    "category": "technical",
    "description": "شرح مشکل",
    "priority": "high",
    "targetRole": "EXECUTIVE"
  }'
```
**خروجی:**
```json
{ "status": true, "ticketId": 3, "ticketCode": "TKT-1001" }
```

---

### replySupportTicket — پاسخ به تیکت
```bash
curl -X POST "http://localhost/apiEntekhabat/replySupportTicket" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"ticketId": 3, "message": "پاسخ به مشکل"}'

# بستن تیکت
curl -X POST "http://localhost/apiEntekhabat/replySupportTicket" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"ticketId": 3, "message": "مشکل حل شد", "closeTicket": true}'
```

---

## چت زنده

### startLiveChat — شروع چت
```bash
curl -X POST "http://localhost/apiEntekhabat/startLiveChat" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"subject": "سوال درباره ثبت‌نام"}'
```
**خروجی:**
```json
{ "status": true, "data": { "sessionId": 1, "status": "waiting" }, "onlineAgents": 2 }
```

---

### sendLiveChatMessage — ارسال پیام
```bash
curl -X POST "http://localhost/apiEntekhabat/sendLiveChatMessage" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"sessionId": 1, "message": "سلام، سوال دارم"}'
```

---

### getLiveChatMessages — دریافت پیام‌ها
```bash
curl -X GET "http://localhost/apiEntekhabat/getLiveChatMessages?sessionId=1" \
  -H "Authorization: Bearer TOKEN"

# فقط پیام‌های جدیدتر از id مشخص (polling)
curl -X GET "http://localhost/apiEntekhabat/getLiveChatMessages?sessionId=1&afterId=15" \
  -H "Authorization: Bearer TOKEN"
```

---

### getLiveChatSessions — لیست session‌ها
```bash
curl -X GET "http://localhost/apiEntekhabat/getLiveChatSessions?limit=20" \
  -H "Authorization: Bearer TOKEN"
```

---

### closeLiveChat — بستن چت
```bash
curl -X POST "http://localhost/apiEntekhabat/closeLiveChat" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"sessionId": 1}'
```

---

## لاگ‌ها

### getLogs — لاگ‌های سیستم (فقط ادمین)
```bash
curl -X GET "http://localhost/apiEntekhabat/getLogs?limit=500" \
  -H "Authorization: Bearer TOKEN"
```

---

### submitFeedback — ثبت نظر
```bash
curl -X POST "http://localhost/apiEntekhabat/submitFeedback" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"rating": 5, "comment": "سیستم خوبی بود"}'
```

---

## HTTP Status Codes

| کد | معنی |
|----|------|
| 200 | موفق |
| 400 | ورودی نامعتبر |
| 401 | توکن ندارد یا منقضی شده |
| 403 | دسترسی ندارد |
| 404 | endpoint یا رکورد یافت نشد |
| 429 | تعداد درخواست زیاد (rate limit) |
| 500 | خطای داخلی سرور |
