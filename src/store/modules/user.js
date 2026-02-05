
import apiservice from '../../store/modules/apiservice'
import { isAuthGuardActive } from '../../constants/config'
import { getCurrentUser } from '../../utils'
const USE_MOCK = true
export default {
  state: {
    currentUser: isAuthGuardActive ? getCurrentUser() : null,
    requestStatus: null,
    hasActiveRequest: false,
    electionActive:true,
    loginError: null,
    processing: false,
    ///RTB//
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
    electionActive: state => state.electionActive,
    requestStatus: state => state.requestStatus,
    panelactiveparvande: state => state.panelactiveparvande,
    currentUser: state => state.currentUser,
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
    },setRequestStatus(state, payload) {
      state.requestStatus = payload
    },setHasActiveRequest(state, payload) {
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

    clearError(state) {
      state.loginError = null
      state.processing = false
    }
  },
  actions: {


    async LoginUserSSO({ commit }, payload) {
      commit('setProcessing', true)

      try {
        let response

        if (USE_MOCK) {
          response = {
            user: {
              nationalId: "1234567890",
              fullName: "میلاد فراهانی",
              token: "test",
              roles: [payload?.myToken]
            },
            requestStatus: 'SUBMITTED',
            hasActiveRequest: true
          }
        } else {
          response = await apiservice(
            { name: "AccountLogin", params: payload }
          )
        }

        // --- ذخیره تمیز ---
        

        commit('setUser', response.user)
        commit('setRequestStatus', response.requestStatus)
        commit('setHasActiveRequest', response.hasActiveRequest)
        commit('clearError')

      } catch (e) {
        commit('setLogout')
        commit('setError', 'خطا در ورود. لطفاً مجدد تلاش نمایید')
      } finally {
        commit('setProcessing', false)
      }
    },

    /////rtb
    GetEmployeeInfo({
      commit,
      state
    }, payload) {
      apiservice({ name: "GetEmployeeInfo", params: payload }, { commit })
        .then(response => {
          if (response)
            commit('setEmployeeInfo', response)
          else
            commit('setError', response.data)
        })
    },

  }
}
