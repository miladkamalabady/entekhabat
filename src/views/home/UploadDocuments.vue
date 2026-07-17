<template>
  <div class="px-2">

    <div class="mt-4">
      <b-container fluid class="upload-wrapper">
        <!-- Wizard -->
        <ul class="wizard mb-3">
          <li class="done">1. اعلام داوطلبی و پذیرش شرایط</li>
          <li class="active">2. بارگذاری مدارک</li>
          <li>3. تأیید ثبت‌نام</li>
        </ul>
        <b-progress height="6px" class="mb-4" :value="progress" max="100" variant="primary" />

        <!-- راهنمای ویدیویی -->
        <b-alert show variant="info" class="guide-banner d-flex align-items-center justify-content-between flex-wrap">
          <div class="d-flex align-items-center">
            <b-icon icon="play-circle-fill" class="ml-2" font-scale="1.3" />
            <span>برای نحوه صحیح بارگذاری هر مدرک می‌توانید ویدیوی آموزشی مربوط به همان بخش را مشاهده کنید.</span>
          </div>
          <b-button size="sm" variant="info" @click="openGuide()">
            مشاهده راهنمای ویدیویی
          </b-button>
        </b-alert>

        <b-alert v-if="isEditMode" show variant="warning" class="d-flex align-items-center">
          <b-icon icon="pencil-square" class="ml-2" font-scale="1.2" />
          <span>
            شما در حال ویرایش مدارک ثبت‌نام قبلی خود هستید. مدارک قبلی در زیر هر بخش نمایش داده شده‌اند؛ در صورت نیاز
            می‌توانید فایل جدید جایگزین کنید، در غیر این‌صورت فایل قبلی همچنان معتبر باقی می‌ماند.
          </span>
        </b-alert>

        <b-alert v-if="isEditMode && loadingExistingDocuments" show variant="secondary" class="text-center">
          در حال بارگذاری مدارک قبلی...
        </b-alert>

        <!-- Upload Documents -->
        <b-card>
          <h6 class="mb-3">بارگذاری مدارک موردنیاز</h6>
          <b-row>
            <b-colxx xxs="12" xs="4">
              <document-upload title="عکس پرسنلی" :file.sync="files.photo" />
              <div v-if="isEditMode && existingDocuments.photo_url && !files.photo" class="small mt-1">
                <a :href="`${apiUrlrtb}/${existingDocuments.photo_url}`" target="_blank" rel="noopener">
                  <b-icon icon="file-earmark-check" /> مشاهده فایل فعلی
                </a>
              </div>
              <div v-if="fileErrors.photo" class="small text-danger mt-1">{{ fileErrors.photo }}</div>
              <div v-else-if="fileWarnings.photo" class="small text-warning mt-1">
                <b-icon icon="exclamation-triangle-fill" /> {{ fileWarnings.photo }}
              </div>
              <a href="#" class="small guide-link" @click.prevent="openGuide('photo')">
                <b-icon icon="play-circle" /> مشاهده ویدیوی راهنما
              </a>
            </b-colxx>
            <b-colxx xxs="12" xs="4" >
              <document-upload title="مدرک تحصیلی" :file.sync="files.degree" />
              <div v-if="isEditMode && existingDocuments.degree_url && !files.degree" class="small mt-1">
                <a :href="`${apiUrlrtb}/${existingDocuments.degree_url}`" target="_blank" rel="noopener">
                  <b-icon icon="file-earmark-check" /> مشاهده فایل فعلی
                </a>
              </div>
              <div v-if="fileErrors.degree" class="small text-danger mt-1">{{ fileErrors.degree }}</div>
              <div v-else-if="fileWarnings.degree" class="small text-warning mt-1">
                <b-icon icon="exclamation-triangle-fill" /> {{ fileWarnings.degree }}
              </div>
              <a href="#" class="small guide-link" @click.prevent="openGuide('degree')">
                <b-icon icon="play-circle" /> مشاهده ویدیوی راهنما
              </a>
            </b-colxx>
            <!-- <b-colxx xxs="12" xs="4">
              <document-upload title="گواهی عدم اعتیاد" :file.sync="files.noAddiction" />
            </b-colxx> -->
            <b-colxx xxs="12" xs="4">
              <document-upload title="گواهی عدم سوپیشینه" :file.sync="files.soPishine" />
              <div v-if="isEditMode && existingDocuments.soPishine_url && !files.soPishine" class="small mt-1">
                <a :href="`${apiUrlrtb}/${existingDocuments.soPishine_url}`" target="_blank" rel="noopener">
                  <b-icon icon="file-earmark-check" /> مشاهده فایل فعلی
                </a>
              </div>
              <div v-if="fileErrors.soPishine" class="small text-danger mt-1">{{ fileErrors.soPishine }}</div>
              <div v-else-if="fileWarnings.soPishine" class="small text-warning mt-1">
                <b-icon icon="exclamation-triangle-fill" /> {{ fileWarnings.soPishine }}
              </div>
              <a href="#" class="small guide-link" @click.prevent="openGuide('soPishine')">
                <b-icon icon="play-circle" /> مشاهده ویدیوی راهنما
              </a>
            </b-colxx>
            <b-colxx xxs="12" xs="4">
              <document-upload title="گواهی برخورداری از سلامت جسمی و روانی کامل " :file.sync="files.ravan" />
              <div v-if="isEditMode && existingDocuments.ravan_url && !files.ravan" class="small mt-1">
                <a :href="`${apiUrlrtb}/${existingDocuments.ravan_url}`" target="_blank" rel="noopener">
                  <b-icon icon="file-earmark-check" /> مشاهده فایل فعلی
                </a>
              </div>
              <label class="small text-justify">برخورداری از سلامت جسمی و روانی کامل (نداشتن اعتیاد به مواد مخدر یا روان
                گردان و هرگونه سابقه بیماری یا نقص عضوی که مانع از انجام وظایف نمایندگی اعضا در هیئت امنا باشد.)
              </label>
              <div v-if="fileErrors.ravan" class="small text-danger mt-1">{{ fileErrors.ravan }}</div>
              <div v-else-if="fileWarnings.ravan" class="small text-warning mt-1">
                <b-icon icon="exclamation-triangle-fill" /> {{ fileWarnings.ravan }}
              </div>
              <a href="#" class="small guide-link" @click.prevent="openGuide('ravan')">
                <b-icon icon="play-circle" /> مشاهده ویدیوی راهنما
              </a>
            </b-colxx>
            <b-colxx xxs="12" xs="4">
              <document-upload title="فرم تعهد ویژه التزام به شفافیت و عدم تعارض منافع" :file.sync="files.transparencyForm" />
              <div v-if="isEditMode && existingDocuments.transparencyForm_url && !files.transparencyForm" class="small mt-1">
                <a :href="`${apiUrlrtb}/${existingDocuments.transparencyForm_url}`" target="_blank" rel="noopener">
                  <b-icon icon="file-earmark-check" /> مشاهده فایل فعلی
                </a>
              </div>
              <a
                href="/assets/forms/transparency-conflict-of-interest-form.docx"
                download="فرم-تعهد-شفافیت-و-عدم-تعارض-منافع.docx"
                class="small d-inline-block mt-1"
              >
                دریافت فرم خام (Word)
              </a>
              <label class="small text-justify d-block">
                فرم را دریافت، تکمیل و امضا نموده، سپس اسکن یا تصویر آن را بارگذاری نمایید.
              </label>
              <div v-if="fileErrors.transparencyForm" class="small text-danger mt-1">{{ fileErrors.transparencyForm }}</div>
              <div v-else-if="fileWarnings.transparencyForm" class="small text-warning mt-1">
                <b-icon icon="exclamation-triangle-fill" /> {{ fileWarnings.transparencyForm }}
              </div>
              <a href="#" class="small guide-link" @click.prevent="openGuide('transparencyForm')">
                <b-icon icon="play-circle" /> مشاهده ویدیوی راهنما
              </a>
            </b-colxx>
          </b-row>

          <b-alert :show="hasValidationErrors" variant="danger" class="mt-3">
            برخی فایل‌های ارسالی نامعتبر یا ناخوانا هستند. لطفاً موارد مشخص‌شده را با فایل صحیح جایگزین کنید.
          </b-alert>

          <b-alert :show="hasValidationWarnings" variant="warning" class="mt-3">
            برخی فایل‌های ارسالی با نوع مدرک موردانتظار همخوانی ندارند (مثلاً احتمال جابه‌جایی عکس پرسنلی با یک سند). لطفاً موارد مشخص‌شده را بازبینی کنید؛ در صورت درست بودن فایل‌ها می‌توانید ادامه دهید.
          </b-alert>
          <!-- Actions -->
          <div class="d-flex justify-content-between mt-4">
            <b-button variant="outline-secondary" @click="goBack">بازگشت</b-button>
            <div>
              <b-button variant="outline-danger" class="mr-2" @click="cancel">انصراف</b-button>
              <b-button variant="primary" :disabled="!canSubmit || submitting" @click="submit">
                {{ submitting ? "در حال ارسال..." : (isEditMode ? "ثبت نهایی ویرایش" : "ثبت و ادامه") }}
              </b-button>
            </div>
          </div>
        </b-card>

        <!-- Modal راهنمای ویدیویی -->
        <b-modal
          id="upload-guide-modal"
          v-model="showGuideModal"
          hide-footer
          centered
          size="lg"
          title="راهنمای ویدیویی بارگذاری مدارک"
        >
          <b-tabs v-model="activeGuideTab" content-class="mt-3" pills>
            <b-tab v-for="video in guideVideos" :key="video.key" :title="video.title">
              <div class="video-wrapper">
                <video
                  v-if="video.url"
                  :src="video.url"
                  controls
                  class="w-100"
                  style="max-height: 360px;"
                ></video>
                <div v-else class="text-center text-muted py-5">
                  ویدیوی این بخش هنوز بارگذاری نشده است.
                </div>
              </div>
              <p class="small text-muted mt-2 mb-0">{{ video.description }}</p>
            </b-tab>
          </b-tabs>
        </b-modal>
      </b-container>
    </div>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapMutations, mapActions } from "vuex";
