<template>
  <div class="px-2">
    <b-row class="m-0">


      <div v-if="(!sidebarVisible && isMobile()) || (!isMobile())" style="padding:0 15px;flex:1;"
        :style="isMobile() ? 'max-width: calc(100%);' : ''">
        <div class="mt-4" style="">

          <b-container fluid class="dashboard-wrapper d-flex align-items-center justify-content-center">
            <b-row class="w-100 justify-content-center">
              <b-col cols="12" md="10" lg="8" class="text-center">

                <!-- Header -->
                <div class="mb-4" v-if="ConfigInfo">
                  <h5 class="mb-2 text-primary-org">
                    سامانه انتخابات نمایندگان اعضای فرهنگی در هیأت امنای موسسه صندوق ذخیره فرهنگیان
                  </h5>

                  <ElectionStatusTimer :config-info="ConfigInfo" />
                </div>
                <div v-if="electionStatusAll == 'upcoming'">
                  <CustomStepper v-if="(currentUser?.roles.includes('CANDIDATE')) && requestStatus"
                    :steps="stepperSteps" :current-step="currentStep" :disabled="processing" />
                  <!-- درخواست ثبت شده -->
                  <b-alert
                    v-if="currentUser?.roles.includes('CANDIDATE') && (requestStatus === 'SUBMITTED' || requestStatus === 'EXECUTIVE_APPROVED' )"
                    variant="warning" show>
                    ⏳ درخواست شما ثبت شده و در حال بررسی توسط مراجع است
                    <br />
                    <b-button variant="outline-danger" class="mt-2" @click="canselRequest()">
                      انصراف
                    </b-button>
                  </b-alert>
                  <b-alert v-else-if="requestStatus === 'EXECUTIVE_REJECTED'" variant="warning" show>
                    ❌ مدارک شما تایید نشده است
                    <div v-if="stateCandidInfo?.reson" class="mt-2">
                      نظر هیأت اجرایی: {{ stateCandidInfo.reson }}
                    </div>
                    <div v-if="stateCandidInfo?.edited_at_sh" class="mt-1">
                      تاریخ آخرین ویرایش: {{ stateCandidInfo.edited_at_sh }}
                    </div>
                    <br />
                    <b-button variant="outline-info" class="mt-2" @click="submitAgain()">
                      ثبت مجدد
                    </b-button>
                  </b-alert>
                  <b-alert v-else-if="requestStatus === 'SUPERVISION_REJECTED'" variant="danger" show>
                    ❌ درخواست شما رد شده است
                    <div v-if="stateCandidInfo?.reson" class="mt-2">
                      نظر نهایی هیأت نظارت: {{ stateCandidInfo.reson }}
                    </div>
                    <div v-if="stateCandidInfo?.edited_at_sh" class="mt-1">
                      تاریخ آخرین ویرایش: {{ stateCandidInfo.edited_at_sh }}
                    </div>
                    <br />
                    <b-button variant="outline-danger" class="mt-2" @click="$router.push('/candidate/objection')">
                      ثبت اعتراض
                    </b-button>
                  </b-alert>
                  <b-alert v-else-if="requestStatus === 'SUPERVISION_APPROVED' && stateCandidInfo?.reson" variant="success" show>
                    درخواست شما تایید شده است
                    <div v-if="stateCandidInfo?.reson" class="mt-2">
                      نظر نهایی هیأت نظارت: {{ stateCandidInfo.reson }}
                    </div>
                    <div v-if="stateCandidInfo?.edited_at_sh" class="mt-1">
                      تاریخ آخرین ویرایش: {{ stateCandidInfo.edited_at_sh }}
                    </div>
                  </b-alert>
                </div>
                <!-- Menu -->
                <b-row>
                  <b-col v-for="(item, index) in filteredMenu" :key="index" cols="12" sm="4" class="mb-3">
                    <b-card class="dashboard-card h-100" :class="{ disabled: isMenuDisabled(item) }"
                      @click="!isMenuDisabled(item) ? handleClick(item) : ''">
                      <div class="icon mb-2">
                        <i :class="item.icon"></i>
                      </div>

                      <div class="title">
                        {{ item.title }}
                      </div>

                      <b-badge v-if="item.badge && electionStatusAll == 'active'" variant="warning" class="mt-2">
                        {{ item.badge }}
                      </b-badge>
                      <b-badge v-if="item.requiresFinalApproval && !isFinalResultAnnouncementActive" variant="secondary"
                        class="mt-2">
                        {{ finalApprovalStatusText }}
                      </b-badge>
                    </b-card>
                  </b-col>
                </b-row>

              </b-col>
            </b-row>
          </b-container>
        </div>
        <b-modal id="final-result-activation" v-model="showFinalResultActivation" title="فعال‌سازی اعلام نتایج نهایی"
          hide-footer centered>
          <p class="text-muted">اعضای هیأت اجرایی و هیأت نظارت باید هر کدام با رمز خود تایید ثبت کنند.</p>
          <b-form-group label="رمز هیأت اجرایی" label-for="approval-passcode">
            <b-form-input id="approval-passcode" v-model="approvalPasscode1" type="password"
              placeholder="رمز هیأت اجرایی را وارد کنید"></b-form-input>
          </b-form-group>
          <b-form-group label="رمز هیأت نظارت" label-for="approval-passcode">
            <b-form-input id="approval-passcode" v-model="approvalPasscode2" type="password"
              placeholder="رمز هیأت نظارت را وارد کنید"></b-form-input>
          </b-form-group>
          <!-- <small class="d-block mb-3 text-muted">
            وضعیت تاییدها: اجرایی {{ finalResultApprovals.executive ? '✅' : '⏳' }} |
            نظارت {{ finalResultApprovals.supervisor ? '✅' : '⏳' }}
          </small> -->
          <div class="d-flex justify-content-end">
            <b-button variant="outline-secondary" class="ml-2" @click="closeActivationModal">انصراف</b-button>
            <b-button variant="success" @click="activateFinalResults">تایید و ثبت</b-button>
          </div>
        </b-modal>
      </div>
    </b-row>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";
