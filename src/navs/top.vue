<template>
  <div class="topbar-modern d-flex align-items-center justify-content-between px-3 px-md-4">
    
    <!-- سمت راست: دکمه منو + لوگو -->
    <div class="d-flex align-items-center gap-3">
      <!-- دکمه همبرگر (منوی موبایل / باز کردن سایدبار) -->
      <div class="menu-toggle-btn" @click="toggleSidebar">
        <div class="hamburger-icon" :class="{ 'active': sidebarVisible }">
          <span></span>
          <span></span>
          <span></span>
        </div>
      </div>

      <!-- لوگو و عنوان -->
      <div class="logo-area d-none d-sm-flex align-items-center gap-2" @click="goHomeIfNeeded">
        <div class="logo-icon">
          <svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect width="32" height="32" rx="8" fill="url(#logoGradient)" />
            <path d="M16 8L20 12L16 16L12 12L16 8Z" fill="white" fill-opacity="0.9" />
            <path d="M16 16L20 20L16 24L12 20L16 16Z" fill="white" fill-opacity="0.6" />
            <defs>
              <linearGradient id="logoGradient" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" stop-color="#3f51b5" />
                <stop offset="100%" stop-color="#5c6bc0" />
              </linearGradient>
            </defs>
          </svg>
        </div>
        <div class="logo-text d-none d-lg-block">
          <h6 class="mb-0 fw-bold">سامانه انتخابات</h6>
          <small class="opacity-75">الکترونیک · امن</small>
        </div>
      </div>
    </div>

    <!-- سمت چپ: اطلاعیه‌ها + پروفایل کاربر -->
    <div class="d-flex align-items-center gap-3 gap-md-4" v-if="showname">
      
      <!-- دکمه اعلان‌ها با Dropdown مدرن -->
      <b-nav-item-dropdown right no-caret toggle-class="p-0" class="notif-dropdown-modern">
        <template #button-content>
          <div class="notif-icon-wrapper position-relative">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"
              :class="{ 'shake-animation': newNotifCount > 0 && !shookOnce }">
              <path d="M12 22C13.1 22 14 21.1 14 20H10C10 21.1 10.9 22 12 22ZM18 16V11C18 7.93 16.36 5.36 13.5 4.68V4C13.5 3.17 12.83 2.5 12 2.5C11.17 2.5 10.5 3.17 10.5 4V4.68C7.63 5.36 6 7.92 6 11V16L4 18V19H20V18L18 16Z"
                fill="currentColor" />
            </svg>
            <span v-if="newNotifCount > 0" class="notif-badge-modern">
              {{ newNotifCount > 9 ? '9+' : newNotifCount }}
            </span>
          </div>
        </template>

        <b-dropdown-item v-for="(item, index) in topNotifs" :key="index" class="notif-item"
          @click="goToNotifs(item)">
          <div class="notif-title">{{ item.title }}</div>
          <small class="notif-date">{{ item.tarikh }}</small>
        </b-dropdown-item>

        <b-dropdown-divider />

        <b-dropdown-item class="text-center text-primary fw-bold" @click="$router.push({ name: 'Notifications' })">
          📢 مشاهده همه اطلاعیه‌ها
        </b-dropdown-item>
      </b-nav-item-dropdown>

      <!-- جداکننده عمودی -->
      <div class="vr opacity-25 d-none d-sm-block"></div>

      <!-- پروفایل کاربر -->
      <div class="user-info-modern d-flex align-items-center gap-2">
        <div class="user-avatar" @click="goToProfile" style="cursor:pointer">
          <svg width="36" height="36" viewBox="0 0 36 36" fill="none">
            <circle cx="18" cy="18" r="17" fill="#e8eef5" stroke="#cbd5e1" stroke-width="1" />
            <path d="M18 16C20.2091 16 22 14.2091 22 12C22 9.79086 20.2091 8 18 8C15.7909 8 14 9.79086 14 12C14 14.2091 15.7909 16 18 16Z"
              fill="#3f51b5" fill-opacity="0.7" />
            <path d="M10 24C10 20.6863 12.6863 18 16 18H20C23.3137 18 26 20.6863 26 24V26H10V24Z"
              fill="#3f51b5" fill-opacity="0.5" />
          </svg>
        </div>
        <div class="user-details d-none d-md-block" @click="goToProfile" style="cursor:pointer">
          <div class="user-name">{{ currentUser?.full_name || 'کاربر مهمان' }}</div>
          <div v-if="currentUser" class="user-meta">
            <small>{{ currentUser.userType == 3 ? 'شاغل' : 'بازنشسته' }}</small>
            <small class="mx-1">•</small>
            <small>{{ currentUser.regionName || 'منطقه نامشخص' }}</small>
          </div>
        </div>
        <div class="logout-btn" @click="logout()" title="خروج از سیستم">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M9 21H5C4.46957 21 3.96086 20.7893 3.58579 20.4142C3.21071 20.0391 3 19.5304 3 19V5C3 4.46957 3.21071 3.96086 3.58579 3.58579C3.96086 3.21071 4.46957 3 5 3H9" 
              stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M16 17L21 12L16 7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
            <path d="M21 12H9" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapActions, mapMutations, mapGetters } from "vuex";

const gohomepath = [];

