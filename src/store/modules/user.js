
import apiservice from '../../store/modules/apiservice'
import { isAuthGuardActive } from '../../constants/config'
import { getCurrentUser } from '../../utils'
export default {
  state: {
    currentUser: isAuthGuardActive ? getCurrentUser() : null,
    userstatusInfo: null,
    UploadUserDocumentsInfo: null,
    confirmRegisterInfo: null,
    ChangeStateInfo: null,
    stateCandidInfo: null,
    EXECUTIVEListInfo: null,
    ConfigInfo:null,
    requestStatus: null,
    hasActiveRequest: false,
    electionStatusAll: 'inactive',
    loginError: null,
    processing: false,
    sidebarVisible: true,
    panelactiveparvande: 'home',
    candidateFiles: {
      photo: null,
      degree: null,
      noAddiction: null
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
    confirmRegisterInfo: state => state.confirmRegisterInfo,
    ChangeStateInfo: state => state.ChangeStateInfo,
    stateCandidInfo: state => state.stateCandidInfo,
    EXECUTIVEListInfo: state => state.EXECUTIVEListInfo,
    ConfigInfo: state => state.ConfigInfo,
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
      state.processing = false
      state.loginError = null
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
    setuserstatusInfo(state, payload) {
      state.userstatusInfo = payload
      state.loginError = null
    }, setUploadUserDocumentsInfo(state, payload) {
      state.UploadUserDocumentsInfo = payload
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
    },setConfigInfo(state, payload) {
      state.ConfigInfo = payload
      state.loginError = null
    },
    clearError(state) {
      state.loginError = null
      state.processing = false
    }
  },
  actions: {


    LoginUserSSO({ commit }, payload) {
      commit('setProcessing', true)
      setTimeout(() => {
        try {
          localStorage.removeItem('user')
          apiservice({ name: "AccountLogin", params: payload }, { commit })
            .then(response => {
              if (response.status) {
                commit('setUser', { ...response.user, token: response.token })
                commit('setHasActiveRequest', response.hasActiveRequest)
                commit('clearError')
              }
            })
        } catch (e) {
          commit('setLogout')
          commit('setError', 'خطا در ورود. لطفاً مجدد تلاش نمایید')
        } finally {
          commit('setProcessing', false)
        }
      }, 2000);
    },
    userstatus({ commit }, payload) {
      apiservice({ name: "userstatus", params: payload }, { commit })
        .then(response => {
          if (response.status) {
            commit('setuserstatusInfo', response.data)
            commit('clearError')
          }
        })

    }, UploadUserDocuments({ commit }, payload) {
      apiservice({ name: "UploadUserDocuments", params: payload }, { commit })
        .then(response => {
          if (response.status) {
            commit('setUploadUserDocumentsInfo', response.data)
            commit('clearError')
          }
        })
    }, confirmRegister({ commit }, payload) {
      apiservice({ name: "confirmRegister", params: payload }, { commit })
        .then(response => {
          if (response.status) {
            commit('setconfirmRegisterInfo', response.data)
            commit('clearError')
          }
        })
    }, ChangeState({ commit }, payload) {
      apiservice({ name: "ChangeState", params: payload }, { commit })
        .then(response => {
          if (response.status) {
            commit('setChangeStateInfo', response.data)
            commit('clearError')
          }
        })
    }, getstateCandid({ commit }, payload) {
      apiservice({ name: "getstateCandid", params: payload }, { commit })
        .then(response => {
          if (response.status) {
            commit('setstateCandidInfo', response.data)
            commit('clearError')
          }
        })

    }, getEXECUTIVEList({ commit }, payload) {
      apiservice({ name: "getEXECUTIVEList", params: payload }, { commit })
        .then(response => {
          if (response.status) {
            commit('setEXECUTIVEListInfo', response.data)
            commit('clearError')
          }
        })
    },advertisementsSave({ commit }, payload) {
      return apiservice({ name: "advertisementsSave", params: payload }, { commit })
        .then(response => {
          if (response?.status) {
            commit('clearError')
          }
          return response
        })
    },async getAdvertisements({ commit }, payload) {
      const response = await apiservice({ name: "getAdvertisements", params: payload }, { commit });
      if (response?.status) {
        commit('clearError');
      }
      return response.data;
    },async increaseViewAdd({ commit }, payload) {
      const response = await apiservice({ name: "increaseViewAdd", params: payload }, { commit });
      if (response?.status) {
        commit('clearError');
      }
      return response.data;
    },async deleteAdv({ commit }, payload) {
      const response = await apiservice({ name: "deleteAdv", params: payload }, { commit });
      if (response?.status) {
        commit('clearError');
      }
      return response.data;
    },async getConfig({ commit }, payload) {
      const response = await apiservice({ name: "getConfig" }, { commit });
      if (response?.status) {
         commit('setConfigInfo', response.data)
        commit('clearError');
      }
      return response.data;
    },async getCandidsList({ commit }, payload) {
      const response = await apiservice({ name: "getCandidsList" }, { commit });
      if (response?.status) {
        commit('clearError');
      }
      return response.data;
    },async insertVote({ commit }, payload) {
      const response = await apiservice({ name: "insertVote", params: payload }, { commit });
      if (response?.status) 
        commit('clearError');
      return response.data;
    },async getVote({ commit }, payload) {
      const response = await apiservice({ name: "getVote"}, { commit });
      if (response?.status) 
        commit('clearError');
      return response.data;
    },

  }
}
