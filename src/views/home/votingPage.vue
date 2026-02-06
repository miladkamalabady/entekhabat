<template>
  <div class="voting-page">
    <!-- Header -->
    <b-container fluid class="voting-header py-4">
      <b-row class="align-items-center">
        <b-col cols="12" md="8">
          <div class="d-flex align-items-center">
            <div class="voting-icon mr-3">
              <b-icon icon="ballot-fill"></b-icon>
            </div>
            <div>
              <h2 class="mb-1">صندوق رأی‌گیری الکترونیکی</h2>
              <p class="text-muted mb-0">انتخابات صندوق ذخیره فرهنگیان</p>
            </div>
          </div>
        </b-col>
        <b-col cols="12" md="4" class="text-left text-md-right">
          <div class="voter-info">
            <div class="voter-name">{{ currentUser.full_name }}</div>
            <div class="voter-id">کد ملی: {{ currentUser.national_id }}</div>
            <div class="voter-status">
              <b-badge :variant="voteStatus === 'voted' ? 'success' : 'warning'">
                {{ electionStatusAll === 'active' ? getVoteStatusText() : 'پایان یافته'}}
              </b-badge>
            </div>
          </div>
        </b-col>
      </b-row>
    </b-container>
    <b-alert show variant="light" class="text-center">
      <b-badge
        :variant="electionStatusAll === 'active' ? 'success' : electionStatusAll === 'upcoming' ? 'warning' : 'secondary'"
        pill>
        {{ electionStatusAll === 'active' ? 'در حال برگزاری' : electionStatusAll === 'upcoming' ? 'آغاز به زودی' :
          'پایان یافته' }}
      </b-badge>
    </b-alert>
    <!-- Voting Progress -->
    <b-container class="voting-progress mb-4" v-if="electionStatusAll === 'active'">
      <b-card>
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h5 class="mb-0">روند رأی‌گیری</h5>
          <div class="time-remaining">
            <b-icon icon="clock" class="ml-1"></b-icon>
            زمان باقیمانده: {{ timeRemaining }}
          </div>
        </div>

        <b-progress height="8px" class="mb-2">
          <b-progress-bar :value="progress" variant="primary" :label="`${progress}%`"></b-progress-bar>
        </b-progress>

        <div class="progress-steps">
          <div class="step" :class="{ active: currentStep >= 1, completed: currentStep > 1 }">
            <span class="step-number">1</span>
            <span class="step-label">مشاهده کاندیداها</span>
          </div>
          <div class="step" :class="{ active: currentStep >= 2, completed: currentStep > 2 }">
            <span class="step-number">2</span>
            <span class="step-label">انتخاب کاندیدا</span>
          </div>
          <div class="step" :class="{ active: currentStep >= 3, completed: currentStep > 3 }">
            <span class="step-number">3</span>
            <span class="step-label">تأیید نهایی</span>
          </div>
        </div>
      </b-card>
    </b-container>

    <!-- Main Voting Content -->
    <b-container class="voting-content" v-if="electionStatusAll === 'active' || electionStatusAll === 'ended'">
      <!-- Step 1: Authentication -->
      <div v-if="currentStep === 0" class="step-container">
        <b-card class="auth-card">
          <div class="text-center mb-4">
            <div class="auth-icon">
              <b-icon icon="shield-lock-fill"></b-icon>
            </div>
            <h4>احراز هویت رأی‌دهنده</h4>
            <p class="text-muted">لطفاً اطلاعات هویتی خود را تأیید کنید</p>
          </div>

          <b-form @submit.prevent="verifyIdentity">
            <b-row>
              <b-col md="6">
                <b-form-group label="کد ملی" label-for="national-id">
                  <b-form-input id="national-id" v-model="authData.nationalId" :state="authState.nationalId"
                    placeholder="مثال: ۰۰۱۲۳۴۵۶۷۸" required maxlength="10"></b-form-input>
                  <b-form-invalid-feedback>
                    کد ملی معتبر نیست
                  </b-form-invalid-feedback>
                </b-form-group>
              </b-col>
              <b-col md="6">
                <b-form-group label="شماره همراه" label-for="mobile">
                  <b-form-input id="mobile" v-model="authData.mobile" :state="authState.mobile"
                    placeholder="مثال: ۰۹۱۲۳۴۵۶۷۸۹" required></b-form-input>
                  <b-form-invalid-feedback>
                    شماره همراه معتبر نیست
                  </b-form-invalid-feedback>
                </b-form-group>
              </b-col>
            </b-row>

            <b-form-group label="کد تأیید" label-for="verification-code" class="mt-3">
              <b-input-group>
                <b-form-input id="verification-code" v-model="authData.verificationCode"
                  :state="authState.verificationCode" placeholder="کد ۶ رقمی ارسال شده" maxlength="6"
                  required></b-form-input>
                <b-input-group-append>
                  <b-button variant="outline-primary" @click="sendVerificationCode" :disabled="cooldown > 0">
                    <span v-if="cooldown > 0">
                      {{ cooldown }} ثانیه
                    </span>
                    <span v-else>
                      دریافت کد
                    </span>
                  </b-button>
                </b-input-group-append>
              </b-input-group>
              <small class="form-text text-muted">
                کد تأیید به شماره همراه شما ارسال خواهد شد
              </small>
            </b-form-group>

            <div class="text-center mt-4">
              <b-button type="submit" variant="primary" size="lg" :disabled="verifying">
                <b-spinner small v-if="verifying" class="ml-1"></b-spinner>
                تأیید هویت و ادامه
              </b-button>
            </div>
          </b-form>

          <div class="auth-note mt-4">
            <b-alert variant="info" show class="text-right">
              <b-icon icon="info-circle" class="ml-1"></b-icon>
              اطلاعات شما صرفاً برای احراز هویت استفاده می‌شود و محرمانه باقی می‌ماند.
            </b-alert>
          </div>
        </b-card>
      </div>

      <!-- Step 2: Candidates List -->
      <div v-else-if="currentStep === 1 && electionStatusAll === 'active'" class="step-container">
        <b-card>
          <div class="text-center mb-4">
            <h4>لیست کاندیداهای انتخابات</h4>
            <p class="text-muted">لطفاً اطلاعات کاندیداها را مطالعه کنید</p>
          </div>

          <div class="candidates-filter mb-4">
            <b-input-group>
              <template #prepend>
                <b-input-group-text>
                  <b-icon icon="search"></b-icon>
                </b-input-group-text>
              </template>
              <b-form-input v-model="searchQuery" placeholder="جستجو در کاندیداها..."></b-form-input>
              <template #append>
                <b-form-select v-model="sortBy" :options="sortOptions" class="w-auto"></b-form-select>
              </template>
            </b-input-group>
          </div>
          <b-row>
            <b-col v-for="candidate in filteredCandidates" :key="candidate.id" cols="12" md="6" lg="4" class="mb-4">
              <b-card class="candidate-card h-100" :class="{ 'selected': selectedCandidate?.id === candidate.id }"
                @click="previewCandidate(candidate)">
                <!-- Candidate Image -->
                <div class="candidate-image-container mb-3">
                  <img :src="`${apiUrlrtb}/${candidate.user_photo}`" :alt="candidate.first_name"
                    class="candidate-image" />

                </div>
                <!-- Candidate Info -->
                <h5 class="candidate-name">{{ candidate.first_name }} {{ candidate.last_name }}</h5>
                <p class="candidate-position text-muted">{{ candidate.org_position_desc }}</p>

                <!-- Candidate Stats -->
                <div class="candidate-stats">
                  <div class="stat-item">
                    <b-icon icon="award" class="ml-1"></b-icon>
                    {{ candidate.personnel_code }}
                  </div>
                </div>

                <!-- Action Button -->
                <div class="text-center mt-3">
                  <b-button variant="outline-primary" size="sm" @click.stop="previewCandidate(candidate)" block>
                    <b-icon icon="eye" class="ml-1"></b-icon>
                    مشاهده جزئیات
                  </b-button>
                </div>
              </b-card>
            </b-col>
          </b-row>

          <div class="text-center mt-4">
            <b-button variant="primary" @click="nextStep" :disabled="!selectedCandidate">
              <b-icon icon="arrow-left" class="ml-1"></b-icon>
              ادامه به انتخاب نهایی
            </b-button>
            <b-button variant="outline-secondary" class="mr-3" @click="prevStep">
              بازگشت
            </b-button>
          </div>
        </b-card>
      </div>

      <!-- Step 3: Final Selection -->
      <div v-else-if="currentStep === 2" class="step-container">
        <b-card>
          <div class="text-center mb-4">
            <h4>انتخاب نهایی کاندیدا</h4>
            <p class="text-muted">لطفاً انتخاب خود را نهایی کنید</p>
          </div>

          <div class="selection-container" v-if="selectedCandidate">
            <b-row class="align-items-center">
              <b-col md="5" class="text-center">
                <div class="selected-candidate-image">
                  <img :src="`${apiUrlrtb}/${selectedCandidate.user_photo}`" :alt="selectedCandidate.first_name"
                    class="selected-image" />
                </div>
              </b-col>

              <b-col md="7">
                <div class="selected-candidate-info">
                  <h3 class="selected-name"> {{ selectedCandidate.gender ? 'آقای' : 'خانم' }} {{
                    selectedCandidate.first_name
                    }} {{ selectedCandidate.last_name }}</h3>
                  <p class="selected-position">{{ selectedCandidate.org_position_desc }}</p>

                  <div class="selected-details">
                    <div class="detail-item">
                      <strong>تولد:</strong>
                      <span>{{ selectedCandidate.persian_birth_date }}</span>
                    </div>
                    <div class="detail-item">
                      <strong>کدکاندید:</strong>
                      <span>{{ selectedCandidate.tracking_code }}</span>
                    </div>
                    <div class="detail-item">
                      <strong>پرسنلی:</strong>
                      <span>{{ selectedCandidate.personnel_code }}</span>
                    </div>
                    <div class="detail-item">
                      <strong>شهر:</strong>
                      <span>{{ selectedCandidate.region_id }}</span>
                    </div>
                  </div>

                </div>
              </b-col>
            </b-row>

            <!-- Confirmation -->
            <div class="confirmation-section mt-5">
              <b-alert variant="warning" show class="text-center">
                <h5 class="alert-heading">تأیید نهایی رأی</h5>
                <p class="mb-3">آیا از انتخاب خود اطمینان دارید؟ پس از ثبت رأی، امکان تغییر وجود ندارد.</p>

                <div class="confirmation-check">
                  <b-form-checkbox v-model="confirmation.accepted" name="confirmation-check" :state="confirmationState">
                    <span class="confirmation-text">
                      تأیید می‌کنم که {{ selectedCandidate.name }} را به عنوان عضو هیئت مدیره صندوق ذخیره فرهنگیان
                      انتخاب
                      کرده‌ام و از غیرقابل تغییر بودن رأی اطلاع دارم.
                    </span>
                  </b-form-checkbox>
                  <b-form-invalid-feedback :state="confirmationState">
                    لطفاً گزینه تأیید را انتخاب کنید
                  </b-form-invalid-feedback>
                </div>
              </b-alert>
            </div>

            <!-- Action Buttons -->
            <div class="text-center mt-4">
              <b-button variant="success" size="lg" class="mr-3" @click="submitVote"
                :disabled="!confirmation.accepted || submitting">
                <b-spinner small v-if="submitting" class="ml-1"></b-spinner>
                <b-icon v-else icon="check-circle" class="ml-1"></b-icon>
                ثبت رأی نهایی
              </b-button>
              <b-button variant="outline-secondary" @click="prevStep">
                بازگشت و تغییر انتخاب
              </b-button>
            </div>
          </div>

          <div v-else class="text-center py-5">
            <b-icon icon="exclamation-circle" font-scale="4" variant="warning"></b-icon>
            <h5 class="mt-3">کاندیدایی انتخاب نشده است</h5>
            <p class="text-muted">لطفاً به مرحله قبل بازگردید و کاندیدای مورد نظر خود را انتخاب کنید.</p>
            <b-button variant="primary" @click="prevStep">
              بازگشت به لیست کاندیداها
            </b-button>
          </div>
        </b-card>
      </div>

      <!-- Step 4: Vote Success -->
      <div v-else-if="currentStep === 3" class="step-container">
        <b-card class="success-card">
          <div class="text-center py-5">
            <div class="success-icon">
              <b-icon icon="check-circle-fill"></b-icon>
            </div>
            <h3 class="mt-4 mb-3">رأی شما با موفقیت ثبت شد!</h3>
            <p class="lead mb-4">از مشارکت شما در انتخابات صندوق ذخیره فرهنگیان سپاسگزاریم.</p>

            <div class="vote-summary">
              <b-card class="summary-card">
                <b-row>
                  <b-col md="6">
                    <div class="summary-item">
                      <strong>شماره پیگیری:</strong>
                      <span class="tracking-number">{{ voteTrackingCode }}</span>
                    </div>
                    <div class="summary-item">
                      <strong>تاریخ رأی‌گیری:</strong>
                      <span>{{ voteDate }}</span>
                    </div>
                    <div class="summary-item">
                      <strong>ساعت رأی‌گیری:</strong>
                      <span>{{ voteTime }}</span>
                    </div>
                  </b-col>
                  <b-col md="6">
                    <div class="summary-item">
                      <strong>کاندیدای انتخاب شده:</strong>
                      <span>{{ selectedCandidate?.first_name }} {{ selectedCandidate?.last_name }} </span>
                    </div>
                    <div class="summary-item">
                      <strong>کد کاندیدای انتخاب شده:</strong>
                      <span>{{ selectedCandidate?.id }}</span>
                    </div>
                    <div class="summary-item">
                      <strong>کدملی رأی‌دهنده:</strong>
                      <span>{{ currentUser?.national_id }}</span>
                    </div>
                  </b-col>
                </b-row>
              </b-card>
            </div>

            <div class="success-actions mt-5">
              <b-button variant="primary" class="mr-3" @click="downloadReceipt">
                <b-icon icon="download" class="ml-1"></b-icon>
                دریافت رسید
              </b-button>
              <b-button variant="outline-info" class="mr-3" @click="viewResults">
                <b-icon icon="bar-chart" class="ml-1"></b-icon>
                مشاهده نتایج
              </b-button>
              <b-button variant="outline-secondary" @click="goToHome">
                <b-icon icon="house-door" class="ml-1"></b-icon>
                بازگشت به صفحه اصلی
              </b-button>
            </div>

            <div class="success-note mt-4">
              <b-alert variant="info" show>
                <b-icon icon="info-circle" class="ml-1"></b-icon>
                شماره پیگیری خود را حفظ کنید. این شماره برای پیگیری رأی شما ضروری است.
              </b-alert>
            </div>
          </div>
        </b-card>
      </div>
    </b-container>

    <!-- Candidate Preview Modal -->
    <b-modal v-model="showCandidateModal" :title="previewCandidateData?.name" size="lg" hide-footer centered scrollable>
      <div v-if="previewCandidateData" class="candidate-preview">
        <b-row class="align-items-center mb-4">
          <b-col md="4" class="text-center">
            <img :src="`${apiUrlrtb}/${previewCandidateData.user_photo}`" :alt="previewCandidateData.first_name"
              class="preview-image" />
          </b-col>
          <b-col md="8">
            <h5>{{ previewCandidateData.org_position_desc }}</h5>
            <div class="preview-stats">
              <b-badge variant="info" class="mr-2">
                <b-icon icon="briefcase" class="ml-1"></b-icon>
                {{ previewCandidateData.persian_birth_date }}
              </b-badge>
              <b-badge variant="success" class="mr-2">
                <b-icon icon="award" class="ml-1"></b-icon>
                {{ previewCandidateData.personnel_code }}
              </b-badge>
            </div>
          </b-col>
        </b-row>

        <b-tabs content-class="mt-3">
          <b-tab title="منطقه" active>
            <p class="preview-text">{{ previewCandidateData.region_id }} -
              {{ previewCandidateData.gender ? 'آقا' : 'خانم' }}</p>
          </b-tab>

          <!-- <b-tab title="سوابق کاری">
            <ul class="preview-list">
              <li v-for="(experience, index) in previewCandidateData.experiences" :key="index">
                {{ experience }}
              </li>
            </ul>
          </b-tab>

          <b-tab title="برنامه انتخابی">
            <ul class="preview-list">
              <li v-for="(item, index) in previewCandidateData.program" :key="index">
                {{ item }}
              </li>
            </ul>
          </b-tab> -->

          <!-- <b-tab title="مدارک و گواهی‌ها">
            <div class="certificates">
              <div v-for="(cert, index) in previewCandidateData.certificates" :key="index" class="certificate-item">
                <b-icon icon="file-earmark-text" class="ml-2"></b-icon>
                {{ cert }}
              </div>
            </div>
          </b-tab> -->
        </b-tabs>

        <div class="text-center mt-4">
          <b-button variant="primary" @click="selectCandidate(previewCandidateData)" :disabled="voteStatus === 'voted'">
            <b-icon icon="check-circle" class="ml-1"></b-icon>
            انتخاب این کاندیدا
          </b-button>
        </div>
      </div>
    </b-modal>

    <!-- Voting Instructions -->
    <b-container class="voting-instructions mt-4" v-if="electionStatusAll === 'active'">
      <b-card>
        <h5 class="mb-3">
          <b-icon icon="info-circle-fill" class="ml-2"></b-icon>
          راهنمای رأی‌گیری
        </h5>
        <b-row>
          <b-col md="4">
            <div class="instruction-item">
              <div class="instruction-icon">
                <b-icon icon="shield-check"></b-icon>
              </div>
              <h6>امنیت کامل</h6>
              <p>رأی شما به صورت کاملاً محرمانه و امن ثبت می‌شود.</p>
            </div>
          </b-col>
          <b-col md="4">
            <div class="instruction-item">
              <div class="instruction-icon">
                <b-icon icon="clock-history"></b-icon>
              </div>
              <h6>زمان محدود</h6>
              <p>تا پایان زمان انتخابات فرصت دارید رأی خود را ثبت کنید.</p>
            </div>
          </b-col>
          <b-col md="4">
            <div class="instruction-item">
              <div class="instruction-icon">
                <b-icon icon="arrow-counterclockwise"></b-icon>
              </div>
              <h6>غیرقابل تغییر</h6>
              <p>پس از ثبت نهایی، رأی شما قابل تغییر نخواهد بود.</p>
            </div>
          </b-col>
        </b-row>
      </b-card>
    </b-container>

    <!-- Timer Warning -->
    <div v-if="showTimeWarning" class="timer-warning">
      <b-alert variant="warning" show class="mb-0 text-center">
        <b-icon icon="exclamation-triangle-fill" class="ml-1"></b-icon>
        زمان باقیمانده تا پایان انتخابات: {{ timeRemaining }}
        <b-button variant="outline-warning" size="sm" class="mr-3" @click="showTimeWarning = false">
          فهمیدم
        </b-button>
      </b-alert>
    </div>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { apiUrlrtb } from '../../constants/config'
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: "VotingPage",
  data() {
    return {
      isMobile, apiUrlrtb,
      // Voter Information

      // Voting Status
      voteStatus: 'not_voted', // 'not_voted', 'voted'
      currentStep: 1,
      progress: 25,
      timeRemaining: '۲ ساعت و ۴۵ دقیقه',

      // Authentication Data
      authData: {
        nationalId: '',
        mobile: '',
        verificationCode: ''
      },
      authState: {
        nationalId: null,
        mobile: null,
        verificationCode: null
      },
      verifying: false,
      cooldown: 0,
      cooldownInterval: null,

      // Candidates Data
      candidates: [],

      // Search and Filter
      searchQuery: '',
      sortBy: 'experience',
      sortOptions: [
        { value: 'persian_birth_date', text: 'سن' },
        { value: 'name', text: 'نام الفبایی' },
      ],

      // Selected Candidate
      selectedCandidate: null,
      previewCandidateData: null,
      showCandidateModal: false,

      // Confirmation
      confirmation: {
        accepted: false
      },
      confirmationState: null,

      // Submission
      submitting: false,

      // Success Data
      voteTrackingCode: '',
      voteDate: '',
      voteTime: '',

      // UI State
      showTimeWarning: false
    };
  },
  computed: {
    ...mapGetters(["currentUser", "electionStatusAll"]),
    filteredCandidates() {

      let filtered = [...this.candidates];


      // Apply search
      if (this.searchQuery) {
        const query = this.searchQuery();
        filtered = filtered.filter(candidate =>
          candidate.name().includes(query) ||
          candidate.position().includes(query) ||
          candidate.specialty().includes(query) ||
          candidate.city().includes(query)
        );
      }

      // Apply sorting
      // switch (this.sortBy) {
      //   case 'experience':
      //     filtered.sort((a, b) => b.experience - a.experience);
      //     break;
      //   case 'name':
      //     filtered.sort((a, b) => a.name.localeCompare(b.name));
      //     break;
      //   case 'city':
      //     filtered.sort((a, b) => a.city.localeCompare(b.city));
      //     break;
      // }
      return filtered;
    }
  },
  async mounted() {
    this.candidates = await this.getCandidsList()

    this.checkVoteStatus();
    this.startTimer();
    this.startCooldownTimer();
  },
  beforeUnmount() {
    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
    }
  },
  methods: {
    ...mapActions(["getCandidsList", "getVote", "insertVote"]),
    // Step Navigation
    nextStep() {
      if (this.currentStep < 4) {
        this.currentStep++;
        this.progress = this.currentStep * 25;
      }
    },

    prevStep() {
      if (this.currentStep > 1) {
        this.currentStep--;
        this.progress = this.currentStep * 25;
      }
    },

    // Authentication
    validateNationalId(id) {
      if (!id || id.length !== 10) return false;

      // Simple validation (in real app, use proper algorithm)
      const regex = /^\d{10}$/;
      return regex.test(id);
    },

    validateMobile(mobile) {
      const regex = /^09\d{9}$/;
      return regex.test(mobile);
    },

    sendVerificationCode() {
      // Validate mobile
      if (!this.validateMobile(this.authData.mobile)) {
        this.authState.mobile = false;
        return;
      }

      this.authState.mobile = true;

      // Start cooldown
      this.cooldown = 120; // 2 minutes

      // Simulate API call
      setTimeout(() => {
        this.$bvToast.toast('کد تأیید به شماره همراه شما ارسال شد', {
          title: 'ارسال کد',
          variant: 'success',
          solid: true
        });
      }, 1000);
    },

    startCooldownTimer() {
      this.cooldownInterval = setInterval(() => {
        if (this.cooldown > 0) {
          this.cooldown--;
        }
      }, 1000);
    },

    async verifyIdentity() {
      // Validate inputs
      let valid = true;

      if (!this.validateNationalId(this.authData.nationalId)) {
        this.authState.nationalId = false;
        valid = false;
      } else {
        this.authState.nationalId = true;
      }

      if (!this.validateMobile(this.authData.mobile)) {
        this.authState.mobile = false;
        valid = false;
      } else {
        this.authState.mobile = true;
      }

      if (!this.authData.verificationCode || this.authData.verificationCode.length !== 6) {
        this.authState.verificationCode = false;
        valid = false;
      } else {
        this.authState.verificationCode = true;
      }

      if (!valid) return;

      this.verifying = true;

      // Simulate API verification
      try {
        await new Promise(resolve => setTimeout(resolve, 2000));

        // Mock verification (in real app, verify with backend)
        const mockCode = '123456';
        if (this.authData.verificationCode === mockCode) {
          this.$bvToast.toast('هویت شما با موفقیت تأیید شد', {
            title: 'احراز هویت موفق',
            variant: 'success',
            solid: true
          });

          // Move to next step
          this.nextStep();
        } else {
          this.$bvToast.toast('کد تأیید نامعتبر است', {
            title: 'خطا در احراز هویت',
            variant: 'danger',
            solid: true
          });
          this.authState.verificationCode = false;
        }
      } catch (error) {
        console.error('Verification error:', error);
        this.$bvToast.toast('خطا در ارتباط با سرور', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      } finally {
        this.verifying = false;
      }
    },

    // Candidate Selection
    previewCandidate(candidate) {
      this.previewCandidateData = candidate;
      this.showCandidateModal = true;
    },

    selectCandidate(candidate) {
      this.selectedCandidate = candidate;
      this.showCandidateModal = false;

      this.$bvToast.toast(`کاندیدای ${candidate.first_name} ${candidate.last_name} انتخاب شد`, {
        title: 'انتخاب کاندیدا',
        variant: 'success',
        solid: true
      });
    },

    // Vote Submission
    async submitVote() {
      if (!this.confirmation.accepted) {
        this.confirmationState = false;
        return;
      }

      this.confirmationState = true;
      this.submitting = true;

      try {
        // Simulate API call
        // await new Promise(resolve => setTimeout(resolve, 3000));

        // Generate tracking code
        this.voteTrackingCode = 'VT' + Date.now().toString().slice(-8);
        this.voteDate = this.getCurrentDate();
        this.voteTime = this.getCurrentTime();

        // Update vote status
        this.voteStatus = 'voted';

        // Move to success step
        this.nextStep();

        this.$bvToast.toast('رأی شما با موفقیت ثبت شد', {
          title: 'ثبت رأی موفق',
          variant: 'success',
          solid: true
        });


        await this.insertVote({ usernationalid: this.selectedCandidate.id, trackingCode: this.voteTrackingCode })
        //localStorage.setItem('lastVote', JSON.stringify(voteLog));

      } catch (error) {
        console.error('Vote submission error:', error);
        this.$bvToast.toast('خطا در ثبت رأی', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      } finally {
        this.submitting = false;
      }
    },

    // Success Actions
    downloadReceipt() {
      // Generate receipt content

      const receiptContent = `
        رسید رأی‌گیری الکترونیکی
        =========================
        
        شماره پیگیری: ${this.voteTrackingCode}
        تاریخ: ${this.voteDate}
        ساعت: ${this.voteTime}
        
        اطلاعات رأی‌دهنده:
        -----------------
        نام: ${this.currentUser.full_name}
        کد ملی: ${this.currentUser.national_id}
        
        کاندیدای انتخاب شده:
        --------------------
        نام: ${this.selectedCandidate?.first_name} ${this.selectedCandidate?.last_name}
        سمت: ${this.selectedCandidate?.org_position_desc}
        
        این سند به عنوان رسید رسمی رأی‌گیری محسوب می‌شود.
        
        تاریخ چاپ: ${new Date().toLocaleDateString('fa-IR')}
      `;

      // Create and download text file
      const blob = new Blob([receiptContent], { type: 'text/plain' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `vote_receipt_${this.voteTrackingCode}.txt`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);

      this.$bvToast.toast('رسید رأی‌گیری دانلود شد', {
        title: 'دانلود موفق',
        variant: 'success',
        solid: true
      });
    },

    viewResults() {
      if (this.electionStatusAll == 'ended')
        this.$router.push('/results/final-election');
      else
        this.$router.push('/results/live-election');
    },

    goToHome() {
      this.$router.push('/');
    },

    // Utilities
    async checkVoteStatus() {
      // Check if user has already voted (from localStorage for demo)
      const voteData = await this.getVote()
      // const lastVote = localStorage.getItem('lastVote');
      if (voteData) {
        this.voteStatus = 'voted';
        this.currentStep = 3;
        this.progress = 100;

        this.voteTrackingCode = voteData.trackingCode;
        this.voteDate = new Date(voteData.createdate).toLocaleDateString('fa-IR');
        this.voteTime = new Date(voteData.createdate).toLocaleTimeString('fa-IR');

        // Find selected candidate
        this.selectedCandidate = this.candidates?.find(c => c.id == voteData.candidateId);

      }
    },

    getVoteStatusText() {
      return this.voteStatus === 'voted' ? 'رأی داده شده' : 'آماده رأی‌گیری';
    },

    getCurrentDate() {
      return new Date().toLocaleDateString('fa-IR');
    },

    getCurrentTime() {
      return new Date().toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });
    },

    startTimer() {
      // Simulate time countdown
      setInterval(() => {
        // Update time remaining (for demo)
        const hours = Math.floor(Math.random() * 3);
        const minutes = Math.floor(Math.random() * 60);
        this.timeRemaining = `${hours} ساعت و ${minutes} دقیقه`;

        // Show warning when time is low
        if (hours === 0 && minutes < 30) {
          this.showTimeWarning = true;
        }
      }, 60000); // Update every minute
    }
  }
};
</script>

