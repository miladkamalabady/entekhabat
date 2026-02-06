<template>
  <div class="live-election-page">
    <!-- Header with Timer -->
    <b-container fluid class="election-header py-4">
      <b-row class="align-items-center">
        <b-col cols="12" md="8">
          <div class="d-flex align-items-center">
            <div class="live-indicator mr-3">
              <span class="pulse"></span>
              <span class="live-text">پخش زنده</span>
            </div>
            <div>
              <h2 class="mb-1">وضعیت زنده انتخابات</h2>
              <p class="text-muted mb-0">انتخابات صندوق ذخیره فرهنگیان</p>
            </div>
          </div>
        </b-col>
        <b-col cols="12" md="4" class="text-left text-md-right">
          <div class="timer-card">
            <div class="timer-label">زمان باقیمانده تا پایان انتخابات</div>
            <div class="timer">
              <div class="time-unit" v-if="timeRemaining.days > 0">
                <span class="time-value">{{ timeRemaining.days }}</span>
                <span class="time-label">روز</span>
              </div>
              <div class="time-separator" v-if="timeRemaining.days > 0">:</div>
              <div class="time-unit">
                <span class="time-value">{{ timeRemaining.hours }}</span>
                <span class="time-label">ساعت</span>
              </div>
              <div class="time-separator">:</div>
              <div class="time-unit">
                <span class="time-value">{{ timeRemaining.minutes }}</span>
                <span class="time-label">دقیقه</span>
              </div>
              <div class="time-separator">:</div>
              <div class="time-unit">
                <span class="time-value">{{ timeRemaining.seconds }}</span>
                <span class="time-label">ثانیه</span>
              </div>
            </div>
          </div>
        </b-col>
      </b-row>
    </b-container>

    <!-- Main Content -->
    <b-container class="election-container">
      <!-- Quick Stats -->
      <b-row class="mb-4">
        <b-col cols="6" md="4">
          <b-card class="stat-card text-center">
            <div class="stat-icon voters-icon">
              <b-icon icon="people-fill"></b-icon>
            </div>
            <div class="stat-number">{{ formatNumber(infoVote?.totalVoters) }}</div>
            <div class="stat-label">کل واجدین شرایط</div>
            <div class="stat-change text-success">
              <b-icon icon="arrow-up"></b-icon>
              {{ infoVote?.voterParticipation.toFixed(2) }}% مشارکت
            </div>
          </b-card>
        </b-col>

        <b-col cols="6" md="4">
          <b-card class="stat-card text-center">
            <div class="stat-icon vote-icon">
              <b-icon icon="check-circle-fill"></b-icon>
            </div>
            <div class="stat-number">{{ formatNumber(infoVote?.totalVotes) }}</div>
            <div class="stat-label">آرای ثبت شده</div>
            <div class="stat-change">
              <b-icon icon="clock-history"></b-icon>
              آخرین بروزرسانی: {{ lastUpdate }}
            </div>
          </b-card>
        </b-col>

        <b-col cols="6" md="4">
          <b-card class="stat-card text-center">
            <div class="stat-icon candidate-icon">
              <b-icon icon="person-badge-fill"></b-icon>
            </div>
            <div class="stat-number">{{ infoVote?.Candidates }}</div>
            <div class="stat-label">کاندیداها</div>
            <div class="stat-change text-info">
              <b-icon icon="person-plus"></b-icon>
              {{ infoVote?.activeCandidates }} کاندیدای تایید شده
            </div>
          </b-card>
        </b-col>

      </b-row>


      <!-- Main Dashboard -->
      <b-row class="mb-4">
        <!-- Candidates Ranking -->
        <b-col lg="8" class="mb-4">
          <b-card class="ranking-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="mb-0">رتبه‌بندی کاندیداها</h5>
              <div class="ranking-actions">
                <b-button-group size="sm">
                  <b-button :variant="rankingView === 'table' ? 'primary' : 'outline-primary'"
                    @click="rankingView = 'table'">
                    <b-icon icon="table"></b-icon>
                  </b-button>
                  <b-button :variant="rankingView === 'chart' ? 'primary' : 'outline-primary'"
                    @click="rankingView = 'chart'">
                    <b-icon icon="bar-chart-fill"></b-icon>
                  </b-button>
                </b-button-group>
              </div>
            </div>

            <!-- Table View -->
            <div v-if="rankingView === 'table'" class="table-responsive">
              <b-table :items="sortedCandidates" :fields="candidateFields" striped hover class="text-right">
                <template #cell(rank)="data">
                  <div class="rank-badge" :class="`rank-${data.index + 1}`">
                    {{ data.index + 1 }}
                  </div>
                </template>

                <template #cell(candidate)="data">
                  <div class="candidate-info">
                    <img v-if="data.item.user_photo" :src="`${apiUrlrtb}/${data.item.user_photo}`"
                      class="candidate-photo" :alt="data.item.name" />
                    <div v-else class="candidate-photo placeholder">
                      <b-icon icon="person-circle"></b-icon>
                    </div>
                    <div class="candidate-details">
                      <strong>{{ data.item.first_name }} {{ data.item.last_name }}</strong>
                      <small class="text-muted d-block">{{ data.item.org_position_desc }}</small>
                    </div>
                  </div>
                </template>

                <template #cell(vote_count)="data">
                  <div class="votes-info">
                    <div class="votes-count">{{ formatNumber(data.item.vote_count) }}</div>
                    <div class="votes-percentage">
                      <b-progress :value="data.item.vote_count" :max="maxVotes" height="4px"
                        class="votes-progress"></b-progress>
                      <small>{{ ((data.item.vote_count / infoVote?.totalVotes) * 100).toFixed(1) }}%</small>
                    </div>
                  </div>
                </template>
              </b-table>
            </div>

            <!-- Chart View -->
            <div v-else class="chart-container">
              <div class="chart-wrapper">
                <canvas ref="votesChart"></canvas>
              </div>
            </div>
          </b-card>
        </b-col>

        <!-- Voting Progress by Region -->
        <b-col lg="4" class="mb-4">
          <b-card class="region-card">
            <h5 class="mb-4">مشارکت بر اساس استان</h5>

            <div class="region-list">
              <div v-for="region in regions" :key="region.id" class="region-item" @click="viewRegionDetails(region)">
                <div class="region-name">{{ region.name }}</div>
                <div class="region-stats">
                  <div class="region-progress">
                    <b-progress :value="region.participation" :max="100" height="6px" class="mb-1"></b-progress>
                    <small class="text-muted">{{ region.participation }}%</small>
                  </div>
                  <div class="region-votes">
                    {{ formatNumber(region.votes) }} رأی
                  </div>
                </div>
              </div>
            </div>

            <div class="text-center mt-3">
              <b-button variant="link" size="sm" @click="showAllRegions">
                مشاهده همه استان‌ها
                <b-icon icon="chevron-left"></b-icon>
              </b-button>
            </div>
          </b-card>
        </b-col>
      </b-row>

    </b-container>

    <!-- Region Details Modal -->
    <b-modal v-model="showRegionModal" :title="selectedRegion ? selectedRegion.name : ''" size="lg" hide-footer centered
      scrollable>
      <div v-if="selectedRegion" class="region-details">
        <b-row class="mb-4">
          <b-col md="6">
            <div class="detail-item">
              <strong>جمعیت واجد شرایط:</strong>
              <span>{{ formatNumber(selectedRegion.eligibleVoters) }} نفر</span>
            </div>
            <div class="detail-item">
              <strong>آرای ثبت شده:</strong>
              <span>{{ formatNumber(selectedRegion.votes) }} رأی</span>
            </div>
            <div class="detail-item">
              <strong>میزان مشارکت:</strong>
              <b-badge :variant="getParticipationVariant(selectedRegion.participation)">
                {{ selectedRegion.participation }}%
              </b-badge>
            </div>
          </b-col>
        </b-row>

        <h6 class="mb-3">توزیع آرا بین کاندیداها</h6>
        <div class="candidates-distribution">
          <div v-for="candidate in selectedRegion.candidates" :key="candidate.id" class="distribution-item">
            <div class="candidate-name">{{ candidate.name }}</div>
            <div class="distribution-bar">
              <div class="bar-fill" :style="{ width: candidate.percentage + '%' }"></div>
            </div>
            <div class="distribution-percentage">{{ candidate.percentage }}%</div>
          </div>
        </div>
      </div>
    </b-modal>

    <!-- Auto-refresh Indicator -->
    <div class="auto-refresh-indicator" v-if="autoRefresh">
      <b-spinner small type="grow" class="ml-2"></b-spinner>
      در حال به‌روزرسانی...
    </div>

    <!-- Refresh Button -->
    <b-button variant="primary" class="refresh-button" @click="manualRefresh" :disabled="refreshing">
      <b-spinner small v-if="refreshing" class="ml-1"></b-spinner>
      <b-icon v-else icon="arrow-clockwise" class="ml-1"></b-icon>
      بروزرسانی
    </b-button>
  </div>
