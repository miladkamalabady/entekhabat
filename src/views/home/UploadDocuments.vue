<template>
  <div class="px-2">

        <div class="mt-4">
          <b-container fluid class="upload-wrapper">
            <!-- Wizard -->
            <ul class="wizard mb-3">
              <li class="done">1. بررسی شرایط احراز</li>
              <li class="done">2. قبول شرایط</li>
              <li class="active">3. بارگذاری مدارک</li>
              <li>4. تأیید ثبت‌نام</li>
            </ul>
            <b-progress
              height="6px"
              class="mb-4"
              :value="progress"
              max="100"
              variant="primary"
            />

            <!-- Upload Documents -->
            <b-card>
              <h6 class="mb-3">بارگذاری مدارک موردنیاز</h6>

              <document-upload
                title="عکس پرسنلی"
                :file.sync="files.photo"
              />
              <document-upload
                title="مدرک تحصیلی"
                :file.sync="files.degree"
              />
              <document-upload
                title="گواهی عدم اعتیاد"
                :file.sync="files.noAddiction"
              />

              <!-- Actions -->
              <div class="d-flex justify-content-between mt-4">
                <b-button variant="outline-secondary" @click="goBack">بازگشت</b-button>
                <div>
                  <b-button variant="outline-danger" class="mr-2" @click="cancel">انصراف</b-button>
                  <b-button variant="primary" :disabled="!canSubmit" @click="submit">ثبت نهایی</b-button>
                </div>
              </div>
            </b-card>
          </b-container>
        </div>
      </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapMutations } from "vuex";
import DocumentUpload from "../../components/Common/DocumentUpload";

export default {
  name: "UploadDocuments",
  components: {  DocumentUpload },
  computed: {
    ...mapGetters(["sidebarVisible"]),
    canSubmit() {
      return Object.values(this.files).every(f => f !== null);
    },
    progress() {
      return this.canSubmit ? 90 : 80;
    }
  },
  data() {
    return {
      isMobile,
      files: {
        photo: null,
        degree: null,
        noAddiction: null
      }
    };
  },
  methods: {
    ...mapMutations(["setsidebarVisible", "setCandidateFiles","setRequestStatus"]),

    goBack() {
      this.setRequestStatus("CANDIDATE")
      this.$router.push("/candidate/AcceptConditions");
    },

    cancel() {
      if (confirm("آیا از ادامه فرآیند ثبت‌نام انصراف می‌دهید؟")) {
        this.$router.push("/home");
      }
    },

    submit() {
       Object.keys(this.files).forEach(key => {
    const f = this.files[key];
    if (f && !f.preview && f.raw && f.raw.type.startsWith("image/")) {
      const reader = new FileReader();
      reader.onload = e => {
        f.preview = e.target.result;
        this.$set(this.files, key, f);
      };
      reader.readAsDataURL(f.raw);
    }
  });
  
      this.setCandidateFiles(this.files);
      this.setRequestStatus("DOCUMENTS_UPLOADED")
      this.$router.push("/candidate/Confirmation");
    }
  },
  created() {
    // بارگذاری فایل‌ها از Vuex در صورت برگشت
    if (this.$store.state.candidateFiles) {
      this.files = { ...this.$store.state.candidateFiles };
    }
  }
};
</script>

<style scoped>
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

.thumbnail {
  max-width: 80px;
  max-height: 80px;
  border-radius: 4px;
  border: 1px solid #ccc;
  object-fit: cover;
}
</style>
