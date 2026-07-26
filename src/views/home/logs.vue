<template>
  <div class="p-3">
    <b-card>
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-3" style="gap: 0.75rem;">
        <h4 class="mb-0">گزارش فعالیت‌ها</h4>
        <b-button variant="outline-primary" size="sm" @click="loadLogs">بارگذاری مجدد</b-button>
      </div>

      <b-row class="mb-3" style="row-gap: 0.75rem;">
        <b-col cols="12" md="8">
          <b-input-group>
            <template #prepend>
              <b-input-group-text>
                <b-icon icon="search"></b-icon>
              </b-input-group-text>
            </template>
            <b-form-input
              v-model.trim="filter"
              placeholder="جستجو در کد ملی، عملیات و توضیحات..."
            />
          </b-input-group>
        </b-col>

        <b-col cols="12" md="4">
          <b-form-select v-model="perPage" :options="pageOptions" />
        </b-col>
      </b-row>

      <div class="table-responsive">
        <b-table
          :items="logs"
          :fields="fields"
          :filter="filter"
          :current-page="currentPage"
          :per-page="perPage"
          :sort-by.sync="sortBy"
          :sort-desc.sync="sortDesc"
          striped
          hover
          small
          responsive
          show-empty
          empty-text="لاگی برای نمایش وجود ندارد"
          class="text-right"
          @filtered="onFiltered"
        />
      </div>

      <div class="d-flex justify-content-between align-items-center mt-3" style="gap: 0.75rem;">
        <small class="text-muted">تعداد نتایج: {{ totalRows }}</small>

        <b-pagination
          v-model="currentPage"
          :total-rows="totalRows"
          :per-page="perPage"
          first-number
          last-number
          align="right"
          size="sm"
          class="mb-0"
        />
      </div>
    </b-card>
  </div>
</template>

<script>
import { mapActions } from 'vuex'

export default {
  name: 'LogsPage',
  data() {
    return {
      logs: [],
      fields: [
        { key: 'id', label: 'شناسه', sortable: true },
        { key: 'nationalId', label: 'کد ملی کاربر', sortable: true },
        { key: 'action', label: 'عملیات', sortable: true },
        { key: 'description', label: 'توضیحات', sortable: true },
        { key: 'create_date_shamsi', label: 'تاریخ ثبت', sortable: true }
      ],
      filter: '',
      perPage: 50,
      currentPage: 1,
      totalRows: 0,
      sortBy: 'id',
      sortDesc: true,
      pageOptions: [
        { value: 10, text: 'نمایش ۱۰ مورد' },
        { value: 20, text: 'نمایش ۲۰ مورد' },
        { value: 50, text: 'نمایش ۵۰ مورد' }
      ]
    }
  },
  mounted() {
    this.loadLogs()
  },
  methods: {
    ...mapActions(['getLogs']),
    async loadLogs() {
      const data = await this.getLogs({ limit: 1000 })
      this.logs = Array.isArray(data) ? data : []
      this.totalRows = this.logs.length
      this.currentPage = 1
    },
    onFiltered(filteredItems) {
      this.totalRows = filteredItems.length
      this.currentPage = 1
    }
  }
}
</script>