</template>

<script>
import Chart from "chart.js";
import { apiUrlrtb } from '../../constants/config'
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: "LiveElectionDashboard",
  data() {
    return {
      apiUrlrtb,
      infoVote: null,
      timeRemaining: {
        days: 0,
        hours: 0,
        minutes: 0,
        seconds: 0
      },

      // Statistics
      lastUpdate: 'لحظاتی پیش',

      // Candidates Data
      candidates: [],

      // Regions Data
      regions: [
        { id: 1, name: 'تهران', votes: 25480, participation: 72.5, eligibleVoters: 35100, activeLocations: 45, topCandidate: 'دکتر محمدرضا احمدی', growth: 2.1 },
        { id: 2, name: 'مشهد', votes: 12450, participation: 65.3, eligibleVoters: 19050, activeLocations: 28, topCandidate: 'مهندس سید علی حسینی', growth: 1.8 },
        { id: 3, name: 'اصفهان', votes: 9870, participation: 61.2, eligibleVoters: 16100, activeLocations: 22, topCandidate: 'دکتر فاطمه کریمی', growth: 0.9 },
        { id: 4, name: 'شیراز', votes: 7650, participation: 58.7, eligibleVoters: 13020, activeLocations: 18, topCandidate: 'دکتر محمدرضا احمدی', growth: 2.5 },
        { id: 5, name: 'تبریز', votes: 6540, participation: 55.4, eligibleVoters: 11800, activeLocations: 16, topCandidate: 'مهندس سید علی حسینی', growth: 1.2 }
      ],


      // UI State
      rankingView: 'table',
      showRegionModal: false,
      selectedRegion: null,
      autoRefresh: true,
      refreshing: false,

      // Chart Instances
      votesChart: null,
      realTimeChart: null,

      // Table Fields
      candidateFields: [
        { key: 'rank', label: 'رتبه', sortable: false },
        { key: 'candidate', label: 'کاندیدا', sortable: false },
        { key: 'vote_count', label: 'آرا', sortable: true },
      ],

      // Predictions
      finalParticipationPrediction: 68
    };
  },
  computed: {
    ...mapGetters(["ConfigInfo", "currentUser"]),
    sortedCandidates() {
      return [...this.candidates].sort((a, b) => b.vote_count - a.vote_count);
    },

    maxVotes() {
      if (!this.candidates.length) return 1;
      return Math.max(...this.candidates.map(c => c.vote_count));
    }
  },
  async mounted() {
    if (!this.ConfigInfo)
      await this.getConfig()
    this.startTimer();
    this.startAutoRefresh();

    this.infoVote = await this.getInfoVote()
    this.candidates = this.infoVote?.listCan

    this.$nextTick(() => {
      if (this.rankingView === 'chart') {
        this.createVotesChart();
      }
    });
  },
  beforeDestroy() {
    clearInterval(this.timerInterval);
    clearInterval(this.refreshInterval);
    if (this.votesChart) this.votesChart.destroy();
    if (this.realTimeChart) this.realTimeChart.destroy();
  }, watch: {
    rankingView(val) {
      if (val === 'chart') {
        this.$nextTick(() => {
          this.createVotesChart();
        });
      }
    }
  },
  methods: {
    ...mapActions(["getConfig", "getInfoVote"]),
    // Timer Functions
    startTimer() {
      this.updateTimer();
      this.timerInterval = setInterval(this.updateTimer, 1000);
    },

    updateTimer() {
      const now = new Date();
      const startDate = new Date(this.ConfigInfo?.startDate);
      const endDate = new Date(this.ConfigInfo?.EndDate);

      const diff = endDate - now;
      if (diff <= 0) {
        this.timeRemaining = { days: 0, hours: 0, minutes: 0, seconds: 0 };
        clearInterval(this.timerInterval);
        return;
      }

      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);

      this.timeRemaining = {
        days: days.toString().padStart(2, '0'),
        hours: hours.toString().padStart(2, '0'),
        minutes: minutes.toString().padStart(2, '0'),
        seconds: seconds.toString().padStart(2, '0')
      };
    },
    createVotesChart() {
      if (!this.$refs.votesChart) return;

      const ctx = this.$refs.votesChart.getContext('2d');

      if (!ctx || !this.sortedCandidates.length) return;

      if (this.votesChart) {
        this.votesChart.destroy();
        this.votesChart = null;
      }

      const labels = this.sortedCandidates.map(c => c.first_name + ' ' + c.last_name);
      const data = this.sortedCandidates.map(c => Number(c.vote_count));

      const colors = this.sortedCandidates.map(() =>
        `hsl(${Math.random() * 360},70%,60%)`
      );

      this.votesChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels,
          datasets: [{
            label: 'تعداد آرا',
            data,
            backgroundColor: colors,
            borderWidth: 0,
            borderRadius: 8,
            barThickness: 28
          }]
        },
        options: {
          animation: {
            duration: 700
          },
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false }
          },
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                precision: 0
              }
            },
            x: {
              ticks: {
                autoSkip: false,
                maxRotation: 45,
                minRotation: 45
              }
            }
          }
        }
      });
    },

    // Data Functions
    formatNumber(num) {
      return new Intl.NumberFormat('fa-IR').format(num);
    },


    getSentimentBadge(sentiment) {
      const variants = {
        positive: 'success',
        neutral: 'info',
        negative: 'danger'
      };
      return variants[sentiment] || 'secondary';
    },

    getSentimentText(sentiment) {
      const texts = {
        positive: 'مثبت',
        neutral: 'خنثی',
        negative: 'منفی'
      };
      return texts[sentiment] || sentiment;
    },

    getParticipationVariant(rate) {
      if (rate >= 70) return 'success';
      if (rate >= 50) return 'info';
      if (rate >= 30) return 'warning';
      return 'danger';
    },

    // UI Actions
    viewRegionDetails(region) {
      // Add candidates distribution for the region
      this.selectedRegion = {
        ...region,
        candidates: this.candidates.map(c => ({
          id: c.id,
          name: c.name,
          percentage: Math.floor(Math.random() * 30) + 10
        }))
      };
      this.showRegionModal = true;
    },

    showAllRegions() {
      // In real app, navigate to regions page
      alert('صفحه استان‌ها در حال توسعه است');
    },

    showLocationsMap() {
      // In real app, show map with locations
      alert('نقشه شعبه‌ها در حال توسعه است');
    },

    startAutoRefresh() {
      this.refreshInterval = setInterval(async () => {
        if (!this.autoRefresh) return;

        const data = await this.getInfoVote();
        this.infoVote = data;
        this.candidates = data?.listCan || [];

        this.lastUpdate = new Date().toLocaleTimeString('fa-IR');

        if (this.rankingView === 'chart') {
          this.$nextTick(() => this.createVotesChart());
        }
      }, 30000);
    },


    manualRefresh() {
      this.refreshing = true;
      this.getInfoVote();
      setTimeout(() => {
        this.refreshing = false;
      }, 1000);
    }
  }
};
</script>

