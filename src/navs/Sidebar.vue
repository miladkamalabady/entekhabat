<template>
  <div id="sidbarrightnew" class=" notinvoice mt-2" @click.stop="() => { }">
    <!-- Sidebar -->
    <div class="custom-sidebar main-menu" v-if="sidebarVisible" style="overflow: auto;">
      <b-nav vertical class="sidebar-nav overflow-auto px-2">
        <b-nav-item>
          <!-- نمایه کاربر -->
          <div class="text-center ">

            <img src="assets/img/szf.0.jpg" style="max-width:100%;height: 50px;" />

            <h6 class="text-dark muirtl-16hg5vz">وزارت آموزش و پرورش</h6>
            <p class="text-dark muirtl-1rehyf">سامانه انتخابات الکترونیک</p>
          </div>
          <hr class="my-2 mb-0" />
          <div class="d-flex align-items-center profile-card" style="gap:0.5rem">
            <!-- <img :src="currentUser?.img" class="profile" /> -->
            <p class="muirtl-78ml10">
              {{ currentUser?.full_name }}
            </p>
          </div>
          <hr class="my-2" />

        </b-nav-item>
        <div v-for="(val, index) in filteredMenu" :key="`a${index}`">
          <!-- منوی بازشو (کالپس) -->
          <div style="padding: 15px 5px;" v-if="val?.cate == 1" class="cursor-pointer mb-0 bb37b3d5 muirtl-zor93q"
            :class="{ active: isParentActive(val.title), 'Mui-selected': openMenu === val.title }"
            @click.prevent="toggleMenu(val.title)">
            <span class="d-flex" style="justify-content:space-between">
              <div>
                <img :src="val.img" style="width:20px;" class="ml-2" />
                {{ val.title }}
              </div>
              <i class="glyph-icon simple-icon-arrow-down rotate-icon"
                :class="{ 'rotated': openMenu === val.title, 'text-light': openMenu === val.title }" style:></i>
            </span>
          </div>

          <!-- زیرمنو -->
          <b-collapse :visible="openMenu === val.title">
            <b-nav vertical class="submenu pl-2 mt-1">
              <b-nav-item v-for="(val1, index1) in profilecontent1?.filter(x => x.cate == val.title)"
                :key="`b${index1}`" class="submenu-item bb37b3d5 "
                :class="{ 'Mui-selected': openMenu === val1.cate && (val1.link == panelactiveparvande) }"
                @click="go2(val1)"><span class="pr-2" style="color:#000">
                  <span class="glyph-icon simple-icon-arrow-left-circle"></span>
                  {{ val1.title }}
                </span>
              </b-nav-item>
            </b-nav>
          </b-collapse>

          <!-- آیتم معمولی -->
          <p v-if="val?.cate != 1" style="padding: 12px 5px;" class="cursor-pointer mb-0 bb37b3d5 muirtl-zor93q"
            :class="panelactiveparvande == val.link && !openMenu ? 'Mui-selected' : ''" @click="go2(val)">
            <img :src="val.img" style="width:20px" class="ml-2" />{{ val.title }}
            <span v-if="val?.active" class="spinner-grow spinner-grow-sm"></span>
          </p>

        </div>
      </b-nav>
    </div>

    <div v-if="!isMobile()" class="muirtl-5tu3nt" :style="sidebarVisible ? 'right: 95%;' : 'right:5%'"
      @click="setsidebarVisible(!sidebarVisible)">
      <span class="glyph-icon "
        :class="sidebarVisible ? 'glyph-icon simple-icon-arrow-left' : 'simple-icon-arrow-right'"></span>
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
        {
          "title": "خانه",
          "link": "home",
          "img": "assets/img/ltms.svg",
          "type": "3",
          "cate": 2,
        }, {
          "title": "ثبت درخواست",
          "link": "request",
          "img": "assets/img/ltms.svg",
          "type": "3",
          "cate": '',
          roles: ['VOTER'],
        }, {
          "title": "تبلیغات",
          "link": "ViewAdvertise",
          "img": "assets/img/ltms.svg",
          "type": "3",
        }, {
          "title": "انتخابات",
          "link": "votingPage",
          "img": "assets/img/ltms.svg",
          "type": "3",
          requiresActive: true,
        }, {
          "title": "مشاهده نتایج",
          "link": "final-election",
          "img": "assets/img/ltms.svg",
          "type": "3",
          "cate": 2,
          requiresActive: true,
        }, {
          "title": "کارتابل اجرایی",
          "link": "executive-dashboard",
          "img": "assets/img/ltms.svg",
          "type": "3",
          "cate": 2,
          roles: ['EXECUTIVE'],
        }, {
          "title": "کارتابل نظارت",
          "link": "supervisor-dashboard",
          "img": "assets/img/ltms.svg",
          "type": "3",
          "cate": 2,
          roles: ['SUPERVISOR'],
        }, {
          "title": "پشتیبانی",
          "link": "Contact",
          "img": "assets/img/ltms.svg",
          "type": "3",
          "cate": 2,
          roles: ['SUPERVISOR']
        },

      ],
    };
  },
  mounted() {
    if(!this.stateCandidInfo && this.currentUser?.roles=='CANDIDATE')
    this.getstateCandid()
  if(!this.ConfigInfo)
    this.getConfig()

    this.setpanelactiveparvande(this.$route?.name)
    if (this.profilecontent1.filter(x => x.link == this.$route?.name).length > 0)
      this.openMenu = this.profilecontent1.filter(x => x.link == this.$route?.name)[0].cate
    else if (this.profilecontent.filter(x => x.link == this.$route?.name).length > 0) {
      this.openMenu = null
    }
  },
  components: {

  },
  computed: {
    ...mapGetters(["currentUser", "panelactiveparvande", "sidebarVisible","ConfigInfo","stateCandidInfo"]),
    filteredMenu() {
      return this.profilecontent.filter(item => {
        let roleAllowed=true
        
        if(item?.roles)
        roleAllowed = item?.roles?.includes(this.currentUser?.roles[0])
 
        const disabled= item.requiresActive && !this.ConfigInfo?.active
        return roleAllowed && !disabled
      })
    }
  },
  methods: {
    ...mapMutations(["setpanelactiveparvande", "setsidebarVisible", "setProcessing","setRequestStatus"]),
    ...mapActions(["getstateCandid","getConfig"]),
    toggleMenu(title) {
      this.openMenu = this.openMenu === title ? null : title;
    },
    isParentActive(title) {
      return this.profilecontent1?.some(
        x => x.cate === title && x.link === this.$route.path
      );
    },
    go2(val) {
      if (val.type == 2) {
        this.$notify("info", "سرویس", val.popup, {
          duration: 3000,
          permanent: false
        })
      }

      else if (val.type == 1) {
        this.setProcessing(false)
        if (this.isMobile()) this.setsidebarVisible(false)
        if (this.$route?.name != 'home')
          this.$router.push({ name: 'home' });
        setTimeout(() => {
          this.setpanelactiveparvande(val.link)
        }, 500);
      }
      else if (val.type == 3) {
        if (this.isMobile()) this.setsidebarVisible(false)
        this.setpanelactiveparvande(val.link)
        if (this.$route?.name != val.link)
          this.$router.push({ name: val.link });
      }
    },

  },
  watch: {
    stateCandidInfo(val){
      if(val){
        this.setRequestStatus(val)
      }
    }
  },
};
</script>

