<template>
  <div class="px-2">

    <b-container fluid class="conditions-wrapper">

      <!-- Wizard -->
      <ul class="wizard mb-3">
        <li class="done">1. بررسی شرایط احراز</li>
        <li class="active">2. قبول شرایط</li>
        <li>3. بارگذاری مدارک</li>
        <li>4. تأیید ثبت‌نام</li>
      </ul>

      <!-- Progress -->
      <b-progress height="6px" class="mb-4" :value="progress" max="100" variant="primary" />

      <b-card>

        <h6 class="mb-3">شرایط و تعهدات داوطلب</h6>

        <!-- Conditions -->
        <b-form-checkbox-group v-model="checkedItems" stacked>
          <b-form-checkbox v-for="item in conditions" :key="item.id" :value="item.id" class="mb-2">
            {{ item.text }}
          </b-form-checkbox>
        </b-form-checkbox-group>

        <hr />

        <!-- Final Confirmation -->
        <b-form-checkbox v-model="finalConfirm" :disabled="!allChecked" class="mt-3 final-confirm">
          <strong>
            اینجانب صحت تمامی موارد فوق را تأیید نموده و مسئولیت هرگونه مغایرت را می‌پذیرم.
          </strong>
        </b-form-checkbox>

        <!-- Action -->
        <!-- Actions -->
        <div class="d-flex justify-content-between mt-4">
          <b-button variant="outline-secondary" @click="goBack">
            بازگشت
          </b-button>
          <div>
            <b-button variant="outline-danger" class="mr-2" @click="cancel">
              انصراف
            </b-button>
            <b-button variant="primary" :disabled="!canContinue" @click="nextStep">
              ادامه
            </b-button>
          </div>
        </div>

      </b-card>
    </b-container>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: 'AcceptConditions',
  components: {
  },
  async mounted() {
  },
  computed: {
    ...mapGetters(["sidebarVisible", "processing", "loginError", "currentUser"]),
    allChecked() {
      return this.checkedItems.length === this.conditions.length
    },

    canContinue() {
      return this.allChecked && this.finalConfirm
    },

    progress() {
      if (!this.allChecked) return 50
      if (this.finalConfirm) return 75
      return 60
    }
  },
  data() {
    return {
      isMobile,
      checkedItems: [],
      finalConfirm: false,

      conditions: [
        {
          id: 1,
          text: 'التزام به قانون اساسی جمهوری اسلامی ایران، امانت‌داری و حسن شهرت'
        },
        {
          id: 2,
          text: 'برخورداری از سلامت کامل جهت انجام وظایف نمایندگی'
        },
        {
          id: 3,
          text: 'نداشتن اعتیاد به مواد مخدر یا روان‌گردان'
        },
        {
          id: 4,
          text: 'نداشتن محکومیت قطعی در جرایم مالی و عدم محرومیت از حقوق اجتماعی'
        },
        {
          id: 5,
          text: 'نداشتن حکم محرومیت و ورشکستگی'
        },
        {
          id: 6,
          text: 'تعهد به تکمیل فرم شفافیت و عدم تعارض منافع در صورت انتخاب شدن'
        },
        {
          id: 7,
          text: 'پذیرش مسئولیت حقوقی و اداری ناشی از عدم رعایت شرایط فوق'
        }
      ]
    }
  },
  methods: {
    ...mapMutations(["setsidebarVisible", "setRequestStatus"]),
    ...mapActions([]),
    goBack() {
      this.setRequestStatus("DRAFT")
      this.$router.push({ name: 'request' });
    },
    cancel() {
      if (confirm("آیا از ادامه فرآیند ثبت‌نام انصراف می‌دهید؟")) {
        this.$router.push("/home");
      }
    },
    nextStep() {
      this.setRequestStatus("CONDITIONS_ACCEPTED")
      this.$router.push('/candidate/UploadDocuments')
    }
  }
}
</script>

<style scoped>
.conditions-wrapper {
  background: #f5f7fb;
  min-height: 100vh;
  padding: 20px;
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

/* Final confirm emphasis */
.final-confirm {
  background: #f9fafc;
  padding: 12px;
  margin-right: 10px;
  border-radius: 6px;
}
</style>
