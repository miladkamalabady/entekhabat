<template>
  <div class="landing-root">

    <!-- ── نوار بالا ── -->
    <header class="landing-header">
      <div class="header-inner">
        <div class="header-logo">
          <img src="assets/img/szf.0.jpg" alt="صندوق ذخیره فرهنگیان" />
          <div class="header-title">
            <span class="title-main">سامانه انتخابات</span>
            <span class="title-sub">صندوق ذخیره فرهنگیان</span>
          </div>
        </div>
        <button class="login-btn" @click="handleLogin">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" style="margin-left:6px">
            <path d="M15 3H19C20.1046 3 21 3.89543 21 5V19C21 20.1046 20.1046 21 19 21H15" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            <path d="M10 17L15 12L10 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M15 12H3" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
          {{ currentUser ? 'ورود به سامانه' : 'ورود با حساب فرهنگی' }}
        </button>
      </div>
    </header>

    <!-- ── hero + slider ── -->
    <section class="hero-section">
      <div class="hero-bg"></div>
      <div class="hero-content">

        <div class="hero-text">
          <h1 class="hero-title">انتخابات نمایندگان اعضای فرهنگی</h1>
          <p class="hero-sub">هیأت امنای موسسه صندوق ذخیره فرهنگیان</p>
          <button class="hero-cta" @click="handleLogin">
            {{ currentUser ? 'ورود به پنل کاربری' : 'ورود با حساب فرهنگی' }}
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" style="margin-right:6px">
              <path d="M19 12H5M5 12L12 19M5 12L12 5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </button>
        </div>

        <!-- اسلایدر -->
        <div class="slider-wrap">
          <b-carousel
            v-model="slide"
            :interval="4500"
            controls
            indicators
            class="election-carousel"
          >
            <b-carousel-slide>
              <template #img>
                <div class="slide-card slide-card--blue">
                  <div class="slide-icon">🗳️</div>
                  <h3>انتخابات الکترونیک</h3>
                  <p>رأی‌گیری آنلاین و امن برای تمامی اعضای فرهنگی سراسر کشور</p>
                </div>
              </template>
            </b-carousel-slide>

            <b-carousel-slide>
              <template #img>
                <div class="slide-card slide-card--green">
                  <div class="slide-icon">📋</div>
                  <h3>ثبت‌نام داوطلبان</h3>
                  <p>داوطلبان واجد شرایط می‌توانند در بازه زمانی مشخص ثبت‌نام کنند</p>
                </div>
              </template>
            </b-carousel-slide>

            <b-carousel-slide>
              <template #img>
                <div class="slide-card slide-card--purple">
                  <div class="slide-icon">🔒</div>
                  <h3>شفافیت و امنیت</h3>
                  <p>فرآیند رأی‌گیری با بالاترین سطح امنیت و شفافیت برگزار می‌شود</p>
                </div>
              </template>
            </b-carousel-slide>

            <b-carousel-slide>
              <template #img>
                <div class="slide-card slide-card--orange">
                  <div class="slide-icon">📊</div>
                  <h3>نتایج لحظه‌ای</h3>
                  <p>نتایج انتخابات بلافاصله پس از پایان رأی‌گیری اعلام خواهد شد</p>
                </div>
              </template>
            </b-carousel-slide>
          </b-carousel>
        </div>

      </div>
    </section>

    <!-- ── آمار ── -->
    <section class="stats-section">
      <div class="stat-item">
        <span class="stat-num">سراسری</span>
        <span class="stat-lbl">پوشش جغرافیایی</span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-num">الکترونیک</span>
        <span class="stat-lbl">روش رأی‌گیری</span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-num">امن</span>
        <span class="stat-lbl">سطح امنیت</span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-num">شفاف</span>
        <span class="stat-lbl">فرآیند انتخابات</span>
      </div>
    </section>

    <!-- ── اطلاعیه‌ها ── -->
    <section class="announcements-section">
      <div class="section-header">
        <div class="section-badge">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M12 22C13.1 22 14 21.1 14 20H10C10 21.1 10.9 22 12 22ZM18 16V11C18 7.93 16.36 5.36 13.5 4.68V4C13.5 3.17 12.83 2.5 12 2.5C11.17 2.5 10.5 3.17 10.5 4V4.68C7.63 5.36 6 7.92 6 11V16L4 18V19H20V18L18 16Z" fill="currentColor"/></svg>
          اطلاعیه‌ها
        </div>
        <h2 class="section-title">آخرین اطلاعیه‌های انتخابات</h2>
      </div>

      <div v-if="loadingAnn" class="text-center py-4">
        <b-spinner variant="primary" />
      </div>

      <div v-else-if="!announcements.length" class="ann-empty">
        <svg width="40" height="40" viewBox="0 0 24 24" fill="none"><path d="M12 22C13.1 22 14 21.1 14 20H10C10 21.1 10.9 22 12 22ZM18 16V11C18 7.93 16.36 5.36 13.5 4.68V4C13.5 3.17 12.83 2.5 12 2.5C11.17 2.5 10.5 3.17 10.5 4V4.68C7.63 5.36 6 7.92 6 11V16L4 18V19H20V18L18 16Z" fill="#cbd5e1"/></svg>
        <p class="mt-2 text-muted">در حال حاضر اطلاعیه‌ای منتشر نشده است.</p>
      </div>

      <div v-else class="ann-grid">
        <div
          v-for="item in announcements"
          :key="item.id"
          class="ann-card"
          @click="openAnn(item)"
        >
          <div class="ann-card-accent"></div>
          <div class="ann-card-body">
            <h4 class="ann-card-title">{{ item.title }}</h4>
            <p class="ann-card-preview">{{ preview(item.content) }}</p>
            <div class="ann-card-footer">
              <span class="ann-date">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" style="margin-left:4px"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/><path d="M12 6V12L16 14" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>
                {{ item.created_at_shamsi }}
              </span>
              <span class="ann-read-more">بیشتر بخوانید ←</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── فوتر ── -->
    <footer class="landing-footer">
      <p>کلیه حقوق متعلق به صندوق ذخیره فرهنگیان می‌باشد</p>
    </footer>

    <!-- مودال اطلاعیه -->
    <b-modal v-model="showAnn" hide-header hide-footer centered size="lg" body-class="p-0" @hidden="selectedAnn=null">
      <div v-if="selectedAnn" class="ann-modal">
        <div class="ann-modal-header">
          <h5 class="ann-modal-title">{{ selectedAnn.title }}</h5>
          <button class="ann-modal-close" @click="showAnn=false">✕</button>
        </div>
        <div class="ann-modal-body">
          <p class="ann-modal-date">{{ selectedAnn.created_at_shamsi }}</p>
          <div class="ann-modal-content">{{ selectedAnn.content }}</div>
        </div>
      </div>
    </b-modal>

  </div>