import Sidebar from "../../navs/Sidebar.vue";
import ElectionStatusTimer from "@/components/Common/ElectionStatusTimer";
import CustomStepper from "@/components/Common/CustomStepper";
export default {
  name: 'Dashboard',
  components: {
    Sidebar, CustomStepper, ElectionStatusTimer
  },
  computed: {
    ...mapGetters(["SystemScheduleInfo", "ConfigInfo", "sidebarVisible", "processing", "loginError", "currentUser", "stateCandidInfo", "electionStatusAll", "requestStatus"]),
    filteredMenu() {
      return this.menu.filter(item => {

        const roleAllowed = item?.roles?.includes(this.currentUser?.roles[0])

        const statusAllowed = item.visibleWhen
          ? item.visibleWhen(this.requestStatus)
          : true

        return roleAllowed && statusAllowed
      })
    }, currentStep() {
      return this.STATUS_STEP_MAP[this.requestStatus] ?? 0
    }, finalApprovalStatusText() {
      if (this.isFinalResultAnnouncementActive) {
        return 'فعال شده'
      }

      if (this.finalResultApprovals.executive && this.finalResultApprovals.supervisor) {
        return 'در انتظار فعال‌سازی'
      }

      if (this.finalResultApprovals.executive || this.finalResultApprovals.supervisor) {
        return 'در انتظار تایید هیأت مقابل'
      }

      return 'نیازمند تایید'
    },

  }, mounted() {
    if (!this.ConfigInfo && this.currentUser)
      this.getConfig()
    
  },
  data() {
    return {
      STATUS_STEP_MAP: {
        DRAFT: 0,
        SUBMITTED: 1,
        EXECUTIVE_APPROVED: 1,
        EXECUTIVE_REJECTED: 1,
        SUPERVISION_APPROVED: 3,
        SUPERVISION_REJECTED: 2
      },
      isMobile,
      stepperSteps: [
        { title: "ثبت درخواست", description: "ثبت درخواست", state: ["DRAFT"] },
        { title: "بررسی توسط مراجع", description: "بررسی توسط مراجع", state: ["SUBMITTED", "EXECUTIVE_APPROVED", "EXECUTIVE_REJECTED"] },
        { title: "ثبت اعتراض", description: "ثبت اعتراض", state: ["SUPERVISION_APPROVED", "SUPERVISION_REJECTED"] },
        { title: "ثبت تبلیغات", description: "ثبت تبلیغات", state: ["SUPERVISION_APPROVED"] },],
      menu: [
        {
          title: 'ثبت نام داوطلبان',
          route: '/candidate/request',
          icon: 'bi bi-person-plus',
          roles: ['VOTER'],
          electionStatusAll: 'upcoming',
          // visibleWhen: status => !status
        },
        {
          title: 'کارتابل اجرایی',
          route: '/supervisor/executive-dashboard',
          icon: 'bi bi-inbox',
          roles: ['EXECUTIVE'],
        },
        {
          title: 'کارتابل نظارت',
          route: '/supervisor/supervisor-dashboard',
          icon: 'bi bi-shield-check',
          roles: ['SUPERVISOR'],
        },
        {
          title: 'تبلیغات انتخابات',
          route: '/candidate/advertise',
          icon: 'bi bi-megaphone',
          roles: ['CANDIDATE'],
          electionStatusAll: 'upcoming',
          visibleWhen: status => status === 'SUPERVISION_APPROVED'
        },
        {
          title: 'اعتراض',
          route: '/candidate/objection',
          icon: 'bi bi-exclamation-triangle',
          roles: ['CANDIDATE'],
          visibleWhen: status => status === 'SUPERVISION_REJECTED'
        },
        {
          title: 'شرکت در انتخابات',
          route: '/User/votingPage',
          icon: 'bi bi-check2-square',
          roles: ['CANDIDATE', 'VOTER'],
          electionStatusAll: 'active',
          visibleWhen: electionStatusAll => electionStatusAll === 'active',
          badge: 'در حال رأی‌گیری'
        },
        {
          title: 'مشاهده نتایج مرحله اول',
          route: '/results/live-election',
          icon: 'bi bi-bar-chart',
          roles: ['ADMIN'],
          electionStatusAll: 'active',
          badge: 'نمایش زنده'
        },
        {
          title: 'مشاهده نتایج',
          route: '/results/final-election',
          icon: 'bi bi-bar-chart',
          roles: ['EXECUTIVE','ADMIN','VOTER','CANDIDATE','SUPERVISOR'],
          electionStatusAll: 'ended',
          badge: 'نمایش نهایی',
          visibleWhen: requiresFinalApproval => requiresFinalApproval === true,
          requiresFinalApproval: true
        },
        {
          title: 'زمان بندی سیستم',
          route: '/supervisor/system-schedule',
          icon: 'bi bi-bar-chart',
          roles: ['ADMIN'],
          badge: 'استانی و کشوری'
        }
      ],
      showFinalResultActivation: false,
      pendingMenuItem: null,
      pendingApprovalRole: null,
      approvalPasscode1: '',
      approvalPasscode2: '',
      isFinalResultAnnouncementActive: false,
      finalResultApprovals: {
        executive: false,
        supervisor: false
      }
    }
  },
  methods: {
    ...mapMutations(["setRequestStatus", "setUser"]),
    ...mapActions(["getConfig", "canselRequestCANDIDATE", "getSystemSchedule", "submitFinalResultsApproval", "getFinalResultsApprovalStatus"]),
    submitAgain(){
      this.canselRequest()
      // this.$router.push("/candidate/request")
    },
    isMenuDisabled(item) {
      if (item.electionStatusAll && item.electionStatusAll != this.electionStatusAll) {
        return true
      }

      if (item.requiresFinalApproval && !this.isFinalResultAnnouncementActive) {
        return !this.currentUser?.roles?.some(role => ['EXECUTIVE', 'SUPERVISOR'].includes(role))
      }

      return false
    },
    async syncFinalResultsStatus(showError = true) {
      try {
        const status = await this.getFinalResultsApprovalStatus()
        if (status) {
          this.finalResultApprovals = {
            executive: !!status.executiveApproved,
            supervisor: !!status.supervisorApproved
          }
          this.isFinalResultAnnouncementActive = (!!status.isActive || (this.finalResultApprovals.executive && this.finalResultApprovals.supervisor) && this.currentUser?.roles?.some(role => ['EXECUTIVE', 'SUPERVISOR'].includes(role)))
        }
      } catch (e) {
        if (showError) {
          this.$bvToast.toast('دریافت وضعیت فعال‌سازی از سرور انجام نشد.', {
            title: 'هشدار',
            variant: 'warning',
            solid: true
          })
        }
      }
    },
    closeActivationModal() {
      this.showFinalResultActivation = false
      this.pendingMenuItem = null
      this.pendingApprovalRole = null
      this.approvalPasscode1 = ''
      this.approvalPasscode2 = ''
    },
    async openFinalResultActivation(item) {
      if (!this.currentUser?.roles?.some(role => ['EXECUTIVE', 'SUPERVISOR'].includes(role))) {
        this.$bvToast.toast('اعلام نتایج نهایی هنوز توسط هیات اجرایی و هیات نظارت فعال نشده است.', {
          title: 'عدم دسترسی',
          variant: 'warning',
          solid: true
        })
        return
      }

      await this.syncFinalResultsStatus(false)

      if (this.isFinalResultAnnouncementActive && item?.route) {
        this.$router.push(item.route)
        return
      }

      const role = this.currentUser?.roles?.find(r => ['EXECUTIVE', 'SUPERVISOR'].includes(r))
      this.pendingApprovalRole = role
      this.pendingMenuItem = item
      this.showFinalResultActivation = true
    },
    async canselRequest() {
      const check = await this.canWithdrawByElectionTime()

      if (!check.ok) {
        await this.$bvModal.msgBoxOk(check.msg, {
          title: 'امکان انصراف/بارگذاری مجدد وجود ندارد',
          centered: true,
          okVariant: 'danger'
        })
        return
      }

      const resp = await this.canselRequestCANDIDATE()
      if (resp.status) {
        this.setRequestStatus("DRAFT")
        const cu = {
          ...this.currentUser,
          roles: ['VOTER']
        }
        this.setUser(cu)
      }
    },
    async canWithdrawByElectionTime() {
      if (!this.SystemScheduleInfo || !this.SystemScheduleInfo.length) {
        await this.getSystemSchedule()
      }

      const votingEvent = this.SystemScheduleInfo?.find(e => e.event_key === 'voting')


      if (!votingEvent?.start_date) {
        return { ok: false, msg: 'زمان انتخابات توسط سیستم تعریف نشده است' }
      }

      const electionStart = this.$moment(votingEvent.start_date, 'jYYYY-jMM-jDD HH:mm:ss')
      const now = this.$moment()
      const hoursLeft = electionStart.diff(now, 'hours', true)

      if (hoursLeft < 0) {
        return { ok: false, msg: 'زمان انتخابات آغاز شده است و امکان انصراف وجود ندارد' }
      }

      if (hoursLeft < 48) {
        return {
          ok: false,
          msg: `انصراف فقط در بازه ۴۸ ساعت مانده تا انتخابات مجاز است. زمان شروع انتخابات: ${electionStart.format('jYYYY/jMM/jDD ساعت HH:mm')}`
        }
      }

      return { ok: true }
    },
    go(route) {
      this.$router.push(route)
    },
    handleClick(item) {
      if (item.requiresFinalApproval && !this.isFinalResultAnnouncementActive) {
        this.openFinalResultActivation(item)
        return
      }

      if (item.route == '/candidate/request')
        this.setRequestStatus("DRAFT")
      this.$router.push(item.route)
    },
    async activateFinalResults() {
      if (!this.pendingApprovalRole) {
        return
      }

      const response = await this.submitFinalResultsApproval({
        role: this.pendingApprovalRole,
        passcode1: this.approvalPasscode1,
        passcode2: this.approvalPasscode2
      })

      if (!response?.status) {
        this.$bvToast.toast(response?.message || 'ثبت تایید در سرور انجام نشد.', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
        return
      }

      await this.syncFinalResultsStatus(false)

      const successMsg = this.isFinalResultAnnouncementActive
        ? 'اعلام نتایج نهایی با تایید هیات اجرایی و هیات نظارت فعال شد.'
        : 'تایید شما ثبت شد. پس از تایید هیأت دیگر، نتایج برای همه کاربران فعال می‌شود.'

      this.$bvToast.toast(successMsg, {
        title: this.isFinalResultAnnouncementActive ? 'فعال‌سازی موفق' : 'تایید ثبت شد',
        variant: this.isFinalResultAnnouncementActive ? 'success' : 'info',
        solid: true
      })

      this.approvalPasscode = ''
      this.showFinalResultActivation = false

      if (this.isFinalResultAnnouncementActive && this.pendingMenuItem) {
        const route = this.pendingMenuItem.route
        this.pendingMenuItem = null
        this.pendingApprovalRole = null
        this.$router.push(route)
        return
      }

      this.pendingMenuItem = null
      this.pendingApprovalRole = null
    }
  },
   async created() {
    await this.syncFinalResultsStatus(true)
  }
}
</script>

<style scoped>
/* 🎨 رنگ سازمانی */
.text-primary-org {
  color: #3f51b5;
}

.dashboard-wrapper {
  /* min-height: 100vh; */
  background: #f4f6fb;
  padding: 20px;
}

/* Mobile-first */
.dashboard-card {
  border-radius: 14px;
  padding: 24px 16px;
  transition: all 0.25s ease;
  cursor: pointer;
}

.dashboard-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 26px rgba(63, 81, 181, 0.25);
}

.dashboard-card.disabled {
  opacity: 0.5;
  cursor: not-allowed;
  box-shadow: none;
}

.icon {
  font-size: 34px;
  color: #3f51b5;
}

.title {
  font-size: 14px;
  font-weight: 600;
}

/* Mobile optimization */
@media (max-width: 576px) {
  .dashboard-card {
    padding: 20px 14px;
  }

  .icon {
    font-size: 30px;
  }
}

.cursor-pointer {
  cursor: pointer;
}
</style>