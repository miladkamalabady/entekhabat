import apiservice from '../../store/modules/apiservice'
import { isAuthGuardActive } from '../../constants/config'
import { getCurrentUser } from '../../utils'
import { mapSearchUsers,mapMaxVotesRegions,mapSearchUsersRequest, mapCurrentUser, mapScheduleConfig, mapLoginResponse, mapSaveSchedule, mapGetSchedule } from '../../utils/Mapper'
export default {
  state: {
    currentUser: isAuthGuardActive ? getCurrentUser() : null,
    announcements: [],
    userstatusInfo: null,
    UploadUserDocumentsInfo: null,
    UpdateUserDocumentsInfo: null,
    confirmRegisterInfo: null,
    ChangeStateInfo: null,
    stateCandidInfo: null,
    EXECUTIVEListInfo: null,
    ConfigInfo: null,
    SystemScheduleInfo: null,
    requestStatus: localStorage.getItem('requestStatus') || null,
    hasActiveRequest: false,
    electionStatusAll: 'inactive',
    loginError: null,
    processing: false,
    sidebarVisible: true,
    panelactiveparvande: 'home',
    candidateFiles: {
      photo: null,
      degree: null,
      noAddiction: null,
      soPishine: null,
      ravan: null
    }
  },
  getters: {
    sidebarVisible: state => state.sidebarVisible,
    electionStatusAll: state => state.electionStatusAll,
    requestStatus: state => state.requestStatus,
    panelactiveparvande: state => state.panelactiveparvande,
    currentUser: state => state.currentUser,
    userstatusInfo: state => state.userstatusInfo,
    UploadUserDocumentsInfo: state => state.UploadUserDocumentsInfo,
    UpdateUserDocumentsInfo: state => state.UpdateUserDocumentsInfo,
    confirmRegisterInfo: state => state.confirmRegisterInfo,
    ChangeStateInfo: state => state.ChangeStateInfo,
    stateCandidInfo: state => state.stateCandidInfo,
    announcements: state => state.announcements,
    EXECUTIVEListInfo: state => state.EXECUTIVEListInfo,
    ConfigInfo: state => state.ConfigInfo,
    SystemScheduleInfo: state => state.SystemScheduleInfo,
    processing: state => state.processing,
    loginError: state => state.loginError,
    candidateFiles: state => state.candidateFiles,
    LoginSSOInfo: state => state.LoginSSOInfo,
  },
  mutations: {
    setCandidateFiles(state, payload) {
      state.candidateFiles = { ...payload };
    },

    setUser(state, payload) {
      localStorage.setItem('user', JSON.stringify(payload))

      state.currentUser = payload

      state.processing = false
      state.loginError = null
    }, setRequestStatus(state, payload) {
      state.requestStatus = payload
      if (payload) localStorage.setItem('requestStatus', payload)
      else localStorage.removeItem('requestStatus')
    }, SetelectionStatusAll(state, payload) {
      state.electionStatusAll = payload
    }, setHasActiveRequest(state, payload) {
      state.hasActiveRequest = payload
    },
    setsidebarVisible(state, payload) {
      state.sidebarVisible = payload
    }, setrotbeAnswers(state, payload) {
      state.rotbeAnswers = payload
    }, setrouteothertab(state, payload) {
      state.routeothertab = payload
    },
    setpanelactiveparvande(state, payload) {
      state.panelactiveparvande = payload
    },
    setLogout(state) {
      state.currentUser = null
      state.requestStatus = null
      state.processing = false
      state.loginError = null
      localStorage.removeItem('requestStatus')
    },
    setProcessing(state, payload) {
      state.processing = payload
      state.loginError = null
    },
    setError(state, payload) {
      if (!((payload?.status || payload?.Status) && payload?.Message))
        // state.loginError = payload.Message
        // else
        state.loginError = payload
      state.processing = false
    },
    setAnnouncements(state, payload) {
      state.announcements = payload || [];
    },
    setuserstatusInfo(state, payload) {
      state.userstatusInfo = payload
      state.loginError = null
    }, setUploadUserDocumentsInfo(state, payload) {
      state.UploadUserDocumentsInfo = payload
      state.loginError = null
    }, setUpdateUserDocumentsInfo(state, payload) {
      state.UpdateUserDocumentsInfo = payload
      state.loginError = null
    }, setconfirmRegisterInfo(state, payload) {
      state.confirmRegisterInfo = payload
      state.loginError = null
    }, setChangeStateInfo(state, payload) {

      state.ChangeStateInfo = payload
      state.loginError = null
    }, setstateCandidInfo(state, payload) {
      state.stateCandidInfo = payload
      state.loginError = null
    }, setEXECUTIVEListInfo(state, payload) {
      state.EXECUTIVEListInfo = payload
      state.loginError = null
    }, setConfigInfo(state, payload) {
      state.ConfigInfo = payload
      state.loginError = null
    }, setSystemScheduleInfo(state, payload) {
      state.SystemScheduleInfo = payload
      state.loginError = null
    },
    clearError(state) {
      state.loginError = null
      state.processing = false
    }
  },
  actions: {
    LoginUserSSO({ commit }, payload) {
      commit("setProcessing", true);

      localStorage.removeItem("user");

      const request = !payload.role
        ? apiservice(
          { name: "AccountLogin", params: payload },
          { commit }
        )
        : apiservice(
          {
            name: "AccountLoginTest",
            params: {
              nationalId: payload.code,
              role: payload.role
            }
          },
          { commit }
        );

      request
        .then(response => {
          if (!response?.succeeded || !response?.data?.status) {
            commit("setError", response?.messages?.join("\n") || "خطا در ورود");
            return;
          }
          response.data.user.roles = response.data.user.roles.map(role => role.toUpperCase());
          const result = response.data;


          commit("setUser", {
            ...mapCurrentUser(result.user),
            token: result.token,
            refreshToken: result.refreshToken
          });

          commit("setHasActiveRequest", result.hasActiveRequest ?? false);
          commit("clearError");
        })
        .catch(() => {
          commit("setLogout");
          commit("setError", "خطا در ورود. لطفاً مجدد تلاش نمایید");
        })
        .finally(() => {
          commit("setProcessing", false);
        });
    },
    userstatus({ commit }, payload) {
      apiservice({ name: "userstatus", params: payload }, { commit })
        .then(response => {
          if (response.succeeded) {
            commit('setuserstatusInfo', response.data)
            commit('clearError')
          }
        })
    }, async Getroles({ commit }, payload) {
      const response = await apiservice({ name: "roles", params: payload }, { commit })
      if (response.succeeded) {
        commit('clearError')
      }
      return response
    }, signOut({ commit }, payload) {
      localStorage.removeItem('user')
    }, async UploadUserDocuments({ commit }, payload) {
      await apiservice({ name: "UploadUserDocuments", params: payload }, { commit })
        .then(response => {
          if (response.succeeded) {
            commit('setUploadUserDocumentsInfo', response.data)
            commit('clearError')
          }
        })
    }, async UpdateUserDocuments({ commit }, payload) {
      await apiservice({ name: "UpdateUserDocuments", params: payload }, { commit })
        .then(response => {
          if (response.succeeded) {
            commit('setUpdateUserDocumentsInfo', response.data)
            commit('clearError')
          }
        })
    }, confirmRegister({ commit }, payload) {
      apiservice({ name: "confirmRegister", params: payload }, { commit })
        .then(response => {
          if (response.succeeded) {
            commit('setconfirmRegisterInfo', response.data)
            commit('clearError')
          }
        })
    }, async ChangeState({ commit }, payload) {
      const response = await apiservice({ name: "ChangeState", params: payload }, { commit });
      if (response.succeeded) {
        commit('setChangeStateInfo', response.data);
        commit('clearError');
      }
      return response;
    }, async UpdateDocumentReview({ commit }, payload) {
      const response = await apiservice({ name: "UpdateDocumentReview", params: payload }, { commit });
      if (response.succeeded) {
        commit('clearError');
      }
      return response;
    }, getstateCandid({ commit }, payload) {
      apiservice({ name: "getstateCandid", params: payload }, { commit })
        .then(response => {
          if (response.succeeded) {
            commit('setstateCandidInfo', response.data)
            commit('clearError')
          }
        })

    }, getEXECUTIVEList({ commit }, payload) {
      apiservice({ name: "getEXECUTIVEList", params: payload }, { commit })
        .then(response => {
          if (response.succeeded) {
            commit('setEXECUTIVEListInfo', response.data)
            commit('clearError')
          }
        })
    }, async advertisementsSave({ commit }, payload) {
      const response = await apiservice({ name: "advertisementsSave", params: payload }, { commit })
      if (response.succeeded) {
        commit('clearError')
      }
      return response
    }, async getAdvertisements({ commit }, payload) {
      const response = await apiservice({ name: "getAdvertisements", params: payload }, { commit });
      
      if (response?.succeeded) {
        commit('clearError');
        return response.data;
      }
      else return response;
    }, async getUsers({ commit }, payload) {
          const request = mapSearchUsersRequest(payload);
      const response = await apiservice({ name: "getUsers", params: request }, { commit });
      if (response.succeeded) {
        const result = mapSearchUsers(response);
        commit('clearError');
        return result;
      }
    }, async updateUser({ commit }, payload) {
      const response = await apiservice({ name: "updateUser", params: payload }, { commit });
      if (response.succeeded) {
        commit('clearError');
      }
      return response.data;
    }, async assignUserRoles({ commit }, payload) {
      const { id, userRoles } = payload || {};
     
      const response = await apiservice(
        { name: "assignUserRoles", params: { id, userRoles } },
        { commit, inline_insert: true }
      );
      if (response?.succeeded || response === true) {
        commit('clearError');
      }
      return response;
    }, async increaseViewAdd({ commit }, payload) {
      const response = await apiservice({ name: "increaseViewAdd", params: payload }, { commit });
      if (response.succeeded) {
        commit('clearError');
      }
      return response.data;
    }, async deleteAdv({ commit }, payload) {
      const response = await apiservice({ name: "deleteAdv", params: payload }, { commit });
      if (response.succeeded) {
        commit('clearError');
      }
      return response.data;
    }, async getConfig({ commit }, payload) {
      const response = await apiservice({ name: "getConfig" }, { commit });
      if (response.succeeded) {
        const config = mapScheduleConfig(response);
        commit('setConfigInfo', config)
        commit('clearError');
      }
      return response.data;
    }, async getCandidsList({ commit }, payload) {
      const response = await apiservice({ name: "getCandidsList" }, { commit });
      if (response.succeeded) {
        commit('clearError');
      }
      return response.data;
    }, async getCandidateDocuments({ commit }) {
      const response = await apiservice({ name: "getCandidateDocuments" }, { commit });
      if (response.succeeded)
        commit('clearError');
      return response?.data || null;
    }, async insertVote({ commit }, payload) {
      const response = await apiservice({ name: "insertVote", params: payload }, { commit });
      if (response.succeeded) {
        commit('clearError');
        return response;
      }
      else return false
    }, async getVote({ commit }, payload) {
      const response = await apiservice({ name: "getVote" }, { commit });
      if (response?.status)
        commit('clearError');
      return response.data;
    }, async getInfoVote({ commit }, payload) {
      const response = await apiservice({ name: "getInfoVote", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response.data;
    }, async createVoteToken({ commit }, payload) {
      const response = await apiservice({ name: "createVoteToken" }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async submitFeedback({ commit }, payload) {
      const response = await apiservice({ name: "submitFeedback", params: payload }, { commit });
      if (response?.status)
        commit('clearError');
      return response.data;
    }, async getSystemSchedule({ commit }) {
      const response = await apiservice(
        { name: "getSystemSchedule" },
        { commit }
      );
      if (response.succeeded) {

        const rows = mapGetSchedule(response);

        commit("setSystemScheduleInfo", rows);
        commit("clearError");

        return rows;
      }

      return [];
    }, async saveSystemSchedule({ commit }, payload) {
      payload = mapSaveSchedule(payload)
      const response = await apiservice({ name: "saveSystemSchedule", params: payload }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async canselRequestCANDIDATE({ commit }, payload) {
      const response = await apiservice({ name: "canselRequestCANDIDATE" }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    },async getRegions({ commit }) {
    const [regionsRes, maxVotesRes] = await Promise.all([
        apiservice({ name: "getRegions" }, { commit }),
        apiservice({ name: "getMaxVotesRegions" }, { commit })
    ]);
    if (regionsRes.succeeded && maxVotesRes.succeeded) {
        return {
            data: regionsRes.data,
            areasByProvince: mapMaxVotesRegions(maxVotesRes.data)
        };
    }
    return {
        data: [],
        areasByProvince: {}
    };
}, 
async getFinalResultsApprovalStatus({ commit }) {
      const response = await apiservice({ name: "getFinalResultsApprovalStatus" }, { commit });
      if (response?.status)
        commit('clearError');
      return response?.data || null;
    },
    async submitFinalResultsApproval({ commit }, payload) {
      const response = await apiservice({ name: "submitFinalResultsApproval", params: payload }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async setFinalResultsApproval({ commit }, payload) {
      const response = await apiservice({ name: "setFinalResultsApproval", params: payload }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async getLogs({ commit }, payload) {
      const response = await apiservice({ name: "getLogs", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response?.data || [];
    }, async getAnnouncements({ commit }) {
      const response = await apiservice({ name: "getAnnouncements" }, { commit });
      if (response.succeeded) {
        commit('setAnnouncements', response.data);
        commit('clearError');
      }
      return response?.data || [];
    }, async getMyAnnouncements({ commit }) {
      const response = await apiservice({ name: "getMyAnnouncements" }, { commit });
      if (response?.status) commit('clearError');
      return response?.data || [];
    }, async saveAnnouncement({ commit }, payload) {
      const response = await apiservice({ name: "saveAnnouncement", params: payload }, { commit });
      if (response?.status) commit('clearError');
      return response;
    }, async deleteAnnouncement({ commit }, payload) {
      const response = await apiservice({ name: "deleteAnnouncement", params: payload }, { commit });
      if (response?.status) commit('clearError');
      return response;
    }, async getRecentLogs({ commit }, payload) {
      const response = await apiservice({ name: "getRecentLogs", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response?.data || [];
    }, async searchUserVotes({ commit }, payload) {
      const response = await apiservice({ name: "searchUserVotes", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response?.data || { items: [], summary: [] };
    }, async getSupportTickets({ commit }, payload) {
      const response = await apiservice({ name: "getSupportTickets", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    },

    async startLiveChat({ commit }, payload) {
      const response = await apiservice({ name: "startLiveChat", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async getLiveChatSessions({ commit }, payload) {
      const response = await apiservice({ name: "getLiveChatSessions", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async getLiveChatMessages({ commit }, payload) {
      const response = await apiservice({ name: "getLiveChatMessages", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async sendLiveChatMessage({ commit }, payload) {
      const response = await apiservice({ name: "sendLiveChatMessage", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async closeLiveChat({ commit }, payload) {
      const response = await apiservice({ name: "closeLiveChat", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    },

    async saveSupportTicket({ commit }, payload) {
      const response = await apiservice({ name: "saveSupportTicket", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async replySupportTicket({ commit }, payload) {
      const response = await apiservice({ name: "replySupportTicket", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async saveRegionMaxVotes({ commit }, payload) { 
      const response = await apiservice({ name: "saveRegionMaxVotes", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response || { items: [], summary: [] };
    }, async getObjections({ commit }, payload) {
      const response = await apiservice({ name: "getObjections", params: payload || {} }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async saveObjection({ commit }, payload) {
      const response = await apiservice({ name: "saveObjection", params: payload }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async updateObjectionStatus({ commit }, payload) {
      const response = await apiservice({ name: "updateObjectionStatus", params: payload }, { commit });
      if (response?.status)
        commit('clearError');
      return response;
    }, async downloadObjectionFile({ commit }, payload) {
      const response = await apiservice({ name: "downloadObjectionFile", params: payload }, { commit });
      if (response)
        commit('clearError');
      return response;
    },

  }
}