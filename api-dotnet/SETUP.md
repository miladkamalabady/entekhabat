# Entekhabat API - .NET Core 8

## نصب و راه‌اندازی

### پیش‌نیازها
1. نصب [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. MySQL در حال اجرا (همان دیتابیس PHP)

### اجرا

```bash
cd api-dotnet
dotnet restore
dotnet run
```

پیش‌فرض روی `http://localhost:5000` اجرا می‌شود.

---

## نقشه endpoint‌ها (PHP → .NET)

| PHP route               | .NET URL                            | Method   |
|-------------------------|-------------------------------------|----------|
| `?route=AccountLogin`   | `/api/AccountLogin`                 | POST/GET |
| `?route=user-status`    | `/api/user-status`                  | GET      |
| `?route=getUsers`       | `/api/getUsers`                     | GET      |
| `?route=updateUser`     | `/api/updateUser`                   | POST     |
| `?route=getEXECUTIVEList`| `/api/getEXECUTIVEList`            | GET      |
| `?route=ChangeState`    | `/api/ChangeState`                  | POST     |
| `?route=getConfig`      | `/api/getConfig`                    | GET      |
| `?route=getSystemSchedule`| `/api/getSystemSchedule`          | GET      |
| `?route=saveSystemSchedule`| `/api/saveSystemSchedule`        | POST     |
| `?route=getRegions`     | `/api/getRegions`                   | GET      |
| `?route=saveRegionMaxVotes`| `/api/saveRegionMaxVotes`        | POST     |
| `?route=getCandidsList` | `/api/getCandidsList`               | GET      |
| `?route=getstateCandid` | `/api/getstateCandid`               | GET      |
| `?route=FinalSubmit`    | `/api/FinalSubmit`                  | POST     |
| `?route=canselRequestCANDIDATE`| `/api/canselRequestCANDIDATE` | POST |
| `?route=createVoteToken`| `/api/createVoteToken`              | GET      |
| `?route=insertVote`     | `/api/insertVote`                   | POST     |
| `?route=getVote`        | `/api/getVote`                      | GET      |
| `?route=getInfoVote`    | `/api/getInfoVote`                  | GET      |
| `?route=searchUserVotes`| `/api/searchUserVotes`              | GET      |
| `?route=UploadUserDocuments`| `/api/UploadUserDocuments`      | POST     |
| `?route=UpdateDocumentReview`| `/api/UpdateDocumentReview`    | POST     |
| `?route=advertisementsSave`| `/api/advertisementsSave`        | POST     |
| `?route=getAdvertisements`| `/api/getAdvertisements`          | GET      |
| `?route=deleteAdv`      | `/api/deleteAdv`                    | POST     |
| `?route=increaseViewAdd`| `/api/increaseViewAdd`              | POST     |
| `?route=getObjections`  | `/api/getObjections`                | GET      |
| `?route=saveObjection`  | `/api/saveObjection`                | POST     |
| `?route=updateObjectionStatus`| `/api/updateObjectionStatus`  | POST     |
| `?route=downloadObjectionFile`| `/api/downloadObjectionFile`  | GET      |
| `?route=startLiveChat`  | `/api/startLiveChat`                | POST     |
| `?route=closeLiveChat`  | `/api/closeLiveChat`                | POST     |
| `?route=getLiveChatMessages`| `/api/getLiveChatMessages`      | GET      |
| `?route=getLiveChatSessions`| `/api/getLiveChatSessions`      | GET      |
| `?route=sendLiveChatMessage`| `/api/sendLiveChatMessage`      | POST     |
| `?route=getSupportTickets`| `/api/getSupportTickets`          | GET      |
| `?route=saveSupportTicket`| `/api/saveSupportTicket`          | POST     |
| `?route=replySupportTicket`| `/api/replySupportTicket`        | POST     |
| `?route=getFinalResultsApprovalStatus`| `/api/getFinalResultsApprovalStatus` | GET |
| `?route=submitFinalResultsApproval`| `/api/submitFinalResultsApproval` | POST |
| `?route=setFinalResultsApproval`| `/api/setFinalResultsApproval` | POST |
| `?route=getLogs`        | `/api/getLogs`                      | GET      |
| `?route=submitFeedback` | `/api/submitFeedback`               | POST     |

---

## تغییر در فرانت‌اند Vue.js

در فایل‌های axios فرانت‌اند، آدرس base از:
```
/api/index.php?route=XXX
```
به:
```
/api/XXX
```
تغییر می‌دهید.

---

## ساختار پروژه

```
api-dotnet/
├── Program.cs                    # ورودی اصلی + پیکربندی JWT/CORS
├── appsettings.json              # تنظیمات دیتابیس، JWT، CORS
├── EntekhabatApi.csproj          # بسته‌های NuGet
├── wwwroot/uploads/              # فایل‌های آپلود شده (همان /api/uploads در PHP)
├── Services/
│   ├── DatabaseService.cs        # اتصال به MySQL (معادل database.php)
│   ├── JwtService.cs             # تولید توکن JWT (معادل config.php)
│   └── JalaliService.cs          # تبدیل تاریخ شمسی (معادل jdf.php)
└── Controllers/
    ├── AuthController.cs          # AccountLogin
    ├── UserController.cs          # user-status, getUsers, updateUser, ...
    ├── ElectionController.cs      # getConfig, getSystemSchedule, getRegions, ...
    ├── CandidateController.cs     # getCandidsList, FinalSubmit, ...
    ├── VoteController.cs          # createVoteToken, insertVote, getVote, ...
    ├── DocumentController.cs      # UploadUserDocuments, UpdateDocumentReview
    ├── AdvertisementController.cs # advertisementsSave, getAdvertisements, ...
    ├── ObjectionController.cs     # getObjections, saveObjection, ...
    ├── LiveChatController.cs      # startLiveChat, sendLiveChatMessage, ...
    ├── SupportTicketController.cs # getSupportTickets, saveSupportTicket, ...
    ├── ApprovalController.cs      # getFinalResultsApprovalStatus, ...
    └── LogFeedbackController.cs   # getLogs, submitFeedback
```