<style scoped>
.voting-page {
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* Header */
.voting-header {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.voting-icon {
  font-size: 2.5rem;
  color: #4CAF50;
}

.voter-info {
  background: rgba(255, 255, 255, 0.1);
  padding: 15px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.voter-name {
  font-size: 1.2rem;
  font-weight: bold;
  margin-bottom: 5px;
}

.voter-id {
  font-size: 0.9rem;
  opacity: 0.9;
  margin-bottom: 8px;
}

/* Progress Steps */
.progress-steps {
  display: flex;
  justify-content: space-between;
  position: relative;
  margin-top: 30px;
}

.progress-steps::before {
  content: '';
  position: absolute;
  top: 15px;
  right: 0;
  left: 0;
  height: 2px;
  background: #e0e0e0;
  z-index: 1;
}

.step {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  z-index: 2;
}

.step-number {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: #e0e0e0;
  color: #666;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  margin-bottom: 8px;
  transition: all 0.3s ease;
}

.step.active .step-number {
  background: #2196F3;
  color: white;
  transform: scale(1.1);
}

.step.completed .step-number {
  background: #4CAF50;
  color: white;
}

.step-label {
  font-size: 0.9rem;
  color: #666;
  text-align: center;
}

.step.active .step-label {
  color: #2196F3;
  font-weight: bold;
}

/* Authentication */
.auth-card {
  max-width: 600px;
  margin: 0 auto;
  border-radius: 15px;
}

.auth-icon {
  font-size: 3rem;
  color: #3F51B5;
  margin-bottom: 15px;
}

/* Candidate Cards */
.candidate-card {
  border-radius: 12px;
  border: 2px solid transparent;
  transition: all 0.3s ease;
  cursor: pointer;
  overflow: hidden;
}

.candidate-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15);
  border-color: #2196F3;
}

.candidate-card.selected {
  border-color: #4CAF50;
  background: #f8fff8;
}

.candidate-image-container {
  position: relative;
  height: 200px;
  overflow: hidden;
  border-radius: 10px;
}

.candidate-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.candidate-badge {
  position: absolute;
  top: 10px;
  left: 10px;
  background: #FFC107;
  color: #333;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
}

.candidate-name {
  font-size: 1.1rem;
  font-weight: bold;
  margin-bottom: 5px;
  color: #2c3e50;
}

.candidate-position {
  font-size: 0.9rem;
  margin-bottom: 10px;
  min-height: 40px;
}

.candidate-stats {
  font-size: 0.8rem;
  color: #666;
}

.stat-item {
  margin-bottom: 5px;
  display: flex;
  align-items: center;
}

/* Selected Candidate */
.selected-candidate-image {
  padding: 20px;
}

.selected-image {
  width: 200px;
  height: 200px;
  border-radius: 50%;
  object-fit: cover;
  border: 5px solid #4CAF50;
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
}

.selected-name {
  color: #2c3e50;
  font-size: 1.8rem;
  margin-bottom: 10px;
}

.selected-position {
  color: #666;
  font-size: 1.1rem;
  margin-bottom: 20px;
}

.selected-details {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 10px;
  margin-bottom: 20px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
}

.detail-item:last-child {
  border-bottom: none;
}

.selected-bio {
  background: white;
  padding: 15px;
  border-radius: 10px;
  border: 1px solid #e0e0e0;
}

.bio-text {
  line-height: 1.8;
  color: #555;
  text-align: justify;
}

.selected-program {
  background: #e8f5e9;
  padding: 15px;
  border-radius: 10px;
}

.program-list {
  padding-right: 20px;
  color: #555;
}

.program-list li {
  margin-bottom: 8px;
  line-height: 1.6;
}

.confirmation-check {
  margin-top: 20px;
}

.confirmation-text {
  font-size: 0.95rem;
  color: #333;
}

/* Success Card */
.success-card {
  max-width: 800px;
  margin: 0 auto;
  border-radius: 15px;
  border: 3px solid #4CAF50;
  background: linear-gradient(135deg, #f8fff8 0%, #e8f5e9 100%);
}

.success-icon {
  font-size: 5rem;
  color: #4CAF50;
  animation: bounce 2s infinite;
}

@keyframes bounce {

  0%,
  100% {
    transform: scale(1);
  }

  50% {
    transform: scale(1.1);
  }
}

.summary-card {
  background: white;
  border-radius: 10px;
  border: 1px solid #e0e0e0;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}

.summary-item:last-child {
  border-bottom: none;
}

.tracking-number {
  font-family: monospace;
  font-size: 1.2rem;
  font-weight: bold;
  color: #3F51B5;
  background: #f0f0f0;
  padding: 5px 10px;
  border-radius: 5px;
}

.success-actions {
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
  gap: 15px;
}

/* Preview Modal */
.preview-image {
  width: 150px;
  height: 150px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #3F51B5;
}

.preview-stats {
  margin-bottom: 15px;
}

.preview-text {
  line-height: 1.8;
  color: #555;
  text-align: justify;
}

.preview-list {
  padding-right: 20px;
  color: #555;
}

.preview-list li {
  margin-bottom: 10px;
  line-height: 1.6;
}

.certificates {
  padding-right: 20px;
}

.certificate-item {
  padding: 10px;
  border-bottom: 1px solid #e0e0e0;
  display: flex;
  align-items: center;
}

.certificate-item:last-child {
  border-bottom: none;
}

/* Instructions */
.instruction-item {
  text-align: center;
  padding: 20px;
}

.instruction-icon {
  font-size: 2.5rem;
  color: #3F51B5;
  margin-bottom: 15px;
}

.instruction-item h6 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.instruction-item p {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.6;
}

/* Timer Warning */
.timer-warning {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1000;
}

/* Responsive Design */
@media (max-width: 768px) {
  .voter-info {
    margin-top: 15px;
    text-align: center;
  }

  .progress-steps {
    flex-direction: column;
    gap: 20px;
  }

  .progress-steps::before {
    display: none;
  }

  .step {
    flex-direction: row;
    align-items: center;
    justify-content: flex-start;
    gap: 15px;
  }

  .step-number {
    margin-bottom: 0;
  }

  .selected-image {
    width: 150px;
    height: 150px;
  }

  .success-actions {
    flex-direction: column;
    align-items: center;
  }

  .success-actions .btn {
    width: 100%;
    max-width: 250px;
    margin-bottom: 10px;
  }
}

@media (max-width: 576px) {
  .candidate-card {
    margin-bottom: 15px;
  }

  .selected-name {
    font-size: 1.4rem;
  }

  .instruction-item {
    margin-bottom: 20px;
  }
}
</style>