<style scoped>
.live-election-page {
  background: linear-gradient(135deg, #f8f9fa 0%, #e3f2fd 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* Header */
.election-header {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.live-indicator {
  display: flex;
  align-items: center;
  background: rgba(220, 53, 69, 0.9);
  padding: 8px 16px;
  border-radius: 20px;
  font-weight: bold;
}

.pulse {
  width: 12px;
  height: 12px;
  background: #fff;
  border-radius: 50%;
  margin-left: 8px;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0% {
    opacity: 1;
  }

  50% {
    opacity: 0.5;
  }

  100% {
    opacity: 1;
  }
}

.live-text {
  font-size: 1rem;
}

.timer-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  padding: 15px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.timer-label {
  font-size: 0.9rem;
  opacity: 0.9;
  margin-bottom: 8px;
}

.timer {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
}

.time-unit {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 50px;
}

.time-value {
  font-size: 2rem;
  font-weight: bold;
  line-height: 1;
  background: rgba(255, 255, 255, 0.2);
  padding: 5px 10px;
  border-radius: 8px;
  min-width: 50px;
  text-align: center;
}

.time-label {
  font-size: 0.8rem;
  margin-top: 4px;
  opacity: 0.8;
}

.time-separator {
  font-size: 1.5rem;
  font-weight: bold;
  color: #fff;
  margin: 0 5px;
}

/* Stat Cards */
.stat-card {
  border-radius: 12px;
  border: none;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);
  transition: transform 0.3s ease;
  height: 100%;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 15px;
  font-size: 1.8rem;
}

.voters-icon {
  background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
  color: white;
}

.vote-icon {
  background: linear-gradient(135deg, #4CAF50 0%, #8BC34A 100%);
  color: white;
}

.candidate-icon {
  background: linear-gradient(135deg, #FF9800 0%, #FFC107 100%);
  color: white;
}

.progress-icon {
  background: linear-gradient(135deg, #2196F3 0%, #03A9F4 100%);
  color: white;
}

.stat-number {
  font-size: 2rem;
  font-weight: bold;
  color: #2c3e50;
  line-height: 1;
}

.stat-label {
  color: #666;
  margin: 8px 0;
  font-size: 0.9rem;
}

.stat-change {
  font-size: 0.8rem;
  margin-top: 5px;
}

/* Realtime Updates */
.realtime-card {
  border-radius: 12px;
  border: 1px solid #e0e0e0;
}

.realtime-updates {
  max-height: 300px;
  overflow-y: auto;
}

.update-item {
  padding: 12px 15px;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  align-items: center;
  transition: background 0.2s ease;
}

.update-item:hover {
  background: #f8f9fa;
}

.update-item:last-child {
  border-bottom: none;
}

.update-time {
  background: #f0f0f0;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.8rem;
  color: #666;
  min-width: 70px;
  text-align: center;
  margin-left: 15px;
}

.update-content {
  flex: 1;
  display: flex;
  align-items: center;
}

.update-vote {
  border-right: 3px solid #4CAF50;
}

.update-announcement {
  border-right: 3px solid #2196F3;
}

.update-candidate {
  border-right: 3px solid #FF9800;
}

.update-system {
  border-right: 3px solid #9C27B0;
}

.update-milestone {
  border-right: 3px solid #FFC107;
}

/* Ranking Table */
.ranking-card {
  border-radius: 12px;
  height: 100%;
}

.rank-badge {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  color: white;
}

.rank-1 {
  background: linear-gradient(135deg, #FFD700 0%, #FFC107 100%);
}

.rank-2 {
  background: linear-gradient(135deg, #C0C0C0 0%, #E0E0E0 100%);
  color: #333 !important;
}

.rank-3 {
  background: linear-gradient(135deg, #CD7F32 0%, #D2691E 100%);
}

.rank-4,
.rank-5 {
  background: #f0f0f0;
  color: #666;
}

.candidate-info {
  display: flex;
  align-items: center;
}

.candidate-photo {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  margin-left: 15px;
  object-fit: cover;
  border: 2px solid #e0e0e0;
}

.candidate-photo.placeholder {
  background: #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: #999;
}

.candidate-details {
  flex: 1;
}

.votes-info {
  text-align: left;
}

.votes-count {
  font-weight: bold;
  font-size: 1.1rem;
  margin-bottom: 5px;
}

.votes-progress {
  width: 100px;
  display: inline-block;
  margin-left: 10px;
}


/* Region Card */
.region-card {
  height: 100%;
  border-radius: 12px;
}

.region-list {
  max-height: 400px;
  overflow-y: auto;
}

.region-item {
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
  cursor: pointer;
  transition: background 0.2s ease;
}

.region-item:hover {
  background: #f8f9fa;
  padding-right: 10px;
  border-radius: 6px;
}

.region-item:last-child {
  border-bottom: none;
}

.region-name {
  font-weight: 600;
  margin-bottom: 8px;
}

.region-stats {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.region-progress {
  flex: 1;
  margin-left: 15px;
}

.region-votes {
  font-size: 0.9rem;
  color: #666;
}

/* Chart Containers */
.chart-container {
  height: 300px;
  position: relative;
}

.chart-wrapper {
  height: 280px;
}

.chart-wrapper-large {
  height: 300px;
  position: relative;
}

/* Locations Status */
.locations-status {
  display: flex;
  justify-content: space-around;
  text-align: center;
}

.status-item {
  padding: 15px;
}

.status-indicator {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  margin: 0 auto 10px;
  position: relative;
}

.status-indicator.active {
  background: #4CAF50;
  animation: pulse 2s infinite;
}

.status-indicator.busy {
  background: #FF9800;
}

.status-indicator.offline {
  background: #F44336;
}

.status-count {
  font-size: 1.5rem;
  font-weight: bold;
  color: #2c3e50;
}

.status-label {
  color: #666;
  font-size: 0.9rem;
}

/* Prediction */
.prediction-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.prediction-item:last-child {
  border-bottom: none;
}

.prediction-label {
  color: #666;
}

.prediction-value {
  font-weight: bold;
  color: #2c3e50;
}

/* Social Feed */
.social-feed-card {
  border-radius: 12px;
}

.social-feed {
  max-height: 400px;
  overflow-y: auto;
}

.feed-item {
  padding: 15px;
  border-bottom: 1px solid #f0f0f0;
}

.feed-item:last-child {
  border-bottom: none;
}

.feed-item.sentiment-positive {
  border-right: 3px solid #4CAF50;
}

.feed-item.sentiment-neutral {
  border-right: 3px solid #2196F3;
}

.feed-item.sentiment-negative {
  border-right: 3px solid #F44336;
}

.feed-header {
  display: flex;
  align-items: center;
  margin-bottom: 10px;
}

.feed-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  margin-left: 15px;
  object-fit: cover;
}

.feed-user {
  flex: 1;
}

.feed-content {
  line-height: 1.6;
  color: #333;
  margin-bottom: 10px;
}

.feed-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.feed-actions {
  color: #666;
  font-size: 0.9rem;
}

.feed-actions span {
  cursor: pointer;
  transition: color 0.2s ease;
}

.feed-actions span:hover {
  color: #2196F3;
}

/* Region Details */
.region-details .detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.region-details .detail-item:last-child {
  border-bottom: none;
}

.candidates-distribution {
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
}

.distribution-item {
  display: flex;
  align-items: center;
  margin-bottom: 10px;
}

.distribution-item:last-child {
  margin-bottom: 0;
}

.candidate-name {
  width: 150px;
  font-size: 0.9rem;
}

.distribution-bar {
  flex: 1;
  height: 20px;
  background: #e0e0e0;
  border-radius: 10px;
  margin: 0 15px;
  overflow: hidden;
}

.bar-fill {
  height: 100%;
  background: linear-gradient(135deg, #2196F3 0%, #03A9F4 100%);
  border-radius: 10px;
  transition: width 0.5s ease;
}

.distribution-percentage {
  width: 50px;
  text-align: left;
  font-weight: bold;
}

/* Auto-refresh Indicator */
.auto-refresh-indicator {
  position: fixed;
  bottom: 80px;
  left: 20px;
  background: rgba(33, 150, 243, 0.9);
  color: white;
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 0.9rem;
  z-index: 1000;
  display: flex;
  align-items: center;
}

/* Refresh Button */
.refresh-button {
  position: fixed;
  bottom: 20px;
  left: 20px;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  font-size: 1.2rem;
  box-shadow: 0 4px 12px rgba(33, 150, 243, 0.4);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Responsive Adjustments */
@media (max-width: 768px) {
  .timer {
    flex-direction: column;
    gap: 5px;
  }

  .time-unit {
    flex-direction: row;
    gap: 5px;
    min-width: auto;
  }

  .time-separator {
    display: none;
  }

  .locations-status {
    flex-direction: column;
  }

  .feed-actions {
    flex-direction: column;
    gap: 5px;
  }

  .refresh-button {
    bottom: 10px;
    left: 10px;
    width: 50px;
    height: 50px;
  }
}
</style>