export default {
  data() {
    return {
      gohomepath,
      shookOnce: false,
      seenIds: JSON.parse(localStorage.getItem('seenAnnouncements') || '[]'),
      location: "HomePage",
      showname: true,
    };
  },
  computed: {
    ...mapGetters(["processing", "sidebarVisible", "currentUser", "announcements"]),
    newNotifCount() {
      return this.announcements?.filter(n => !this.seenIds.includes(n.id)).length || 0;
    },
    topNotifs() {
      return this.announcements?.slice(0, 3) || [];
    }
  },
  methods: {
    ...mapActions(["signOut", "getAnnouncements"]),
    ...mapMutations(["setsidebarVisible"]),
    toggleSidebar() {
      this.setsidebarVisible(!this.sidebarVisible);
    },
    goHomeIfNeeded() {
      if (gohomepath.includes(this.$route.name)) {
        this.$router.push({ name: "home" });
      }
    },
    goToNotifs(item) {
      if (!this.seenIds.includes(item.id)) {
        this.seenIds.push(item.id);
        localStorage.setItem('seenAnnouncements', JSON.stringify(this.seenIds));
      }
      this.$router.push({ name: "Notifications", query: { id: item.id } });
    },
    logout() {
      this.signOut().then(() => {
        this.$router.push({ name: "landing" });
      });
    },
    goToProfile() {
      if (this.currentUser) this.$router.push({ name: "Profile" });
    },
    async loadAnnouncements() {
      if (this.currentUser) {
        await this.getAnnouncements();
      }
    }
  },
  mounted() {
    if (this.$route?.path?.includes("/sso")) this.showname = false;
    this.loadAnnouncements();
  },
  watch: {
    currentUser(val) {
      if (val) this.loadAnnouncements();
    },
    newNotifCount(val) {
      if (val > 0 && !this.shookOnce) {
        this.shookOnce = true;
        setTimeout(() => { this.shookOnce = false; }, 3000);
      }
    }
  }
};
</script>

<style scoped>
/* ========== توپبار مدرن ========== */
.topbar-modern {
  background: linear-gradient(135deg, #2c3e8f 0%, #3f51b5 50%, #5c6bc0 100%);
  height: 70px;
  position: fixed;      /* تغییر از sticky به fixed */
  top: 0;
 left: 0;

  right: 0;
  z-index: 1040;       /* بالاتر از سایدبار (1050) */
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  color: white;
}

/* دکمه همبرگر (هَمبورگر منو) */
.menu-toggle-btn {
  cursor: pointer;
  padding: 8px;
  border-radius: 12px;
  transition: background 0.2s;
}

.menu-toggle-btn:hover {
  background: rgba(255, 255, 255, 0.12);
}

.hamburger-icon {
  width: 24px;
  height: 18px;
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.hamburger-icon span {
  display: block;
  height: 2.5px;
  width: 100%;
  background: white;
  border-radius: 4px;
  transition: all 0.25s ease;
}

.hamburger-icon.active span:nth-child(1) {
  transform: translateY(7.5px) rotate(45deg);
}

.hamburger-icon.active span:nth-child(2) {
  opacity: 0;
}

.hamburger-icon.active span:nth-child(3) {
  transform: translateY(-7.5px) rotate(-45deg);
}

/* لوگو */
.logo-area {
  cursor: pointer;
}

.logo-text h6 {
  font-size: 0.9rem;
  line-height: 1.2;
}

.logo-text small {
  font-size: 0.65rem;
  opacity: 0.8;
}

/* ناحیه اعلان‌ها */
.notif-dropdown-modern ::v-deep .dropdown-toggle {
  background: transparent !important;
  border: none !important;
  color: white;
}

.notif-icon-wrapper {
  cursor: pointer;
  padding: 6px;
  border-radius: 50%;
  transition: background 0.2s;
  color: white;
}

.notif-icon-wrapper:hover {
  background: rgba(255, 255, 255, 0.15);
}

.notif-badge-modern {
  position: absolute;
  top: -2px;
  right: -2px;
  background: #ef4444;
  color: white;
  font-size: 9px;
  font-weight: bold;
  padding: 2px 5px;
  border-radius: 20px;
  line-height: 1;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
}

/* انیمیشن زنگوله */
@keyframes shakeNotification {
  0% { transform: rotate(0deg); }
  15% { transform: rotate(12deg); }
  30% { transform: rotate(-10deg); }
  45% { transform: rotate(8deg); }
  60% { transform: rotate(-6deg); }
  75% { transform: rotate(4deg); }
  100% { transform: rotate(0deg); }
}

.shake-animation {
  animation: shakeNotification 0.6s ease-in-out;
  transform-origin: top center;
}

/* آیتم اعلان در dropdown */
.notif-item {
  padding: 10px 16px;
  border-bottom: 1px solid #f0f0f0;
}

.notif-item:hover {
  background: #f8faff;
}

.notif-title {
  font-size: 0.85rem;
  font-weight: 500;
  color: #1e293b;
}

.notif-date {
  font-size: 0.7rem;
  color: #94a3b8;
}

/* پروفایل کاربر */
.user-info-modern {
  padding: 4px 8px;
  border-radius: 40px;
  transition: background 0.2s;
}

.user-info-modern:hover {
  background: rgba(255, 255, 255, 0.1);
}

.user-avatar {
  display: flex;
  align-items: center;
}

.user-name {
  font-size: 0.85rem;
  font-weight: 600;
  line-height: 1.3;
}

.user-meta {
  font-size: 0.7rem;
  opacity: 0.85;
}

/* دکمه خروج */
.logout-btn {
  cursor: pointer;
  padding: 6px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  transition: all 0.2s;
  color: white;
}

.logout-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: scale(1.05);
}

/* جداکننده عمودی */
.vr {
  width: 1px;
  height: 30px;
  background: white;
}

/* موبایل */
@media (max-width: 576px) {
  .topbar-modern {
    height: 60px;
    padding: 0 12px;
  }
  
  .user-name {
    font-size: 0.75rem;
  }
}
</style>