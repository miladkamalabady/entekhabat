<template>
  <footer class="page-footer">
    <div class="footer-content">
      <div class="container-fluid">
        <div class="row">
          <div class="foot1" :style="location == 'HomePage' ? 'width:20%;' : ''"></div>
          <div class="foot text-center copyr text-muted">
            <div style="font-size: 0.7rem">
              {{ $t("general.copyright") }}<br />طــراحی، تولیـد و توسعه توسط ایــزایران
            </div>
            <!-- <div>
                              <img style="width: 40px;" src='assets/img/landing-page/logo_isiran.svg' />
                          </div> -->
          </div>
        </div>
      </div>
    </div>

    <!-- <AccessibilityToolbar
      accessibileFontSizeText="افزایش اندازه متن"
      accessibileFontSizeText1="کاهش اندازه متن"
      grayscaleText="سیاه و سفید کردن متن"
      invertColorsText="معکوس کردن رنگ نوشته"
      highlightLinksText="روشن کردن متن"
    ></AccessibilityToolbar> -->
  </footer>
</template>

<script>
import { adminRoot } from "../../constants/config";
// import AccessibilityToolbar from "vue-accessibility-toolbar";
import { mapActions } from "vuex";
export default {
  // components: {
  //   AccessibilityToolbar,
  // },
  data() {
    return {
      adminRoot,
      location: "HomePage",
    };
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
</style>
