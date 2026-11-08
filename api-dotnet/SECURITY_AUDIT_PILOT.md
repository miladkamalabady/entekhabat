# Pilot API authorization audit

Static route inventory; role and scope checks require runtime tests.

| Controller | Endpoint | Auth | Role check indicator | Region indicator |
|---|---|---|---|---|
| Advertisement | POST /api/advertisementsSave | JWT | no | no |
| Advertisement | GET /api/getAdvertisements | JWT | yes | yes |
| Advertisement | POST /api/deleteAdv | JWT | yes | no |
| Advertisement | POST /api/approveAdvertisement | JWT | yes | yes |
| Advertisement | GET /api/getPublicAdvertisements | Public | no | yes |
| Advertisement | POST /api/increaseViewAdd | JWT | no | no |
| Announcement | GET /api/getPublicAnnouncements | Public | no | no |
| Announcement | GET /api/getAnnouncements | JWT | no | yes |
| Announcement | GET /api/getMyAnnouncements | JWT | yes | no |
| Announcement | POST /api/saveAnnouncement | JWT | yes | yes |
| Announcement | POST /api/deleteAnnouncement | JWT | yes | no |
| Approval | GET /api/getFinalResultsApprovalStatus | JWT | yes | yes |
| Approval | GET /api/getRegionalApprovalStatuses | JWT | no | yes |
| Approval | POST /api/submitFinalResultsApproval | JWT | yes | yes |
| Approval | POST /api/publishFinalResults | JWT | yes | yes |
| Approval | POST /api/setFinalResultsApproval | JWT | yes | yes |
| Auth | POST /api/AccountLogin | Public | no | no |
| Auth | GET /api/AccountLogin | Public | yes | yes |
| Auth | POST /api/SsoAccountLogin | Public | no | no |
| Auth | GET /api/SsoAccountLogin | Public | yes | yes |
| Candidate | GET /api/getCandidsList | JWT | no | no |
| Candidate | GET /api/getstateCandid | JWT | no | no |
| Candidate | POST /api/FinalSubmit | JWT | yes | no |
| Candidate | POST /api/canselRequestCANDIDATE | JWT | yes | no |
| Document | POST /api/UploadUserDocuments | JWT | no | no |
| Document | POST /api/UpdateDocumentReview | JWT | yes | yes |
| Document | GET /api/getCandidateDocuments | JWT | no | no |
| Document | POST /api/UpdateUserDocuments | JWT | no | no |
| Election | GET /api/getConfig | JWT | no | no |
| Election | GET /api/getSystemSchedule | JWT | no | no |
| Election | POST /api/saveSystemSchedule | JWT | yes | no |
| Election | GET /api/getRegions | Public | yes | yes |
| Election | POST /api/saveRegionMaxVotes | JWT | yes | yes |
| Election | POST /api/invalidateElection | JWT | yes | yes |
| Election | GET /api/getElectionInvalidations | JWT | no | yes |
| ElectionMinutes | GET /api/regionalElectionMinutes | JWT | yes | yes |
| ElectionMinutes | POST /api/uploadSignedElectionMinutes | JWT | yes | yes |
| LiveChat | POST /api/startLiveChat | JWT | yes | yes |
| LiveChat | POST /api/closeLiveChat | JWT | yes | no |
| LiveChat | GET /api/getLiveChatMessages | JWT | no | no |
| LiveChat | GET /api/getLiveChatSessions | JWT | yes | yes |
| LiveChat | POST /api/sendLiveChatMessage | JWT | yes | no |
| LogFeedback | GET /api/getLogs | JWT | yes | no |
| LogFeedback | GET /api/getRecentLogs | JWT | yes | yes |
| LogFeedback | POST /api/submitFeedback | JWT | no | no |
| Objection | GET /api/getObjections | JWT | yes | yes |
| Objection | POST /api/saveObjection | JWT | no | no |
| Objection | POST /api/updateObjectionStatus | JWT | yes | yes |
| Objection | GET /api/downloadObjectionFile | JWT | yes | no |
| RegionMapping | GET /api/provincialRegionMappings | JWT | yes | yes |
| RegionMapping | POST /api/setProvincialRegionMapping | JWT | yes | yes |
| SupportTicket | GET /api/getSupportTickets | JWT | yes | yes |
| SupportTicket | POST /api/saveSupportTicket | JWT | yes | yes |
| SupportTicket | POST /api/replySupportTicket | JWT | yes | yes |
| User | GET /api/user-status | JWT | no | no |
| User | GET /api/getUsers | JWT | yes | yes |
| User | POST /api/updateUser | JWT | yes | yes |
| User | GET /api/getEXECUTIVEList | JWT | yes | yes |
| User | POST /api/ChangeState | JWT | yes | yes |
| Vote | GET /api/createVoteToken | JWT | no | no |
| Vote | POST /api/createVoteToken | JWT | no | no |
| Vote | POST /api/insertVote | JWT | no | yes |
| Vote | GET /api/getVote | JWT | no | yes |
| Vote | GET /api/getInfoVote | JWT | no | yes |
| Vote | GET /api/searchUserVotes | JWT | yes | yes |
| Vote | GET /api/getFinalCandidateResults | JWT | yes | yes |
| Vote | GET /api/exportResults | JWT | yes | yes |
| Vote | GET /api/getVotingCandidates | JWT | no | yes |
| Vote | GET /api/getAvailableVoteRegions | JWT | no | yes |

## Confirmed issues

- Election/saveSystemSchedule: admin gate added (90e4d8e).
- Document/UpdateDocumentReview: jurisdiction gate added (c3f21c0).
- Vote/insertVote: transaction-owned per-voter application lock and duplicate checks added (e08947b).
- Vote/createVoteToken and insertVote: server-side schedule checks added (e08947b).
- Vote/searchUserVotes: supervisors can access identity-linked votes; technical-only access and sensitive-read audit remain OPEN.
- Vote/EnsureAdminSecondPasswordSetting: hardcoded fallback password remains OPEN.
- Approval/getRegionalApprovalStatuses: cross-province status access remains OPEN.
- Election/getElectionInvalidations: broad authenticated read remains OPEN.
- Advertisement/deleteAdv: staff region scope remains OPEN.
- Joint dual-passcode final approval intentionally preserved.

## Required verification

- Run dotnet build and SQL Server integration tests.
- Verify transaction rollback and locking with 20-100 simultaneous vote submissions.
- Check for existing duplicate participant/vote rows and add database unique constraints scoped to an election.
- Test every role, region, province and endpoint with positive and negative access cases.
- Route inventory indicators do not prove authorization correctness.