import DocumentUpload from "../../components/Common/DocumentUpload";
import { apiUrlrtb } from '../../constants/config'
export default {
  name: "UploadDocuments",
  components: { DocumentUpload },
  computed: {
    ...mapGetters(["UploadUserDocumentsInfo", "currentUser"]),
    canSubmit() {
      const requiredFields = Object.keys(this.files).filter(key => {
        
        return true;
      });

      const allFilesPresent = requiredFields.every(key => {
        const f = this.files[key];
        if (f && f.raw) return true;
        if (this.isEditMode && this.existingDocuments[`${key}_url`]) return true;
        return false;
      });

      return allFilesPresent && !this.hasValidationErrors;
    },
    hasValidationErrors() {
      return Object.values(this.fileErrors).some(msg => !!msg);
    },
    hasValidationWarnings() {
      return Object.values(this.fileWarnings).some(msg => !!msg);
    },
    progress() {
      return this.canSubmit ? 90 : 80;
    }
  },
  data() {
    return {
      apiUrlrtb,
      isMobile,
      isEditMode: false,
      loadingExistingDocuments: false,
      existingDocuments: {},
      submitting: false,
      files: {
        photo: null,
        degree: null,
        // noAddiction: null,
        soPishine: null,
        ravan: null,
        transparencyForm: null
      },
      fileErrors: {
        photo: '',
        degree: '',
        soPishine: '',
        ravan: '',
        transparencyForm: ''
      },
      fileWarnings: {
        photo: '',
        degree: '',
        soPishine: '',
        ravan: '',
        transparencyForm: ''
      },
      fileValidationConfig: {
        photo: { label: 'عکس پرسنلی', allowed: ['image/jpeg', 'image/png', 'image/jpg'], maxSize: 1 * 1024 * 1024, category: 'photo' },
        degree: { label: 'مدرک تحصیلی', allowed: ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'], maxSize: 1 * 1024 * 1024, category: 'document' },
        soPishine: { label: 'گواهی عدم سوءپیشینه', allowed: ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'], maxSize: 1 * 1024 * 1024, category: 'document' },
        ravan: { label: 'گواهی سلامت جسمی و روانی', allowed: ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'], maxSize: 1 * 1024 * 1024, category: 'document' },
        transparencyForm: { label: 'فرم تعهد شفافیت', allowed: ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'], maxSize: 1 * 1024 * 1024, category: 'document' }
      },
      showGuideModal: false,
      activeGuideTab: 0,
      guideVideos: [
        {
          key: 'photo',
          title: 'عکس پرسنلی',
          url: '/assets/videos/guide-user-photo.mp4',
          description: 'راهنمای نحوه تهیه و بارگذاری عکس پرسنلی مناسب.'
        },
        {
          key: 'degree',
          title: 'مدرک تحصیلی',
          url: '/assets/videos/guide-education-doc.mp4',
          description: 'راهنمای اسکن و بارگذاری مدرک تحصیلی.'
        },
        {
          key: 'soPishine',
          title: 'گواهی عدم سوءپیشینه',
          url: '/assets/videos/guide-sopishine.mp4',
          description: 'راهنمای دریافت و بارگذاری گواهی عدم سوءپیشینه.'
        },
        {
          key: 'ravan',
          title: 'گواهی سلامت جسمی و روانی',
          url: '/assets/videos/guide-ravan.mp4',
          description: 'راهنمای دریافت و بارگذاری گواهی سلامت جسمی و روانی.'
        },
        {
          key: 'transparencyForm',
          title: 'فرم تعهد شفافیت',
          url: '/assets/videos/guide-transparency-form.mp4',
          description: 'راهنمای دریافت، تکمیل، امضا و بارگذاری فرم تعهد شفافیت و عدم تعارض منافع.'
        }
      ]
    };
  },
  methods: {
    ...mapMutations(["setCandidateFiles", "setRequestStatus"]),
    ...mapActions(["UploadUserDocuments", "getCandidateDocuments", "UpdateUserDocuments"]),

    async loadExistingDocuments() {
      this.loadingExistingDocuments = true;
      try {
        const info = await this.getCandidateDocuments();
        this.existingDocuments = info || {};
      } catch (e) {
        this.$bvToast.toast('دریافت مدارک قبلی با خطا مواجه شد.', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      } finally {
        this.loadingExistingDocuments = false;
      }
    },

    goBack() {
      this.setRequestStatus("DRAFT");
      this.$router.push({ name: 'request' });
    },

    openGuide(key) {
      if (key) {
        const idx = this.guideVideos.findIndex(v => v.key === key);
        this.activeGuideTab = idx > -1 ? idx : 0;
      } else {
        this.activeGuideTab = 0;
      }
      this.showGuideModal = true;
    },

    // بررسی می‌کند تصویر واقعاً قابل رمزگشایی و نمایش است (خراب/ناخوانا نباشد)
    checkImageReadable(raw) {
      return new Promise(resolve => {
        const url = URL.createObjectURL(raw);
        const img = new Image();
        img.onload = () => {
          URL.revokeObjectURL(url);
          resolve(img.naturalWidth > 0 && img.naturalHeight > 0);
        };
        img.onerror = () => {
          URL.revokeObjectURL(url);
          resolve(false);
        };
        img.src = url;
      });
    },

    // بررسی می‌کند فایل PDF با هدر معتبر (%PDF) شروع می‌شود
    checkPdfHeader(raw) {
      return new Promise(resolve => {
        const reader = new FileReader();
        reader.onload = () => {
          const bytes = new Uint8Array(reader.result);
          const valid = bytes.length >= 4 &&
            bytes[0] === 0x25 && bytes[1] === 0x50 && bytes[2] === 0x44 && bytes[3] === 0x46;
          resolve(valid);
        };
        reader.onerror = () => resolve(false);
        reader.readAsArrayBuffer(raw.slice(0, 4));
      });
    },

    // نمونه‌برداری از پیکسل‌های تصویر برای تشخیص «سند اسکن‌شده» در برابر «عکس پرسنلی»
    // اسناد اسکن‌شده معمولاً پس‌زمینه سفید زیاد و اشباع رنگ پایین دارند (متن سیاه/آبی روی کاغذ سفید)
    // درحالی‌که عکس پرسنلی رنگی‌تر است (پوست، لباس، پس‌زمینه رنگی)
    analyzeImagePixels(raw) {
      return new Promise(resolve => {
        const url = URL.createObjectURL(raw);
        const img = new Image();
        img.onload = () => {
          try {
            const size = 60;
            const canvas = document.createElement('canvas');
            canvas.width = size;
            canvas.height = size;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0, size, size);
            const { data } = ctx.getImageData(0, 0, size, size);

            let whiteCount = 0;
            let saturationSum = 0;
            const totalPixels = size * size;

            for (let i = 0; i < data.length; i += 4) {
              const r = data[i], g = data[i + 1], b = data[i + 2];
              const max = Math.max(r, g, b);
              const min = Math.min(r, g, b);

              if (r > 235 && g > 235 && b > 235) whiteCount++;
              saturationSum += (max - min);
            }

            URL.revokeObjectURL(url);
            resolve({
              whiteRatio: whiteCount / totalPixels,
              avgSaturation: saturationSum / totalPixels
            });
          } catch (e) {
            URL.revokeObjectURL(url);
            // در صورت بروز خطای canvas (مثلاً CORS)، از این بررسی صرف‌نظر می‌کنیم
            resolve(null);
          }
        };
        img.onerror = () => {
          URL.revokeObjectURL(url);
          resolve(null);
        };
        img.src = url;
      });
    },

    async validateFile(key, fileObj) {
      const config = this.fileValidationConfig[key];
      if (!config) return;

      // فایلی انتخاب نشده
      if (!fileObj || !fileObj.raw) {
        this.$set(this.fileErrors, key, '');
        this.$set(this.fileWarnings, key, '');
        return;
      }

      const raw = fileObj.raw;

      // بررسی حجم
      if (raw.size > config.maxSize) {
        this.$set(this.fileErrors, key, `حجم فایل «${config.label}» نباید بیشتر از ۱ مگابایت باشد.`);
        this.$set(this.fileWarnings, key, '');
        return;
      }

      // بررسی نوع فایل
      if (!config.allowed.includes(raw.type)) {
        this.$set(this.fileErrors, key, `فرمت فایل «${config.label}» مجاز نیست. فقط JPG، PNG و PDF قابل قبول است.`);
        this.$set(this.fileWarnings, key, '');
        return;
      }

      // بررسی خوانا و معتبر بودن محتوای فایل
      let isValidContent = true;
      if (raw.type === 'application/pdf') {
        isValidContent = await this.checkPdfHeader(raw);
      } else {
        isValidContent = await this.checkImageReadable(raw);
      }

      if (!isValidContent) {
        this.$set(this.fileErrors, key, `فایل «${config.label}» ناخوانا یا خراب است. لطفاً فایل معتبری بارگذاری کنید.`);
        this.$set(this.fileWarnings, key, '');
        return;
      }

      this.$set(this.fileErrors, key, '');

      // بررسی این‌که نوع محتوای تصویر با دستهٔ موردانتظار (عکس پرسنلی / سند اسکن‌شده) همخوانی دارد
      // این فقط یک هشدار غیرمسدودکننده است، نه خطای مانع از ثبت
      if (raw.type !== 'application/pdf') {
        const pixelInfo = await this.analyzeImagePixels(raw);
        if (pixelInfo) {
          const looksLikeScannedDoc = pixelInfo.whiteRatio > 0.55 && pixelInfo.avgSaturation < 20;
          const looksLikePersonalPhoto = pixelInfo.whiteRatio < 0.3 && pixelInfo.avgSaturation > 40;

          if (config.category === 'photo' && looksLikeScannedDoc) {
            this.$set(
              this.fileWarnings,
              key,
              `این تصویر بیشتر شبیه یک سند اسکن‌شده است تا عکس پرسنلی. لطفاً مطمئن شوید فایل درستی برای «${config.label}» انتخاب کرده‌اید.`
            );
            return;
          }

          if (config.category === 'document' && looksLikePersonalPhoto) {
            this.$set(
              this.fileWarnings,
              key,
              `این تصویر بیشتر شبیه یک عکس پرسنلی است تا سند اسکن‌شده. لطفاً مطمئن شوید فایل درستی برای «${config.label}» انتخاب کرده‌اید.`
            );
            return;
          }
        }
      }

      this.$set(this.fileWarnings, key, '');
    },

    cancel() {
      if (confirm("آیا از ادامه فرآیند ثبت‌نام انصراف می‌دهید؟")) {
        this.$router.push("/home");
      }
    },
    async submit() {
      if (!this.canSubmit || this.submitting) return;

      // بررسی نهایی همه فایل‌ها قبل از ارسال
      await Promise.all(
        Object.keys(this.fileValidationConfig).map(key => this.validateFile(key, this.files[key]))
      );
      if (this.hasValidationErrors || !this.canSubmit) return;

      this.submitting = true;
      try {
        const formData = new FormData();
        if (this.files.photo?.raw) formData.append("user_photo", this.files.photo.raw);
        if (this.files.degree?.raw) formData.append("education_doc", this.files.degree.raw);
        // if (this.files.noAddiction?.raw) formData.append("employment_cert", this.files.noAddiction.raw);
        if (this.files.soPishine?.raw) formData.append("soPishine_cert", this.files.soPishine.raw);
        if (this.files.ravan?.raw) formData.append("ravan_cert", this.files.ravan.raw);
        if (this.files.transparencyForm?.raw) formData.append("transparency_form", this.files.transparencyForm.raw);

        if (this.isEditMode) {
          await this.UpdateUserDocuments(formData);
          this.setCandidateFiles({ ...this.files });
          this.setRequestStatus("SUBMITTED");
          this.submitting = false;
          this.$bvToast.toast('مدارک با موفقیت به‌روزرسانی و ثبت نهایی شد.', {
            title: 'موفق',
            variant: 'success',
            solid: true
          });
          this.$router.push("/home");
          return;
        }

        await this.UploadUserDocuments(formData);


      } catch (error) {
        const message = error?.message || "ارسال مدارک با خطا مواجه شد.";
        alert(message);
      } finally {
        this.submitting = false;
      }
    }
  },
  watch: {
    'files.photo'(val) { this.validateFile('photo', val); },
    'files.degree'(val) { this.validateFile('degree', val); },
    'files.soPishine'(val) { this.validateFile('soPishine', val); },
    'files.ravan'(val) { this.validateFile('ravan', val); },
    'files.transparencyForm'(val) { this.validateFile('transparencyForm', val); },
    UploadUserDocumentsInfo(val) {
      if (val) {
        this.setCandidateFiles({ ...this.files });
        this.setRequestStatus("DOCUMENTS_UPLOADED");
        this.submitting = false;
        this.$router.push("/candidate/Confirmation");
      }
    }
  },
  created() {
    this.isEditMode = this.$route?.query?.edit === 'true';

    // بارگذاری فایل‌ها از Vuex در صورت برگشت
    if (this.$store.state.candidateFiles) {
      // this.files = { ...this.$store.state.candidateFiles };
      this.files = {
        photo: this.$store.state.candidateFiles.photo || null,
        degree: this.$store.state.candidateFiles.degree || null,
        // noAddiction: this.$store.state.candidateFiles.noAddiction || null,
        soPishine: this.$store.state.candidateFiles.soPishine || null,
        ravan: this.$store.state.candidateFiles.ravan || null,
        transparencyForm: this.$store.state.candidateFiles.transparencyForm || null
      };
    }

    if (this.isEditMode) {
      this.loadExistingDocuments();
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

/* راهنمای ویدیویی */
.guide-banner {
  gap: 10px;
}

.guide-link {
  display: inline-block;
  margin-top: 6px;
  font-size: 12px;
  color: #3f51b5;
}

.guide-link:hover {
  text-decoration: underline;
}

.video-wrapper {
  background: #000;
  border-radius: 6px;
  overflow: hidden;
}
</style>