</template>

<script>
import { apiUrlrtb } from "@/constants/config";
import { mapGetters, mapMutations, mapActions } from "vuex";
export default {
  name: "LandingPage",
  data() {
    return {
      slide: 0,
      announcements: [],
      loadingAnn: false,
      showAnn: false,
      selectedAnn: null,
    };
  },
  computed: {
    currentUser() {
      try { return JSON.parse(localStorage.getItem("user")); } catch { return null; }
    },
  },
  methods: {
     ...mapActions(["getAnnouncements"]),
    handleLogin() {
      if (this.currentUser) {
        this.$router.push({ name: "home" });
      } else {
        window.location.href = "https://my.medu.ir";
      }
    },
    preview(text) {
      if (!text) return "";
      return text.length > 120 ? text.slice(0, 120) + "..." : text;
    },
    openAnn(item) {
      this.selectedAnn = item;
      this.showAnn = true;
    },
    async loadAnnouncements() {
      this.loadingAnn = true;
      try {
        const res = await this.getAnnouncements();
        const json = await res.json();
        this.announcements = json?.data || [];
      } catch {
        this.announcements = [];
      } finally {
        this.loadingAnn = false;
      }
    },
  },
  mounted() {
    this.loadAnnouncements();
  },
};
</script>

<style scoped>
* { box-sizing: border-box; }

.landing-root {
  min-height: 100vh;
  background: #f8fafc;
  font-family: inherit;
  direction: rtl;
}

/* ── هدر ── */
.landing-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(255,255,255,0.95);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid #e2e8f0;
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
}
.header-inner {
  max-width: 1100px;
  margin: 0 auto;
  padding: 12px 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.header-logo {
  display: flex;
  align-items: center;
  gap: 12px;
}
.header-logo img {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  object-fit: cover;
}
.title-main {
  display: block;
  font-size: 0.95rem;
  font-weight: 700;
  color: #1e293b;
}
.title-sub {
  display: block;
  font-size: 0.72rem;
  color: #64748b;
}
.login-btn {
  display: flex;
  align-items: center;
  background: linear-gradient(135deg, #3f51b5, #5c6bc0);
  color: white;
  border: none;
  border-radius: 12px;
  padding: 10px 20px;
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 14px rgba(63,81,181,0.3);
}
.login-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(63,81,181,0.4);
}

/* ── hero ── */
.hero-section {
  position: relative;
  padding: 60px 24px 80px;
  overflow: hidden;
}
.hero-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(145deg, #2c3e8f 0%, #3f51b5 45%, #5c6bc0 100%);
  z-index: 0;
}
.hero-bg::after {
  content: '';
  position: absolute;
  inset: 0;
  background: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.04'%3E%3Ccircle cx='30' cy='30' r='4'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
}
.hero-content {
  position: relative;
  z-index: 1;
  max-width: 1100px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 48px;
  align-items: center;
}
.hero-title {
  font-size: 1.9rem;
  font-weight: 800;
  color: white;
  line-height: 1.4;
  margin-bottom: 12px;
}
.hero-sub {
  font-size: 1rem;
  color: rgba(255,255,255,0.75);
  margin-bottom: 28px;
}
.hero-cta {
  display: inline-flex;
  align-items: center;
  background: white;
  color: #3f51b5;
  border: none;
  border-radius: 14px;
  padding: 14px 28px;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 6px 24px rgba(0,0,0,0.15);
}
.hero-cta:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 32px rgba(0,0,0,0.2);
}

