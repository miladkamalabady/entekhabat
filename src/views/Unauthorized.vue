<template>
  <div id="root">
    <main>
      <div class="container">
        <b-row class="h-100">
          <b-colxx xxs="10" md="4" class="m-auto">
            <b-card class="auth-card" no-body>
              <div class="form-side">
                <h6 class="mb-4">{{ $t("pages.error-title503") }}</h6>
                <p class="mb-0 text-muted text-small mb-0">{{ $t("pages.error-code") }}</p>
                <p class="display-4 font-weight-bold mb-5">503</p>
                <b-button type="submit" variant="primary" class="btn-shadow" @click="goBack">{{
                  $t("pages.go-back-home")
                }}</b-button>
                <b-button type="submit" variant="danger" class="btn-shadow" @click="goBack1">خروج</b-button>
              </div>
            </b-card>
          </b-colxx>
        </b-row>
      </div>
    </main>
  </div>
</template>
<script>
import { adminRoot } from '../constants/config';
import {
  mapGetters,
  mapMutations,
  mapActions
} from "vuex";
import { UserRole } from "../utils/auth.roles";
export default {
  methods: {
  ...mapMutations(["setRequestStatus"]),
    goBack() {
      // this.setRequestStatus(null)
        this.$router.push({name:'home'});
    },
    goBack1() {
      localStorage.clear();
       this.$router.push({name:'home'});
    },
  },
  data() {
    return {
      UserRole,
      adminRoot
    };
  },
  computed: {
    ...mapGetters(["currentUser"])
  },
  mounted: function () {
    document.body.classList.add("background");
  },
  beforeDestroy() {
    document.body.classList.remove("background");
  },
};
</script>
