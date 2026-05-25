<template>
  <div id="app-container">
    <Topnav />
    <div style="" class="d-flex">
   <div class=""  v-if="currentUser"
        :style="sidebarVisible ? !isMobile() ? 'width:250px;' : 'width:100%;' : 'width:0;'">
        <b-card no-body class="p-0 ">
          <Transition name="fade" appear>
            <Sidebar />
          </Transition>
        </b-card>
      </div>

     <div v-if="(!sidebarVisible && isMobile()) || (!isMobile())" style="padding:0 15px;flex:1;"
        :style="isMobile() ? 'max-width: calc(100%);' : ''" class="request-page-container">
        <slot></slot>
      </div>
</div>
  </div>
</template>

<script>

import Sidebar from "./Sidebar";
import Topnav from "./top";

import { isMobile } from "../utils";
import { mapGetters } from "vuex";

export default {
  components: {
     Sidebar,
    Topnav,
  },
  data() {
    return {
      isMobile,
    };
  },
  computed: {
    ...mapGetters([ "currentUser","sidebarVisible"]),
  },
  mounted() {
    setTimeout(() => {
      document.body.classList.add("default-transition");
    }, 100);
  },
};
</script>
<style >
@media (min-width: 780px) {
main.mr120 {
  margin-right: 100px !important;
}
}
.main-hidden main.mr120 {
  margin-right: 20px !important;
}
/* ========== اصلاح فاصله از topbar ========== */
.request-page-container {
  margin-top: 70px;  /* فاصله از topbar ثابت */
}
@media (max-width: 576px) {
  .request-page-container {
    margin-top: 60px;
  }
  
}
</style>
