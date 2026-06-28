<template>
  <div class="election-results-page">
    <b-container class="ads-container mt-4" v-if="electionStatusAll !== 'ended' && !isAdmin">
      <b-alert variant="danger" class="text-center" show>زمان انتخابات به اتمام نرسیده است!</b-alert>
    </b-container>


    <!-- Celebration Header -->
    <b-container fluid class="celebration-header py-5" v-if="electionStatusAll === 'ended' || isAdmin">
      <div class="text-center">
        <div class="celebration-icon">
          <b-icon icon="trophy-fill"></b-icon>
        </div>
        <h1 class="mt-4 mb-3">انتخابات به پایان رسید!</h1>
        <p class="lead mb-4">نتایج نهایی انتخابات صندوق ذخیره فرهنگیان</p>
        <div class="completion-badge">
          <b-badge variant="success" class="p-3">
            <b-icon icon="check-circle-fill" class="ml-2"></b-icon>
            تکمیل شده در تاریخ {{ electionDate }}
          </b-badge>
        </div>
      </div>
    </b-container>

    <!-- Results Summary -->
    <b-container class="results-container" v-if="electionStatusAll === 'ended' || isAdmin">

      <!-- Admin Filter Panel -->
      <b-card v-if="isAdmin" class="admin-filter-card mb-4">
        <div class="d-flex align-items-center mb-2">
          <b-icon icon="funnel-fill" class="ml-2 text-warning"></b-icon>
          <strong>فیلتر نتایج بر اساس منطقه (مخصوص مدیر)</strong>
          <b-badge v-if="electionStatusAll !== 'ended'" variant="warning" class="mr-auto">پیش‌نمایش — منتشر نشده</b-badge>
        </div>
        <b-row>
          <b-col md="4" sm="6" class="mb-2">
            <label class="mb-1 small">استان:</label>
            <b-form-select v-model="selectedProvince" @change="onProvinceChange" size="sm">
              <option :value="null">همه استان‌ها (کل کشور)</option>
              <option v-for="prov in provinces" :key="prov.id" :value="prov.id">{{ prov.name }}</option>
            </b-form-select>
          </b-col>
          <b-col md="4" sm="6" class="mb-2" v-if="selectedProvince">
            <label class="mb-1 small">منطقه:</label>
            <b-form-select v-model="selectedArea" @change="onAreaChange" size="sm">
              <option :value="null">همه مناطق استان</option>
              <option v-for="area in areasForSelectedProvince" :key="area.id" :value="area.id">{{ area.name }}</option>
            </b-form-select>
          </b-col>
          <b-col cols="auto" class="mb-2 d-flex align-items-end" v-if="selectedProvince || selectedArea">
            <b-button size="sm" variant="outline-secondary" @click="clearFilter">
              <b-icon icon="x-circle" class="ml-1"></b-icon>
              پاک کردن فیلتر
            </b-button>
          </b-col>
        </b-row>
        <div v-if="selectedArea || selectedProvince" class="mt-1">
          <small class="text-muted">
            نمایش نتایج برای:
            <strong>{{ selectedArea ? (areasForSelectedProvince.find(a=>a.id===selectedArea)||{}).name : (provinces.find(p=>p.id===selectedProvince)||{}).name }}</strong>
          </small>
        </div>
      </b-card>

      <!-- Overall Stats -->
      <b-row class="mb-5">
        <b-col cols="12">
          <b-card class="summary-card">
            <b-row class="align-items-center">
              <b-col md="8">
                <h4 class="mb-3">خلاصه نتایج انتخابات</h4>
                <b-list-group class="summary-stats">
                  <b-list-group-item class="d-flex justify-content-between align-items-center">
                    <span>کل واجدین شرایط</span>
                    <b-badge variant="primary" pill>
                      {{ formatNumber(finalResults.totalVoters) }}
                    </b-badge>
                  </b-list-group-item>
                  <b-list-group-item class="d-flex justify-content-between align-items-center">
                    <span>کل آرای مأخوذه</span>
                    <b-badge variant="primary" pill>
                      {{ formatNumber(finalResults.totalVotes) }} رأی
                    </b-badge>
                  </b-list-group-item>
                  <b-list-group-item class="d-flex justify-content-between align-items-center">
                    <span>میزان مشارکت</span>
                    <b-badge variant="success" pill>
                      {{ finalResults.participationRate.toFixed(2) }}%
                    </b-badge>
                  </b-list-group-item>
                  <b-list-group-item class="d-flex justify-content-between align-items-center">
                    <span>تعداد کاندیداها</span>
                    <b-badge variant="info" pill>
                      {{ finalResults.totalCandidates }} نفر
                    </b-badge>
                  </b-list-group-item>
                  <!-- <b-list-group-item class="d-flex justify-content-between align-items-center">
                    <span>آرای باطله</span>
                    <b-badge variant="secondary" pill>
                      {{ formatNumber(finalResults.invalidVotes) }} رأی
                    </b-badge>
                  </b-list-group-item> -->
                </b-list-group>
              </b-col>
              <b-col md="4" class="text-center">
                <div class="participation-chart">
                  <canvas ref="participationChart"></canvas>
                </div>
              </b-col>
            </b-row>
          </b-card>
        </b-col>
      </b-row>

      <!-- Winner Announcement -->
      <b-card class="winner-card mb-5">
        <div class="winner-header text-center mb-4">
          <h3>
            <b-icon icon="trophy" variant="warning" class="ml-2"></b-icon>
            منتخب فرهنگیان
          </h3>
          <p class="text-muted">برنده نهایی انتخابات</p>
        </div>

        <b-row class="align-items-center">
          <b-col md="4" class="text-center">
            <div class="winner-photo-container">
              <img v-if="winner.photo" :src="winner.photo" :alt="winner.name"
                class="winner-photo" />
              <div v-else class="winner-photo placeholder">
                <b-icon icon="person-circle"></b-icon>
              </div>
              <div class="winner-crown">
                <b-icon icon="crown-fill"></b-icon>
              </div>
            </div>
          </b-col>

          <b-col md="8">
            <div class="winner-info">
              <h2 class="winner-name">{{ winner.name }}</h2>
              <p class="winner-position">{{ winner.org_position_desc }}</p>

              <div class="winner-stats">
                <b-row>
                  <b-col>
                    <div class="stat-item">
                      <div class="stat-value">{{ formatNumber(winner.votes) }}</div>
                      <div class="stat-label">آرای کسب شده</div>
                    </div>
                  </b-col>
                  <b-col>
                    <div class="stat-item">
                      <div class="stat-value">{{ winner.percentage }}%</div>
                      <div class="stat-label">درصد آرا</div>
                    </div>
                  </b-col>
                  <b-col>
                    <div class="stat-item">
                      <div class="stat-value">{{ winner.margin }}%</div>
                      <div class="stat-label">تفاوت با نفر دوم</div>
                    </div>
                  </b-col>
                </b-row>
              </div>

            </div>
          </b-col>
        </b-row>

        <div class="victory-message text-center mt-4">
          <b-alert variant="success" show class="d-inline-block">
            <h5 class="alert-heading mb-2">پیروزی با {{ winner.name }}!</h5>
            <p class="mb-0">با کسب {{ winner.percentage }}% از آراء به عنوان عضو جدید صندوق ذخیره فرهنگیان
              انتخاب شدند.</p>
          </b-alert>
        </div>
      </b-card>

      <!-- Final Ranking -->
      <b-card class="ranking-card mb-5">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h4>رتبه‌بندی نهایی کاندیداها</h4>
          <div class="ranking-actions">
            <b-button-group>
              <b-button :variant="viewMode === 'table' ? 'primary' : 'outline-primary'" @click="viewMode = 'table'">
                <b-icon icon="table"></b-icon>
                جدول
              </b-button>
              <b-button :variant="viewMode === 'chart' ? 'primary' : 'outline-primary'" @click="viewMode = 'chart'">
                <b-icon icon="bar-chart-fill"></b-icon>
                نمودار
              </b-button>
            </b-button-group>
          </div>
        </div>

        <!-- Table View -->
        <div v-if="viewMode === 'table'" class="table-responsive">
          <b-table :items="sortedCandidates" :fields="rankingFields" striped hover class="text-right"
            thead-class="bg-primary text-white">
            <template #cell(rank)="data">
              <div class="rank-display" :class="`rank-${data.index + 1}`">
                <span class="rank-number">{{ data.index + 1 }}</span>
                <span v-if="data.index === 0" class="rank-icon">
                  <b-icon icon="trophy-fill"></b-icon>
                </span>
                <span v-else-if="data.index === 1" class="rank-icon">
                  <b-icon icon="award-fill"></b-icon>
                </span>
                <span v-else-if="data.index === 2" class="rank-icon">
                  <b-icon icon="award"></b-icon>
                </span>
              </div>
            </template>

            <template #cell(candidate)="data">
              <div class="candidate-info">
                <img v-if="data.item.photo" :src="data.item.photo" class="candidate-photo" :alt="data.item.name" />
                <div v-else class="candidate-photo placeholder"><b-icon icon="person-circle"></b-icon></div>
                <div class="candidate-details">
                  <strong>{{ data.item.name }} </strong>
                  <small class="text-muted d-block">{{ data.item.position }}</small>
                  <div class="candidate-tags">
                    <b-badge v-if="data.index === 0" variant="warning" class="mr-1">
                      برنده
                    </b-badge>
                  </div>
                </div>
              </div>
            </template>

            <template #cell(votes)="data">
              <div class="votes-display">
                <div class="votes-count">{{ formatNumber(data.item.votes) }}</div>
                <div class="votes-percentage">
                  <b-progress :value="data.item.votes" :max="maxVotes" height="6px" class="mt-1"
                    :variant="getProgressVariant(data.index)"></b-progress>
                  <small>{{ data.item.percentage }}%</small>
                </div>
              </div>
            </template>

            <template #cell(status)="data">
              <b-badge :variant="getStatusVariant(data.item.status)">
                {{ getStatusText(data.item.status) }}
              </b-badge>
            </template>

          </b-table>
        </div>


        <!-- Chart View -->
        <div v-else class="chart-container">
          <div class="chart-wrapper">
            <canvas ref="resultsChart"></canvas>
          </div>
        </div>

        <!-- Chart Legend -->
        <div class="chart-legend mt-4">
          <b-row>
            <b-col md="4">
              <div class="legend-item">
                <span class="legend-color first-place"></span>
                <span>رتبه اول</span>
              </div>
            </b-col>
            <b-col md="4">
              <div class="legend-item">
                <span class="legend-color second-place"></span>
                <span>رتبه دوم</span>
              </div>
            </b-col>
            <b-col md="4">
              <div class="legend-item">
                <span class="legend-color other-place"></span>
                <span>سایر کاندیداها</span>
              </div>
            </b-col>
          </b-row>
        </div>

      </b-card>
      <b-card v-if="!this.isActive">
        <div class="align-items-center mb-4">
          <h4>تایید انتشار</h4>
          <b-form-file v-model="document" placeholder="صورتجلسه را انتخاب کنید یا اینجا رها کنید"
            drop-placeholder="فایل‌ها را اینجا رها کنید" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"></b-form-file>
          <b-button variant="success" @click="activateFinalResults">تایید و انتشار</b-button>
        </div>

      </b-card>
      <hr />
      <!-- Detailed Analysis -->
      <b-row class="mb-5">
        <b-col lg="6" class="mb-4">
          <b-card class="h-100">
            <h5 class="mb-3">
              <b-icon icon="pie-chart-fill" class="ml-2"></b-icon>
              توزیع آرا
            </h5>
            <div class="distribution-chart">
              <canvas ref="distributionChart"></canvas>
            </div>
            <div class="distribution-list mt-3">
              <div v-for="candidate in topCandidates" :key="candidate.id" class="distribution-item">
                <span class="distribution-color" :style="{ backgroundColor: candidate.color }"></span>
                <span class="distribution-name">{{ candidate.name.split(' ')[1] }}</span>
                <span class="distribution-percentage">{{ candidate.percentage }}%</span>
                <span class="distribution-votes">{{ formatNumber(candidate.votes) }} رأی</span>
              </div>
            </div>
          </b-card>
        </b-col>

        <b-col lg="6" class="mb-4">
          <b-card class="h-100">
            <h5 class="mb-3">
              <b-icon icon="geo-alt-fill" class="ml-2"></b-icon>
              نتایج بر اساس استان
            </h5>
            <div class="region-results">
              <b-table :items="regionResults" :fields="regionFields" small striped class="text-right">
                <template #cell(votes)="data">
                  {{ formatNumber(data.value) }}
                </template>
                <template #cell(participation)="data">
                  <div class="region-participation">
                    <b-progress :value="data.value" :max="100" height="4px" class="mb-1"
                      :variant="getRegionVariant(data.value)"></b-progress>
                    <small>{{ data.value }}%</small>
                  </div>
                </template>
              </b-table>
            </div>

            <div class="text-center mt-3">
              <b-button variant="outline-primary" size="sm" @click="downloadRegionalResults">
                <b-icon icon="download" class="ml-1"></b-icon>
                دریافت نتایج استانی
              </b-button>
            </div>
          </b-card>
        </b-col>
      </b-row>
      <b-card class="mb-5 iran-heatmap-card">
        <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap">
          <h5 class="mb-2 mb-md-0">نقشه حرارتی ایران در نتایج نهایی</h5>
          <small class="text-muted">استان‌های پرمشارکت با رنگ پررنگ‌تر نمایش داده می‌شوند.</small>
        </div>
        <div class="iran-heatmap-grid">
          <div v-for="province in finalProvinceHeatmapData" :key="`final-map-${province.id}`" class="heat-province"
            :style="{ backgroundColor: province.color }" @mouseenter="hoveredFinalProvince = province"
            @mouseleave="hoveredFinalProvince = null">
            <div class="heat-province-name">{{ province.name }}</div>
            <div class="heat-province-rate">{{ province.participation }}%</div>
          </div>
        </div>

        <b-alert v-if="hoveredFinalProvince" show variant="info" class="hovered-province-popup mt-3 mb-0">
          <strong>{{ hoveredFinalProvince.name }}</strong>
          — مشارکت: {{ hoveredFinalProvince.participation }}% |
          آرا: {{ formatNumber(hoveredFinalProvince.votes) }}
        </b-alert>
      </b-card>
      <!-- Share Results -->
      <b-card class="share-card">
        <div class="text-center">
          <h5 class="mb-3">اشتراک‌گذاری نتایج</h5>
          <p class="text-muted mb-4">نتایج انتخابات را با دیگران به اشتراک بگذارید</p>

          <div class="share-buttons">
            <b-button variant="outline-info" @click="printResults">
              <b-icon icon="printer" class="ml-1"></b-icon>
              چاپ نتایج
            </b-button>
            <b-button variant="outline-primary" class="share-btn" @click="shareTelegram">
              <b-icon icon="telegram"></b-icon>
              تلگرام
            </b-button>
            <b-button variant="outline-info" class="share-btn" @click="shareWhatsApp">
              <b-icon icon="whatsapp"></b-icon>
              واتس‌اپ
            </b-button>
            <b-button variant="outline-secondary" class="share-btn" @click="copyLink">
              <b-icon icon="link"></b-icon>
              کپی لینک
            </b-button>
            <b-button variant="outline-dark" class="share-btn" @click="shareTwitter">
              <b-icon icon="twitter"></b-icon>
              توییتر
            </b-button>
          </div>

        </div>
      </b-card>
    </b-container>

    <!-- Statistics Footer -->
    <b-container fluid class="stats-footer text-center py-4" v-if="electionStatusAll === 'ended' || isAdmin">
      صندوق ذخیره فرهنگیان
    </b-container>
  </div>
