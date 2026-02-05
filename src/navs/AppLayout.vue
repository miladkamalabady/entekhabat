<template>
  <div id="app-container">
    <Topnav />
    <div style="" class="d-flex">
   <div class=""
        :style="sidebarVisible ? !isMobile() ? 'width:250px;' : 'width:100%;' : 'width:0;'">
        <b-card no-body class="p-0 " style="">
          <Transition name="fade" appear>
            <Sidebar />
          </Transition>
        </b-card>
      </div>

     <div v-if="(!sidebarVisible && isMobile()) || (!isMobile())" style="padding:0 15px;flex:1;"
        :style="isMobile() ? 'max-width: calc(100%);' : ''">
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
<style scoped>
@media (min-width: 780px) {
main.mr120 {
  margin-right: 100px !important;
}
}
.main-hidden main.mr120 {
  margin-right: 20px !important;
}
</style>
