<template>
  <div class="vote-search-page" dir="rtl">
    <div class="page-header">
      <div>
        <h2>جستجوی کاربران</h2>
        <p>
          ادمین و اعضای نظارت اطلاعات کاربران، زمان ثبت رأی، منطقه ثبت رأی و کد پیگیری را بدون نمایش نامزدهای انتخاب‌شده مشاهده می‌کنند.
        </p>
      </div>
      <button class="btn btn-outline-primary" :disabled="loading || !canSearch" @click="searchVotes">
        {{ loading ? 'در حال جستجو...' : 'جستجو' }}
      </button>
    </div>

    <div class="search-card">
      <div class="form-group search-box">
        <label>جستجوی کاربر</label>
        <input
          v-model.trim="filters.q"
          type="text"
          placeholder="کد ملی، کد پرسنلی، نام یا نام خانوادگی کاربر را وارد کنید"
          @keyup.enter="searchVotes"
        >
        <small>برای شروع جستجو حداقل دو کاراکتر وارد کنید.</small>
      </div>
      <div class="form-group">
        <label>تعداد نمایش</label>
        <select v-model.number="filters.limit">
          <option :value="50">۵۰ مورد</option>
          <option :value="100">۱۰۰ مورد</option>
          <option :value="200">۲۰۰ مورد</option>
          <option :value="500">۵۰۰ مورد</option>
        </select>
      </div>
      <div class="scope-box">
        <span>سطح دسترسی شما:</span>
        <strong>{{ scopeLabel }}</strong>
      </div>
    </div>

    <div v-if="searched" class="summary-grid">
      <div class="summary-card">
        <span>کاربران پیدا شده</span>
        <strong>{{ voterSummaries.length }}</strong>
      </div>
    </div>

    <div v-if="searched" class="result-card">
      <div class="list-header">
        <h3>خلاصه کاربران</h3>
        <span>{{ voterSummaries.length }} کاربر</span>
      </div>

      <div v-if="loading" class="state-message">در حال دریافت اطلاعات کاربران...</div>
      <div v-else-if="!voterSummaries.length" class="state-message">کاربری در محدوده دسترسی شما یافت نشد.</div>
      <div v-else class="table-responsive">
        <table class="votes-table">
          <thead>
            <tr>
              <th>کد ملی</th>
              <th>نام و نام خانوادگی</th>
              <th>کد پرسنلی</th>
              <th>استان</th>
              <th>منطقه</th>
              <th>منطقه ثبت رأی</th>
              <th>زمان ثبت رأی</th>
              <th>کد پیگیری رأی</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in voterSummaries" :key="user.national_id">
              <td>{{ user.national_id }}</td>
              <td>{{ fullName(user.first_name, user.last_name) }}</td>
              <td>{{ user.personnel_code || '---' }}</td>
              <td>{{ user.province_name || '---' }}</td>
              <td>{{ user.region_name || user.region_id || '---' }}</td>
              <td>{{ user.vote_region_name || '---' }}</td>
              <td>{{ formatVoteDateTime(user) }}</td>
              <td>{{ user.tracking_code || 'ثبت نشده' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'

export default {
  name: 'UserVoteSearch',
  data() {
    return {
      loading: false,
      searched: false,
      accessScope: 'all',
      filters: {
        q: '',
        limit: 200
      },
      result: {
        summary: []
      }
    }
  },
  computed: {
    ...mapGetters(['currentUser']),
    canSearch() {
      return (this.filters.q || '').length >= 2
    },
    voterSummaries() {
      return Array.isArray(this.result.summary) ? this.result.summary : []
    },
    scopeLabel() {
      if (this.accessScope === 'province') return 'استان خودتان'
      if (this.accessScope === 'region') return 'منطقه خودتان'
      if (this.currentUser?.roles?.includes('ADMIN')) return 'همه کاربران'
      return 'محدوده مجاز شما'
    }
  },
  methods: {
    ...mapActions(['searchUserVotes']),
    fullName(firstName, lastName) {
      return [firstName, lastName].filter(Boolean).join(' ') || '---'
    },
    formatVoteDateTime(user) {
      const shamsiValue = user?.voted_at_shamsi || user?.participant_created_at_shamsi
      if (shamsiValue) return shamsiValue

      const rawValue = user?.voted_at || user?.participant_created_at
      if (!rawValue) return 'ثبت نشده'

      const normalizedValue = typeof rawValue === 'string' ? rawValue.replace(' ', 'T') : rawValue
      const date = new Date(normalizedValue)
      if (Number.isNaN(date.getTime())) return rawValue

      return new Intl.DateTimeFormat('fa-IR-u-ca-persian', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      }).format(date)
    },
    async searchVotes() {
      if (!this.canSearch || this.loading) return
      this.loading = true
      this.searched = true
      try {
        const data = await this.searchUserVotes({
          q: this.filters.q,
          limit: this.filters.limit
        })
        this.result = {
          summary: Array.isArray(data?.summary) ? data.summary : []
        }
        this.accessScope = data?.scope || 'all'
      } catch (error) {
        this.result = { summary: [] }
        this.$bvToast.toast('خطا در جستجوی کاربران', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
      } finally {
        this.loading = false
      }
    }
  }
}
</script>

<style scoped>
.vote-search-page {
  padding: 24px;
  color: #263238;
}

.page-header,
.search-card,
.result-card,
.summary-card {
  background: #fff;
  border-radius: 18px;
  box-shadow: 0 10px 30px rgba(31, 45, 61, 0.08);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding: 22px;
  margin-bottom: 18px;
}

.page-header h2,
.list-header h3 {
  margin: 0;
  font-weight: 700;
  color: #1f2d3d;
}

.page-header p {
  margin: 8px 0 0;
  color: #607d8b;
}

.search-card {
  display: grid;
  grid-template-columns: minmax(260px, 1fr) 180px 220px;
  gap: 16px;
  align-items: end;
  padding: 20px;
  margin-bottom: 18px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 0;
}

.form-group label {
  font-weight: 600;
  color: #455a64;
}

.form-group input,
.form-group select {
  min-height: 42px;
  border: 1px solid #d8e1e8;
  border-radius: 12px;
  padding: 8px 12px;
  background: #fbfdff;
}

.form-group small {
  color: #78909c;
}

.scope-box {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px 14px;
  background: #eef5ff;
  border-radius: 14px;
  color: #3f51b5;
}

.summary-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 14px;
  margin-bottom: 18px;
}

.summary-card {
  padding: 18px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.summary-card span {
  color: #607d8b;
}

.summary-card strong {
  font-size: 1.5rem;
  color: #3f51b5;
}

.result-card {
  padding: 20px;
  margin-bottom: 18px;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}

.list-header span {
  color: #607d8b;
}

.votes-table {
  width: 100%;
  border-collapse: collapse;
  white-space: nowrap;
}

.votes-table th,
.votes-table td {
  padding: 12px;
  border-bottom: 1px solid #edf2f7;
  text-align: right;
  vertical-align: middle;
}

.votes-table th {
  background: #f6f9fc;
  color: #455a64;
  font-weight: 700;
}


.state-message {
  padding: 24px;
  color: #78909c;
  text-align: center;
  background: #f8fafc;
  border-radius: 14px;
}

@media (max-width: 992px) {
  .page-header,
  .search-card {
    grid-template-columns: 1fr;
    flex-direction: column;
    align-items: stretch;
  }

  .summary-grid {
    grid-template-columns: 1fr;
  }
}
</style>
