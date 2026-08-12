<template>
  <div class="px-2">
    <b-row class="m-0">


      <div v-if="(!sidebarVisible && isMobile()) || (!isMobile())" style="padding:0 15px;flex:1;"
        :style="isMobile() ? 'max-width: calc(100%);' : ''">
        <div class="mt-4" style="">

          <b-container fluid class="dashboard-wrapper d-flex justify-content-center">
            <b-row class="w-100 justify-content-center">
              <b-col cols="12" md="10" lg="8" class="text-center">

                <!-- Header -->
                <div class="election-header mb-5 text-center" v-if="ConfigInfo">
                  <div class="header-glow"></div>
                  <h4 class="mb-2 fw-bold text-white">
                    🗳️ سامانه انتخابات نمایندگان اعضای فرهنگی
                  </h4>
                  <p class="text-white-50 mb-3">هیأت امنای موسسه صندوق ذخیره فرهنگیان</p>
                  <ElectionStatusTimer :config-info="ConfigInfo" />
                </div>
                <!-- بخش وضعیت داوطلب - با UI بهبود یافته -->
                <div v-if="electionStatusAll == 'upcoming'">
                  <CustomStepper v-if="(currentUser?.roles.includes('CANDIDATE')) && requestStatus"
                    :steps="stepperSteps" :current-step="currentStep" :disabled="processing" />

                  <!-- ========== کارت وضعیت: در حال بررسی ========== -->
                  <div
                    v-if="currentUser?.roles.includes('CANDIDATE')"
                    class="status-card status-pending mb-4">
                    <div class="status-card-icon">
                      <div class="icon-circle-pending">
                        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                          <path
                            d="M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                            stroke="currentColor" stroke-width="1.5" fill="none" />
                          <path d="M12 6V12L16 14" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
                        </svg>
                      </div>
                    </div>
                    <div class="status-card-content">
                      <h6 class="status-title">در انتظار بررسی</h6>
                      <p class="status-message">درخواست شما ثبت شده و در حال بررسی توسط مراجع ذی‌صلاح است.</p>
                      <div class="status-progress">
                        <div class="progress-step active"></div>
                        <div class="progress-step"></div>
                        <div class="progress-step"></div>
                        <div class="progress-step"></div>
                      </div>
                    </div>
                    <div class="status-card-action d-flex flex-column">
                      <button class="btn-action-primary mb-2" @click="editDocuments()">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                          <path
                            d="M11 4H4C3.44772 4 3 4.44772 3 5V20C3 20.5523 3.44772 21 4 21H19C19.5523 21 20 20.5523 20 20V13"
                            stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                          <path
                            d="M18.5 2.50001C18.8978 2.10219 19.4374 1.87869 20 1.87869C20.5626 1.87869 21.1022 2.10219 21.5 2.50001C21.8978 2.89784 22.1213 3.4374 22.1213 4.00001C22.1213 4.56262 21.8978 5.10219 21.5 5.50001L12 15L8 16L9 12L18.5 2.50001Z"
                            stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                        </svg>
                        ویرایش مدارک
                      </button>
                      <button class="btn-action-outline" @click="canselRequest()">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                          <path d="M18 6L6 18M6 6L18 18" stroke="currentColor" stroke-width="1.8"
                            stroke-linecap="round" />
                        </svg>
                        انصراف از ثبت‌نام
                      </button>
                    </div>
                  </div>

                  <!-- ========== کارت وضعیت: تایید نشده (اجرایی) ========== -->
                  <div v-else-if="requestStatus === 'EXECUTIVE_REJECTED'" class="status-card status-rejected mb-4">
                    <div class="status-card-icon">
                      <div class="icon-circle-rejected">
                        <svg width="32" height="32" viewBox="0 0 24 24" fill="none">
                          <path
                            d="M12 8V12M12 16H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                            stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
                        </svg>
                      </div>
                    </div>
                    <div class="status-card-content">
                      <h6 class="status-title text-danger">مدارک تایید نشد</h6>
                      <p class="status-message">متأسفانه مدارک شما توسط هیأت اجرایی تأیید نشده است.</p>
                      <div v-if="stateCandidInfo?.reson" class="reject-reason">
                        <span class="reason-label">نظر هیأت اجرایی:</span>
                        <span class="reason-text">{{ stateCandidInfo.reson }}</span>
                      </div>
                      <div v-if="stateCandidInfo?.edited_at_sh" class="edit-date">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path d="M20 12V18H4V12M12 4V16M12 16L15 13M12 16L9 13" stroke-width="1.5"
                            stroke-linecap="round" />
                        </svg>
                        آخرین ویرایش: {{ stateCandidInfo.edited_at_sh }}
                      </div>
                    </div>
                    <div class="status-card-action">
                      <!--<button class="btn-action-primary" @click="submitAgain()">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                          <path d="M1 12C1 12 4 4 12 4C20 4 23 12 23 12C23 12 20 20 12 20C4 20 1 12 1 12Z"
                            stroke="currentColor" stroke-width="1.5" fill="none" />
                          <path
                            d="M12 15C13.6569 15 15 13.6569 15 12C15 10.3431 13.6569 9 12 9C10.3431 9 9 10.3431 9 12C9 13.6569 10.3431 15 12 15Z"
                            stroke="currentColor" stroke-width="1.5" />
                        </svg>
                        ثبت مجدد درخواست
                      </button>-->
                      <button class="btn-action-primary mb-2" @click="editDocuments()">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                          <path
                            d="M11 4H4C3.44772 4 3 4.44772 3 5V20C3 20.5523 3.44772 21 4 21H19C19.5523 21 20 20.5523 20 20V13"
                            stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                          <path
                            d="M18.5 2.50001C18.8978 2.10219 19.4374 1.87869 20 1.87869C20.5626 1.87869 21.1022 2.10219 21.5 2.50001C21.8978 2.89784 22.1213 3.4374 22.1213 4.00001C22.1213 4.56262 21.8978 5.10219 21.5 5.50001L12 15L8 16L9 12L18.5 2.50001Z"
                            stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                        </svg>
                        ویرایش مدارک
                      </button>
                    </div>
                  </div>

                  <!-- ========== کارت وضعیت: رد شده توسط نظارت ========== -->
                  <div v-else-if="requestStatus === 'SUPERVISION_REJECTED'"
                    class="status-card status-final-rejected mb-4">
                    <div class="status-card-icon">
                      <div class="icon-circle-final-rejected">
                        <svg width="32" height="32" viewBox="0 0 24 24" fill="none">
                          <path
                            d="M12 8V12M12 16H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                            stroke="currentColor" stroke-width="1.5" />
                          <path d="M18 6L6 18" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
                        </svg>
                      </div>
                    </div>
                    <div class="status-card-content">
                      <h6 class="status-title text-danger">درخواست رد شده</h6>
                      <p class="status-message">درخواست شما توسط هیأت نظارت رد شده است.</p>
                      <div v-if="stateCandidInfo?.reson" class="reject-reason">
                        <span class="reason-label">نظر نهایی هیأت نظارت:</span>
                        <span class="reason-text">{{ stateCandidInfo.reson }}</span>
                      </div>
                      <div v-if="stateCandidInfo?.edited_at_sh" class="edit-date">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path d="M20 12V18H4V12M12 4V16M12 16L15 13M12 16L9 13" stroke-width="1.5"
                            stroke-linecap="round" />
                        </svg>
                        آخرین ویرایش: {{ stateCandidInfo.edited_at_sh }}
                      </div>
                    </div>
                    <div class="status-card-action">
                      <button class="btn-action-warning" @click="$router.push('/candidate/objection')">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                          <path
                            d="M12 9V13M12 17H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                            stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
                        </svg>
                        ثبت اعتراض به رأی
                      </button>
                    </div>
                  </div>

                  <!-- ========== کارت وضعیت: تایید شده ========== -->
                  <div v-else-if="requestStatus === 'SUPERVISION_APPROVED' && stateCandidInfo?.reson"
                    class="status-card status-approved mb-4">
                    <div class="status-card-icon">
                      <div class="icon-circle-approved">
                        <svg width="32" height="32" viewBox="0 0 24 24" fill="none">
                          <path d="M20 6L9 17L4 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                            stroke-linejoin="round" />
                        </svg>
                      </div>
                    </div>
                    <div class="status-card-content">
                      <h6 class="status-title text-success">درخواست شما تأیید شد</h6>
                      <p class="status-message">درخواست ثبت‌نام شما با موفقیت تأیید شده است.</p>
                      <div v-if="stateCandidInfo?.reson" class="approve-note">
                        <span class="reason-label">نظر نهایی هیأت نظارت:</span>
                        <span class="reason-text">{{ stateCandidInfo.reson }}</span>
                      </div>
                      <div v-if="stateCandidInfo?.edited_at_sh" class="edit-date">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path d="M20 12V18H4V12M12 4V16M12 16L15 13M12 16L9 13" stroke-width="1.5"
                            stroke-linecap="round" />
                        </svg>
                        تاریخ تأیید: {{ stateCandidInfo.edited_at_sh }}
                      </div>
                    </div>
                  </div>
                </div>
                <!-- Menu -->

                <!-- Menu Cards - Improved Grid -->
                <b-row class="g-4">
                  <b-col v-for="(item, index) in filteredMenu" :key="index" cols="12" sm="6" md="4" lg="4" class="mb-4">
                    <div class="dashboard-card-modern h-100" :class="{ 'card-disabled': isMenuDisabled(item) }"
                      @click="!isMenuDisabled(item) ? handleClick(item) : null">

                      <!-- Badge on corner -->
                      <div v-if="item.badge && electionStatusAll == 'active'" class="card-badge">
                        <span>{{ item.badge }}</span>
                      </div>
                      <div v-if="item.requiresFinalApproval && !isFinalResultAnnouncementActive && !currentUser?.roles?.includes('ADMIN')"
                        class="card-badge secondary">
                        <span>{{ finalApprovalStatusText }}</span>
                      </div>

                      <!-- Icon with circle background -->
                      <div class="icon-wrapper mb-3">
                        <div class="icon-circle">
                          <i :class="item.icon"></i>
                        </div>
                      </div>

                      <h5 class="card-title">{{ item.title }}</h5>
                      <p class="card-desc mt-2">
                        {{ getCardDescription(item) }}
                      </p>

                      <div class="card-footer-link">
                        <span>مشاهده و اقدام <i class="bi bi-arrow-left-short"></i></span>
                      </div>
                    </div>
                  </b-col>
                </b-row>

              </b-col>
            </b-row>
          </b-container>
        </div>
        <b-modal id="final-result-activation" v-model="showFinalResultActivation" title="🔐 فعال‌سازی اعلام نتایج نهایی"
          hide-footer centered body-class="p-4" header-class="border-0 bg-light">
          <p class="text-muted mb-4">اعضای هیأت اجرایی و هیأت نظارت هر کدام باید با رمز خود تایید کنند.</p>
          <b-form-group label="🔑 رمز هیأت اجرایی" label-for="approval-passcode1">
            <b-form-input id="approval-passcode1" v-model="approvalPasscode1" type="password"
              placeholder="رمز را وارد کنید" class="rounded-pill"></b-form-input>
          </b-form-group>
          <b-form-group label="🔑 رمز هیأت نظارت" label-for="approval-passcode2">
            <b-form-input id="approval-passcode2" v-model="approvalPasscode2" type="password"
              placeholder="رمز را وارد کنید" class="rounded-pill"></b-form-input>
          </b-form-group>
          <div class="d-flex justify-content-end gap-2 mt-3">
            <b-button variant="outline-secondary" @click="closeActivationModal"
              class="rounded-pill px-4">انصراف</b-button>
            <b-button variant="success" @click="activateFinalResults" class="rounded-pill px-4">تایید و ثبت</b-button>
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
          roles: ['EXECUTIVE', 'ADMIN', 'VOTER', 'CANDIDATE', 'SUPERVISOR'],
          electionStatusAll: 'ended',
          badge: 'نمایش نهایی',
          requiresFinalApproval: true
        },
        {
          title: 'زمان بندی سیستم',
          route: '/supervisor/system-schedule',
          icon: 'bi bi-bar-chart',
          roles: ['ADMIN'],
          badge: 'استانی و کشوری'
        },
        {
          title: 'مدیریت کاربران',
          route: '/supervisor/UsersManagment',
          icon: 'bi bi-people',
          roles: ['ADMIN'],
          badge: 'دسترسی و منطقه'
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
    getCardDescription(item) {
      if (item.route === '/candidate/request') return 'ثبت نام و ارسال مدارک';
      if (item.route === '/User/votingPage') return 'شرکت در انتخابات و رأی‌دهی';
      if (item.route === '/results/final-election') return 'مشاهده نتایج نهایی انتخابات';
      if (item.roles.includes('EXECUTIVE')) return 'بررسی درخواست‌ها و تایید مدارک';
      if (item.roles.includes('SUPERVISOR')) return 'نظارت بر فرآیند انتخابات';
      return 'برای مشاهده کلیک کنید';
    },
    editDocuments() {
      this.setRequestStatus("CONDITIONS_ACCEPTED")
      this.$router.push({ path: '/candidate/UploadDocuments', query: { edit: 'true' } })
    },
    submitAgain() {
      this.canselRequest()
      // this.$router.push("/candidate/request")
    },
    isMenuDisabled(item) {
      if (item.electionStatusAll && item.electionStatusAll != this.electionStatusAll) {
        return true
      }

      if (item.requiresFinalApproval && !this.isFinalResultAnnouncementActive) {
        const roles = this.currentUser?.roles ?? ''
        if (roles.includes('ADMIN')) return false
        return !roles.includes('EXECUTIVE') && !roles.includes('SUPERVISOR')
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

      const confirmed = await this.$bvModal.msgBoxConfirm(
        'در صورت ثبت نهایی انصراف، امکان ثبت‌نام مجدد در این دوره انتخابات وجود نخواهد داشت',
        {
          title: 'آیا از انصراف ثبت‌نام اطمینان دارید؟',
          centered: true,
          okTitle: 'بله، انصراف می‌دهم',
          cancelTitle: 'انصراف',
          okVariant: 'danger',
          cancelVariant: 'secondary'
        }
      )

      if (!confirmed) {
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
      const roles = this.currentUser?.roles ?? ''
      if (item.requiresFinalApproval && !this.isFinalResultAnnouncementActive && !roles.includes('ADMIN')) {
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

      if (!response?.succeeded) {
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

/* ========== بهبودهای گرافیکی و UI مدرن ========== */
.dashboard-wrapper {
  background: linear-gradient(135deg, #f5f7ff 0%, #eef2fa 100%);
  min-height: 100vh;
  padding: 20px 0;
}

/* هدر گرادیانتی */
.election-header {
  background: linear-gradient(120deg, #1e2a6e, #2b3b8a, #1e2a6e);
  border-radius: 32px;
  padding: 28px 20px;
  position: relative;
  overflow: hidden;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.15);
}

.header-glow {
  position: absolute;
  top: -30%;
  right: -20%;
  width: 200px;
  height: 200px;
  background: radial-gradient(circle, rgba(255, 255, 255, 0.15) 0%, rgba(255, 255, 255, 0) 70%);
  border-radius: 50%;
}

/* کارت مدرن */
.dashboard-card-modern {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(0px);
  border-radius: 28px;
  padding: 28px 20px 24px;
  transition: all 0.3s cubic-bezier(0.2, 0.9, 0.4, 1.1);
  cursor: pointer;
  position: relative;
  box-shadow: 0 12px 24px -12px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.5);
}

.dashboard-card-modern:hover {
  transform: translateY(-8px) scale(1.02);
  box-shadow: 0 24px 36px -12px rgba(33, 33, 68, 0.25);
  background: white;
  border-color: #cbd5ff;
}

.card-disabled {
  opacity: 0.55;
  cursor: not-allowed;
  filter: grayscale(0.1);
}

.card-disabled:hover {
  transform: none;
  box-shadow: 0 12px 24px -12px rgba(0, 0, 0, 0.1);
}

/* آیکون با دایره رنگی */
.icon-wrapper {
  display: flex;
  justify-content: center;
}

.icon-circle {
  width: 70px;
  height: 70px;
  background: linear-gradient(145deg, #eef2ff, #ffffff);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10px 18px -8px rgba(63, 81, 181, 0.3);
  transition: 0.2s;
}

.dashboard-card-modern:hover .icon-circle {
  background: linear-gradient(145deg, #3f51b5, #2c3e9e);
  box-shadow: 0 12px 20px -6px #3f51b580;
}

.icon-circle i {
  font-size: 34px;
  color: #3f51b5;
  transition: 0.2s;
}

.dashboard-card-modern:hover .icon-circle i {
  color: white;
}

/* عنوان و توضیحات */
.card-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1a2c6e;
  margin-top: 12px;
}

.card-desc {
  font-size: 0.8rem;
  color: #6c757d;
  line-height: 1.4;
}

.card-footer-link {
  margin-top: 20px;
  font-size: 0.8rem;
  font-weight: 500;
  color: #3f51b5;
  opacity: 0;
  transition: 0.2s;
  text-align: left;
}

.dashboard-card-modern:hover .card-footer-link {
  opacity: 1;
}

/* نشانگر (Badge) گوشه کارت */
.card-badge {
  position: absolute;
  top: 16px;
  left: 16px;
  background: #ff9800;
  color: #2c2c2c;
  font-size: 0.7rem;
  font-weight: bold;
  padding: 4px 12px;
  border-radius: 40px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.card-badge.secondary {
  background: #6c757d;
  color: white;
}

/* موبایل */
@media (max-width: 576px) {
  .dashboard-card-modern {
    padding: 20px 16px;
  }

  .icon-circle {
    width: 56px;
    height: 56px;
  }

  .icon-circle i {
    font-size: 28px;
  }

  .card-title {
    font-size: 1rem;
  }

  .election-header {
    padding: 20px 16px;
  }
}

/* ریسپانسیو گریッド */
.g-4 {
  --bs-gutter-y: 1.5rem;
}
/* ========== کارت‌های وضعیت داوطلب ========== */
.status-card {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  background: white;
  border-radius: 24px;
  padding: 1.25rem 1.5rem;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
  transition: all 0.3s ease;
  text-align: right;
}

.status-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.1);
}

/* آیکون‌های وضعیت */
.status-card-icon {
  flex-shrink: 0;
}

.icon-circle-pending {
  width: 56px;
  height: 56px;
  background: #fff3e0;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #f5a623;
}

.icon-circle-rejected {
  width: 56px;
  height: 56px;
  background: #fee2e2;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ef4444;
}

.icon-circle-final-rejected {
  width: 56px;
  height: 56px;
  background: #fef3f2;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #dc2626;
}

.icon-circle-approved {
  width: 56px;
  height: 56px;
  background: #e0f2fe;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #0ea5e9;
}

/* محتوای کارت */
.status-card-content {
  flex: 1;
}

.status-title {
  font-size: 1rem;
  font-weight: 700;
  margin-bottom: 0.25rem;
}

.status-message {
  font-size: 0.85rem;
  color: #6c757d;
  margin-bottom: 0.75rem;
}

/* دلیل رد/تایید */
.reject-reason,
.approve-note {
  background: #f8f9fa;
  padding: 0.5rem 0.75rem;
  border-radius: 12px;
  margin-top: 0.5rem;
  font-size: 0.8rem;
}

.reason-label {
  font-weight: 600;
  color: #495057;
  margin-left: 0.5rem;
}

.reason-text {
  color: #6c757d;
}

.edit-date {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  margin-top: 0.5rem;
  font-size: 0.7rem;
  color: #94a3b8;
}

/* نوار پیشرفت مراحل */
.status-progress {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.progress-step {
  width: 60px;
  height: 4px;
  background: #e2e8f0;
  border-radius: 4px;
  transition: all 0.3s;
}

.progress-step.active {
  background: #f5a623;
  width: 80px;
}

/* دکمه‌های اقدام */
.status-card-action {
  flex-shrink: 0;
}

.btn-action-outline,
.btn-action-primary,
.btn-action-warning {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 40px;
  font-size: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
  background: transparent;
}

.btn-action-outline {
  border: 1px solid #ef4444;
  color: #ef4444;
  background: white;
}

.btn-action-outline:hover {
  background: #ef4444;
  color: white;
}

.btn-action-primary {
  background: #e0e7ff;
  color: #3f51b5;
}

.btn-action-primary:hover {
  background: #3f51b5;
  color: white;
}

.btn-action-warning {
  background: #fef3c7;
  color: #d97706;
}

.btn-action-warning:hover {
  background: #d97706;
  color: white;
}

/* موبایل */
@media (max-width: 576px) {
  .status-card {
    flex-direction: column;
    align-items: center;
    text-align: center;
    padding: 1rem;
  }
  
  .status-progress {
    justify-content: center;
  }
  
  .status-card-action {
    width: 100%;
  }
  
  .btn-action-outline,
  .btn-action-primary,
  .btn-action-warning {
    justify-content: center;
    width: 100%;
  }
}
</style>