<template>
  <div id="sidbarrightnew" class="notinvoice mt-2" @click.stop="() => {}">
    <!-- Sidebar با رنگ هماهنگ توپبار -->
    <div class="custom-sidebar main-menu" :class="{ 'sidebar-collapsed': !sidebarVisible }" v-if="sidebarVisible"
      style="overflow: auto;">
      <b-nav vertical class="sidebar-nav overflow-auto px-3 py-2">
        
        <!-- Header Sidebar -->
        <b-nav-item class="mb-3">
          <div class="text-center sidebar-header">
            <img src="assets/img/szf.0.jpg" style="max-width: 80px; height: auto; border-radius: 16px; background: white; padding: 4px;" />
            <h6 class="mt-3 fw-bold text-white">سامانه انتخابات</h6>
            <p class="text-white-50 small">الکترونیک · امن · شفاف</p>
          </div>
          <hr class="my-2" style="border-color: rgba(255,255,255,0.2);" />
          
          <!-- پروفایل کاربر -->
          <div class="d-flex align-items-center gap-2 p-2 rounded-3 user-profile-card">
            <div class="avatar-icon">
              <i class="bi bi-person-circle fs-4 text-white"></i>
            </div>
            <div class="user-info text-white">
              <p class="mb-0 fw-bold small">{{ currentUser?.full_name || 'کاربر مهمان' }}</p>
              <small class="text-white-50">{{ currentUser?.roles?.[0] || 'نقش نامشخص' }}</small>
            </div>
          </div>
          <hr class="my-2" style="border-color: rgba(255,255,255,0.2);" />
        </b-nav-item>

        <!-- منوها -->
        <div v-for="(val, index) in filteredMenu" :key="`a${index}`">
          <!-- منوی دارای زیرمنو (cate == 1) -->
          <div v-if="val?.cate == 1" class="sidebar-menu-parent mb-1"
            :class="{ 'active-parent': isParentActive(val.title), 'menu-open': openMenu === val.title }"
            @click.prevent="toggleMenu(val.title)">
            <span class="d-flex align-items-center justify-content-between w-100">
              <div class="d-flex align-items-center gap-2 text-white">
                <i :class="val.icon || 'bi bi-folder'"></i>
                <span>{{ val.title }}</span>
              </div>
              <i class="bi bi-chevron-down menu-arrow text-white" :class="{ 'rotated': openMenu === val.title }"></i>
            </span>
          </div>

          <!-- زیرمنوها -->
          <b-collapse :visible="openMenu === val.title" class="mt-1">
            <b-nav vertical class="submenu-modern ps-3">
              <b-nav-item v-for="(val1, index1) in profilecontent1?.filter(x => x.cate == val.title)"
                :key="`b${index1}`" class="submenu-item"
                :class="{ 'active-submenu': $route.name === val1.link }" @click="go2(val1)">
                <span class="d-flex align-items-center gap-2 text-white">
                  <i :class="val1.icon || 'bi bi-arrow-left-circle'"></i>
                  <span>{{ val1.title }}</span>
                </span>
              </b-nav-item>
            </b-nav>
          </b-collapse>

          <!-- آیتم ساده (بدون زیرمنو) -->
          <div v-if="val?.cate != 1" class="sidebar-menu-item mb-1"
            :class="{ 'active-item': panelactiveparvande == val.link && !openMenu }" @click="go2(val)">
            <div class="d-flex align-items-center gap-2 w-100 text-white">
              <i :class="val.icon || 'bi bi-grid'"></i>
              <span>{{ val.title }}</span>
            </div>
            <span v-if="val?.active" class="spinner-grow spinner-grow-sm text-light ms-2"></span>
          </div>
        </div>
      </b-nav>
    </div>

  </div>
</template>

<script>
import "../navs/sideffect.css";
import { isMobile } from "../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";

