<template>
  <div class="px-2">

    <b-container fluid class="upload-wrapper">
      <!-- Wizard -->
      <ul class="wizard mb-3">
        <li class="done">1. بررسی شرایط احراز</li>
        <li class="done">2. قبول شرایط</li>
        <li class="done">3. بارگذاری مدارک</li>
        <li class="active">4. تأیید ثبت‌نام</li>
      </ul>
      <b-progress height="6px" class="mb-4" :value="progress" max="100" variant="primary" />

      <!-- Confirmation Card -->
      <b-card>
        <h6 class="mb-3">تأیید نهایی ثبت‌نام</h6>
        <p>تمام مدارک شما با موفقیت بارگذاری شده است. لطفاً قبل از ثبت نهایی، اطلاعات خود را بررسی کنید.</p>

        <div class="final-confirm mb-3">
          <!-- عکس پرسنلی -->
          <b-row class="align-items-center mb-2">
            <b-col cols="4"><strong>عکس پرسنلی:</strong></b-col>
            <b-col>
              <img v-if="files.photo?.preview" :src="files.photo.preview" class="thumbnail" />
              <span v-else-if="files.photo">{{ files.photo.name }}</span>
              <span v-else>بارگذاری نشده</span>
            </b-col>
          </b-row>

          <!-- مدرک تحصیلی -->
          <b-row class="align-items-center mb-2">
            <b-col cols="4"><strong>مدرک تحصیلی:</strong></b-col>
            <b-col>
              <img v-if="files.degree?.preview" :src="files.degree.preview" class="thumbnail" />
              <span v-else-if="files.degree">{{ files.degree.name }}</span>
              <span v-else>بارگذاری نشده</span>
            </b-col>
          </b-row>

          <!-- گواهی عدم اعتیاد -->
          <b-row class="align-items-center mb-2">
            <b-col cols="4"><strong>گواهی عدم اعتیاد:</strong></b-col>
            <b-col>
              <img v-if="files.noAddiction?.preview" :src="files.noAddiction.preview" class="thumbnail" />
              <span v-else-if="files.noAddiction">{{ files.noAddiction.name }}</span>
              <span v-else>بارگذاری نشده</span>
            </b-col>
          </b-row>
        </div>

        <!-- Actions -->
        <div class="d-flex justify-content-between mt-4">
          <b-button variant="outline-secondary" @click="goBack">بازگشت</b-button>
          <div>
            <b-button variant="outline-danger" class="mr-2" @click="cancel">انصراف</b-button>
            <b-button variant="success" :disabled="!canConfirm" @click="confirm">ثبت نهایی</b-button>
          </div>
        </div>
      </b-card>
    </b-container>

    <!-- Modal ثبت نهایی -->
    <b-modal id="success-modal" v-model="showSuccess" hide-footer centered title="ثبت نام تکمیل شد">
      <div class="text-center">
        <b-icon icon="check-circle" variant="success" font-scale="4"></b-icon>
        <h5 class="mt-3">ثبت نام شما با موفقیت انجام شد!</h5>
        <p class="mt-2">کد رهگیری شما:</p>
        <h4 class="font-weight-bold">{{ trackingCode }}</h4>
        <b-button variant="primary" class="mt-3" @click="closeModal">تایید</b-button>
      </div>
    </b-modal>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapMutations, mapActions } from "vuex";
import { v4 as uuidv4 } from "uuid"; // برای تولید کد رهگیری تصادفی

export default {
  name: "Confirmation",
  components: {},
  data() {
    return {
      isMobile,
      files: {
        photo: null,
        degree: null,
        noAddiction: null
      },
      showSuccess: false,
      trackingCode: ""
    };
  },
  computed: {
    ...mapGetters(["confirmRegisterInfo", "candidateFiles", "currentUser"]),
    canConfirm() {
      return this.files.photo && this.files.degree && this.files.noAddiction;
    },
    progress() {
      return 100;
    }
  },
  created() {
    if (this.candidateFiles) {
      this.files = { ...this.candidateFiles };

      // ایجاد preview اگر وجود نداشته باشد
      Object.keys(this.files).forEach(key => {
        const f = this.files[key];
        if (f && !f.preview && f.raw && f.raw.type.startsWith("image/")) {
          const reader = new FileReader();
          reader.onload = e => {
            this.$set(this.files[key], "preview", e.target.result);
          };
          reader.readAsDataURL(f.raw);
        }
      });
    }
  },
  watch: {
    confirmRegisterInfo(val) {
      if (val) {
        this.showSuccess = true;
        this.setRequestStatus("SUBMITTED")

        const cu = {
          ...this.currentUser,
          roles: ['CANDIDATE']
        }
        this.setUser(cu)
      }
    }
  },
  methods: {
    ...mapMutations(["setRequestStatus", "setUser"]),
    ...mapActions(["confirmRegister"]),
    goBack() {
      this.setRequestStatus("CONDITIONS_ACCEPTED")
      this.$router.push("/candidate/UploadDocuments");
    },
    cancel() {
      if (confirm("آیا از ادامه فرآیند ثبت‌نام انصراف می‌دهید؟")) {
        this.$router.push("/home");
      }
    },
    confirm() {
      // تولید کد رهگیری تصادفی (می‌توانید از API واقعی هم استفاده کنید)
      this.trackingCode = uuidv4().split("-")[0].toUpperCase();
      this.confirmRegister({ tracking_code: this.trackingCode })

    },
    closeModal() {
      this.showSuccess = false;
      this.$router.push("/home");
    }
  }
};
</script>

<style scoped>
.final-confirm {
  background: #f9fafc;
  padding: 12px;
  margin-bottom: 10px;
  border-radius: 6px;
  border: 1px solid #ddd;
}

/* Wizard */
.wizard {
  display: flex;
  list-style: none;
  padding: 0;
  font-size: 13px;
}

.wizard li {
  flex: 1;
  text-align: center;
  border-bottom: 3px solid #ccc;
  padding: 8px;
  color: #999;
}

.wizard li.active {
  border-color: #3f51b5;
  color: #3f51b5;
  font-weight: 600;
}

.wizard li.done {
  border-color: #4caf50;
  color: #4caf50;
}

/* Thumbnail */
.thumbnail {
  max-width: 80px;
  max-height: 80px;
  border-radius: 4px;
  border: 1px solid #ccc;
  object-fit: cover;
}
</style>