/* ── اسلایدر ── */
.slider-wrap { border-radius: 20px; overflow: hidden; box-shadow: 0 20px 60px rgba(0,0,0,0.25); }
.election-carousel { border-radius: 20px; }

.slide-card {
  height: 280px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px;
  text-align: center;
  border-radius: 20px;
}
.slide-card--blue   { background: linear-gradient(135deg, #1a237e, #3949ab); }
.slide-card--green  { background: linear-gradient(135deg, #1b5e20, #388e3c); }
.slide-card--purple { background: linear-gradient(135deg, #4a148c, #7b1fa2); }
.slide-card--orange { background: linear-gradient(135deg, #bf360c, #e64a19); }
.slide-icon { font-size: 3rem; margin-bottom: 12px; }
.slide-card h3 { color: white; font-size: 1.25rem; font-weight: 700; margin-bottom: 8px; }
.slide-card p  { color: rgba(255,255,255,0.82); font-size: 0.88rem; line-height: 1.7; margin: 0; }

/* ── آمار ── */
.stats-section {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0;
  background: white;
  border-bottom: 1px solid #e2e8f0;
  padding: 28px 24px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.04);
}
.stat-item { text-align: center; padding: 0 36px; }
.stat-num { display: block; font-size: 1.4rem; font-weight: 800; color: #3f51b5; }
.stat-lbl { display: block; font-size: 0.75rem; color: #64748b; margin-top: 2px; }
.stat-divider { width: 1px; height: 40px; background: #e2e8f0; }

/* ── اطلاعیه‌ها ── */
.announcements-section {
  max-width: 1100px;
  margin: 0 auto;
  padding: 60px 24px;
}
.section-header { text-align: center; margin-bottom: 36px; }
.section-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #ede7f6;
  color: #5e35b1;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 12px;
}
.section-title { font-size: 1.6rem; font-weight: 800; color: #1e293b; margin: 0; }

.ann-empty { text-align: center; padding: 40px; color: #94a3b8; }

.ann-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}
.ann-card {
  background: white;
  border-radius: 16px;
  border: 1.5px solid #e2e8f0;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  display: flex;
}
.ann-card:hover {
  border-color: #3f51b5;
  box-shadow: 0 8px 28px rgba(63,81,181,0.12);
  transform: translateY(-2px);
}
.ann-card-accent {
  width: 5px;
  min-width: 5px;
  background: linear-gradient(180deg, #3f51b5, #5c6bc0);
}
.ann-card-body { padding: 20px; flex: 1; display: flex; flex-direction: column; }
.ann-card-title { font-size: 0.95rem; font-weight: 700; color: #1e293b; margin-bottom: 8px; }
.ann-card-preview { font-size: 0.82rem; color: #64748b; line-height: 1.7; flex: 1; margin-bottom: 14px; }
.ann-card-footer { display: flex; justify-content: space-between; align-items: center; }
.ann-date { font-size: 0.72rem; color: #94a3b8; display: flex; align-items: center; }
.ann-read-more { font-size: 0.75rem; color: #3f51b5; font-weight: 600; }

/* ── فوتر ── */
.landing-footer {
  background: #1e293b;
  color: #94a3b8;
  text-align: center;
  padding: 20px;
  font-size: 0.8rem;
}

/* ── مودال ── */
.ann-modal { border-radius: 16px; overflow: hidden; }
.ann-modal-header {
  background: linear-gradient(135deg, #3f51b5, #5c6bc0);
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}
.ann-modal-title { color: white; font-size: 1rem; font-weight: 700; margin: 0; flex: 1; line-height: 1.5; }
.ann-modal-close {
  background: rgba(255,255,255,0.2);
  border: none;
  color: white;
  border-radius: 8px;
  padding: 4px 8px;
  cursor: pointer;
  font-size: 0.85rem;
  margin-right: 12px;
}
.ann-modal-body { padding: 24px; }
.ann-modal-date { font-size: 0.75rem; color: #94a3b8; margin-bottom: 16px; }
.ann-modal-content { font-size: 0.92rem; line-height: 2; color: #334155; white-space: pre-wrap; }

/* ── موبایل ── */
@media (max-width: 768px) {
  .hero-content { grid-template-columns: 1fr; }
  .hero-title { font-size: 1.4rem; }
  .slider-wrap { margin-top: 24px; }
  .stats-section { flex-wrap: wrap; gap: 16px; }
  .stat-item { padding: 0 16px; }
  .stat-divider { display: none; }
  .ann-grid { grid-template-columns: 1fr; }
}
</style>
