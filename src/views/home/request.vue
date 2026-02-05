<template>
  <div class="px-2">


    <b-container fluid class="request-wrapper">

      <!-- Wizard -->
      <ul class="wizard mb-3">
        <li class="active">1. بررسی شرایط احراز</li>
        <li>2. قبول شرایط</li>
        <li>3. بارگذاری مدارک</li>
        <li>4. تأیید ثبت‌نام</li>
      </ul>

      <!-- Progress Bar -->
      <b-progress height="6px" class="mb-4" :value="progress" max="100" variant="primary" animated />

      <b-card>

        <!-- Declaration -->
        <b-form-checkbox v-model="accepted">
          اینجانب <strong>{{ user.fullName }}</strong>
          با کد ملی <strong>{{ user.nationalCode }}</strong>
          و کد پرسنلی <strong>{{ user.personnelCode }}</strong>
          با آخرین پست <strong>{{ user.position }}</strong>
          متقاضی ثبت‌نام در انتخابات می‌باشم.
        </b-form-checkbox>

        <hr />

        <!-- Conditions -->
        <ul class="conditions">
          <li v-for="c in conditions" :key="c.key">
            <div class="icon">
              <b-spinner small v-if="c.state === 'checking'" variant="secondary" />
              <b-icon v-else-if="c.state === 'success'" icon="check-circle-fill" variant="success"
                class="icon-animate" />
              <b-icon v-else icon="x-circle-fill" variant="danger" class="icon-animate" />
            </div>

            <div class="content">
              <div class="label">{{ c.label }}</div>
              <div v-if="c.state === 'error'" class="reason text-danger">
                {{ c.reason }}
              </div>
            </div>
          </li>
        </ul>

        <!-- Action -->
        <div class="text-center mt-4">
          <b-button variant="primary" :disabled="!canContinue" @click="nextStep">
            ادامه
          </b-button>
        </div>

      </b-card>
    </b-container>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: 'CandidateRequest',
  components: {
  },
  async mounted() {

    this.runChecks()
  },
  computed: {
    ...mapGetters(["sidebarVisible", "processing", "loginError", "currentUser"]),
    canContinue() {
      if (!this.accepted || !this.checksFinished) return false
      return this.conditions.every(c => c.state === 'success')
    }
  },
  data() {
    return {
      isMobile,
      accepted: false,
      checksFinished: false,
      progress: 0,

      user: {
        fullName: 'میلاد فراهانی',
        nationalCode: '1234567890',
        personnelCode: '98765',
        position: 'دبیر رسمی',
        region: 'منطقه ۳ تهران',
        degree: 'BSC',
        membershipActive: true,
        membershipYears: 2,
        alreadyRegistered: false
      },

      conditions: [
        {
          key: 'membership',
          label: 'استعلام عضویت فعال در صندوق ذخیره فرهنگیان',
          state: 'checking',
          reason: ''
        },
        {
          key: 'duration',
          label: 'حداقل یک سال سابقه عضویت',
          state: 'checking',
          reason: ''
        },
        {
          key: 'degree',
          label: 'مدرک تحصیلی حداقل کارشناسی',
          state: 'checking',
          reason: ''
        },
        {
          key: 'region',
          label: 'شناسایی منطقه کاربر',
          state: 'checking',
          reason: ''
        },
        {
          key: 'notRegistered',
          label: 'عدم ثبت‌نام قبلی',
          state: 'checking',
          reason: ''
        }
      ]
    }
  },
  methods: {
    ...mapMutations(["setsidebarVisible", "setRequestStatus"]),
    ...mapActions(["GetNationalCardPhoto"]),
    async runChecks() {
      const step = 100 / this.conditions.length

      for (let i = 0; i < this.conditions.length; i++) {
        const c = this.conditions[i]

        c.state = 'checking'
        await this.delay(600)

        const result = this.evaluate(c.key)
        c.state = result.ok ? 'success' : 'error'
        c.reason = result.reason || ''

        this.progress = Math.round((i + 1) * step)
      }

      this.checksFinished = true
    },

    evaluate(key) {
      switch (key) {
        case 'membership':
          return this.user.membershipActive
            ? { ok: true }
            : { ok: false, reason: 'عضویت فعال در صندوق احراز نشد' }

        case 'duration':
          return this.user.membershipYears >= 1
            ? { ok: true }
            : { ok: false, reason: 'سابقه عضویت کمتر از یک سال است' }

        case 'degree':
          return ['BSC', 'MSC', 'PHD'].includes(this.user.degree)
            ? { ok: true }
            : { ok: false, reason: 'مدرک تحصیلی کمتر از کارشناسی است' }

        case 'region':
          return this.user.region
            ? { ok: true }
            : { ok: false, reason: 'منطقه خدمتی کاربر شناسایی نشد' }

        case 'notRegistered':
          return !this.user.alreadyRegistered
            ? { ok: true }
            : { ok: false, reason: 'قبلاً برای این انتخابات ثبت‌نام انجام شده است' }

        default:
          return { ok: false, reason: 'خطای سیستمی' }
      }
    },

    delay(ms) {
      return new Promise(resolve => setTimeout(resolve, ms))
    },

    nextStep() {
      this.setRequestStatus("CANDIDATE")
      this.$router.push('/candidate/AcceptConditions')
    }
  }
}
</script>

<style scoped>
.request-wrapper {
  background: #f5f7fb;
  min-height: 100vh;
  padding: 20px;
}

/* Wizard */
.wizard {
  display: flex;
  list-style: none;
  padding: 0;
  font-size: 13px;
}

.wizard li {
  flex: 1;
  text-align: center;
  border-bottom: 3px solid #ccc;
  padding: 8px;
  color: #999;
}

.wizard li.active {
  border-color: #3f51b5;
  color: #3f51b5;
  font-weight: 600;
}

/* Conditions */
.conditions {
  list-style: none;
  padding: 0;
}

.conditions li {
  display: flex;
  align-items: center;
  margin-bottom: 14px;
}

.icon {
  width: 28px;
}

/* Enterprise Animation */
.icon-animate {
  animation: fadeSlide 0.25s ease-out;
}

@keyframes fadeSlide {
  from {
    opacity: 0;
    transform: translateY(4px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
