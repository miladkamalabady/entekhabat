<template>
  <div id="root">
    <div class="container">
      <b-row style="height: 80vh">
        <b-colxx xxs="12" md="12" class="m-auto">
          <b-card v-if="!this.$route?.query?.code" class="mb-4 text-center">
            <b-alert show variant="danger"> خطای دریافت اطلاعات!<br />
              <a href="https://my.medu.ir">خروج</a>
            </b-alert>
          </b-card>
          <b-card v-else-if="!errorMessage" class="mb-4 text-center">
            در حال بررسی وضعیت...
            <img src="/assets/img/Loading.gif" style="max-width: 100%; width: 50px" />
          </b-card>
          <b-card v-else class="mb-4 text-center">
            <b-alert show variant="danger">
              {{ errorMessage }}<br />
              <a href="https://my.medu.ir">خروج</a>
            </b-alert>
          </b-card>
        </b-colxx>
      </b-row>
    </div>
  </div>
</template>
<script>
import { mapGetters, mapActions, mapMutations } from "vuex";
import { adminRoot } from "../../constants/config";
import { isMobile } from "../../utils";
import vSelect from "vue-select";
import "vue-select/dist/vue-select.css";
export default {
  components: {
    vSelect,
  },
  data() {
    return {
      isMobile,
      adminRoot,
      errorMessage:null,
    };
  },
  computed: {
    ...mapGetters([
      "currentUser",
      "processing",
      "loginError",
      "LoginSSOInfo",
    ]),
  },
  methods: {
    ...mapActions([
      "LoginUserSSO",
      "LoginSSO",
      "signOut",
    ]),
    ...mapMutations(["setpanelactiveparvande"]),
  },
  async mounted() {
    const code = this.$route?.query?.code

  if (!code && !this.currentUser) return

  if (code) {
    await this.LoginUserSSO({ myToken: code })
  }


  },
  watch: {
    currentUser(val) {
    if (!val) return
    this.$router.push({ name: 'home' })
    if (this.$route?.query?.page) {
      this.setpanelactiveparvande(this.$route.query.page)
    }
    },
    loginError(val) {
      if (val) {
        this.errorMessage=val
      }
    },
  },
};
</script>