<style scoped>
.bb37b3d5 {
  border-bottom: 1px solid #37b3d5;
  font-size: small;
}

.bb37b3d5:hover {
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.7) 0%, rgba(31, 122, 77, 0.8) 40%, rgba(255, 255, 255, 0.6) 100%);
}

.custom-sidebar {
  min-height: calc(100vh - 40px);
}

.muirtl-16hg5vz {
  margin: 0px 0px 2.4px;
  font-weight: 700;
  font-size: 0.9rem;
  line-height: 1.3;
}

.muirtl-1rehyf {
  margin: 0px;
  letter-spacing: 0em;
  font-weight: 400;
  line-height: 1.5em;
  font-size: 0.75rem;
}

.muirtl-78ml10 {
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 14px;
  font-weight: 700;
  color: rgba(0, 0, 0, 0.87);
}

.muirtl-12xv2pg {
  position: relative;
  display: flex;
  -webkit-box-align: center;
  align-items: center;
  -webkit-box-pack: center;
  justify-content: center;
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  font-size: 1.25rem;
  line-height: 1;
  border-radius: 50%;
  overflow: hidden;
  user-select: none;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.8) 0%, rgba(31, 122, 77, 0.2) 40%, rgba(255, 255, 255, 0.6) 100%);
  backdrop-filter: blur(12px) saturate(140%);
  border: 1px solid rgba(255, 255, 255, 0.35);
  box-shadow: rgba(31, 122, 77, 0.45) 0px 10px 28px, rgba(255, 255, 255, 0.45) 0px 1px 1px inset;
  transition: 225ms;
  color: rgb(6, 78, 59);
}

