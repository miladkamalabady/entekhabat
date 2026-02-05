const global = {
    recivecartApi: {
        url: "/Amar/GetHozeAddress",
        method: 'GET'
    },

    SapGetServices: {
        url: "/sap/GetServices/",
        method: 'GET'
    },
    SetAnswerByUser: {
        url: "/question/SetAnswerByUser",
        method: 'POST'
    },
    SetQuestionOfDayAnswerByUser: {
        url: "/question/SetQuestionOfDayAnswerByUser",
        method: 'POST'
    },
    GetScore: {
        url: "/question/GetScore",
        method: 'GET'
    },
    GetQuestion: {
        url: "/question/GetQuestion",
        method: 'GET'
    },
    GetQuestionOfDay: {
        url: "/question/GetQuestionOfDay",
        method: 'GET'
    },
    SapGetQuestionList: {
        url: "/sap/GetQuestionList",
        method: 'GET'
    },
    SapSetSurvayAnswer: {
        url: "/sap/SetSurvayAnswer",
        method: 'POST'
    },
    SapGetSurvayAnswer: {
        url: "/sap/GetSurvayAnswer",
        method: 'GET'
    },

    GetSanjeshSurvay: {
        url: "/General/GetSanjeshSurvay",
        method: 'GET'
    }, SaveSanjeshSurvay: {
        url: "/General/SaveSanjeshSurvay",
        method: 'POST'
    },

    Captcha: {
        url: "/Captcha/GetCaptcha/",
        method: 'GET'
    },
    Login: {
        url: "/Login/Login",
        method: 'POST'
    },
    Login2: {
        url: "/Login/Login2",
        method: 'POST'
    },
    UpdateProfileOther: {
        url: "/Login/updateprofile",
        method: 'GET'
    },
    GetClientData: {
        url: "/Login/GetClientData",
        method: 'POST'
    },
    SwitchUserType: {
        url: "/Login/SwitchUserType",
        method: 'GET'
    },
    LoginAsStudent: {
        url: "/Login/LoginAsStudent",
        method: 'POST'
    },
    LoginSSO: {
        url: "/sso/LoginSSO",
        method: 'GET'
    },
    LoginSSOAuth: {
        url: "/sso/LoginSSOAuth",
        method: 'GET'
    },
    GetManagerRequest: {
        url: "/External/GetManagerRequest",
        method: 'POST'
    }, getlistofschools: {
        url: "/Ranking/getlistofschools",
        method: 'GET'
    },
    UpdateManagerRequest: {
        url: "/Ranking/UpdateManagerRequest",
        method: 'POST'
    },
    DelManagerRequest: {
        url: "/Ranking/DelManagerRequest",
        method: 'GET'
    },
    GetManagerOstansRequest: {
        url: "/Ranking/GetManagerTestPlace",
        method: 'POST'
    },
    SetManagerTestRequest: {
        url: "/Ranking/SetManagerTestRequest",
        method: 'POST'
    },
    GetKarnamehNomrehGozari: {
        url: "/general/GetKarnamehNomrehGozari",
        method: 'GET'
    },
    GetRequestEdari: {
        url: "/general/GetSavabegh",
        method: 'GET'
    },
    PostRequestEdari: {
        url: "/general/SetSavabegh",
        method: 'POST'
    },
    GetRequestToTeaching: {
        url: "/RequestToTeaching/GetRequestToTeaching",
        method: 'GET'
    }, DelRequestToTeaching: {
        url: "/RequestToTeaching/DelRequestToTeaching",
        method: 'GET'
    },
    PostRequestToTeaching: {
        url: "/RequestToTeaching/CreateRequestToTeaching",
        method: 'POST'
    },

    GetMeduPortalRss: {
        url: "/general/GetMeduPortalRss",
        method: 'GET'
    }, GetServicesInfo: {
        url: "/general/GetServicesInfo",
        method: 'GET'
    },
    SetMyBiography: {
        url: "/Profile/SetMyBiography",
        method: 'POST'
    }, GetMyBiography: {
        url: "/Profile/GetMyBiography",
        method: 'GET'
    }, subscribe: {
        url: "/WebPush/SetClientPushSubscription",
        method: 'POST'
    },
    getmotahedin: {
        url: "/CommittedToServicePersonnel/GetCommittedServiceCrdRequest",
        method: 'GET'
    },
    insertmotahedin: {
        url: "/CommittedToServicePersonnel/UpdateCommittedServiceCrdRequest",
        method: 'POST'
    },
    AddRequestForWorkOnAbroad: {
        url: "/RequestForWorkOnAbroad/AddRequestForWorkOnAbroad",
        method: 'GET'
    }, GetProtestListByPaging: {
        url: "/RequestForWorkOnAbroad/GetProtestListByPaging",
        method: 'GET'
    }, GetProtestByIdForOperator: {
        url: "/RequestForWorkOnAbroad/GetProtestByIdForOperator",
        method: 'GET'
    }, SetParentMobileForShaadUsage: {
        url: "/General/SetParentMobileForShaadUsage",
        method: 'POST'
    },SetMobileForPersonnelForShaadUsage: {
        url: "/General/SetMobileForPersonnelForShaadUsage",
        method: 'POST'
    }, SendProtestAbroad: {
        url: "/RequestForWorkOnAbroad/SendProtest",
        method: 'POST'
    }, SetAnswerByOperator: {
        url: "/RequestForWorkOnAbroad/SetAnswerByOperator",
        method: 'POST'
    }, GetUserProtestAbroad: {
        url: "/RequestForWorkOnAbroad/GetUserProtest",
        method: 'GET'
    },
    GetAgentPeriodCountry: {
        url: "/RequestForWorkOnAbroad/GetAgentPeriodCountry",
        method: 'GET'
    }, GetAgentPeriodCatagory: {
        url: "/RequestForWorkOnAbroad/GetAgentPeriodCatagory",
        method: 'GET'
    }, GetAgentPeriodCatagoryAgrement: {
        url: "/RequestForWorkOnAbroad/GetAgentPeriodCatagoryAgrement",
        method: 'GET'
    }, GetAgentPeriodTestCity: {
        url: "/RequestForWorkOnAbroad/GetAgentPeriodTestCity",
        method: 'GET'
    }, GetAgentPeriodTestProvince: {
        url: "/RequestForWorkOnAbroad/GetAgentPeriodTestProvince",
        method: 'GET'
    }, GetAgentEmployeeByEmployeeID: {
        url: "/RequestForWorkOnAbroad/GetAgentEmployeeByEmployeeID",
        method: 'POST'
    }, GetAgentEmployeeByEmployeeKey: {
        url: "/RequestForWorkOnAbroad/GetAgentEmployeeByEmployeeKey",
        method: 'GET'
    }, insertabroad: {
        url: "/RequestForWorkOnAbroad/AgentRegisterRequest",
        method: 'POST'
    },
    GetAddChannell: {
        url: "/EnteghadatVaPishnahadatReciecerOffice/GetByRegionId",
        method: 'GET'
    }, PostAddChannell: {
        url: "/EnteghadatVaPishnahadatReciecerOffice/save",
        method: 'GET'
    },

    PayeshApproveForm: {
        url: "/Payesh/ApproveForm",
        method: 'GET'
    },PayeshGetForm: {
        url: "/Payesh/GetForm",
        method: 'GET'
    },PayeshSaveForm: {
        url: "/Payesh/SaveForm",
        method: 'POST'
    },

    getroidadshahed: {
        url: "/JavanehShahed/GetForms",
        method: 'GET'
    }, sabtroidadshahed: {
        url: "/JavanehShahed/SendForm",
        method: 'POST'
    }, DownloadFileShahed: {
        url: "/JavanehShahed/DownloadFile",
        method: 'GET'
    }, DeleteForm: {
        url: "/JavanehShahed/DeleteForm",
        method: 'GET'
    },GetAllByPaging: {
        url: "/JavanehShahed/GetAllByPaging",
        method: 'GET'
    },

    GetMoalemPrize: {
        url: "/TeacherAward/GetMeduTeacherAwardByUserId",
        method: 'GET'
    }, AddMeduTeacherAward: {
        url: "/TeacherAward/AddMeduTeacherAward",
        method: 'POST'
    }, DeleteMeduTeacherAward: {
        url: "/TeacherAward/DeleteMeduTeacherAward",
        method: 'GET'
    }, GetUnofficialTeacherAwardByUserId: {
        url: "/TeacherAward/GetUnofficialTeacherAwardByUserId",
        method: 'GET'
    }, AddUnofficialTeacherAward: {
        url: "/TeacherAward/AddUnofficialTeacherAward",
        method: 'POST'
    }, DeleteUnofficialTeacherAward: {
        url: "/TeacherAward/DeleteUnofficialTeacherAward",
        method: 'GET'
    }, GetStudentAwardByUserId: {
        url: "/TeacherAward/GetStudentAwardByUserId",
        method: 'GET'
    }, AddStudentAward: {
        url: "/TeacherAward/AddStudentAward",
        method: 'POST'
    },  DeleteStudentAward: {
        url: "/TeacherAward/DeleteStudentAward",
        method: 'GET'
    }, 


    getablborz: {
        url: "/JayezeAlborz/GetAll",
        method: 'GET'
    }, UploadFileAlborz: {
        url: "/JayezeAlborz/UploadFile",
        method: 'POST'
    }, DownloadFile: {
        url: "/JayezeAlborz/DownloadFile",
        method: 'GET'
    }

    , getabroad: {
        url: "/RequestForWorkOnAbroad/GetAgentRegister",
        method: 'GET'
    }, AgentRegisterRequestBankResponse: {
        url: "/RequestForWorkOnAbroad/AgentRegisterRequestBankResponse",
        method: 'POST'
    }, UploadFileAbroad: {
        url: "/RequestForWorkOnAbroad/UploadFile",
        method: 'POST'
    }, UploadFilemotahedin: {
        url: "/CommittedToServicePersonnel/UploadFile",
        method: 'POST'
    }, GetFile: {
        url: "/CommittedToServicePersonnel/GetFile",
        method: 'GET'
    },

    getformreshte: {
        url: "/CommittedToServicePersonnel/getformreshte",
        method: 'GET'
    }, getpostlist: {
        url: "/CommittedToServicePersonnel/getpostlist",
        method: 'GET'
    }, getreshtelist: {
        url: "/CommittedToServicePersonnel/getreshtelist",
        method: 'GET'
    }, insertformreshte: {
        url: "/CommittedToServicePersonnel/insertformreshte",
        method: 'POST'
    },

    GetSampadKarnameh: {
        url: "/Karnameh/GetSampadKarnameh",
        method: 'GET'
    },

    changeconfirmstate: {
        url: "/ApiToOtherSystem/SetMapping",
        method: 'POST'
    },
    getconfirmstate: {
        url: "/ApiToOtherSystem/GetMapping",
        method: 'GET'
    },

    SetServiceRequest: {
        url: "/General/SetService",
        method: 'POST'
    },

    LoginLegalSSO: {
        url: "/Login/LoginLegalSSO",
        method: 'POST'
    },
    SwitchUserType: {
        url: "/Login/SwitchUserType",
        method: 'GET'
    },
    logoutsso: {
        url: "/logoutsso",
        method: 'GET'
    },
    Logout: {
        url: "/Login/Logout",
        method: 'GET'
    },
    
    DateTimeNow: {
        url: "/general/DateTimeNow",
        method: 'GET'
    },
    GetMonasebatha: {
        url: "/general/GetMonasebatha",
        method: 'GET'
    },
    UpdateFamilyGraph: {
        url: "/general/UpdateFamilyGraph",
        method: 'GET'
    }, GetClientsProposalList: {
        url: "/SupportingForm/GetClientsProposalList",
        method: 'GET'
    }, UpdateClientsProposalExtraFields: {
        url: "/SupportingForm/UpdateClientsProposalExtraFields",
        method: 'POST'
    },

    sabtformContact: {
        url: "/SupportingForm/SaveFailedLogonSupportForm",
        method: 'POST'
    },
    requestsSupport: {
        url: "/SupportingForm/Getlist",
        method: 'GET'
    },
    SendRequestFormAnswer: {
        url: "/SupportingForm/SendRequestFormAnswer",
        method: 'POST'
    },
    GetAllMessageTag: {
        url: "/SupportingForm/GetAllMessageTag",
        method: 'GET'
    },
    GetAllMessageByUserId: {
        url: "/SupportingForm/GetAllMessageByUserId",
        method: 'GET'
    }, GetAllMessage46: {
        url: "/SupportingForm/GetListByUserIdFor46",
        method: 'GET'
    }, GetAllMessage44: {
        url: "/SupportingForm/GetListByUserIdFor44",
        method: 'GET'
    },GetAllMessage43: {
        url: "/SupportingForm/GetListByUserIdFor43",
        method: 'GET'
    }, LikeOrDislike: {
        url: "/SupportingForm/LikeOrDislike",
        method: 'GET'
    }, GetLikeOrDislikeOfForm: {
        url: "/SupportingForm/GetLikeOrDislikeOfForm",
        method: 'GET'
    },



    // getip: {
    //     url: "/general/getip",
    //     method: 'GET'
    // },
    RefreshOnlineStatus: {
        url: "/Login/RefreshOnlineStatus",
        method: 'GET'
    },
    GetAllPersonnelId: {
        url: "/general/getallpersonnelId",
        method: 'GET'
    },
    SetAllPersonnelId: {
        url: "/general/setpersonnelId",
        method: 'POST'
    },
    SchoolModalityType: {
        url: "/general/SchoolModalityType",
        method: 'GET'
    },
    GenderType: {
        url: "/general/GenderType",
        method: 'GET'
    },
    StageType: {
        url: "/general/StageTypeForCombo",
        method: 'GET'
    },


    //////////////

    GetMajors: {
        url: "/general/GetMajors",
        method: 'GET'
    },
    GetSchools: {
        url: "/general/GetMotevaseteSchool",
        method: 'GET'
    },
    GetNotification: {
        url: "/general/GetNotification",
        method: 'GET'
    },


    GetAminGraduationResult: {
        url: "/student/GetAminGraduationResult",
        method: 'GET'
    },

    GetStudentForeigner: {
        url: "/atba/GetStudentForeigner",
        method: 'POST'
    },
    PushStudentForeignerToSida: {
        url: "/atba/PushStudentForeignerToSida",
        method: 'POST'
    },
    UpdateInfoPersonnel: {
        url: "/Personnel/UpdateInfo",
        method: 'GET'
    },
    SendRequestToSabtAhvalStudent: {
        url: "/Student/SendRequestToSabtAhval",
        method: 'GET'
    },


    SendRequestToSabtAhval: {
        url: "/Tarmim/SendRequestToSabtAhval",
        method: 'GET'
    },

    GetOpenedSession: {
        url: "/UserSession/GetOpenedSession",
        method: 'GET'
    },
    RemoveSession: {
        url: "/UserSession/RemoveSession",
        method: 'GET'
    },
    GetNotifications: {
        url: "/StateManeger/GetNotifications",
        method: 'GET'
    },
    GetSliders: {
        url: "/StateManeger/GetSliders",
        method: 'GET'
    },
    GetPage: {
        url: "/StateManeger/GetPage",
        method: 'GET'
    },
    GetPage2: {
        url: "/StateManeger/GetPage2",
        method: 'GET'
    },
    GetSalarySlipByEmployeeCode: {
        url: "/Fish/GetSalarySlipByEmployeeCode",
        method: 'POST'
    }, EmployeeHasCompletedInfoForMy: {
        url: "/Fish/EmployeeHasCompletedInfoForMy",
        method: 'GET'
    },
    FishGetList: {
        url: "/Fish/GetList",
        method: 'POST'
    },
    FishGetFish: {
        url: "/Fish/GetFish",
        method: 'POST'
    },
    FishGetPdf: {
        url: "/Fish/GetPdf",
        method: 'POST'
    },

    GetNationalCardPhoto: {
        url: "/EmpAccount/GetSabteAhvalImage",
        method: 'GET'
    },InsertConfirmationDetail: {
        url: "/RtbN/InsertConfirmationDetail",
        method: 'POST'
    },CanselRequest: {
        url: "/RtbN/CancelRequest",
        method: 'POST'
    },FinalConfirmRequest: {
        url: "/RtbN/FinalConfirmRequest",
        method: 'POST'
    },SummeryRankingSelf: {
        url: "/RtbN/SummeryRankingSelf",
        method: 'GET'
    },
    AddCardSerial: {
        url: "/EmpAccount/AddCardSerial",
        method: 'GET'
    },GetEmployeeRankingStatus : {
        url: "/RtbN/GetEmployeeRankingStatus",
        method: 'GET'
    },ConfirmRequest : {
        url: "/RtbN/ConfirmRequest",
        method: 'POST'
    },RankingSelf : {
        url: "/RtbN/RankingSelf",
        method: 'GET'
    },sendanswerRanking : {
        url: "/RtbN/sendanswerRanking",
        method: 'POST'
    },ExternalServiceGetAll : {
        url: "/ExternalServiceGetAll",
        method: 'POST'
    },

    Form502: {
        url: "/OrdinancePrs/Form502",
        method: 'GET'
    },
    SignificationList: {
        url: "/OrdinancePrs/SignificationList",
        method: 'GET'
    },
    GetSignificationHeader: {
        url: "/OrdinancePrs/GetSignificationHeader",
        method: 'GET'
    },
    GetSignificationDetail: {
        url: "/OrdinancePrs/GetSignificationDetail",
        method: 'GET'
    },
    GetOrdinanceData: {
        url: "/OrdinancePrs/GetOrdinanceData",
        method: 'GET'
    },
    GetOrdinanceDetail: {
        url: "/OrdinancePrs/GetDetail",
        method: 'GET'
    }, GetDetailScoreWithTitle: {
        url: "/OrdinancePrs/GetDetailScoreWithTitle",
        method: 'GET'
    },
    GetOrdinanceScore: {
        url: "/OrdinancePrs/GetOrdinanceScore",
        method: 'GET'
    },GetReportInfo: {
        url: "/Reports/GetAll",
        method: 'GET'
    },


    tafaolHafez: {
        url: "/general/getfalehafez",
        method: 'GET'
    },
    GetChangesHistory: {
        url: "/general/GetChangesHistory",
        method: 'GET'
    },
    AmlakPrimaryResidentVerify: {
        url: "/general/AmlakPrimaryResidentVerify",
        method: 'POST'
    },
    GetSchool: {
        url: "/School/GetSchool",
        method: 'POST'
    }, GetSchoolbyregin: {
        url: "/School/GetSchoolbyregion",
        method: 'POST'
    },
    GetTeacherSchoolCourse: {
        url: "/Teacher/GetTeacherSchoolCourse",
        method: 'GET'
    },
    GetTeacherSchoolCourseClassmate: {
        url: "/Teacher/GetTeacherSchoolCourseClassmate",
        method: 'GET'
    },

    GetMyClassmate: {
        url: "/Student/GetMyClassmate",
        method: 'GET'
    },
    GetCourseScore: {
        url: "/Student/GetCourseScore",
        method: 'GET'
    },
    getProvinceslist: {
        url: "/general/getProvinceslist",
        method: 'GET'
    },
    TimeYearTypeValid: {
        url: "/general/TimeYearTypeValid",
        method: 'GET'
    },
    getlistmantaghe: {
        url: "/general/getlist",
        method: 'GET'
    },
    TimeDoreType: {
        url: "/general/TimeDoreType",
        method: 'GET'
    },

    SendVerificationFieldsToSida: {
        url: "/Student/SendVerificationFieldsToSida",
        method: 'POST'
    },
    sendmoghayerChild: {
        url: "/Student/ReportChildNursyMismatch",
        method: 'GET'
    },
    GetBussyStudingAsPDF: {
        url: "/Student/GetBussyStudingAsPDF",
        method: 'POST'
    },
    GetBussyStuding: {
        url: "/Student/GetBussyStuding",
        method: 'POST'
    }, ValidateBussyStuding: {
        url: "/Student/ValidateBussyStuding",
        method: 'GET'
    }, GetListOfBusyStudy: {
        url: "/Student/GetListOfBusyStudy",
        method: 'GET'
    },

    sendreqeteraz: {
        url: "/sahat/AddSahatContradictionRequest",
        method: 'POST'
    },
    getreqeteraz: {
        url: "/sahat/GetSahatContradictionRequest",
        method: 'GET'
    },

    GetNazar: {
        url: "/Survey/GetEmptyForm",
        method: 'GET'
    },
    SetNazar: {
        url: "/Survey/PostForm",
        method: 'POST'
    },

    GetDelegationToOthersList: {
        url: "/DelegationOfAuthority/GetDelegationToOthersList",
        method: 'GET'
    }, InsertDelegationOfAuthority: {
        url: "/DelegationOfAuthority/InsertDelegationOfAuthority",
        method: 'POST'
    }, DeleteDelegationOfAuthority: {
        url: "/DelegationOfAuthority/DeleteDelegationOfAuthority",
        method: 'GET'
    }, GetAllowSites: {
        url: "/DelegationOfAuthority/GetAllowSites",
        method: 'GET'
    }, GetDelegationToMeList: {
        url: "/DelegationOfAuthority/GetDelegationToMeList",
        method: 'GET'
    }, LoginSSOAsOtherPeople: {
        url: "/DelegationOfAuthority/LoginSSOAsOtherPeople",
        method: 'POST'
    },

    setuserchatgoftino: {
        url: "/chat/setKey",
        method: 'GET'
    },
    getuserchatgoftino: {
        url: "/chat/getKey",
        method: 'GET'
    },
    Setrotbe: {
        url: "/Ranking/UpdateRequest",
        method: 'POST'
    },
    Getrotbe: {
        url: "/Ranking/GetObjectionByEmployeeId",
        method: 'POST'
    },
    FinalConfirmObjectionRequestByEmployee: {
        url: "/Ranking/FinalConfirmObjectionRequestByEmployee",
        method: 'GET'
    },
    ObjectionRequest: {
        url: "/Ranking/ObjectionRequest",
        method: 'POST'
    },
    RefuseObjectionRequest: {
        url: "/Ranking/RefuseObjectionRequest",
        method: 'POST'
    },
    DeleteObjectionsThatDontHaveScaleCode: {
        url: "/Ranking/DeleteObjectionsThatDontHaveScaleCode",
        method: 'POST'
    },
    CreateScaleCodeForObjection: {
        url: "/Ranking/CreateScaleCodeForObjection",
        method: 'POST'
    },
    ConfirmAndSendToTheEvaluator: {
        url: "/Ranking/ConfirmAndSendToTheEvaluator",
        method: 'POST'
    },
    GetObjectionSummary: {
        url: "/Ranking/GetObjectionSummaryInfo",
        method: 'POST'
    },
    EditEterazRtb: {
        url: "/Ranking/EditEterazRtb",
        method: 'POST'
    },
    ReadEditStatus: {
        url: "/Ranking/ReadEditStatus",
        method: 'GET'
    },

    SaveIncorrectedRTBResult: {
        url: "/Ranking/SaveIncorrectedRTBResult",
        method: 'GET'
    },
    ApplayDate: {
        url: "/Ranking/ApplayDate",
        method: 'POST'
    },
    GetAttachmentDocByEmployeeKey: {
        url: "/Ranking/GetAttachmentDocByEmployeeKey",
        method: 'GET'
    },
    UploadFileData: {
        url: "/upload/UploadFileData",
        method: 'POST'
    },
    GetFileByEntityRowID: {
        url: "/Ranking/GetFiles",
        method: 'POST'
    },
    SetObjection: {
        url: "/Ranking/SetObjection",
        method: 'POST'
    },
    SendObjectionItems: {
        url: "/Ranking/SetFirstObjection",
        method: 'POST'
    },

    GetOghatbycity: {
        url: "/GetOghatbycity",
        method: 'GET'
    },
    Getweather: {
        url: "/Getweather",
        method: 'GET'
    },
    FindweatherCity: {
        url: "/FindweatherCity",
        method: 'GET'
    },
    DorehhaActiveForFaragir: {
        url: "/Personnel/DorehhaActiveForFaragir",
        method: 'GET'
    },
    SaveInvestigationForNotAppearChildsSida: {
        url: "/login/AddUnderGuardianshipChild",
        method: 'GET'
    }, SaveInvestigationForNotAppearChilds: {
        url: "/general/SaveInvestigationForNotAppearChilds",
        method: 'POST'
    },
    DeleteInvestigationForNotAppearChilds: {
        url: "/general/DeleteInvestigationForNotAppearChilds",
        method: 'POST'
    }, DeleteUnderGuardianship: {
        url: "/login/DeleteUnderGuardianship",
        method: 'GET'
    },
    ConfirmUserAgrement: {
        url: "/general/ConfirmUserAgrement",
        method: 'GET'
    },

    updateprofile: {
        url: "/general/updateprofile",
        method: 'POST'
    },
    setUniqCode: {
        url: "/general/SetUniqueCode",
        method: 'GET'
    },
    AllowedForProvinces: {
        url: "/general/AllowedForProvinces",
        method: 'GET'
    },

    GetKarnamehNewVersion: {
        url: "/Karnameh/GetKarnamehNewVersion",
        method: 'POST'
    },
    GetKarnameh: {
        url: "/Karnameh/GetKarnameh",
        method: 'POST'
    },
    GetKarnamehPak: {
        url: "/Karnameh/GetKarnamehPak",
        method: 'POST'
    },
    GetKarnamehFahak: {
        url: "/Karnameh/GetKarnamehFahak",
        method: 'POST'
    },

    GetHedayatTahsili: {
        url: "/Student/GetHedayatTahsili",
        method: 'GET'
    },
    GetReghbatTavanaee: {
        url: "/Student/GetReghbatTavanaee",
        method: 'GET'
    },
    GraduateMotevasetaAval: {
        url: "/Student/GraduateMotevasetaAval",
        method: 'GET'
    },
    InsertHedayaTahsiliTemplateType: {
        url: "/Student/InsertHedayaTahsiliTemplateType",
        method: 'POST'
    },
    GetHedayaTahsiliTemplateType: {
        url: "/Student/GetHedayaTahsiliTemplateType",
        method: 'GET'
    },

    reverse: {
        url: "/v5/reverse",
        method: 'GET'
    },
    staticmap: {
        url: "/v2/staticmap",
        method: 'GET'
    },
    GetContactToProvinces: {
        url: "/general/GetContactToProvinces",
        method: 'GET'
    },
    GetStudentPhoto: {
        url: "/Student/GetStudentPhoto",
        method: 'GET'
    }, GetTracking: {
        url: "/Pargar/TrackingLetter",
        method: 'GET'
    }, insertHoze: {
        url: "/TahsilateHozavi/SetData",
        method: 'POST'
    }, GetHoze: {
        url: "/TahsilateHozavi/GetData",
        method: 'GET'
    }, insertAmin: {
        url: "/TahsilateHozavi/SetDataAmin",
        method: 'POST'
    }, GetAmin: {
        url: "/TahsilateHozavi/GetDataAmin",
        method: 'GET'
    }, GetSchoolByCode: {
        url: "/School/GetSchoolByCode",
        method: 'GET'
    },
    GetRecievedFromBaleBotMessage: {
        url: "/AutoAnswerToQuestion/GetRecievedFromBaleBotMessage",
        method: 'GET'
    },
    faqGetlist: {
        url: "/faq/getlist",
        method: 'GET'
    },

    //cardp
    GetEmployeeInfoForCardRequest: {
        url: "/EmpCard/GetEmployeeInfoForCardRequest",
        method: 'GET'
    },CartpGetDetail: {
        url: "/EmpCard/GetLastEmpCardStatus",
        method: 'GET'
    },Sendmoghayer: {
        url: "/EmpCard/AddDiscrepancyOfInformation",
        method: 'GET'
    },CreateCard: {
        url: "/EmpCard/CreateCard",
        method: 'POST'
    },GetReasons: {
        url: "/EmpCard/GetReasons",
        method: 'GET'
    },UploadFileDataCart: {
        url: "/EmpCard/UploadFileData",
        method: 'POST'
    },

    //////Tavana
    tavanaGetDetail: {
        url: "/Tavana/GetDetail",
        method: 'GET'
    },
    tavanaGetList: {
        url: "/Tavana/GetList",
        method: 'GET'
    }, CancelRequestByOwner: {
        url: "/Tavana/CancelRequestByOwner",
        method: 'POST'
    },
    UpdateCreditCard: {
        url: "/Tavana/UpdateCreditCardByOwner",
        method: 'POST'
    }, GetLastPayment: {
        url: "/Tavana/GetLastPayment",
        method: 'GET'
    },
    tavanaGetGuarantorList: {
        url: "/Tavana/GetGuarantorList",
        method: 'GET'
    },
    tavanaAcceptedByGuarantor: {
        url: "/Tavana/AcceptByGuarantor",
        method: 'POST'
    },
    tavanaRejectByGuarantor: {
        url: "/Tavana/RejectByGuarantor",
        method: 'POST'
    },

    tavanaGetAllProvinces: {
        url: "/Tavana/GetAllProvinces",
        method: 'GET'
    },
    RegisterCreditCard: {
        url: "/Tavana/RegisterCreditCard",
        method: 'POST'
    },

    GetStatesV2: {
        url: "/isipayment/GeographyManagement/GetStatesV2",
        method: 'POST'
    },
    GetStateCitiesV2: {
        url: "/isipayment/GeographyManagement/GetStateCitiesV2",
        method: 'POST'
    },
    GetCategoriesV2: {
        url: "/isipayment/StoreManagement/GetCategoriesV2",
        method: 'POST'
    },
    GetSubCategoriesV2: {
        url: "/isipayment/StoreManagement/GetSubCategoriesV2",
        method: 'POST'
    },
    GetStoreByFilters: {
        url: "/isipayment/MerchantPriorityManagement/GetStoreByFilters",
        method: 'POST'
    }, GetStoreByIdV2: {
        url: "/isipayment/MerchantPriorityManagement/GetStoreByIdV2",
        method: 'POST'
    },
    ////////////chat
    getConversations: {
        url: "/isipayment/MerchantPriorityManagement/GetStoreByIdV2",
        method: 'POST'
    }, getContacts: {
        url: "/isipayment/MerchantPriorityManagement/GetStoreByIdV2",
        method: 'POST'
    }, searchContacts: {
        url: "/isipayment/MerchantPriorityManagement/GetStoreByIdV2",
        method: 'POST'
    },
    //////////rtbGet
    WorkingGroupGetAll: {
        url: "/rtb/WorkingGroup/GetAll",
        method: 'GET'
    },EncouragementGetAll: {
        url: "/rtb/Encouragement/GetAll",
        method: 'GET'
    },TeachingGetAll: {
        url: "/rtb/Teaching/GetAll",
        method: 'GET'
    },JobSkillGetAll: {
        url: "/rtb/JobSkill/GetAll",
        method: 'GET'
    },InnovationGetAll: {
        url: "/rtb/Innovation/GetAll",
        method: 'GET'
    },TakeRankInFestivalGetAll: {
        url: "/rtb/TakeRankInFestival/GetAll",
        method: 'GET'
    },AcademyDegreesGetAll: {
        url: "/rtb/AcademyDegrees/GetAll",
        method: 'GET'
    },AcademyDegreesGetAcademicHigherDegrees: {
        url: "/rtb/AcademyDegrees/GetAcademicHigherDegrees",
        method: 'GET'
    },AchievementGetAll: {
        url: "/rtb/Achievement/GetAll",
        method: 'GET'
    },ResearchGetAll: {
        url: "/rtb/Research/GetAll",
        method: 'GET'
    },    BookGetAll: {
        url: "/rtb/Book/GetAll",
        method: 'GET'
    },ArticleGetAll: {
        url: "/rtb/Article/GetAll",
        method: 'GET'
    },FacorGetAllFactorsPublic: {
        url: "/rtb/Facor/GetAllFactorsPublic",
        method: 'GET'
    },FacorGetAllFactorsSpecialized: {
        url: "/rtb/Facor/GetAllFactorsSpecialized",
        method: 'GET'
    },FacorGetAllFactorsProfessional: {
        url: "/rtb/Facor/GetAllFactorsProfessional",
        method: 'GET'
    },AcademyDegreesGetLastSummary: {
        url: "/rtb/AcademyDegrees/GetLastSummary",
        method: 'GET'
    },CommitedToServiceGetCommitedToService: {
        url: "/rtb/CommitedToService/GetAll",
        method: 'GET'
    },AccountLogin: {
        url: "/rtb/Account/Login",
        method: 'POST'
    },StationGetAll: {
        url: "/rtb/Station/GetAll",
        method: 'GET'
    },
    ////RtbCreate
    EncouragementRemove: {
        url: "/rtb/Encouragement/Remove",
        method: 'Delete'
    },TeachingRemove: {
        url: "/rtb/Teaching/Remove",
        method: 'Delete'
    },JobSkillRemove: {
        url: "/rtb/JobSkill/Remove",
        method: 'Delete'
    },InnovationRemove: {
        url: "/rtb/Innovation/Remove",
        method: 'Delete'
    },TakeRankInFestivalRemove: {
        url: "/rtb/TakeRankInFestival/Remove",
        method: 'Delete'
    },AchievementRemove: {
        url: "/rtb/Achievement/Remove",
        method: 'Delete'
    },ResearchRemove: {
        url: "/rtb/Research/Remove",
        method: 'Delete'
    },BookRemove: {
        url: "/rtb/Book/Remove",
        method: 'Delete'
    },ArticleRemove: {
        url: "/rtb/Article/Remove",
        method: 'Delete'
    },WorkingGroupRemove: {
        url: "/rtb/WorkingGroup/Remove",
        method: 'Delete'
    },CommitedToServiceRemove: {
        url: "/rtb/CommitedToService/Remove",
        method: 'Delete'
    },
    ////RtbCreate
    EncouragementCreate: {
        url: "/rtb/Encouragement/Create",
        method: 'POST'
    },TeachingCreate: {
        url: "/rtb/Teaching/Create",
        method: 'POST'
    },JobSkillCreate: {
        url: "/rtb/JobSkill/Create",
        method: 'POST'
    },InnovationCreate: {
        url: "/rtb/Innovation/Create",
        method: 'POST'
    },TakeRankInFestivalCreate: {
        url: "/rtb/TakeRankInFestival/Create",
        method: 'POST'
    },AcademyDegreesCreate: {
        url: "/rtb/AcademyDegrees/Create",
        method: 'POST'
    },AchievementCreate: {
        url: "/rtb/Achievement/Create",
        method: 'POST'
    },ResearchCreate: {
        url: "/rtb/Research/Create",
        method: 'POST'
    },BookCreate: {
        url: "/rtb/Book/Create",
        method: 'POST'
    },ArticleCreate: {
        url: "/rtb/Article/Create",
        method: 'POST'
    },WorkingGroupCreate: {
        url: "/rtb/WorkingGroup/Create",
        method: 'POST'
    },CommitedToServiceCreate: {
        url: "/rtb/CommitedToService/Create",
        method: 'POST'
    },
    ////RtbUpdate
    EncouragementUpdate: {
        url: "/rtb/Encouragement/Update",
        method: 'PUT'
    },TeachingUpdate: {
        url: "/rtb/Teaching/Update",
        method: 'PUT'
    },JobSkillUpdate: {
        url: "/rtb/JobSkill/Update",
        method: 'PUT'
    },InnovationUpdate: {
        url: "/rtb/Innovation/Update",
        method: 'PUT'
    },TakeRankInFestivalUpdate: {
        url: "/rtb/TakeRankInFestival/Update",
        method: 'PUT'
    },AcademyDegreesUpdate: {
        url: "/rtb/AcademyDegrees/Update",
        method: 'PUT'
    },AchievementUpdate: {
        url: "/rtb/Achievement/Update",
        method: 'PUT'
    },ResearchUpdate: {
        url: "/rtb/Research/Update",
        method: 'PUT'
    },BookUpdate: {
        url: "/rtb/Book/Update",
        method: 'PUT'
    },ArticleUpdate: {
        url: "/rtb/Article/Update",
        method: 'PUT'
    },WorkingGroupUpdate: {
        url: "/rtb/WorkingGroup/Update",
        method: 'PUT'
    },CommitedToServiceUpdate: {
        url: "/rtb/CommitedToService/Update",
        method: 'PUT'
    },
    //////rtbFiles
    GetHrmImage: {
        url: "/EmpAccount/GetHrmImage",
        method: 'GET'
    },
    GetEmployeeInfo: {
        url: "/rtb/EmpAccount/GetEmployeeInfo",
        method: 'GET'
    },
    GetAllFilesByIds: {
        url: "/hrmdms/upload/GetAllFilesByIds",
        method: 'POST'
    },CommitedToServiceGetFileById: {
        url: "/rtb/CommitedToService/GetFileById",
        method: 'GET'
    },AchievementGetFileById: {
        url: "/rtb/Achievement/GetFileById",
        method: 'GET'
    },TeachingGetFileById: {
        url: "/rtb/Teaching/GetFileById",
        method: 'GET'
    },ArticleGetFileById: {
        url: "/rtb/Article/GetFileById",
        method: 'GET'
    },BookGetFileById: {
        url: "/rtb/Book/GetFileById",
        method: 'GET'
    },InnovationGetFileById: {
        url: "/rtb/Innovation/GetFileById",
        method: 'GET'
    },TakeRankInFestivalGetFileById: {
        url: "/rtb/TakeRankInFestival/GetFileById",
        method: 'GET'
    },WorkingGroupGetFileById: {
        url: "/rtb/WorkingGroup/GetFileById",
        method: 'GET'
    },EncouragementGetFileById: {
        url: "/rtb/Encouragement/GetFileById",
        method: 'GET'
    },JobSkillGetFileById: {
        url: "/rtb/JobSkill/GetFileById",
        method: 'GET'
    },ResearchGetFileById: {
        url: "/rtb/Research/GetFileById",
        method: 'GET'
    },
};

export default global;
