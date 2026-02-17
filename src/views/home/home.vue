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
                    سامانه انتخابات صندوق ذخیره فرهنگیان
                  </h5>

                  <ElectionStatusTimer :config-info="ConfigInfo" />
                </div>
                <div v-if="electionStatusAll == 'upcoming'">
                  <CustomStepper
                    v-if="(currentUser?.roles.includes('CANDIDATE')) && requestStatus "
                    :steps="stepperSteps" :current-step="currentStep" :disabled="processing" />
                  <!-- درخواست ثبت شده -->
                  <b-alert v-if="currentUser?.roles.includes('CANDIDATE') && (requestStatus === 'SUBMITTED' || requestStatus === 'EXECUTIVE_APPROVED' || requestStatus === 'EXECUTIVE_REJECTED')" variant="warning" show>
                    ⏳ درخواست شما ثبت شده و در حال بررسی توسط مراجع است
                    <br/>
                    <b-button variant="outline-danger" class="mt-2" @click="canselRequest()">
                      انصراف
                    </b-button>
                  </b-alert>
                  <b-alert v-else-if="requestStatus === 'SUPERVISION_REJECTED'" variant="danger" show>
                    ❌ درخواست شما رد شده است
                    <br />
                    <b-button variant="outline-danger" class="mt-2" @click="$router.push('/candidate/objection')">
                      ثبت اعتراض
                    </b-button>
                  </b-alert>
                </div>
                <!-- Menu -->
                <b-row>
                  <b-col v-for="(item, index) in filteredMenu" :key="index" cols="12" sm="4" class="mb-3">
                    <b-card class="dashboard-card h-100"
                      :class="{ disabled: (item.electionStatusAll && item.electionStatusAll != electionStatusAll) }"
                      @click="!(item.electionStatusAll && item.electionStatusAll != electionStatusAll) ? handleClick(item) : ''">
                      <div class="icon mb-2">
                        <i :class="item.icon"></i>
                      </div>

                      <div class="title">
                        {{ item.title }}
                      </div>

                      <b-badge v-if="item.badge && electionStatusAll == 'active'" variant="warning" class="mt-2">
                        {{ item.badge }}
                      </b-badge>
                    </b-card>
                  </b-col>
                </b-row>

              </b-col>
            </b-row>
          </b-container>
        </div>
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
    ...mapGetters(["ConfigInfo", "sidebarVisible", "processing", "loginError", "currentUser","stateCandidInfo", "electionStatusAll", "requestStatus"]),
    filteredMenu() {
      return this.menu.filter(item => {
        const roleAllowed = item.roles.includes(this.currentUser?.roles[0])

        const statusAllowed = item.visibleWhen
          ? item.visibleWhen(this.requestStatus)
          : true

        return roleAllowed && statusAllowed
      })
    }, currentStep() {      
      return this.STATUS_STEP_MAP[this.requestStatus] ?? 0
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
          electionStatusAll:'upcoming',
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
          roles: ['CANDIDATE', 'VOTER', 'EXECUTIVE', 'SUPERVISOR'],
          electionStatusAll: 'active',
          badge: 'در حال رأی‌گیری'
        },
        {
          title: 'مشاهده نتایج مرحله اول',
          route: '/results/live-election',
          icon: 'bi bi-bar-chart',
          roles: ['VOTER', 'CANDIDATE', 'EXECUTIVE', 'SUPERVISOR'],
          electionStatusAll: 'active',
          badge: 'نمایش زنده'
        },
        {
          title: 'مشاهده نتایج',
          route: '/results/final-election',
          icon: 'bi bi-bar-chart',
          roles: ['VOTER', 'CANDIDATE', 'EXECUTIVE', 'SUPERVISOR'],
          electionStatusAll: 'ended',
          badge: 'نمایش نهایی'
        }
      ]
    }
  },
  methods: {
    ...mapMutations(["setRequestStatus","setUser"]),
    ...mapActions(["getConfig","canselRequestCANDIDATE"]),
   async canselRequest() {
     await this.canselRequestCANDIDATE()
     this.setRequestStatus("DRAFT")
        const cu = {
          ...this.currentUser,
          roles: ['VOTER']
        }
        this.setUser(cu)
    },
    go(route) {
      this.$router.push(route)
    },
    handleClick(item) {
      if (item.route == '/candidate/request')
        this.setRequestStatus("DRAFT")
      this.$router.push(item.route)
    }
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