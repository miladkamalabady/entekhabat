<template>
  <div  style="" v-if="onLine" @contextmenu="handler($event)">
    <app-layout>
    <router-view />
  </app-layout>
  </div>
  <div class="h-100 " v-else>
    <div class="form-side ">
      <b-alert show variant="danger" class="rounded text-center alertinternet">
        <h3 style="line-height: 2.2rem;">
          به نظر میرسد اتصال اینترنت شما قطع شده است
        </h3>
      </b-alert>
    </div>
    <router-view />
  </div>
</template>

<script>
import AppLayout from "./navs/AppLayout";
import {
  mapGetters,
  mapActions
} from "vuex";
import { getDirection } from "./utils";

export default {
  components: {
    AppLayout
  },
  data() {
    return {
      dev: false,
      onLine: navigator.onLine,
      status: 1,
      showBackOnline: false
    }
  },
  metaInfo: {
    title: 'سامانه انتخابات الکترونیک معلم وزارت آموزش و پرورش',
    meta: [
      { charset: 'utf-8' },
      { name: 'description', content: 'سامانه انتخابات الکترونیک معلم وزارت آموزش و پرورش' },
      { name: 'og:title', content: 'سامانه انتخابات الکترونیک معلم وزارت آموزش و پرورش' },
      { property: 'og:type', content: 'سامانه انتخابات الکترونیک معلم وزارت آموزش و پرورش' },
      { name: 'twitter:image:alt', content: 'سامانه انتخابات الکترونیک معلم وزارت آموزش و پرورش' },
      { property: 'al:ios:app_name', content: 'سامانه انتخابات الکترونیک معلم وزارت آموزش و پرورش' },
    ]
  },

  created() {
    // window.addEventListener("beforeunload", this.leaving);


  },

  mounted() {
    window.addEventListener('online', this.updateOnlineStatus);
    window.addEventListener('offline', this.updateOnlineStatus);
    window.addEventListener('visibilitychange', this.onblur);
  },
  beforeDestroy() {
    window.removeEventListener('online', this.updateOnlineStatus);
    window.removeEventListener('offline', this.updateOnlineStatus);
    window.removeEventListener('visibilitychange', this.onblur);
  },
  beforeMount() {
    window.addEventListener("beforeunload", this.preventNav)
    this.$once("hook:beforeDestroy", () => {
      window.removeEventListener("beforeunload", this.preventNav);
    })

    const direction = getDirection();
    if (direction.isRtl) {
      document.body.classList.add("rtl");
      document.dir = "rtl";
      document.body.classList.remove("ltr");
    } else {
      document.body.classList.add("ltr");
      document.dir = "ltr";
      document.body.classList.remove("rtl");
    }
  },
  computed: {
    ...mapGetters(["currentUser", "loginError"])
  },
  methods: {
    ...mapActions([]),
    handler: function (e) {
      if (!location.origin.includes('http://localhost'))
        e.preventDefault();
    },
    onblur(e) {
      if (this.currentUser) {
        if (document.hidden && this.status == 1) {
          this.status = 0
        }
        else {
          this.status = 1
        }
      }
    },
    updateOnlineStatus(e) {
      const {
        type
      } = e;
      this.onLine = type === 'online';
    },

  },
  watch: {
    loginError(val) {
      if (val) {
        if (val?.resultCode)
          this.$notify("info", "هشدار", val?.data, {
            duration: 6000,
            permanent: false,
          });
        else {
          this.$notify("error", "خطا", val, {
            duration: 6000,
            permanent: false,
          });
        }
      }
    },
    $route(v) {

    }
  },
};
</script>
<style scoped>
.alertinternet {
  color: #fff;
  background-color: rgba(196, 61, 75, 0.8);
  position: absolute;
  margin-left: auto;
  margin-right: auto;
  width: fit-content;
  left: 0;
  right: 0;
  margin-top: 2%;
  z-index: 999;
}

@import "assets/fonts/simple-line-icons/css/simple-line-icons.css";
@import "assets/css/bootstrap-icons.css";

</style>