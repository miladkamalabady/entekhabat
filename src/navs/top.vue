<template>
  <div class="topdiv d-flex navbar px-2 notinvoice navtopall">
    
    <div @click="gohomepath.includes($route.name) ? gohome() : setsidebarVisible(!sidebarVisible)" class="mr-4">
      <div style="gap: 1rem; display: flex">
        <svg
          class="mt-2"
          width="16"
          height="14"
          viewBox="0 0 16 14"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M1.33398 1.66663H14.6673M1.33398 6.99996H14.6673M1.33398 12.3333H14.6673"
            stroke="#fff"
            stroke-width="1.5"
            stroke-linecap="round"
          ></path>
        </svg>
        
          <div class="text-center muirtl-rmig3n d-flex flex-col mx-auto mb-2  items-center justify-center">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="-1 -0.968 2 1.968">
              <defs>
                <linearGradient id="blackGradient135" x1="0%" y1="0%" x2="100%" y2="100%">
                  <stop offset="0%" stop-color="#6EE7B7"></stop>
                  <stop offset="100%" stop-color="#34D399"></stop>
                </linearGradient>
              </defs>
              <g id="A" fill="url(#blackGradient135)">
                <path
                  d="m 1,6.5298e-4 c 0,0.2832 -0.1568,0.5432 -0.4072,0.6756 0.3408,-0.4264 0.2712,-1.0484 -0.1552,-1.3896 v 0 c -0.0144,-0.0116 -0.0292,-0.0228 -0.0444,-0.0336 0.3536,0.074 0.6068,0.386 0.6068,0.7476 z">
                </path>
                <path
                  d="m 0.6456,-0.03014702 c -4e-4,0.504 -0.4092,0.9124 -0.9132,0.912 -0.0956,0 -0.1904,-0.0152 -0.2812,-0.0444 0.016,8e-4 0.0324,0.0012 0.0484,0.0012 0.5524,0 1,-0.448 1,-1.0004 0,-0.1992 -0.0596,-0.394 -0.1712,-0.5592 0.202,0.1728 0.3176,0.4252 0.3172,0.6908 z">
                </path>
                <path
                  d="m 0.2576,-0.91334702 c 0,0.0768 -0.0624,0.1392 -0.1392,0.1392 -0.048,0 -0.0928,-0.0248 -0.118,-0.0656 l -0.0168,-0.0168 0.0164,-0.082 c 0.03,0.0652 0.1076,0.0936 0.1728,0.0636 0.038,-0.0176 0.0656,-0.0528 0.0732,-0.094 0.0076,0.0176 0.0116,0.0364 0.0116,0.0556 z">
                </path>
                <path
                  d="m 0.118,-0.68774702 c -0.0464,-0.0212 -0.0872,-0.0536 -0.118,-0.094 l -0.05,0.7816 L 0,1.000653 0.0776,0.89385298 l 0.0104,-0.2464 0.0108,-0.2564 0.0012,-0.0328 4e-4,-0.0124 0.0024,-0.0532 0.0068,-0.1616 0.0056,-0.1304 0.0016,-0.0376 0.002,-0.048 z">
                </path>
              </g>
              <use href="#A" transform="scale(-1 1)" fill="url(#blackGradient135)"></use>
            </svg>
          </div>
        <p class="m-0">سامانه انتخابات الکترونیک</p>
      </div>
    </div>
    <div v-if="showname" style="gap: 1rem; display: flex;align-items: center;">
      
    <b-nav-item-dropdown
  right
  no-caret
  toggle-class="p-0"
  class="notif-dropdown"
>
  <!-- آیکن -->
  <template #button-content>
    <div class="position-relative">
      <svg
      :class="{ shake: newNotifCount > 0 }"
        width="25"
        height="25"
        viewBox="0 0 24 24"
      >
        <path
          fill="#feffffff"
          d="M12 22a2 2 0 0 0 2-2h-4a2 2 0 0 0 2 2Zm6-6V11a6 6 0 0 0-5-5.91V4a1 1 0 0 0-2 0v1.09A6 6 0 0 0 6 11v5l-2 2v1h16v-1l-2-2Z"
        />
      </svg>
      <span
        v-if="newNotifCount > 0"
        class="notif-badge"
      >
        {{ newNotifCount }}
      </span>
    </div>
  </template>
  <b-dropdown-item
    v-for="(item, index) in topNotifs"
    :key="index"
    class="text-right small"
    @click="goToNotifs(item)"
  >
    <div class="font-weight-bold">
      {{ item.title }}
    </div>
    <small class="text-muted">{{ item.tarikh }}</small>
  </b-dropdown-item>

  <b-dropdown-divider />

  <b-dropdown-item
    class="text-center text-primary"
    @click="$router.push({ name: 'Notifications' })"
  >
    مشاهده همه اطلاعیه‌ها
  </b-dropdown-item>
</b-nav-item-dropdown>

      <img
        src="/assets/img/logout1.png"
        @click="logout()"
        style="width: 35px; cursor: pointer"
      />

      <span class="small">
        {{ currentUser?.fullName }} 
      </span>
      
    </div>
  </div>
</template>

<script>
import { mapActions, mapMutations, mapGetters } from "vuex";
  import { notifs } from "../data/dataConst";
const gohomepath=[]
export default {
  data() {
    return {
      gohomepath,
      shookOnce: false,
      notifs,
      location: "HomePage",
      showname: true,
    };
  },
  computed: {
    ...mapGetters(["processing", "sidebarVisible", "currentUser"]),
    newNotifCount() {
    return this.notifs?.filter(n => n.isNew).length;
  },
  topNotifs() {
    return this.notifs?.slice(0, 3);
  }
  },

  methods: {
    ...mapActions(["signOut"]),
    ...mapMutations(["setsidebarVisible"]),
    gohome(){
      this.$router.push({ name: "home" });
    },
    goToNotifs(item) {
    this.$router.push({
      name: "Notifications",
      query: { id: item.id }
    });
  },
    logout() {
      this.signOut().then(() => {
        window.location.href = "https://my.medu.ir";
      });
    },
  },
  mounted() {
    if (this.$route?.path?.includes("/sso")) this.showname = false;
  },watch: {
  newNotifCount(val) {
    if (val > 0 && !this.shookOnce) {
      this.shookOnce = true;

      // بعد از یک بار shake کافیه
      setTimeout(() => {
        this.shookOnce = false;
      }, 3000);
    }
  }
}
};
</script>
<style scoped>
.topdiv {
  background-color: #73c2fb;
  box-shadow: rgba(74, 107, 99, 0.69) 0px 4px 20px;
  color: #fff;
  /* position: fixed; */
  width: 100%;
  z-index: 121;
}.notif-badge {
  position: absolute;
  top: -4px;
  right: -6px;
  background: #dc3545;
  color: #fff;
  font-size: 10px;
  padding: 2px 5px;
  border-radius: 10px;
  line-height: 1;
}
.notif-dropdown::marker {
  color:#73c2fb
}
.notif-dropdown svg {
  transition: transform 0.2s ease, fill 0.2s ease;
}

.notif-dropdown:hover svg {
  transform: scale(1.1);
  fill: #0d6efd; /* آبی جذاب‌تر */
}
@keyframes bell-shake {
  0% { transform: rotate(0); }
  15% { transform: rotate(10deg); }
  30% { transform: rotate(-10deg); }
  45% { transform: rotate(8deg); }
  60% { transform: rotate(-8deg); }
  75% { transform: rotate(4deg); }
  100% { transform: rotate(0); }
}

.shake {
  animation: bell-shake 0.8s ease-in-out;
  transform-origin: top center;
}

</style>
