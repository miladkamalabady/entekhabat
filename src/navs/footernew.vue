<template>
  <footer class="mt-2" v-if="false">
    <b-row class="text-center m-0 mr-4" style="background-color: #fff">
      <b-colxx xxs="12" class="text-center">
        <div class="foot1" :style="location == 'HomePage' ? 'width:20%;' : ''"></div>
      </b-colxx>
      <b-colxx xxs="12" class="text-center" style="padding-top: 10px">
        <div style="font-size: 0.7rem">
          {{ $t("general.copyright") }}<br />{{ $t("dashboards.teacher.isIranFooter") }}
        </div>
      </b-colxx>
      <b-colxx :style="currentUser?.result?.student?.gradeTypeId > 9
          ? 'background-color: #21928A;'
          : currentUser?.result?.student?.gradeTypeId > 6
            ? 'background-color: #0061f2;'
            : currentUser?.result?.student?.gradeTypeId > 0
              ? 'background-color: #18d4ff;'
              : 'background-color: #115ca7;'" style="border-radius: 60px 0 0 0" xxs="5" class="text-center py-4">
      </b-colxx>
      <b-colxx xxs="2" class="text-center p-0 d-flex" style="align-items: center; justify-content: space-around">
        <!-- <b-img src="assets/img/pages/social.png" /> -->

        <a href="https://shad.ir/Fanavari_setad" target="_blank">
          <b-img src="assets/img/shareimg/shad.png" style="width: 30px" />
        </a>
      </b-colxx>
      <b-colxx :style="currentUser?.result?.student?.gradeTypeId > 9
          ? 'background-color: #21928A;'
          : currentUser?.result?.student?.gradeTypeId > 6
            ? 'background-color: #0061f2;'
            : currentUser?.result?.student?.gradeTypeId > 0
              ? 'background-color: #18d4ff;'
              : 'background-color: #115ca7;'" style=" border-radius: 0 60px 0 0" xxs="5" class="text-center">
      </b-colxx>
    </b-row>
  </footer>
</template>

<script>
import { adminRoot } from "../constants/config";
import { mapActions, mapGetters } from "vuex";
// import AccessibilityToolbar from "vue-accessibility-toolbar";
export default {
  // components: { AccessibilityToolbar },
  data() {
    return {
      adminRoot,
      location: "HomePage",
    };
  },
  computed: {
    ...mapGetters(["currentUser"]),
  },
  methods: {
    ...mapActions(["signOut"]),

    logout() {
      this.signOut().then(() => {
        // this.$router.push("/user/login");
        window.location.href = "/";
      });
    },
  },
  mounted() {

  },
};
</script>
<style>
@media (max-width: 1440px) {
  .foot1 {
    width: 3%;
  }

  .foot {
    width: 93%;
  }
}

@media (min-width: 1440px) {
  .foot1 {
    width: 20%;
  }

  .foot {
    width: 70%;
  }
}

.copyr {
  color: rgba(39, 50, 122, 1);
  font-size: 1rem;
  display: flex;
  flex-wrap: nowrap;
  justify-content: center;
  align-items: center;
}

@media (max-width: 500px) {
  .copyr {
    font-size: 0.6rem;
  }
}

.copyr img {
  width: 40px;
  /* filter: invert(95%) sepia(17%) saturate(0%) hue-rotate(272deg) brightness(104%) contrast(104%) */
}

body {
  padding-bottom: 0;
}
</style>