</template>

<script>
import Chart from 'chart.js';
import { mapGetters, mapActions, mapMutations } from "vuex";
import { apiUrlrtb } from '../../constants/config';
export default {
  name: "ElectionFinalResults",
  data() {
    return {
      isActive: false,
      document: null,
      electionDate: '۱۴۰۲/۱۱/۱۵',
      viewMode: 'table',
      selectedProvince: null,
      selectedArea: null,
      provinces: [],
      areasByProvince: {},

      // Chart Instances
      participationChart: null,
      resultsChart: null,
      distributionChart: null,

      // Final Results Data
      finalResults: {
        totalVotes: 0,
        participationRate: 0,
        totalCandidates: 0,
        invalidVotes: 0,
        declarationDate: '۱۴۰۲/۱۱/۱۶',
        objectionDeadline: '۱۴۰۲/۱۱/۲۰',
        duration: '۲۴ ساعت',
        locations: 125,
        observers: 250,
        transparency: 98.7
      },

      // Winner Data
      winner: {
        id: null,
        name: '',
        position: '',
        photo: null,
        votes: 0,
        percentage: 0,
        margin: 0
      },

      // Candidates Data
      candidates: [],

      // Region Results (from API)
      regionResults: [],

      // Table Fields
      rankingFields: [
        { key: 'rank', label: 'رتبه', sortable: false },
        { key: 'candidate', label: 'کاندیدا', sortable: false },
        { key: 'votes', label: 'آرا', sortable: true },
        { key: 'status', label: 'وضعیت', sortable: true }
      ],

      regionFields: [
        { key: 'name', label: 'استان', sortable: true },
        { key: 'votes', label: 'آرای معتبر', sortable: true },
        { key: 'participation', label: 'مشارکت', sortable: true }
      ],

      // Share Link
      shareLink: ''
      , hoveredFinalProvince: null
    };
  },
  computed: {
    ...mapGetters(["electionStatusAll", "ConfigInfo", "currentUser"]),
    isAdmin() {
      console.log(this.currentUser);
      
      return this.currentUser?.roles[0] === 'ADMIN';
    },
    areasForSelectedProvince() {
      if (!this.selectedProvince) return [];
      return this.areasByProvince[this.selectedProvince] || [];
    },
    sortedCandidates() {
      return [...this.candidates].sort((a, b) => b.votes - a.votes);
    },

    maxVotes() {
      if (!this.candidates.length) return 1;
      return Math.max(...this.candidates.map(c => c.votes));
    },

    topCandidates() {
      return this.sortedCandidates.slice(0, 5);
    },
    finalProvinceHeatmapData() {
      const maxParticipation = Math.max(...this.regionResults.map(r => Number(r.participation) || 0), 1);
      return this.regionResults.map(region => {
        const ratio = (Number(region.participation) || 0) / maxParticipation;
        const alpha = 0.25 + (ratio * 0.7);
        return {
          ...region,
          color: `rgba(220, 53, 69, ${alpha.toFixed(2)})`
        };
      });
    },
  },
  async mounted() {
    if (!this.ConfigInfo) {
      await this.getConfig();
    }
    await this.loadFinalResults();
    this.generateShareLink();

    const response = await this.getFinalResultsApprovalStatus()
    this.isActive = response.isActive

  },
  beforeUnmount() {
    // Clean up chart instances to prevent memory leaks
    if (this.participationChart) {
      this.participationChart.destroy();
    }
    if (this.resultsChart) {
      this.resultsChart.destroy();
    }
    if (this.distributionChart) {
      this.distributionChart.destroy();
    }
  },
  methods: {
    ...mapActions(["getConfig", "getInfoVote", "getRegions", "getFinalResultsApprovalStatus", "setFinalResultsApproval"]),
    // Formatting
    async activateFinalResults() {
      const response = await this.setFinalResultsApproval()
      if (response.status) {
        const response1 = await this.getFinalResultsApprovalStatus()
        this.isActive = response1.isActive
      }
    },
    formatNumber(num) {
      return new Intl.NumberFormat('fa-IR').format(num);
    },
    async onProvinceChange() {
      this.selectedArea = null;
      await this.loadFinalResults();
    },
    async onAreaChange() {
      await this.loadFinalResults();
    },
    async clearFilter() {
      this.selectedProvince = null;
      this.selectedArea = null;
      await this.loadFinalResults();
    },
    async loadFinalResults() {
      let params = {};
      if (this.selectedArea) params = { region: this.selectedArea };
      else if (this.selectedProvince) params = { province: this.selectedProvince };
      const [data, regionsResponse] = await Promise.all([
        this.getInfoVote(params),
        this.getRegions()
      ]);

      const totalVotes = Number(data?.totalVotes) || 0;
      const totalVoters = Number(data?.totalVoters) || 0;
      const listCandidates = data?.listCan || [];

      const colors = ['#3F51B5', '#4CAF50', '#FF9800', '#9C27B0', '#2196F3', '#E91E63', '#795548', '#607D8B'];

      this.candidates = listCandidates.map((candidate, index) => {
        const votes = Number(candidate.vote_count) || 0;
        const percentage = totalVotes ? Number(((votes / totalVotes) * 100).toFixed(1)) : 0;
        return {
          id: candidate.id ?? index,
          name: `${candidate.first_name || ''} ${candidate.last_name || ''}`.trim(),
          position: candidate.org_position_desc || '',
          photo: candidate.user_photo ? `${apiUrlrtb}/${candidate.user_photo}` : null,
          votes,
          percentage,
          color: colors[index % colors.length],
          status: index === 0 ? 'winner' : 'qualified'
        };
      }).sort((a, b) => b.votes - a.votes);

      const winner = this.candidates[0];
      const runnerUp = this.candidates[1];
      const margin = winner && runnerUp && totalVotes
        ? Number((((winner.votes - runnerUp.votes) / totalVotes) * 100).toFixed(1))
        : 0;

      this.winner = winner ? { ...winner, margin } : this.winner;

      this.finalResults = {
        ...this.finalResults,
        totalVoters,
        totalVotes,
        participationRate: Number(data?.voterParticipation) || 0,
        totalCandidates: Number(data?.Candidates) || this.candidates.length,
        invalidVotes: 0
      };

      if (this.ConfigInfo?.EndDate) {
        this.electionDate = new Date(this.ConfigInfo.EndDate).toLocaleDateString('fa-IR');
      }

      // Build province results from API
      const provinces = regionsResponse?.data || [];
      this.provinces = provinces;
      this.areasByProvince = regionsResponse?.areasByProvince || {};
      const pvs = data?.provinceVoteStats || {};
      const epv = data?.eligiblePerProvince || {};
      this.regionResults = provinces
        .map(province => {
          const key = Number(province.id) * 100;
          const stat = pvs[key] || {};
          const votes = Number(stat.votes || 0);
          const eligible = Number(epv[key] || stat.eligible || 0);
          const participation = eligible > 0 ? Number(((votes / eligible) * 100).toFixed(1)) : 0;
          return { id: province.id, name: province.name, votes, participation };
        })
        .filter(r => r.votes > 0 || r.participation > 0)
        .sort((a, b) => b.votes - a.votes);

      this.$nextTick(() => {
        this.initializeCharts();
      });
    },
    // Chart Initialization
    initializeCharts() {
      this.createParticipationChart();
      this.createResultsChart();
      this.createDistributionChart();
    },

    createParticipationChart() {
      const ctx = this.$refs.participationChart?.getContext('2d');
      if (!ctx) return;

      if (this.participationChart) {
        this.participationChart.destroy();
      }

      this.participationChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['مشارکت کنندگان', 'غیرمشارکت کنندگان'],
          datasets: [{
            data: [this.finalResults.participationRate, 100 - this.finalResults.participationRate],
            backgroundColor: ['#4CAF50', '#F44336'],
            borderWidth: 1,
            borderColor: '#fff'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              rtl: true,
              labels: {
                font: {
                  family: 'IRANSans, Arial, sans-serif'
                },
                padding: 20
              }
            },
            tooltip: {
              callbacks: {
                label: (context) => `${context.label}: ${context.raw}%`
              }
            }
          },
          cutout: '70%'
        }
      });
    },

    createResultsChart() {
      const ctx = this.$refs.resultsChart?.getContext('2d');
      if (!ctx) return;

      if (!this.sortedCandidates.length) return;
      if (this.resultsChart) {
        this.resultsChart.destroy();
      }

      const labels = this.sortedCandidates.map(c => c.name.split(' ').pop());
      const data = this.sortedCandidates.map(c => c.votes);
      const colors = this.sortedCandidates.map(c => c.color);

      this.resultsChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: labels,
          datasets: [{
            label: 'تعداد آرا',
            data: data,
            backgroundColor: colors,
            borderColor: colors.map(color => color + 'CC'),
            borderWidth: 1,
            borderRadius: 6
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: (context) => {
                  const candidate = this.sortedCandidates[context.dataIndex];
                  return `${candidate.name}: ${this.formatNumber(context.raw)} رأی (${candidate.percentage}%)`;
                }
              }
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'تعداد آرا',
                font: {
                  family: 'IRANSans, Arial, sans-serif'
                }
              },
              ticks: {
                font: {
                  family: 'IRANSans, Arial, sans-serif'
                }
              }
            },
            x: {
              title: {
                display: true,
                text: 'کاندیداها',
                font: {
                  family: 'IRANSans, Arial, sans-serif'
                }
              },
              ticks: {
                font: {
                  family: 'IRANSans, Arial, sans-serif'
                }
              }
            }
          }
        }
      });
    },

    createDistributionChart() {
      const ctx = this.$refs.distributionChart?.getContext('2d');
      if (!ctx) return;

      if (!this.sortedCandidates.length || !this.finalResults.totalVotes) return;

      if (this.distributionChart) {
        this.distributionChart.destroy();
      }

      const top5 = this.sortedCandidates.slice(0, 5);
      const othersVotes = this.sortedCandidates.slice(5).reduce((sum, c) => sum + c.votes, 0);
      const othersPercentage = ((othersVotes / this.finalResults.totalVotes) * 100).toFixed(1);

      this.distributionChart = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: [...top5.map(c => c.name.split(' ')[1]), 'سایرین'],
          datasets: [{
            data: [...top5.map(c => c.percentage), parseFloat(othersPercentage)],
            backgroundColor: [...top5.map(c => c.color), '#9E9E9E'],
            borderWidth: 1,
            borderColor: '#fff'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              rtl: true,
              labels: {
                font: {
                  family: 'IRANSans, Arial, sans-serif'
                },
                padding: 20
              }
            },
            tooltip: {
              callbacks: {
                label: (context) => {
                  const value = context.raw;
                  const total = this.finalResults.totalVotes;
                  const votes = Math.round((value / 100) * total);
                  return `${context.label}: ${value}% (${this.formatNumber(votes)} رأی)`;
                }
              }
            }
          }
        }
      });
    },

    // UI Helpers
    getProgressVariant(index) {
      if (index === 0) return 'warning';
      if (index === 1) return 'info';
      if (index === 2) return 'success';
      return 'primary';
    },


    getStatusVariant(status) {
      const variants = {
        winner: 'warning',
        runner_up: 'info',
        qualified: 'success',
        not_qualified: 'secondary'
      };
      return variants[status] || 'secondary';
    },

    getStatusText(status) {
      const texts = {
        winner: 'برنده',
        runner_up: 'نایب قهرمان',
        qualified: 'صلاحیت احراز کرد',
        not_qualified: 'صلاحیت احراز نکرد'
      };
      return texts[status] || status;
    },

    getRegionVariant(percentage) {
      if (percentage >= 70) return 'success';
      if (percentage >= 60) return 'info';
      if (percentage >= 50) return 'warning';
      return 'danger';
    },

    getTimelineIcon(type) {
      const icons = {
        start: 'flag-fill',
        registration: 'person-plus',
        announcement: 'megaphone',
        vote_start: 'play-fill',
        vote_end: 'stop-fill',
        results: 'trophy'
      };
      return icons[type] || 'circle';
    },

    // Actions
    downloadRegionalResults() {
      if (!this.regionResults.length) return;
      const header = 'استان,آرا,مشارکت%';
      const rows = this.regionResults.map(r => `${r.name},${r.votes},${r.participation}`);
      const csv = [header, ...rows].join('\n');
      const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = 'نتایج_استانی.csv'; a.click();
      URL.revokeObjectURL(url);
    },


    printResults() {
      window.print();
    },

    // Share Functions
    generateShareLink() {
      this.shareLink = window.location.href;
    },

    async copyLink() {
      try {
        await navigator.clipboard.writeText(this.shareLink);
        this.$bvToast.toast('لینک نتایج کپی شد', {
          title: 'موفقیت',
          variant: 'success',
          solid: true
        });
      } catch (err) {
        console.error('Failed to copy link:', err);
        this.$bvToast.toast('خطا در کپی لینک', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      }
    },

    shareTelegram() {
      const text = `نتایج نهایی انتخابات صندوق ذخیره فرهنگیان\nبرنده: ${this.winner.name}\nمشارکت: ${this.finalResults.participationRate}%\n\n`;
      const url = `https://t.me/share/url?url=${encodeURIComponent(this.shareLink)}&text=${encodeURIComponent(text)}`;
      window.open(url, '_blank');
    },

    shareWhatsApp() {
      const text = `نتایج نهایی انتخابات صندوق ذخیره فرهنگیان\nبرنده: ${this.winner.name}\nمشارکت: ${this.finalResults.participationRate}%\n${this.shareLink}`;
      const url = `https://wa.me/?text=${encodeURIComponent(text)}`;
      window.open(url, '_blank');
    },

    shareTwitter() {
      const text = `نتایج انتخابات صندوق ذخیره فرهنگیان\nبرنده: ${this.winner.name} با ${this.winner.percentage}% آرا\nمشارکت: ${this.finalResults.participationRate}%\n#انتخابات_فرهنگیان`;
      const url = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(this.shareLink)}`;
      window.open(url, '_blank');
    },

    // Chart View Change
    changeViewMode() {
      if (this.viewMode === 'chart') {
        // Recreate chart when switching to chart view
        this.$nextTick(() => {
          this.createResultsChart();
        });
      }
    }
  },
  watch: {
    viewMode(newVal) {
      if (newVal === 'chart') {
        this.$nextTick(() => {
          this.createResultsChart();
        });
      }
    }
  }
}
</script>

<style scoped>
.admin-filter-card {
  border: 2px dashed #ffc107;
  background: #fffbee;
  border-radius: 12px;
}

.iran-heatmap-card {
  border-radius: 16px;
}

.iran-heatmap-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: 10px;
}

.heat-province {
  color: #fff;
  border-radius: 10px;
  padding: 12px 10px;
  min-height: 72px;
  cursor: pointer;
  transition: transform .2s ease, box-shadow .2s ease;
}

.heat-province:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, .12);
}

.heat-province-name {
  font-weight: 700;
  font-size: 0.9rem;
}

.heat-province-rate {
  font-size: 0.85rem;
  margin-top: 4px;
}

.hovered-province-popup {
  border-radius: 10px;
}

.election-results-page {
  background: linear-gradient(135deg, #f8f9fa 0%, #e3f2fd 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* Celebration Header */
.celebration-header {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  text-align: center;
  position: relative;
  overflow: hidden;
}

.celebration-header::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: url("data:image/svg+xml,%3Csvg width='100' height='100' viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M11 18c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm48 25c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm-43-7c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm63 31c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM34 90c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm56-76c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM12 86c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm28-65c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm23-11c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm-6 60c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm29 22c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zM32 63c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm57-13c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm-9-21c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM60 91c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM35 41c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM12 60c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2z' fill='%23ffffff' fill-opacity='0.05' fill-rule='evenodd'/%3E%3C/svg%3E");
  opacity: 0.1;
}

.celebration-icon {
  font-size: 4rem;
  color: #FFD700;
  animation: bounce 2s infinite;
}

@keyframes bounce {

  0%,
  100% {
    transform: translateY(0);
  }

  50% {
    transform: translateY(-10px);
  }
}

.completion-badge {
  margin-top: 20px;
}

/* Summary Card */
.summary-card {
  border-radius: 15px;
  border: none;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
  background: white;
}

.summary-stats .list-group-item {
  border: none;
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
}

.summary-stats .list-group-item:last-child {
  border-bottom: none;
}

.participation-chart {
  height: 200px;
  position: relative;
}

/* Winner Card */
.winner-card {
  border-radius: 15px;
  border: 3px solid #FFD700;
  background: linear-gradient(135deg, #fff9c4 0%, #fffde7 100%);
  position: relative;
  overflow: hidden;
}

.winner-card::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle, rgba(255, 215, 0, 0.1) 0%, rgba(255, 215, 0, 0) 70%);
}

.winner-header {
  position: relative;
  z-index: 1;
}

.winner-photo-container {
  position: relative;
  display: inline-block;
}

.winner-photo {
  width: 200px;
  height: 200px;
  border-radius: 50%;
  border: 5px solid #FFD700;
  object-fit: cover;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

.winner-photo.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f0f0f0;
  color: #9e9e9e;
}

.winner-photo.placeholder .b-icon {
  font-size: 3rem;
}

.winner-crown {
  position: absolute;
  top: -10px;
  right: 50%;
  transform: translateX(50%);
  background: #FFD700;
  color: #333;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
}

.winner-info {
  padding: 20px;
}

.winner-name {
  color: #2c3e50;
  font-size: 2.2rem;
  font-weight: bold;
  margin-bottom: 10px;
}

.winner-position {
  color: #666;
  font-size: 1.1rem;
  margin-bottom: 25px;
}

.winner-stats {
  background: rgba(255, 255, 255, 0.8);
  padding: 20px;
  border-radius: 10px;
  border: 1px solid #e0e0e0;
}

.stat-item {
  text-align: center;
  padding: 10px;
}

.stat-value {
  font-size: 1.8rem;
  font-weight: bold;
  color: #2c3e50;
  margin-bottom: 5px;
}

.stat-label {
  color: #e0dada;
  font-size: 0.9rem;
}

.winner-quote {
  background: rgba(255, 255, 255, 0.9);
  padding: 20px;
  border-radius: 10px;
  border-right: 4px solid #3F51B5;
  font-size: 1.1rem;
  line-height: 1.8;
  color: #555;
}

.victory-message {
  position: relative;
  z-index: 1;
}

/* Ranking Card */
.ranking-card {
  border-radius: 15px;
  border: none;
  box-shadow: 0 5px 20px rgba(0, 0, 0, 0.08);
}

.rank-display {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  font-weight: bold;
  position: relative;
}

.rank-1 {
  background: linear-gradient(135deg, #FFD700 0%, #FFC107 100%);
  color: #333;
}

.rank-2 {
  background: linear-gradient(135deg, #C0C0C0 0%, #E0E0E0 100%);
  color: #333;
}

.rank-3 {
  background: linear-gradient(135deg, #CD7F32 0%, #D2691E 100%);
  color: white;
}

.rank-4,
.rank-5,
.rank-6,
.rank-7,
.rank-8 {
  background: #f5f5f5;
  color: #666;
}

.rank-number {
  font-size: 1.2rem;
}

.rank-icon {
  position: absolute;
  top: -5px;
  left: -5px;
  font-size: 1.2rem;
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

.candidate-details {
  flex: 1;
}

.candidate-tags {
  margin-top: 5px;
}

.votes-display {
  text-align: left;
}

.votes-count {
  font-weight: bold;
  font-size: 1.1rem;
  margin-bottom: 5px;
}

.chart-container {
  height: 400px;
  position: relative;
}

.chart-wrapper {
  height: 380px;
}

.chart-legend {
  padding: 15px;
  background: #f8f9fa;
  border-radius: 10px;
}

.legend-item {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 5px;
}

.legend-color {
  width: 20px;
  height: 20px;
  border-radius: 4px;
  margin-left: 10px;
}

.first-place {
  background: #FFD700;
}

.second-place {
  background: #C0C0C0;
}

.other-place {
  background: #2196F3;
}

/* Distribution Chart */
.distribution-chart {
  height: 250px;
  position: relative;
}

.distribution-list {
  max-height: 200px;
  overflow-y: auto;
}

.distribution-item {
  display: flex;
  align-items: center;
  padding: 10px;
  border-bottom: 1px solid #f0f0f0;
}

.distribution-item:last-child {
  border-bottom: none;
}

.distribution-color {
  width: 15px;
  height: 15px;
  border-radius: 3px;
  margin-left: 10px;
}

.distribution-name {
  flex: 1;
}

.distribution-percentage {
  width: 60px;
  text-align: left;
  font-weight: bold;
}

.distribution-votes {
  width: 100px;
  text-align: left;
  color: #666;
}

/* Region Results */
.region-winner {
  display: flex;
  align-items: center;
}

.region-winner-photo {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  margin-left: 10px;
  object-fit: cover;
}

.region-winner-name {
  font-size: 0.9rem;
}

.region-participation {
  min-width: 100px;
}

/* Timeline */
.timeline {
  position: relative;
  padding: 20px 0;
}

.timeline::before {
  content: '';
  position: absolute;
  top: 0;
  bottom: 0;
  right: 50%;
  width: 2px;
  background: #e0e0e0;
  transform: translateX(50%);
}

.timeline-item {
  display: flex;
  margin-bottom: 30px;
  position: relative;
}

.timeline-item-reverse {
  flex-direction: row-reverse;
}

.timeline-marker {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  position: relative;
  z-index: 1;
  flex-shrink: 0;
}

.marker-start {
  background: linear-gradient(135deg, #4CAF50 0%, #8BC34A 100%);
}

.marker-registration {
  background: linear-gradient(135deg, #2196F3 0%, #03A9F4 100%);
}

.marker-announcement {
  background: linear-gradient(135deg, #9C27B0 0%, #E91E63 100%);
}

.marker-vote_start {
  background: linear-gradient(135deg, #FF9800 0%, #FFC107 100%);
}

.marker-vote_end {
  background: linear-gradient(135deg, #F44336 0%, #EF5350 100%);
}

.marker-results {
  background: linear-gradient(135deg, #3F51B5 0%, #7986CB 100%);
}

.timeline-content {
  flex: 1;
  padding: 15px;
  margin: 0 20px;
  background: white;
  border-radius: 10px;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.08);
}

.timeline-item-reverse .timeline-content {
  text-align: left;
}

.timeline-date {
  color: #3F51B5;
  font-weight: bold;
  margin-bottom: 5px;
}

.timeline-title {
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 5px;
}

.timeline-description {
  color: #666;
  font-size: 0.9rem;
}

/* Certification Card */
.certification-card {
  border: 2px dashed #4CAF50;
  background: #f8fff8;
  border-radius: 15px;
}

.certification-icon {
  font-size: 3rem;
  color: #4CAF50;
}

.certification-details {
  background: white;
  padding: 20px;
  border-radius: 10px;
  border: 1px solid #e0e0e0;
  margin: 20px 0;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.detail-item:last-child {
  border-bottom: none;
}

/* Share Card */
.share-card {
  border-radius: 15px;
  background: linear-gradient(135deg, #f8f9fa 0%, #e8f4fd 100%);
}

.share-buttons {
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
  gap: 10px;
}

.share-btn {
  min-width: 120px;
}

/* Statistics Footer */
.stats-footer {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  margin-top: 50px;
}

.stats-footer .stat-number {
  font-size: 2rem;
  font-weight: bold;
  margin-bottom: 5px;
}

.stats-footer .stat-label {
  font-size: 0.9rem;
  opacity: 0.8;
}

/* Responsive Design */
@media (max-width: 768px) {
  .winner-photo {
    width: 150px;
    height: 150px;
  }

  .winner-name {
    font-size: 1.8rem;
  }

  .timeline::before {
    right: 30px;
  }

  .timeline-item,
  .timeline-item-reverse {
    flex-direction: column;
  }

  .timeline-marker {
    margin-bottom: 15px;
  }

  .timeline-content {
    margin: 0;
  }

  .share-buttons {
    flex-direction: column;
    align-items: center;
  }

  .share-btn {
    width: 100%;
    max-width: 250px;
    margin-bottom: 10px;
  }
}

@media print {
  .election-results-page {
    background: white;
  }

  .celebration-header,
  .stats-footer,
  .share-card,
  button {
    display: none;
  }
}
</style>