<template>
  <div class="voting-page-modern">
    <!-- فاصله از topbar -->
    <div class="page-spacer"></div>

    <!-- Header با گرادیانت -->
    <div class="voting-header-modern">
      <div class="container-fluid">
        <div class="header-content">
          <div class="header-title">
            <div class="icon-wrapper">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z"/>
                <path d="M22 6L12 13L2 6" stroke-linecap="round"/>
                <path d="M16 16L8 16" stroke-linecap="round"/>
              </svg>
            </div>
            <div>
              <h2>صندوق رأی‌گیری الکترونیکی</h2>
              <p>انتخابات هیأت امنای صندوق ذخیره فرهنگیان</p>
            </div>
          </div>
          <div class="voter-card">
            <div class="voter-avatar">
              <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M20 21V19C20 16.8 18.2 15 16 15H8C5.8 15 4 16.8 4 19V21" stroke-width="1.5"/>
                <circle cx="12" cy="7" r="4" stroke-width="1.5"/>
              </svg>
            </div>
            <div class="voter-details">
              <div class="voter-name">{{ currentUser?.full_name || 'کاربر مهمان' }}</div>
              <div class="voter-id">کد ملی: {{ currentUser?.national_id || '---' }}</div>
              <div class="voter-status">
                <span class="status-badge" :class="voteStatus === 'voted' ? 'voted' : 'ready'">
                  {{ electionStatusAll === 'active' ? (voteStatus === 'voted' ? '✓ رأی داده شده' : '● آماده رأی‌گیری') : 'پایان یافته' }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- هشدار وضعیت انتخابات -->
    <div class="container-fluid mt-3 ">
      <div class="election-alert" :class="electionStatusAll">
        <span class="alert-dot"></span>
        <span class="alert-text">
          {{ electionStatusAll === 'active' ? 'در حال برگزاری انتخابات' : electionStatusAll === 'upcoming' ? 'آغاز به زودی' : 'پایان یافته' }}
        </span>
      </div>
    </div>

    <!-- Stepper مدرن -->
    <div class="container-fluid mt-4" v-if="electionStatusAll === 'active' && voteStatus !== 'voted'">
      <div class="stepper-modern">
        <div class="step-item" :class="{ active: currentStep >= 1, completed: currentStep > 1 }">
          <div class="step-circle">1</div>
          <div class="step-label">انتخاب داوطلب</div>
        </div>
        <div class="step-line" :class="{ active: currentStep > 1 }"></div>
        <div class="step-item" :class="{ active: currentStep >= 2, completed: currentStep > 2 }">
          <div class="step-circle">2</div>
          <div class="step-label">برگ رأی</div>
        </div>
        <div class="step-line" :class="{ active: currentStep > 2 }"></div>
        <div class="step-item" :class="{ active: currentStep >= 3 }">
          <div class="step-circle">3</div>
          <div class="step-label">تأیید نهایی</div>
        </div>
      </div>
    </div>

    <!-- محتوای اصلی -->
    <div class="container-fluid voting-content-modern p-4" v-if="electionStatusAll === 'active' || electionStatusAll === 'ended'">

      <!-- مرحله 1: لیست کاندیداها -->
      <div v-if="currentStep === 1 && electionStatusAll === 'active'" class="step-container">
        <div class="candidates-header">
          <h3>📋 فهرست داوطلبان</h3>
          <p>حوزه انتخابیه: <strong>{{ currentUser?.regionName || 'منطقه نامشخص' }}</strong></p>
          <div class="max-votes-info">
            <span class="info-badge">حداکثر انتخاب: {{ maxVotes }} نفر</span>
          </div>
        </div>

        <!-- فیلتر جستجو -->
        <div class="search-filter-modern">
          <div class="search-box">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <circle cx="11" cy="11" r="8" stroke-width="1.5"/>
              <path d="M21 21L17 17" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
            <input type="text" v-model="searchQuery" placeholder="جستجو در داوطلبان...">
          </div>
          <select v-model="sortBy" class="sort-select">
            <option value="persian_birth_date">مرتب‌سازی بر اساس سن</option>
            <option value="name">مرتب‌سازی بر اساس نام</option>
          </select>
        </div>

        <!-- لیست کاندیداها -->
        <div class="candidates-grid">
          <div v-for="candidate in filteredCandidates" :key="candidate.id" class="candidate-card-modern"
            :class="{ selected: selectedCandidates?.includes(candidate.codeentekhabati) }"
            @click="toggleCandidate(candidate)">
            <div class="candidate-image">
              <img :src="`${apiUrlrtb}/${candidate.user_photo}`" :alt="candidate.first_name">
              <div v-if="selectedCandidates?.includes(candidate.codeentekhabati)" class="check-mark">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="white">
                  <path d="M20 6L9 17L4 12" stroke-width="2" stroke-linecap="round"/>
                </svg>
              </div>
            </div>
            <div class="candidate-info">
              <h4>{{ candidate.first_name }} {{ candidate.last_name }}</h4>
              <p class="position">{{ candidate.org_position_desc }}</p>
              <div class="candidate-meta">
                <span>کد انتخاباتی: {{ candidate.codeentekhabati }}</span>
              </div>
            </div>
            <button class="detail-btn" @click.stop="previewCandidate(candidate)">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <circle cx="12" cy="12" r="3" stroke-width="1.5"/>
                <path d="M1 12C1 12 5 4 12 4C19 4 23 12 23 12C23 12 19 20 12 20C5 20 1 12 1 12Z" stroke-width="1.5"/>
              </svg>
            </button>
          </div>
        </div>

        <div class="action-buttons-step">
          <button class="btn-next" :disabled="selectedCandidates?.length === 0" @click="goToConfirmation">
            ادامه به برگ رأی
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M5 12H19M19 12L12 5M19 12L12 19" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
          </button>
        </div>
      </div>

      <!-- مرحله 2: برگ رأی (تأیید نهایی) -->
      <div v-else-if="currentStep === 2" class="step-container">
        <div class="ballot-paper">
          <div class="ballot-header">
            <h3>📄 برگ رأی انتخابات</h3>
            <p>هیأت امنای موسسه صندوق ذخیره فرهنگیان</p>
          </div>
          
          <div class="ballot-body">
            <div class="ballot-info">
              <div class="info-line">
                <span>نام رأی‌دهنده:</span>
                <strong>{{ currentUser?.full_name }}</strong>
              </div>
              <div class="info-line">
                <span>کد ملی:</span>
                <strong>{{ currentUser?.national_id }}</strong>
              </div>
              <div class="info-line">
                <span>حوزه انتخابیه:</span>
                <strong>{{ currentUser?.regionName }}</strong>
              </div>
              <div class="info-line">
                <span>تاریخ رأی‌گیری:</span>
                <strong>{{ getCurrentDate() }}</strong>
              </div>
            </div>

            <div class="ballot-separator"></div>

            <div class="selected-candidates-list">
              <h4>داوطلبان انتخاب شده</h4>
              <div class="candidates-ballot">
                <div v-for="cid in selectedCandidates" :key="cid" class="ballot-candidate-item">
                  <div class="ballot-candidate-number">{{ selectedCandidates.indexOf(cid) + 1 }}</div>
                  <div class="ballot-candidate-info">
                    <img :src="`${apiUrlrtb}/${findCandidate(cid).user_photo}`" :alt="findCandidate(cid).first_name">
                    <div>
                      <div class="ballot-candidate-name">
                        {{ findCandidate(cid).gender == 1 ? 'آقای' : 'خانم' }} {{ findCandidate(cid).first_name }} {{ findCandidate(cid).last_name }}
                      </div>
                      <div class="ballot-candidate-code">کد انتخاباتی: {{ findCandidate(cid).codeentekhabati }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="ballot-separator"></div>

            <div class="confirmation-section">
              <label class="checkbox-container">
                <input type="checkbox" v-model="confirmation.accepted">
                <span class="checkmark"></span>
                <span class="confirmation-text">
                  اینجانب پس از مشاهده و بررسی کامل لیست فوق، انتخاب‌های خود را تأیید نموده و از غیرقابل تغییر بودن رأی پس از ثبت نهایی آگاه هستم.
                </span>
              </label>
            </div>
          </div>

          <div class="ballot-footer">
            <button class="btn-back" @click="prevStep">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M19 12H5M5 12L12 5M5 12L12 19" stroke-width="1.5" stroke-linecap="round"/>
              </svg>
              بازگشت و تغییر انتخاب
            </button>
            <button class="btn-submit-ballot" :disabled="!confirmation.accepted || submitting" @click="submitVote">
              <svg v-if="!submitting" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M20 6L9 17L4 12" stroke-width="2" stroke-linecap="round"/>
              </svg>
              <span v-else class="spinner-small"></span>
              ثبت رأی نهایی
            </button>
          </div>
        </div>
      </div>

      <!-- مرحله 3: موفقیت -->
      <div v-else-if="currentStep === 3" class="step-container">
        <div class="success-card-modern">
          <div class="success-animation">
            <div class="success-checkmark">
              <svg viewBox="0 0 52 52">
                <circle class="checkmark-circle" cx="26" cy="26" r="25" fill="none"/>
                <path class="checkmark-check" fill="none" d="M14 27L23 36L38 15"/>
              </svg>
            </div>
          </div>
          
          <h2>رأی شما با موفقیت ثبت شد!</h2>
          <p>از مشارکت شما در این انتخابات سپاسگزاریم.</p>

          <div class="vote-receipt">
            <div class="receipt-header">
              <span>رسید رأی‌گیری الکترونیکی</span>
            </div>
            <div class="receipt-body">
              <div class="receipt-row">
                <span>شماره پیگیری:</span>
                <strong class="tracking-code">{{ voteTrackingCode }}</strong>
              </div>
              <div class="receipt-row">
                <span>تاریخ ثبت:</span>
                <span>{{ voteDate }} - {{ voteTime }}</span>
              </div>
              <div class="receipt-row">
                <span>داوطلبان انتخاب شده:</span>
                <span>{{ selectedCandidate?.map(c => c.first_name + ' ' + c.last_name).join(' - ') }}</span>
              </div>
            </div>
          </div>

          <div class="success-actions">
            <button class="btn-outline" @click="printReceipt">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M6 9V2H18V9" stroke-width="1.5" stroke-linecap="round"/>
                <path d="M6 18H4C3 18 2 17 2 16V11C2 10 3 9 4 9H20C21 9 22 10 22 11V16C22 17 21 18 20 18H18" stroke-width="1.5"/>
                <rect x="6" y="14" width="12" height="8" rx="1" stroke-width="1.5"/>
              </svg>
              چاپ رسید
            </button>
            <button class="btn-outline" @click="downloadReceipt">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M12 3V16M12 16L9 13M12 16L15 13" stroke-width="1.5" stroke-linecap="round"/>
                <path d="M5 21H19" stroke-width="1.5" stroke-linecap="round"/>
              </svg>
              دریافت رسید
            </button>
            <button class="btn-primary" @click="goToHome">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M3 9L12 3L21 9V20H15V14H9V20H3V9Z" stroke-width="1.5" stroke-linejoin="round"/>
              </svg>
              بازگشت به صفحه اصلی
            </button>
          </div>

          <!-- بخش نظر و امتیاز -->
          <div class="feedback-section">
            <h4>نظر و امتیاز شما</h4>
            <div class="star-rating-modern">
              <span v-for="star in 5" :key="star" class="star" :class="{ filled: star <= feedback.rating }"
                @click="feedback.rating = star">★</span>
            </div>
            <textarea v-model="feedback.comment" placeholder="نظر خود را بنویسید..." rows="3"></textarea>
            <button class="btn-feedback" :disabled="submittingFeedback" @click="submitFeedbackT">
              {{ submittingFeedback ? 'در حال ارسال...' : 'ارسال نظر' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- مودال جزئیات کاندیدا -->
    <b-modal v-model="showCandidateModal" hide-footer centered size="lg" class="candidate-modal-modern">
      <template #modal-header>
        <div class="modal-header-custom">
          <h4>{{ previewCandidateData?.first_name }} {{ previewCandidateData?.last_name }}</h4>
          <button type="button" class="close" @click="showCandidateModal = false">×</button>
        </div>
      </template>
      <div v-if="previewCandidateData" class="modal-body-custom">
        <div class="modal-candidate-image">
          <img :src="`${previewCandidateData.user_photo}`" :alt="previewCandidateData.first_name">
        </div>
        <div class="modal-candidate-info">
          <div class="info-row">
            <span>کد انتخاباتی:</span>
            <strong>{{ previewCandidateData.codeentekhabati }}</strong>
          </div>
          <div class="info-row">
            <span>جنسیت:</span>
            <strong>{{ previewCandidateData.gender == 1 ? 'آقا' : 'خانم' }}</strong>
          </div>
          <div class="info-row">
            <span>حوزه انتخابیه:</span>
            <strong>{{ previewCandidateData.regionName }}</strong>
          </div>
          <div class="info-row">
            <span>سمت:</span>
            <strong>{{ previewCandidateData.org_position_desc }}</strong>
          </div>
        </div>
      </div>
      <template #modal-footer>
        <button class="btn-select" :disabled="voteStatus === 'voted'" @click="toggleCandidate(previewCandidateData); showCandidateModal = false">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path d="M20 6L9 17L4 12" stroke-width="2" stroke-linecap="round"/>
          </svg>
          {{ selectedCandidates?.includes(previewCandidateData.codeentekhabati) ? 'حذف از لیست' : 'انتخاب این داوطلب' }}
        </button>
      </template>
    </b-modal>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapActions } from "vuex";
import { apiUrlrtb } from '../../constants/config';
export default {
  name: "VotingPage",
  data() {
    return {
      isMobile,apiUrlrtb,
      feedback: { rating: 0, comment: '' },
      submittingFeedback: false,
      voteStatus: '',
      currentStep: 1,
      progress: 25,
      searchQuery: '',
      sortBy: 'persian_birth_date',
      candidates: [],
      selectedCandidates: [],
      maxVotes: null,
      voteSessionToken: null,
      previewCandidateData: null,
      showCandidateModal: false,
      confirmation: { accepted: false },
      submitting: false,
      voteTrackingCode: '',
      voteDate: '',
      voteTime: '',
      selectedCandidate: []
    };
  },
  computed: {
    ...mapGetters(["currentUser", "electionStatusAll"]),
    filteredCandidates() {
      let filtered = [...this.candidates];
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        filtered = filtered.filter(c => 
          (c.first_name + ' ' + c.last_name).toLowerCase().includes(query) ||
          c.codeentekhabati?.toString().includes(query)
        );
      }
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
    } catch (e) {
      this.$notify("warning", "هشدار", 'امکان ورود به صندوق رأی وجود ندارد', { duration: 6000 });
      this.$router.push('/home');
    }
  },
  methods: {
    ...mapActions(["getCandidsList", "getVote", "insertVote", "createVoteToken", "submitFeedback"]),
    
    async submitFeedbackT() {
      if (!this.feedback.comment && !this.feedback.rating) {
        this.$bvToast.toast('لطفاً امتیاز یا نظر خود را وارد کنید', { variant: 'warning' });
        return;
      }
      this.submittingFeedback = true;
      try {
        const ret = await this.submitFeedback({
          rating: this.feedback.rating,
          comment: this.feedback.comment
        });
        if (ret) this.$bvToast.toast(ret, { variant: 'success' });
      } catch (err) {
        this.$bvToast.toast('خطا در ثبت نظر', { variant: 'danger' });
      }
      this.submittingFeedback = false;
    },

    goToConfirmation() {
      if (this.selectedCandidates.length === 0) {
        this.$bvToast.toast('حداقل یک داوطلب باید انتخاب شود', { variant: 'warning' });
        return;
      }
      this.nextStep();
    },

    findCandidate(id) {
      return this.candidates.find(c => c.codeentekhabati == id) || {};
    },

    nextStep() {
      if (this.currentStep < 3) {
        this.currentStep++;
      }
    },

    prevStep() {
      if (this.currentStep > 1) {
        this.currentStep--;
      }
    },

    previewCandidate(candidate) {
      this.previewCandidateData = candidate;
      this.showCandidateModal = true;
    },

    toggleCandidate(candidate) {
      const id = candidate.codeentekhabati;
      if (this.selectedCandidates.includes(id)) {
        this.selectedCandidates = this.selectedCandidates.filter(c => c !== id);
        return;
      }
      if (this.selectedCandidates.length >= this.maxVotes) {
        this.$bvToast.toast(`حداکثر ${this.maxVotes} انتخاب مجاز است`, { variant: 'warning' });
        return;
      }
      this.selectedCandidates.push(id);
      this.$bvToast.toast(`داوطلب ${candidate.first_name} ${candidate.last_name} انتخاب شد`, {
        title: 'انتخاب شد',
        variant: 'success'
      });
    },

    async submitVote() {
      if (!this.confirmation.accepted) return;
      this.submitting = true;
      try {
        const response = await this.insertVote({
          vote_token: this.voteSessionToken,
          candidateIds: this.selectedCandidates
        });
        if (response.status) {
          await this.checkVoteStatus();
          this.voteStatus = 'voted';
          this.currentStep = 3;
          this.$bvToast.toast('رأی شما با موفقیت ثبت شد', { variant: 'success' });
        } else {
          this.$bvToast.toast(response.message || 'خطا در ثبت رأی', { variant: 'danger' });
        }
      } catch (e) {
        this.$bvToast.toast('خطا در ثبت رأی', { variant: 'danger' });
      }
      this.submitting = false;
    },

    downloadReceipt() {
      let na = '', ids = '';
      this.selectedCandidate.forEach(element => {
        na += element.first_name + ' ' + element.last_name + '-';
        ids += (element.codeentekhabati) + '-';
      });
      const receiptContent = `
رسید رأی‌گیری الکترونیکی
=========================
شماره پیگیری: ${this.voteTrackingCode}
تاریخ: ${this.voteDate}
ساعت: ${this.voteTime}

اطلاعات رأی‌دهنده:
نام: ${this.currentUser.full_name}
کد ملی: ${this.currentUser.national_id}

داوطلبان انتخاب شده:
${na}
کدها: ${ids}

این سند به عنوان رسید رسمی رأی‌گیری محسوب می‌شود.
تاریخ چاپ: ${new Date().toLocaleDateString('fa-IR')}
      `;
      const blob = new Blob([receiptContent], { type: 'text/plain' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `vote_receipt_${this.voteTrackingCode}.txt`;
      a.click();
      window.URL.revokeObjectURL(url);
      this.$bvToast.toast('رسید رأی‌گیری دانلود شد', { variant: 'success' });
    },

    printReceipt() {
      const candidates = this.selectedCandidate || [];
      const candidateRows = candidates.map((c, i) =>
        `<tr>
          <td>${i + 1}</td>
          <td>${c.first_name} ${c.last_name}</td>
          <td>${c.codeentekhabati || '-'}</td>
          <td>${c.org_position_desc || '-'}</td>
        </tr>`
      ).join('');

      const html = `<!DOCTYPE html>
<html dir="rtl" lang="fa">
<head>
  <meta charset="UTF-8">
  <title>تعرفه رأی - رسید رأی‌گیری</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: 'Tahoma', 'Arial', sans-serif; background: #fff; color: #000; direction: rtl; }
    .page { width: 148mm; min-height: 210mm; margin: 0 auto; padding: 8mm; border: 2px solid #000; }
    .header { text-align: center; border-bottom: 3px double #000; padding-bottom: 6mm; margin-bottom: 5mm; }
    .logo-area { display: flex; align-items: center; justify-content: center; gap: 10px; margin-bottom: 3mm; }
    .logo-box { width: 18mm; height: 18mm; border: 2px solid #000; display: flex; align-items: center; justify-content: center; font-size: 22px; }
    .title-area h1 { font-size: 14pt; font-weight: bold; }
    .title-area h2 { font-size: 11pt; }
    .title-area h3 { font-size: 9pt; color: #333; }
    .ballot-label { background: #000; color: #fff; text-align: center; padding: 2mm 4mm; font-size: 12pt; font-weight: bold; margin: 4mm 0; letter-spacing: 2px; }
    .section { margin-bottom: 4mm; }
    .section-title { font-size: 9pt; font-weight: bold; border-bottom: 1px solid #000; padding-bottom: 1mm; margin-bottom: 2mm; }
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 2mm; font-size: 8.5pt; }
    .info-item { display: flex; gap: 4px; }
    .info-item label { font-weight: bold; white-space: nowrap; }
    table { width: 100%; border-collapse: collapse; font-size: 8pt; margin-top: 2mm; }
    th { background: #000; color: #fff; padding: 2mm; text-align: center; font-size: 8pt; }
    td { border: 1px solid #555; padding: 1.5mm 2mm; text-align: center; }
    tr:nth-child(even) td { background: #f5f5f5; }
    .tracking { background: #f0f0f0; border: 2px solid #000; text-align: center; padding: 3mm; margin: 4mm 0; }
    .tracking-label { font-size: 8pt; }
    .tracking-code { font-size: 16pt; font-weight: bold; font-family: monospace; letter-spacing: 3px; }
    .footer { border-top: 2px solid #000; padding-top: 3mm; margin-top: 4mm; font-size: 7.5pt; text-align: center; color: #333; }
    .stamp-area { display: flex; justify-content: space-between; margin-top: 5mm; font-size: 8pt; }
    .stamp-box { border: 1px solid #000; width: 35mm; height: 15mm; text-align: center; padding-top: 1mm; }
    .official-mark { border: 3px double #000; display: inline-block; padding: 1mm 3mm; font-size: 8pt; font-weight: bold; margin: 2mm 0; }
    @media print {
      body { margin: 0; }
      .page { border: 2px solid #000; margin: 0; width: 148mm; }
      @page { size: A5; margin: 5mm; }
    }
  </style>
</head>
<body>
<div class="page">
  <div class="header">
    <div class="logo-area">
      <div class="logo-box">🗳️</div>
      <div class="title-area">
        <h1>صندوق ذخیره فرهنگیان</h1>
        <h2>رسید رسمی رأی‌گیری الکترونیکی</h2>
        <h3>انتخابات هیأت امنا</h3>
      </div>
    </div>
    <div class="official-mark">سند رسمی - تعرفه رأی</div>
  </div>

  <div class="ballot-label">◀ تعرفه رأی‌گیری ▶</div>

  <div class="section">
    <div class="section-title">اطلاعات رأی‌دهنده</div>
    <div class="info-grid">
      <div class="info-item"><label>نام و نام خانوادگی:</label> <span>${this.currentUser?.full_name || '-'}</span></div>
      <div class="info-item"><label>کد ملی:</label> <span>${this.currentUser?.national_id || '-'}</span></div>
      <div class="info-item"><label>حوزه انتخابیه:</label> <span>${this.currentUser?.regionName || '-'}</span></div>
      <div class="info-item"><label>تاریخ رأی:</label> <span>${this.voteDate} - ${this.voteTime}</span></div>
    </div>
  </div>

  <div class="tracking">
    <div class="tracking-label">شماره پیگیری رأی</div>
    <div class="tracking-code">${this.voteTrackingCode}</div>
  </div>

  <div class="section">
    <div class="section-title">داوطلبان انتخاب شده (${candidates.length} نفر)</div>
    <table>
      <thead>
        <tr><th>#</th><th>نام داوطلب</th><th>کد انتخاباتی</th><th>سمت</th></tr>
      </thead>
      <tbody>${candidateRows}</tbody>
    </table>
  </div>

  <div class="stamp-area">
    <div class="stamp-box">مهر صندوق</div>
    <div style="text-align:center;font-size:7.5pt;color:#555;">
      این سند به عنوان رسید رسمی<br>رأی‌گیری الکترونیکی معتبر است
    </div>
    <div class="stamp-box">امضای ناظر</div>
  </div>

  <div class="footer">
    <p>تاریخ چاپ: ${new Date().toLocaleDateString('fa-IR')} | این رسید را نزد خود نگهدارید</p>
    <p>سامانه رأی‌گیری الکترونیکی صندوق ذخیره فرهنگیان</p>
  </div>
</div>
<script>window.onload = function(){ window.print(); }<\/script>
</body>
</html>`;

      const win = window.open('', '_blank', 'width=600,height=800');
      win.document.write(html);
      win.document.close();
    },

    goToHome() {
      this.$router.push('/');
    },

    async checkVoteStatus() {
      const voteData = await this.getVote();
      const isNumber = (value) => Number.isFinite(value);
      if (!voteData) {
        this.$router.push('/home');
      } else if (isNumber(voteData)) {
        this.maxVotes = voteData;
        this.voteStatus = 'not_voted';
        const session = await this.createVoteToken();
        this.voteSessionToken = session.vote_token;
        this.candidates = await this.getCandidsList();
      } else if (voteData) {
        this.voteStatus = 'voted';
        this.currentStep = 3;
        this.voteTrackingCode = voteData[0]?.tracking_code;
        this.voteDate = voteData[0]?.date1;
        this.voteTime = voteData[0]?.Time1;
        this.selectedCandidate = voteData;
      }
    },

    getCurrentDate() {
      return new Date().toLocaleDateString('fa-IR');
    }
  },
  watch: {
    selectedCandidates: {
      handler(val) {
        localStorage.setItem('ballot', JSON.stringify(val));
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
/* ========== استایل‌های مدرن صفحه رأی‌گیری ========== */


.voting-page-modern {
  background: linear-gradient(135deg, #f5f7ff 0%, #eef2fa 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* ========== هدر ========== */
.voting-header-modern {
  background: linear-gradient(135deg, #1e2a6e, #2b3b8a, #1e2a6e);
  padding: 24px 0;
  color: white;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 20px;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-title .icon-wrapper {
  width: 56px;
  height: 56px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-title h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.header-title p {
  margin: 0;
  opacity: 0.8;
  font-size: 0.85rem;
}

/* کارت رأی‌دهنده */
.voter-card {
  display: flex;
  align-items: center;
  gap: 12px;
  background: rgba(255, 255, 255, 0.12);
  padding: 12px 20px;
  border-radius: 40px;
  backdrop-filter: blur(8px);
}

.voter-avatar {
  width: 44px;
  height: 44px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.voter-details {
  text-align: left;
}

.voter-name {
  font-weight: 700;
  font-size: 0.9rem;
}

.voter-id {
  font-size: 0.7rem;
  opacity: 0.8;
}

.status-badge {
  display: inline-block;
  font-size: 0.7rem;
  padding: 2px 8px;
  border-radius: 20px;
}

.status-badge.ready {
  background: #f59e0b;
  color: white;
}

.status-badge.voted {
  background: #10b981;
  color: white;
}

/* ========== هشدار وضعیت ========== */
.election-alert {
  max-width: 1200px;
  margin: 0 auto;
  padding: 12px 24px;
  border-radius: 60px;
  display: inline-flex;
  align-items: center;
  gap: 12px;
}

.election-alert.active {
  background: #dcfce7;
  color: #166534;
}

.election-alert.upcoming {
  background: #fef3c7;
  color: #92400e;
}

.alert-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

/* ========== Stepper مدرن ========== */
.stepper-modern {
  max-width: 600px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: white;
  padding: 20px 30px;
  border-radius: 60px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.step-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.step-circle {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #e2e8f0;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  transition: all 0.3s;
}

.step-item.active .step-circle {
  background: #3f51b5;
  color: white;
  box-shadow: 0 0 0 4px rgba(63, 81, 181, 0.2);
}

.step-item.completed .step-circle {
  background: #10b981;
  color: white;
}

.step-label {
  font-size: 0.7rem;
  color: #64748b;
}

.step-item.active .step-label {
  color: #3f51b5;
  font-weight: 600;
}

.step-line {
  width: 80px;
  height: 2px;
  background: #e2e8f0;
}

.step-line.active {
  background: #10b981;
}

/* ========== لیست کاندیداها ========== */
.candidates-header {
  text-align: center;
  margin-bottom: 30px;
}

.candidates-header h3 {
  color: #1e293b;
  margin-bottom: 8px;
}

.max-votes-info {
  margin-top: 12px;
}

.info-badge {
  display: inline-block;
  background: #e0e7ff;
  color: #3f51b5;
  padding: 6px 16px;
  border-radius: 40px;
  font-size: 0.8rem;
  font-weight: 600;
}

/* فیلتر جستجو */
.search-filter-modern {
  display: flex;
  gap: 16px;
  margin-bottom: 30px;
  flex-wrap: wrap;
}

.search-box {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 10px;
  background: white;
  padding: 12px 18px;
  border-radius: 50px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.search-box input {
  flex: 1;
  border: none;
  outline: none;
  font-size: 0.9rem;
  background: transparent;
}

.sort-select {
  padding: 12px 20px;
  border-radius: 50px;
  border: 1px solid #e2e8f0;
  background: white;
  font-size: 0.85rem;
  outline: none;
}

/* گرید کاندیداها */
.candidates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.candidate-card-modern {
  background: white;
  border-radius: 24px;
  padding: 20px;
  display: flex;
  gap: 16px;
  align-items: center;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  border: 2px solid transparent;
}

.candidate-card-modern:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
}

.candidate-card-modern.selected {
  border-color: #10b981;
  background: #f0fdf4;
}

.candidate-image {
  position: relative;
  width: 70px;
  height: 70px;
  flex-shrink: 0;
}

.candidate-image img {
  width: 100%;
  height: 100%;
  border-radius: 20px;
  object-fit: cover;
}

.check-mark {
  position: absolute;
  bottom: -4px;
  left: -4px;
  width: 24px;
  height: 24px;
  background: #10b981;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.candidate-info {
  flex: 1;
}

.candidate-info h4 {
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 4px;
}

.candidate-info .position {
  font-size: 0.7rem;
  color: #64748b;
  margin-bottom: 6px;
}

.candidate-meta {
  font-size: 0.7rem;
  color: #94a3b8;
}

.detail-btn {
  width: 36px;
  height: 36px;
  border-radius: 12px;
  background: #f1f5f9;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.detail-btn:hover {
  background: #e2e8f0;
}

/* ========== برگ رأی (Ballot Paper) ========== */
.ballot-paper {
  max-width: 700px;
  margin: 0 auto;
  background: white;
  border-radius: 32px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.ballot-header {
  background: linear-gradient(135deg, #1e2a6e, #2b3b8a);
  color: white;
  padding: 24px;
  text-align: center;
}

.ballot-header h3 {
  margin: 0 0 6px;
}

.ballot-header p {
  margin: 0;
  opacity: 0.8;
  font-size: 0.85rem;
}

.ballot-body {
  padding: 28px;
}

.ballot-info {
  background: #f8fafc;
  padding: 16px;
  border-radius: 20px;
}

.info-line {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px dashed #e2e8f0;
}

.info-line:last-child {
  border-bottom: none;
}

.info-line span {
  color: #64748b;
  font-size: 0.85rem;
}

.ballot-separator {
  height: 2px;
  background: repeating-linear-gradient(90deg, #cbd5e1, #cbd5e1 10px, transparent 10px, transparent 20px);
  margin: 24px 0;
}

.selected-candidates-list h4 {
  font-size: 1rem;
  margin-bottom: 16px;
  color: #1e293b;
}

.candidates-ballot {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ballot-candidate-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px;
  background: #f8fafc;
  border-radius: 16px;
}

.ballot-candidate-number {
  width: 32px;
  height: 32px;
  background: #e2e8f0;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: #475569;
}

.ballot-candidate-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.ballot-candidate-info img {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  object-fit: cover;
}

.ballot-candidate-name {
  font-weight: 600;
  font-size: 0.9rem;
}

.ballot-candidate-code {
  font-size: 0.7rem;
  color: #64748b;
}

/* چک‌باکس سفارشی */
.checkbox-container {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  cursor: pointer;
  font-size: 0.85rem;
  line-height: 1.5;
}

.checkbox-container input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
}

.checkmark {
  width: 20px;
  height: 20px;
  background: #e2e8f0;
  border-radius: 6px;
  display: inline-block;
  position: relative;
  flex-shrink: 0;
}

.checkbox-container input:checked ~ .checkmark {
  background: #10b981;
}

.checkbox-container input:checked ~ .checkmark:after {
  content: '';
  position: absolute;
  left: 7px;
  top: 3px;
  width: 5px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.confirmation-text {
  color: #475569;
}

.ballot-footer {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 20px 28px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.btn-back, .btn-submit-ballot, .btn-next {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 28px;
  border-radius: 40px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-back {
  background: white;
  border: 1px solid #cbd5e1;
  color: #475569;
}

.btn-back:hover {
  background: #f1f5f9;
}

.btn-submit-ballot, .btn-next {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
}

.btn-submit-ballot:hover, .btn-next:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 14px rgba(16, 185, 129, 0.3);
}

.btn-submit-ballot:disabled, .btn-next:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

/* ========== کارت موفقیت ========== */
.success-card-modern {
  max-width: 600px;
  margin: 0 auto;
  background: white;
  border-radius: 32px;
  padding: 40px;
  text-align: center;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

.success-animation {
  margin-bottom: 24px;
}

.success-checkmark svg {
  width: 80px;
  height: 80px;
}

.checkmark-circle {
  stroke: #10b981;
  stroke-width: 3;
  stroke-dasharray: 166;
  stroke-dashoffset: 166;
  animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}

.checkmark-check {
  stroke: #10b981;
  stroke-width: 3;
  stroke-dasharray: 48;
  stroke-dashoffset: 48;
  animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.5s forwards;
}

@keyframes stroke {
  100% { stroke-dashoffset: 0; }
}

.success-card-modern h2 {
  color: #1e293b;
  margin-bottom: 8px;
}

.success-card-modern > p {
  color: #64748b;
  margin-bottom: 24px;
}

.vote-receipt {
  background: #f8fafc;
  border-radius: 20px;
  padding: 20px;
  margin: 24px 0;
  text-align: right;
  border: 1px solid #e2e8f0;
}

.receipt-header {
  font-weight: 700;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
  margin-bottom: 12px;
}

.receipt-row {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  font-size: 0.85rem;
}

.tracking-code {
  font-family: monospace;
  font-size: 1rem;
  color: #3f51b5;
}

.success-actions {
  display: flex;
  gap: 16px;
  justify-content: center;
  flex-wrap: wrap;
}

.btn-outline, .btn-primary {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 24px;
  border-radius: 40px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-outline {
  background: white;
  border: 1px solid #cbd5e1;
  color: #475569;
}

.btn-outline:hover {
  background: #f1f5f9;
}

.btn-primary {
  background: #3f51b5;
  color: white;
}

.btn-primary:hover {
  background: #2c3e8f;
  transform: translateY(-2px);
}

/* بخش نظر و امتیاز */
.feedback-section {
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1px solid #e2e8f0;
}

.feedback-section h4 {
  font-size: 1rem;
  margin-bottom: 12px;
}

.star-rating-modern {
  display: flex;
  gap: 8px;
  justify-content: center;
  margin-bottom: 16px;
}

.star-rating-modern .star {
  font-size: 2rem;
  cursor: pointer;
  color: #cbd5e1;
  transition: all 0.2s;
}

.star-rating-modern .star:hover,
.star-rating-modern .star.filled {
  color: #f59e0b;
}

.feedback-section textarea {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  font-size: 0.85rem;
  resize: vertical;
  margin-bottom: 16px;
}

.btn-feedback {
  background: #8b5cf6;
  color: white;
  border: none;
  padding: 10px 28px;
  border-radius: 40px;
  cursor: pointer;
}

/* مودال */
.candidate-modal-modern ::v-deep .modal-content {
  border-radius: 32px;
  overflow: hidden;
}

.modal-header-custom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  background: linear-gradient(135deg, #1e2a6e, #2b3b8a);
  color: white;
}

.modal-header-custom button {
  background: none;
  border: none;
  font-size: 28px;
  color: white;
  cursor: pointer;
}

.modal-body-custom {
  padding: 24px;
  display: flex;
  gap: 24px;
  flex-wrap: wrap;
}

.modal-candidate-image img {
  width: 120px;
  height: 120px;
  border-radius: 24px;
  object-fit: cover;
}

.modal-candidate-info {
  flex: 1;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid #e2e8f0;
}

.btn-select {
  width: 100%;
  padding: 14px;
  background: #3f51b5;
  color: white;
  border: none;
  border-radius: 40px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

/* دکمه اقدام مرحله */
.action-buttons-step {
  text-align: center;
  margin-top: 20px;
}

.spinner-small {
  width: 16px;
  height: 16px;
  border: 2px solid white;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* موبایل */
@media (max-width: 768px) {
  .page-spacer { margin-top: 60px; }
  
  .header-content { flex-direction: column; text-align: center; }
  
  .stepper-modern { padding: 15px 20px; }
  .step-label { display: none; }
  .step-line { width: 40px; }
  
  .ballot-body { padding: 20px; }
  .ballot-footer { flex-direction: column; }
  .btn-back, .btn-submit-ballot { justify-content: center; }
  
  .success-card-modern { padding: 24px; margin: 0 16px; }
  
  .candidates-grid { grid-template-columns: 1fr; }
}
</style>