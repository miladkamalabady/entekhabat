<template>
  <div class="px-2">
    <b-container fluid class="request-wrapper" v-if="isRegistrationOpen">

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
          اینجانب <strong>{{ currentUser.full_name }}</strong>
          با کد ملی <strong>{{ currentUser.national_id }}</strong>
          و کد پرسنلی <strong>{{ currentUser.personnel_code }}</strong>
          با آخرین پست <strong>{{ currentUser.orgPositionDesc }}</strong>
          متقاضی ثبت‌نام در انتخابات می‌باشم.
        </b-form-checkbox>

        <hr />

        <!-- Conditions -->
        <div v-if="userstatusInfo">
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
        </div>
        <b-card v-else class="mb-4 text-center">
          در حال بررسی...
          <img src="/assets/img/Loading.gif" style="max-width: 100%; width: 50px" />
        </b-card>
      </b-card>
    </b-container>
    <b-container fluid class="request-wrapper" v-else>
      <b-alert show>زمان انتخابات مشخص نشده است!</b-alert>
    </b-container>
  </div>
</template>

<script>
import { convertDate, isMobile } from "../../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: 'CandidateRequest',
  components: {
  },
  async created() {
    if (!this.SystemScheduleInfo) {
      const rows = await this.getSystemSchedule()
      if (Array.isArray(rows) && rows.length) {
        const mapByKey = rows.reduce((acc, item) => {
          acc[item.event_key] = item
          return acc
        }, {})

        this.events = this.events.map(event => ({
          ...event,
          startDate: mapByKey[event.key]?.start_date || event.startDate,
          endDate: mapByKey[event.key]?.end_date || event.endDate
        }))
      }
    }
  },
  async mounted() {

    // گرفتن زمان‌بندی اگر هنوز نیامده
    if (!this.SystemScheduleInfo || !this.SystemScheduleInfo.length) {
      await this.getSystemSchedule()
    }

    const check = this.isRegistrationOpen();

    if (!check.ok) {

      await this.$bvModal.msgBoxOk(check.msg, {
        title: 'امکان ثبت‌نام وجود ندارد',
        centered: true,
        okVariant: 'danger'
      });

      this.$router.replace('/');
      return;
    }

    // اگر مجاز بود تازه احراز شرایط شروع شود
    this.userstatus();
  },
  computed: {
    ...mapGetters(["sidebarVisible", "processing", "loginError", "currentUser", "userstatusInfo", "SystemScheduleInfo"]),
    canContinue() {
      if (!this.accepted || !this.checksFinished) return false
      return this.conditions.every(c => c.state === 'success')
    }
  },
  data() {
    return {
      isMobile, convertDate,
      accepted: false,
      checksFinished: false,
      progress: 0,
      events: [
        { id: 1, key: 'candidate_registration', name: 'ثبت نام داوطلبان', startDate: null, endDate: null },
        { id: 2, key: 'supervision_review', name: 'بررسی نتایج در هیات نظارت', startDate: null, endDate: null },
        { id: 3, key: 'first_stage_announce', name: 'اعلام نتایج مرحله اول', startDate: null, endDate: null },
        { id: 4, key: 'first_stage_objection', name: 'اعتراض به نتایج مرحله اول', startDate: null, endDate: null },
        { id: 5, key: 'first_stage_final_announce', name: 'اعلام نتیجه پس از بررسی مرحله اول', startDate: null, endDate: null },
        { id: 6, key: 'ads_upload_start', name: 'شروع بارگذاری اقلام تبلیغات', startDate: null, endDate: null },
        { id: 7, key: 'ads_review_approve', name: 'بررسی تبلیغات/تایید', startDate: null, endDate: null },
        { id: 8, key: 'campaign_start', name: 'شروع تبلیغات', startDate: null, endDate: null },
        { id: 9, key: 'voting', name: 'رای گیری', startDate: null, endDate: null },
        { id: 10, key: 'results_announce', name: 'اعلام نتایج', startDate: null, endDate: null },
        { id: 11, key: 'objections_registration', name: 'ثبت اعتراضات', startDate: null, endDate: null },
        { id: 12, key: 'final_results_announce', name: 'اعلام نتایج نهایی', startDate: null, endDate: null },
        { id: 13, key: 'certificate_issue', name: 'صدور ابلاغ و گواهی فعالیت', startDate: null, endDate: null }
      ],
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
  watch: {
    userstatusInfo(val) {
      if (val) {

        this.runChecks()
      }
    }
  },
  methods: {
    ...mapMutations(["setRequestStatus"]),
    ...mapActions(["userstatus", "getSystemSchedule"]),
    isRegistrationOpen() {

      const event = this.SystemScheduleInfo?.find(e => e.event_key === 'candidate_registration');

      if (!event)
        return { ok: false, msg: 'زمان ثبت‌نام انتخابات توسط سیستم تعریف نشده است' };

      // تبدیل تاریخ شمسی API
      const start = this.$moment(event.start_date, "jYYYY-jMM-jDD HH:mm:ss");
      const end = this.$moment(event.end_date, "jYYYY-jMM-jDD HH:mm:ss");

      const now = this.$moment();

      // هنوز شروع نشده
      if (now.isBefore(start))
        return {
          ok: false,
          msg: `ثبت‌نام از تاریخ ${start.format("jYYYY/jMM/jDD ساعت HH:mm")} آغاز می‌شود`
        };

      // تمام شده
      if (now.isAfter(end))
        return {
          ok: false,
          msg: `مهلت ثبت‌نام در تاریخ ${end.format("jYYYY/jMM/jDD ساعت HH:mm")} به پایان رسیده است`
        };

      return { ok: true };
    },
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
          return this.userstatusInfo.membershipActive
            ? { ok: true }
            : { ok: false, reason: 'عضویت فعال در صندوق احراز نشد' }

        case 'duration':
          return this.userstatusInfo.membershipYears
            ? { ok: true }
            : { ok: false, reason: 'سابقه عضویت کمتر از یک سال است' }

        case 'degree':
          return this.userstatusInfo.degree
            ? { ok: true }
            : { ok: false, reason: 'مدرک تحصیلی کمتر از کارشناسی است' }

        case 'region':
          return this.currentUser.regionId
            ? { ok: true }
            : { ok: false, reason: 'منطقه خدمتی کاربر شناسایی نشد' }

        case 'notRegistered':
          return !this.userstatusInfo.alreadyRegistered
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
