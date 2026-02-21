<template>
  <div class="system-schedule-page p-3 p-md-4">
    <b-card class="shadow-sm border-0 mb-4">
      <div class="d-flex align-items-center justify-content-between flex-wrap gap-2">
        <div>
          <h4 class="mb-1">زمان‌بندی رویدادهای انتخابات</h4>
          <p class="text-muted mb-0">مدیریت بازه زمانی هر مرحله توسط ادمین سیستم</p>
        </div>
        <b-badge variant="primary" class="px-3 py-2">پنل ادمین</b-badge>
      </div>
    </b-card>

    <b-card class="shadow-sm border-0">
      <b-row class="mb-3 align-items-end">
        <b-col cols="12" lg="8">
          <label class="d-block font-weight-bold mb-2">استان‌های هدف</label>
          <b-form-select v-model="selectedProvinceIds" :options="provinceOptions" multiple :disabled="!canEditSchedule"
             />
          <small class="text-muted d-block mt-2">
            با انتخاب یک یا چند استان، زمان‌بندی رویدادها فقط برای همان استان‌ها اعمال می‌شود.
          </small>
        </b-col>
      </b-row>
      <b-table :items="events" :fields="fields" responsive striped hover class="text-right align-middle">
        <template #cell(index)="data">
          <strong>{{ data.index + 1 }}</strong>
        </template>

        <template #cell(name)="data">
          <div class="event-name">{{ data.item.name }}</div>
        </template>

        <template #cell(startDate)="data">
          <datePicker placeholder="شروع" type="datetime" v-model="data.item.startDate" :auto-submit="true" class="w-100"
            :max="data.item.endDate || undefined" :disabled="!canEditSchedule" simple />
        </template>

        <template #cell(endDate)="data">
          <datePicker placeholder="پایان" type="datetime" v-model="data.item.endDate" :auto-submit="true" class="w-100"
            :min="data.item.startDate || undefined" :disabled="!canEditSchedule" simple />
        </template>

        <template #cell(status)="data">
          <b-badge :variant="getStatusVariant(data.item)">{{ getStatusText(data.item) }}</b-badge>
        </template>
      </b-table>

      <div class="d-flex flex-wrap justify-content-end mt-3">
        <b-button variant="success" @click="saveSchedule" :disabled="!canEditSchedule">
          <b-icon icon="check2-circle" class="ml-1"></b-icon>
          ذخیره زمان‌بندی
        </b-button>
      </div>
    </b-card>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import VuePersianDatetimePicker from "vue-persian-datetime-picker";
import "vue-good-table/dist/vue-good-table.css";
export default {
  name: 'SystemSchedule',
  components: {
    datePicker: VuePersianDatetimePicker,
  },
  computed: {
    ...mapGetters(["SystemScheduleInfo", "currentUser"]),
    canEditSchedule() {
      return this.currentUser?.roles?.includes('ADMIN')
    },
    provinceOptions() {
      return this.provinces.map(province => ({
        value: province.id,
        text: province.name
      }))
    }
  },
  data() {
    return {
      selectedProvinceIds: [],
      fields: [
        { key: 'index', label: '#' },
        { key: 'name', label: 'رویداد' },
        { key: 'startDate', label: 'تاریخ شروع' },
        { key: 'endDate', label: 'تاریخ پایان' },
        { key: 'status', label: 'وضعیت' }
      ],
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
      provinces: []
    }
  },
  async created() {
    // if (!this.SystemScheduleInfo) {
    //   const rows = await this.getSystemSchedule()
    //   if (Array.isArray(rows) && rows.length) {
    //     const mapByKey = rows.reduce((acc, item) => {
    //       acc[item.event_key] = item
    //       return acc
    //     }, {})

    //     this.events = this.events.map(event => ({
    //       ...event,
    //       startDate: mapByKey[event.key]?.start_date || event.startDate,
    //       endDate: mapByKey[event.key]?.end_date || event.endDate
    //     }))
    //   }
    // }
    await this.loadSchedule()
  },
  methods: {
    ...mapActions(['getSystemSchedule', 'saveSystemSchedule','getRegions']),
    async loadSchedule() {
      const payload = this.selectedProvinceIds.length ? { province_ids: this.selectedProvinceIds } : {}
      const rows = await this.getSystemSchedule(payload)
      const mapByKey = Array.isArray(rows)
        ? rows.reduce((acc, item) => {
          acc[item.event_key] = item
          return acc
        }, {})
        : {}
      this.events = this.events.map(event => ({
        ...event,
        startDate: mapByKey[event.key]?.start_date || null,
        endDate: mapByKey[event.key]?.end_date || null
      }))
       const r1 = await this.getRegions()
       this.provinces=r1?.data
       
    },

    getStatusVariant(event) {
      if (event.startDate && event.endDate) return 'success'
      if (event.startDate || event.endDate) return 'warning'
      return 'secondary'
    },
    getStatusText(event) {
      if (event.startDate && event.endDate) return 'تکمیل شده'
      if (event.startDate || event.endDate) return 'ناقص'
      return 'ثبت نشده'
    },
    async saveSchedule() {
      const payload = {
        events: this.events.map(event => ({
          id: event.id,
          key: event.key,
          name: event.name,
          startDate: event.startDate || null,
          endDate: event.endDate || null
        })),
        province_ids: this.selectedProvinceIds
      }

      const response = await this.saveSystemSchedule(payload)
      if (response?.status) {
        this.$notify('success', 'ثبت موفق', 'زمان‌بندی رویدادها ذخیره شد.', {
          duration: 3000,
          permanent: false
        })
      }
    }
  }
}
</script>

<style scoped>
.system-schedule-page {
  max-width: 1400px;
  margin: 0 auto;
}

.event-name {
  font-weight: 600;
}
</style>