export default {
  data() {
    return {
      openMenu: null,
      isMobile,
      profilecontent1: [],
      profilecontent: [
        { title: "خانه", link: "home", icon: "bi bi-house-door", type: "3", cate: 2 },
        { title: "ثبت درخواست", link: "request", icon: "bi bi-file-earmark-text", type: "3", roles: ['VOTER'] },
        { title: "تبلیغات", link: "ViewAdvertise", icon: "bi bi-megaphone", type: "3" },
        { title: "انتخابات", link: "votingPage", icon: "bi bi-check2-square", type: "3", requiresActive: true },
        { title: "مشاهده نتایج", link: "final-election", icon: "bi bi-bar-chart-steps", type: "3", cate: 2, requiresActive: true },
        { title: "کارتابل اجرایی", link: "executive-dashboard", icon: "bi bi-inbox", type: "3", cate: 2, roles: ['EXECUTIVE'] },
        { title: "کارتابل نظارت", link: "supervisor-dashboard", icon: "bi bi-shield-check", type: "3", cate: 2, roles: ['SUPERVISOR'] },
        { title: "زمان‌بندی انتخابات", link: "system-schedule", icon: "bi bi-calendar-event", type: "3", cate: 2, roles: ['ADMIN'] },
        { title: "گزارش لاگ", link: "logs", icon: "bi bi-journal-text", type: "3", cate: 2, roles: ['ADMIN'] },
        { title: "پشتیبانی", link: "Contact", icon: "bi bi-headset", type: "3", cate: 2, roles: ['CANDIDATE', 'SUPERVISOR'] },
      ],
    };
  },
  mounted() {
    if (this.currentUser?.roles == 'CANDIDATE') {
      this.getstateCandid();
    }
    setTimeout(() => {
      if (this.$route?.name != 'home' && !this.ConfigInfo && this.currentUser)
        this.getConfig();
    }, 1000);

    this.setpanelactiveparvande(this.$route?.name);
    if (this.profilecontent1.filter(x => x.link == this.$route?.name).length > 0)
      this.openMenu = this.profilecontent1.filter(x => x.link == this.$route?.name)[0].cate;
    else if (this.profilecontent.filter(x => x.link == this.$route?.name).length > 0) {
      this.openMenu = null;
    }
  },
  beforeDestroy() {
    if (this.stateRefreshIntervalId) {
      clearInterval(this.stateRefreshIntervalId);
      this.stateRefreshIntervalId = null;
    }
  },
  computed: {
    ...mapGetters(["currentUser", "panelactiveparvande", "sidebarVisible", "ConfigInfo", "stateCandidInfo"]),
    filteredMenu() {
      return this.profilecontent.filter(item => {
        let roleAllowed = true;
        if (item?.roles) roleAllowed = item?.roles?.includes(this.currentUser?.roles[0]);
        const disabled = item.requiresActive && !this.ConfigInfo?.active;
        return roleAllowed && !disabled;
      });
    }
  },
  methods: {
    ...mapMutations(["setpanelactiveparvande", "setsidebarVisible", "setProcessing", "setRequestStatus"]),
    ...mapActions(["getstateCandid", "getConfig"]),
    toggleMenu(title) {
      this.openMenu = this.openMenu === title ? null : title;
    },
    isParentActive(title) {
      return this.profilecontent1?.some(x => x.cate === title && x.link === this.$route.name);
    },
    go2(val) {
      if (val.type == 2) {
        this.$notify("info", "سرویس", val.popup, { duration: 3000, permanent: false });
      } else if (val.type == 1) {
        this.setProcessing(false);
        if (this.isMobile()) this.setsidebarVisible(false);
        if (this.$route?.name != 'home') this.$router.push({ name: 'home' });
        setTimeout(() => this.setpanelactiveparvande(val.link), 500);
      } else if (val.type == 3) {
        if (this.isMobile()) this.setsidebarVisible(false);
        this.setpanelactiveparvande(val.link);
        if (this.$route?.name != val.link) this.$router.push({ name: val.link });
      }
    },
  },
  watch: {
    stateCandidInfo(val) {
      if (val) this.setRequestStatus(val?.requestStatus);
    }
  },
};
</script>

<style scoped>
/* ========== سایدبار هماهنگ با توپبار (آبی گرادیانتی) ========== */
.custom-sidebar {
  position: fixed;
  right: 0;
  top: 70px;           /* فاصله از بالا به اندازه ارتفاع topbar */
  width: 280px;
  height: calc(100vh - 70px);  /* ارتفاع باقیمانده صفحه */
  background: linear-gradient(145deg, #2c3e8f 0%, #3f51b5 50%, #5c6bc0 100%);
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.15);
  transition: all 0.3s ease;
  z-index: 1040;   
  overflow-y: auto;
}

.sidebar-collapsed {
  transform: translateX(100%);
}

/* هدر سایدبار */
.sidebar-header {
  background: rgba(255, 255, 255, 0.08);
  border-radius: 24px;
  padding: 12px;
  backdrop-filter: blur(4px);
}

/* کارت کاربر */
.user-profile-card {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 48px;
  padding: 8px 12px;
  backdrop-filter: blur(4px);
}

.avatar-icon i {
  font-size: 2rem;
  color: white;
}

/* آیتم‌های منو - متن سفید */
.sidebar-menu-parent,
.sidebar-menu-item {
  padding: 12px 12px;
  border-radius: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.9rem;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.85);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.sidebar-menu-parent:hover,
.sidebar-menu-item:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: translateX(-4px);
  color: white;
}

/* آیتم فعال */
.active-parent,
.active-item {
  background: rgba(255, 255, 255, 0.25);
  color: white;
  font-weight: 600;
  border-right: 3px solid white;
}

/* زیرمنوها */
.submenu-modern .submenu-item {
  padding: 8px 12px;
  font-size: 0.85rem;
  cursor: pointer;
  transition: 0.2s;
  border-radius: 12px;
  color: rgba(255, 255, 255, 0.8);
}

.submenu-modern .submenu-item:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: translateX(-4px);
  color: white;
}

.active-submenu {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  font-weight: 500;
  color: white !important;
}

/* آیکون فلش */
.menu-arrow {
  transition: transform 0.25s ease;
}

.menu-arrow.rotated {
  transform: rotate(180deg);
}


/* اسکرول‌بار */
.custom-sidebar::-webkit-scrollbar {
  width: 5px;
}

.custom-sidebar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
}

.custom-sidebar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.4);
  border-radius: 10px;
}

/* موبایل */
@media (max-width: 768px) {
  .custom-sidebar {
    top: 60px;
    height: calc(100vh - 60px);
    width: 260px;
  }
  
}
</style>