.muirtl-5tu3nt {
  display: inline-flex;
  -webkit-box-align: center;
  align-items: center;
  -webkit-box-pack: center;
  justify-content: center;
  box-sizing: border-box;
  -webkit-tap-highlight-color: transparent;
  outline: 0px;
  border: 0px;
  margin: 0px;
  cursor: pointer;
  user-select: none;
  vertical-align: middle;
  appearance: none;
  text-decoration: none;
  text-align: center;
  flex: 0 0 auto;
  border-radius: 50%;
  --IconButton-hoverBg: rgba(34, 197, 94, 0.04);
  padding: 5px;
  font-size: 1.125rem;
  position: absolute;
  top: 50%;
  z-index: 120;
  background-color: rgb(255, 255, 255);
  color: rgb(31, 122, 77);
  box-shadow: rgba(0, 0, 0, 0.2) 0px 3px 3px -2px, rgba(0, 0, 0, 0.14) 0px 3px 4px 0px, rgba(0, 0, 0, 0.12) 0px 1px 8px 0px;
  transform: translateY(-50%) rotate(0deg);
  transition: 400ms;
}

.muirtl-zor93q {
  color: #000;
}

.muirtl-zor93q:hover {
  background-color: rgb(31, 122, 77);
  color: rgb(255, 255, 255);
  border-radius: 10px
}

.Mui-selected {
  background-color: #73c2fb;
  border-radius: 10px;
  color: #fff !important;
}

.Mui-selected span {
  color: #fff !important;
}

.sidebar-nav {
  width: 100%;
}

.submenu {
  background: #f8f9fa;
}

.submenu-item.active {
  background-color: #dbeafe;
  color: #1d4ed8;
  border-radius: 6px;
}

.submenu-item .glyph-icon {
  vertical-align: middle;
}

.nav-link-custom.active {
  background-color: #e3f2fd;
  font-weight: bold;
}

.rotate-icon {
  display: inline-block;
  /* مهم */
  transition: transform 0.3s ease;
  /* انیمیشن نرم */
  align-content: center;
  font-weight: bold;
}

.rotate-icon.rotated {
  transform: rotate(180deg);
}

.collapse-enter-active,
.collapse-leave-active {
  transition: max-height 0.35s ease, opacity 0.25s ease;
}

.collapse-enter,
.collapse-leave-to {
  max-height: 0;
  opacity: 0;
}

.collapse-enter-to,
.collapse-leave {
  max-height: 500px;
  opacity: 1;
}

.muirtl-rmig3n {
  width: 4rem;
  padding: 5px;
  border-radius: 8px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.8) 0%, rgba(31, 122, 77, 0.2) 40%, rgba(255, 255, 255, 0.6) 100%);
  backdrop-filter: blur(12px) saturate(140%);
  border: 1px solid rgba(255, 255, 255, 0.35);
  box-shadow: rgba(31, 122, 77, 0.45) 0px 10px 28px, rgba(255, 255, 255, 0.45) 0px 1px 1px inset;
  transition: 225ms;
  justify-content: center;
}
</style>
