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
                {{ electionStatusAll === 'active' ? getVoteStatusText() : 'پایان یافته' }}
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

      <!-- Step 2: Candidates List -->
      <div v-if="currentStep === 1 && electionStatusAll === 'active'" class="step-container">
        <b-card>
          <div class="text-center mb-4">
            <h4>لیست کاندیداهای انتخابات</h4>
            <p v-if="maxVotes" class="text-muted">لطفاً اطلاعات کاندیداها را مطالعه کنید<br />
              تعداد حداکثر کاندید انتخابی در حوزه <u class="text-success">{{ currentUser?.regionName }}</u> تعداد <u
                class="text-success">{{ maxVotes }}</u> کاندید می‌باشد</p>
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

          <div class="text-center mt-2 mb-4">
            <b-button variant="primary" @click="goToConfirmation" :disabled="selectedCandidates?.length === 0">
              <b-icon icon="arrow-left" class="ml-1"></b-icon>
              ادامه به انتخاب نهایی
            </b-button>
            <b-button variant="outline-secondary" class="mr-3" @click="prevStep">
              بازگشت
            </b-button>
          </div>
          <b-row>
            <b-col v-for="candidate in filteredCandidates" :key="candidate.id" cols="12" md="4" lg="3" class="mb-4">
              <b-card class="candidate-card h-100" :class="{ 'selected': selectedCandidates?.includes(candidate.id) }"
                @click="previewCandidate(candidate)">
                <!-- Candidate Image -->
                <div class="candidate-image-container mb-3">
                  <img :src="`${apiUrlrtb}/${candidate.user_photo}`" :alt="candidate.first_name"
                    class="candidate-image" />

                </div>
                <!-- Candidate Info -->
                <h5 class="candidate-name">{{ candidate.first_name }} {{ candidate.last_name }}</h5>
                <p class="candidate-position" :class="{ 'text-muted': !selectedCandidates?.includes(candidate.id) }">{{
                  candidate.org_position_desc }}</p>

                <!-- Candidate Stats -->
                <div class="candidate-stats" :class="{ 'text-muted': !selectedCandidates?.includes(candidate.id) }">
                  <div class="stat-item">
                    <b-icon icon="award" class="ml-1"></b-icon>
                    {{ candidate.personnel_code }}
                  </div>
                  <div class="stat-item">
                    <b-icon icon="award" class="ml-1"></b-icon>
                    {{ candidate.id *1404 }}
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

        </b-card>
      </div>

      <!-- Step 3: Final Selection -->
      <div v-else-if="currentStep === 2" class="step-container">
        <b-card>
          <div class="selection-container" v-if="selectedCandidates?.length">

            <!-- Confirmation -->
            <div class="confirmation-section mt-1">
              <b-alert variant="warning" show class="text-center">
                <h5 class="alert-heading">بازبینی و تأیید نهایی رأی</h5>
                <p class="mb-3"> شما افراد زیر را برای عضویت در هیئت مدیره صندوق ذخیره فرهنگیان انتخاب کرده‌اید. لطفاً
                  لیست را
                  با دقت بررسی نمایید. پس از ثبت، امکان تغییر رأی وجود نخواهد داشت. </p>

                <!-- لیست انتخاب‌ها -->
                <div class="selected-review-box mb-4">
                  <b-row>
                    <b-col v-for="cid in selectedCandidates" :key="cid" md="3" class="p-1 mb-2">
                      <div class="review-item p-2">
                        <div class="selected-candidate-image">
                          <img :src="`${apiUrlrtb}/${findCandidate(cid).user_photo}`"
                            :alt="findCandidate(cid).first_name" class="selected-image" />
                        </div>

                        <div class="selected-details">
                          <div class="text-center">
                            <span>{{ findCandidate(cid).gender==1 ? 'آقای' : 'خانم' }} {{ findCandidate(cid).first_name }}
                              {{
                                findCandidate(cid).last_name }}</span>
                            <hr />
                          </div>
                          <div class="detail-item">
                            <strong>کدانتخاباتی:</strong>
                            <span>{{ findCandidate(cid).id*1404 }}</span>
                          </div>
                          <div class="detail-item">
                            <strong>تولد:</strong>
                            <span>{{ findCandidate(cid).persian_birth_date }}</span>
                          </div>
                          <div class="detail-item">
                            <strong>کدکاندید:</strong>
                            <span>{{ findCandidate(cid).tracking_code }}</span>
                          </div>
                          <div class="detail-item">
                            <strong>پرسنلی:</strong>
                            <span>{{ findCandidate(cid).personnel_code }}</span>
                          </div>
                          <div class="detail-item">
                            <strong>منطقه:</strong>
                            <span>{{ findCandidate(cid).regionName }}</span>
                          </div>
                        </div>
                      </div>
                    </b-col>
                  </b-row>
                </div>

                <!-- چک تأیید -->
                <div class="confirmation-check">
                  <b-form-checkbox v-model="confirmation.accepted" name="confirmation-check" :state="confirmationState">
                    <span class="confirmation-text"> اینجانب پس از
                      مشاهده و بررسی کامل لیست فوق،
                      انتخاب‌های خود را تأیید نموده و از غیرقابل تغییر بودن رأی پس از ثبت نهایی آگاه هستم. </span>
                  </b-form-checkbox>
                  <b-form-invalid-feedback :state="confirmationState"> لطفاً گزینه تأیید را انتخاب کنید
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
                      <span v-for="(cid, i) in selectedCandidate" :key="`${i}a`">{{ cid?.first_name }} {{ cid?.last_name
                      }}<br /></span>
                    </div>
                    <div class="summary-item">
                      <strong>کد کاندیدای انتخاب شده:</strong>
                      <span v-for="(cid, i) in selectedCandidate" :key="`${i}b`">{{ cid?.candidate_id*1404 }}</span>
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
        
          <h5 class="mb-3">
            <b-icon icon="chat-dots-fill" class="ml-1"></b-icon>
            نظر و امتیاز شما
          </h5>

          <!-- امتیاز ستاره -->
          <div class="star-rating mb-3">
            <b-form-group label="امتیاز (ستاره‌ها)" label-for="rating">
              <div class="stars">
                <span v-for="star in 5" :key="star" class="star" :class="{ filled: star <= feedback.rating }"
                  @click="feedback.rating = star">★</span>
              </div>
            </b-form-group>
          </div>

          <!-- متن نظر -->
          <b-form-group label="نظر شما" label-for="feedback-text">
            <b-form-textarea id="feedback-text" v-model="feedback.comment" placeholder="نظر خود را بنویسید..." rows="3"
              max-rows="6"></b-form-textarea>
          </b-form-group>

          <!-- دکمه ارسال -->
          <div class="text-center mt-3">
            <b-button variant="success" @click="submitFeedbackT" :disabled="submittingFeedback">
              <b-spinner v-if="submittingFeedback" small class="ml-1"></b-spinner>
              ارسال نظر
            </b-button>
          </div>
        </b-card>

      </div>
    </b-container>

    <!-- Candidate Preview Modal -->
    <b-modal v-model="showCandidateModal"
      :title="`${previewCandidateData?.first_name} ${previewCandidateData?.last_name} `" size="lg" hide-footer centered
      scrollable>
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
              <b-badge variant="warning" class="mr-2">
                <b-icon icon="award" class="ml-1"></b-icon>
                {{ previewCandidateData.id*1404 }}
              </b-badge>
            </div>
          </b-col>
        </b-row>

        <b-tabs content-class="mt-3">
          <b-tab title="منطقه" active>
            <p class="preview-text">{{ previewCandidateData.regionName }} -
              {{ previewCandidateData.gender==1 ? 'آقا' : 'خانم' }} {{ previewCandidateData?.first_name }}
              {{ previewCandidateData?.last_name }}</p>
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
          <b-button variant="primary" @click="toggleCandidate(previewCandidateData)" :disabled="voteStatus === 'voted'">
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
import { apiUrlrtb, currentUser } from '../../constants/config'
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: "VotingPage",
  data() {
    return {
      isMobile, apiUrlrtb,
      feedback: {
        rating: 0, // امتیاز 1 تا 5
        comment: ''
      },
      submittingFeedback: false,
      // Voting Status
      voteStatus: '', // 'not_voted', 'voted'
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
      selectedCandidates: [],
      maxVotes: null,
      voteSessionToken: null,
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
    if (this.electionStatusAll == 'inactive') {
      this.$router.push('/home');
      return;
    }

    try {
      await this.checkVoteStatus();
      this.startTimer();
      this.startCooldownTimer();

    } catch (e) {
      this.$notify("warning", "هشدار", 'امکان ورود به صندوق رأی وجود ندارد', {
        duration: 6000,
        permanent: false,
      });
      this.$router.push('/home');
    }
  },
  beforeUnmount() {
    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
    }
  },
  methods: {
    ...mapActions(["getCandidsList", "getVote", "insertVote", "createVoteToken","submitFeedback"]),
    async submitFeedbackT() {
      if (!this.feedback.comment && !this.feedback.rating) {
        this.$bvToast.toast('لطفاً امتیاز یا نظر خود را وارد کنید', { variant: 'warning' });
        return;
      }

      this.submittingFeedback = true;
      try {
        // ارسال به سرور
      const ret= await this.submitFeedback({
          rating: this.feedback.rating,
          comment: this.feedback.comment
        });
        if(ret)
        this.$bvToast.toast(ret, { variant: 'success' });
        
      } catch (err) {
        console.error(err);
        this.$bvToast.toast('خطا در ثبت نظر', { variant: 'danger' });
      }
      this.submittingFeedback = false;
    },
    goToConfirmation() {

      if (this.selectedCandidates.length === 0) {
        this.$bvToast.toast('حداقل یک کاندیدا باید انتخاب شود', {
          variant: 'warning'
        });
        return;
      }

      this.nextStep();
    },
    findCandidate(id) {
      return this.candidates.find(c => c.id == id) || {};
    },
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

    startCooldownTimer() {
      this.cooldownInterval = setInterval(() => {
        if (this.cooldown > 0) {
          this.cooldown--;
        }
      }, 1000);
    },

    // Candidate Selection
    previewCandidate(candidate) {
      this.previewCandidateData = candidate;
      this.showCandidateModal = true;
    },
    toggleCandidate(candidate) {

      const id = candidate.id;
      // اگر قبلاً انتخاب شده → حذف
      if (this.selectedCandidates.includes(id)) {
        this.selectedCandidates =
          this.selectedCandidates.filter(c => c !== id);
        this.showCandidateModal = false;
        return;
      }
      // محدودیت تعداد
      if (this.selectedCandidates.length >= this.maxVotes) {
        this.$bvToast.toast(`حداکثر ${this.maxVotes} انتخاب مجاز است`, {
          variant: 'warning'
        });
        this.showCandidateModal = false;
        return;
      }

      this.selectedCandidates.push(id);
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
        // ارسال یک درخواست به جای حلقه
        const response = await this.insertVote({
          vote_token: this.voteSessionToken,
          candidateIds: this.selectedCandidates  // <-- آرایه همه کاندیداها
        });

        if (response.status) {
          // موفقیت
          await this.checkVoteStatus()
          this.voteStatus = 'voted';

          // // کد رهگیری سرور
          // this.voteTrackingCode = response.data.tracking_code;

          // // تاریخ و زمان رأی‌گیری
          // const now = new Date();
          // this.voteDate = now.toLocaleDateString('fa-IR');
          // this.voteTime = now.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });

          // // انتخاب کاندیدا (فقط برای نمایش)
          // this.selectedCandidate = (this.selectedCandidates);

          // // پاک کردن localStorage
          localStorage.removeItem('ballot');

          // رفتن به مرحله موفقیت
          this.currentStep = 3;
          this.progress = 100;


          this.$bvToast.toast('رأی شما با موفقیت ثبت شد', {
            variant: 'success'
          });
        } else {
          this.$bvToast.toast(response.message || 'خطا در ثبت رأی', {
            variant: 'danger'
          });
        }

      } catch (e) {
        console.error(e);
        this.$bvToast.toast('خطا در ثبت رأی', {
          variant: 'danger'
        });
      }

      this.submitting = false;
    },

    // Success Actions
    downloadReceipt() {
      // Generate receipt content
      //  <span  v-for="(cid,i) in selectedCandidate" :key="`${i}a`">{{ cid?.first_name }} {{ cid?.last_name }}<br/></span>
      let na = ''; let ids = '';
      this.selectedCandidate.forEach(element => {
        na += element.first_name + ' ' + element.last_name + '-'
        ids += (element.candidate_id*1404) + '-'
      });

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
        ${na}
        --------------------
        کد: ${ids}
        
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
      const isNumber = (value) => Number.isFinite(value);

      if (!voteData) {
        this.$notify("warning", "هشدار", 'امکان ورود به صندوق رأی وجود ندارد', {
          duration: 6000,
          permanent: false,
        });
        this.$router.push('/home');
      }
      else if (isNumber(voteData)) {
        this.maxVotes = voteData;
        this.voteStatus = 'not_voted';
        const session = await this.createVoteToken();
        this.voteSessionToken = session.vote_token;
        this.candidates = await this.getCandidsList();
      }
      else if (voteData) {
        this.voteStatus = 'voted';
        this.currentStep = 3;
        this.progress = 100;

        this.voteTrackingCode = voteData[0].tracking_code;
        this.voteDate = (voteData[0].date1);
        this.voteTime = (voteData[0].Time1);

        // Find selected candidate
        this.selectedCandidate = voteData;
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
  },
  watch: {
    selectedCandidates: {
      handler(val) {
        localStorage.setItem('ballot', JSON.stringify(val))
      },
      deep: true
    }
  },
  created() {
    const saved = localStorage.getItem('ballot');
    if (saved) this.selectedCandidates = JSON.parse(saved);
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
  border-color: #000;
  background: #4CAF50;
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
  font-size: 0.7rem;
  margin-bottom: 10px;
  min-height: 40px;
}

.candidate-stats {
  font-size: 0.8rem;
  color: #5a5454;
}

.stat-item {
  margin-bottom: 5px;
  display: flex;
  align-items: center;
}

/* Selected Candidate */
.selected-candidate-image {
  padding: 10px;
}

.selected-image {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
  border: 5px solid #4CAF50;
  box-shadow: 0 10px 10px rgba(0, 0, 0, 0.2);
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
.star-rating .stars {
  display: inline-block;
  font-size: 1.5rem;
  cursor: pointer;
}
.star-rating .star {
  color: #ccc;
  margin-right: 4px;
}
.star-rating .star.filled {
  color: gold;
}

</style>