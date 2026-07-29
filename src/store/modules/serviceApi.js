const global = {
    AccountLogin: {
        url: "/api/v2/Tokens/sso-login",
        method: 'POST'
    },AccountLoginTest: {
        url: "/api/v2/Tokens/national-login",
        method: 'POST'
    }, userstatus: {
        url: "/api/v1/election/users/status",
        method: 'GET'
    }, UploadUserDocuments: {
        url: "/api/v1/election/documents/upload",
        method: 'POST'
    }, confirmRegister: {
        url: "/api/v1/election/candidates/final-submit",
        method: 'POST'
    }, canselRequestCANDIDATE: {
        url: "/api/v1/election/candidates/cancel",
        method: 'POST'
    }, getstateCandid: {
        url: "/api/v1/election/candidates/state",
        method: 'GET'
    }, getEXECUTIVEList: {
        url: "/api/v1/election/candidates/review",
        method: 'GET'
    }, ChangeState: {
        url: "/api/v1/election/candidates/review/status",
        method: 'POST'
    }, UpdateDocumentReview: {
        url: "/api/v1/election/documents/review",
        method: 'POST'
    }, advertisementsSave: {
        url: "/api/v1/election/advertisements/save",
        method: 'POST'
    }, getAdvertisements: {
        url: "/api/v1/election/advertisements/public",
        method: 'GET'
    },getUsers: {
        url: "/api/v1/election/users/search",
        method: 'POST'
    },assignUserRoles: { 
        url: '/api/v2/Identity/{{id}}/roles',
         method: 'POST' 
    },updateUser: {
        url: "/api/v1/election/users/assignment",
        method: 'PUT'
    },increaseViewAdd: {
        url: "/api/v1/election/advertisements/increase-view",
        method: 'POST'
    }, deleteAdv: {
        url: "/api/v1/election/advertisements/delete",
        method: 'POST'
    }, getConfig: {
        url: "/api/v1/election/schedule/config",
        method: 'GET'
    }, getCandidsList: {
        url: "/api/v1/election/candidates",
        method: 'GET'
    }, insertVote: {
        url: "/api/v1/election/votes",
        method: 'POST'
    }, getVote: {
        url: "/api/v1/election/votes/my",
        method: 'GET'
    }, getInfoVote: {
        url: "/api/v1/election/votes/info",
        method: 'GET'
    }, createVoteToken: {
        url: "/api/v1/election/votes/token",
        method: 'POST'
    }, submitFeedback: {
        url: "/api/v1/election/feedback",
        method: 'POST'
    }, getSystemSchedule: {
        url: "/api/v1/election/schedule",
        method: 'GET'
    }, saveSystemSchedule: {
        url: "/api/v1/election/schedule/save",
        method: 'POST'
    }, getObjections: {
        url: "/api/v1/election/objections",
        method: 'GET'
    }, saveObjection: {
        url: "/api/v1/election/objections",
        method: 'POST'
    }, roles: {
        url: "/api/v1/Roles",
        method: 'GET'
    },updateObjectionStatus: {
        url: "/api/v1/election/objections/status",
        method: 'POST'
    }, getRegions: {
        url: "/api/v1/election/regions",
        method: 'GET'
    }, getMaxVotesRegions: {
        url: "/api/v1/election/regions/max-votes",
        method: 'GET'
    }, getFinalResultsApprovalStatus: {
        url: "/api/v1/election/approval/final-results/status",
        method: 'GET'
    }, submitFinalResultsApproval: {
        url: "/api/v1/election/approval/final-results/submit",
        method: 'POST'
    }, setFinalResultsApproval: {
        url: "/api/v1/election/approval/final-results/set-active",
        method: 'POST'
    },getLogs: {
        url: "/api/v1/election/logs",
        method: 'GET'
    },getRecentLogs: {
        url: "/api/v1/election/logs",
        method: 'GET'
    },searchUserVotes: {
        url: "/api/v1/election/votes/search",
        method: 'GET'
    },saveRegionMaxVotes: {
        url: "/api/v1/election/regions/max-votes",
        method: 'POST'
    },getSupportTickets: {
        url: "/api/v1/election/support-tickets",
        method: 'GET'
    }, saveSupportTicket: {
        url: "/api/v1/election/support-tickets",
        method: 'POST'
    }, replySupportTicket: {
        url: "/api/v1/election/support-tickets/reply",
        method: 'POST'
    }, startLiveChat: {
        url: "/api/v1/election/live-chat/start",
        method: 'POST'
    }, getLiveChatSessions: {
        url: "/api/v1/election/live-chat/sessions",
        method: 'GET'
    }, getLiveChatMessages: {
        url: "/api/v1/election/live-chat/{sessionId}/messages",
        method: 'GET'
    }, sendLiveChatMessage: {
        url: "/api/v1/election/live-chat/message",
        method: 'POST'
    }, closeLiveChat: {
        url: "/api/v1/election/live-chat/close",
        method: 'POST'
    }, getAnnouncements: {
        url: "/api/v1/election/announcements/public",
        method: 'GET'
    }, getMyAnnouncements: {
        url: "/api/v1/election/announcements/managed",
        method: 'GET'
    }, saveAnnouncement: {
        url: "/api/v1/election/announcements",
        method: 'POST'
    }, deleteAnnouncement: {
        url: "/api/v1/election/announcements",
        method: 'DELETE'
    }

};

export default global;
