<template>
  <div class="profile-page">

    <!-- هدر -->
    <div class="profile-hero">
      <div class="avatar-circle">
        <svg width="64" height="64" viewBox="0 0 64 64" fill="none">
          <circle cx="32" cy="32" r="31" fill="#e8eef5" stroke="#cbd5e1" stroke-width="1.5"/>
          <path d="M32 29C35.31 29 38 26.31 38 23C38 19.69 35.31 17 32 17C28.69 17 26 19.69 26 23C26 26.31 28.69 29 32 29Z" fill="#3f51b5" fill-opacity="0.8"/>
          <path d="M18 43C18 37.48 22.48 33 28 33H36C41.52 33 46 37.48 46 43V46H18V43Z" fill="#3f51b5" fill-opacity="0.6"/>
        </svg>
      </div>
      <div class="hero-name">{{ currentUser.full_name }}</div>
      <div class="hero-role">{{ roleLabel }}</div>
    </div>

    <!-- اطلاعات اصلی -->
    <div class="info-grid">
      <div class="info-card">
        <div class="info-icon">🪪</div>
        <div class="info-body">
          <div class="info-label">کد ملی</div>
          <div class="info-value ltr">{{ currentUser.national_id || '—' }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">🔢</div>
        <div class="info-body">
          <div class="info-label">کد پرسنلی</div>
          <div class="info-value ltr">{{ currentUser.personnel_code || '—' }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">📍</div>
        <div class="info-body">
          <div class="info-label">حوزه انتخابیه</div>
          <div class="info-value">{{ currentUser.regionName || '—' }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">💼</div>
        <div class="info-body">
          <div class="info-label">آخرین پست سازمانی</div>
          <div class="info-value">{{ currentUser.orgPositionDesc || '—' }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">{{ currentUser.userType == 3 ? '🟢' : '🔵' }}</div>
        <div class="info-body">
          <div class="info-label">وضعیت اشتغال</div>
          <div class="info-value">{{ currentUser.userType == 3 ? 'شاغل' : 'بازنشسته' }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">🏷️</div>
        <div class="info-body">
          <div class="info-label">نقش در سامانه</div>
          <div class="info-value">{{ roleLabel }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">🎓</div>
        <div class="info-body">
          <div class="info-label">مدرک تحصیلی</div>
          <div class="info-value">{{ userstatusInfo ? (userstatusInfo.education || '—') : '...' }}</div>
        </div>
      </div>

      <div class="info-card">
        <div class="info-icon">📅</div>
        <div class="info-body">
          <div class="info-label">سنوات عضویت در صندوق</div>
          <div class="info-value">{{ userstatusInfo ? (userstatusInfo.yearsOfService != null ? userstatusInfo.yearsOfService + ' سال' : '—') : '...' }}</div>
        </div>
      </div>
    </div>

    <!-- وضعیت شرایط احراز -->
    <div class="status-section" v-if="userstatusInfo">
      <div class="section-title">استعلام شرایط احراز</div>
      <div class="status-list">
        <div class="status-row" :class="userstatusInfo.membershipActive ? 'ok' : 'fail'">
          <span class="status-icon">{{ userstatusInfo.membershipActive ? '✅' : '❌' }}</span>
          <span>عضویت در صندوق ذخیره فرهنگیان</span>
        </div>
        <div class="status-row" :class="userstatusInfo.membershipYears ? 'ok' : 'fail'">
          <span class="status-icon">{{ userstatusInfo.membershipYears ? '✅' : '❌' }}</span>
          <span>حداقل یک سال سابقه عضویت در صندوق</span>
        </div>
        <div class="status-row" v-if="userstatusInfo.alreadyRegistered !== undefined"
          :class="!userstatusInfo.alreadyRegistered ? 'ok' : 'warn'">
          <span class="status-icon">{{ !userstatusInfo.alreadyRegistered ? '✅' : '⚠️' }}</span>
          <span>{{ userstatusInfo.alreadyRegistered ? 'ثبت‌نام داوطلبی قبلاً انجام شده' : 'بدون ثبت‌نام قبلی' }}</span>
        </div>
      </div>
    </div>
    <div class="status-section loading-status" v-else>
      <b-spinner small variant="primary" class="me-2" />
      <span>در حال بارگذاری اطلاعات...</span>
    </div>

    <!-- دکمه خروج -->
    <div class="logout-section">
      <b-button variant="outline-danger" size="sm" @click="logout">
        <b-icon icon="box-arrow-right" class="ml-1" />
        خروج از سامانه
      </b-button>
    </div>

  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

const roleMap = {
  ADMIN:      'مدیر سیستم',
  SUPERVISOR: 'ناظر',
  EXECUTIVE:  'اجرایی',
  CANDIDATE:  'داوطلب',
  VOTER:      'رأی‌دهنده',
};

export default {
  name: "UserProfile",
  computed: {
    ...mapGetters(["currentUser", "userstatusInfo"]),
    roleLabel() {
      const role = this.currentUser?.roles?.[0] || this.currentUser?.roles || '';
      return roleMap[role] || role;
    }
  },
  methods: {
    ...mapActions(["signOut", "userstatus"]),
    logout() {
      this.signOut().then(() => {
        this.$router.push({ name: "landing" });
      });
    }
  },
  mounted() {
    if (!this.userstatusInfo) {
      this.userstatus();
    }
  }
};
</script>

<style scoped>
.profile-page {
  max-width: 600px;
  margin: 0 auto;
  padding: 24px 16px 40px;
}

/* هدر */
.profile-hero {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 28px 20px 24px;
  background: linear-gradient(135deg, #3f51b5 0%, #5c6bc0 100%);
  border-radius: 18px;
  color: #fff;
  margin-bottom: 20px;
  box-shadow: 0 6px 24px rgba(63, 81, 181, 0.25);
}

.avatar-circle {
  background: rgba(255,255,255,0.15);
  border-radius: 50%;
  padding: 8px;
  backdrop-filter: blur(6px);
}

.hero-name {
  font-size: 1.2rem;
  font-weight: 700;
  letter-spacing: 0.3px;
}

.hero-role {
  font-size: 0.82rem;
  background: rgba(255,255,255,0.2);
  padding: 3px 14px;
  border-radius: 20px;
}

/* گرید اطلاعات */
.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 20px;
}

.info-card {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #fff;
  border-radius: 12px;
  padding: 14px 16px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}

.info-icon {
  font-size: 1.4rem;
  flex-shrink: 0;
}

.info-label {
  font-size: 0.72rem;
  color: #94a3b8;
  margin-bottom: 3px;
}

.info-value {
  font-size: 0.88rem;
  font-weight: 600;
  color: #1e293b;
}

.info-value.ltr {
  direction: ltr;
  text-align: right;
}

/* وضعیت احراز شرایط */
.status-section {
  background: #fff;
  border-radius: 14px;
  padding: 18px 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
  margin-bottom: 20px;
}

.loading-status {
  display: flex;
  align-items: center;
  color: #64748b;
  font-size: 0.88rem;
}

.section-title {
  font-size: 0.85rem;
  font-weight: 700;
  color: #475569;
  margin-bottom: 14px;
  padding-bottom: 10px;
  border-bottom: 1px solid #f1f5f9;
}

.status-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.status-row {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.88rem;
  padding: 10px 14px;
  border-radius: 10px;
}

.status-row.ok   { background: #f0fdf4; color: #15803d; }
.status-row.fail { background: #fef2f2; color: #b91c1c; }
.status-row.warn { background: #fffbeb; color: #b45309; }

.status-icon {
  font-size: 1.1rem;
}

/* خروج */
.logout-section {
  text-align: center;
}
</style>
