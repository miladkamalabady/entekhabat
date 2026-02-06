<template>
  <div class="election-results-page">
    <b-container class="ads-container mt-4" v-if="electionStatusAll !== 'ended'">

      <b-alert variant="danger" class="text-center" show>زمان انتخابات به اتمام نرسیده است!</b-alert>

    </b-container>
    <!-- Celebration Header -->
    <b-container fluid class="celebration-header py-5" v-if="electionStatusAll === 'ended'">
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
    <b-container class="results-container" v-if="electionStatusAll === 'ended'">
      <!-- Overall Stats -->
      <b-row class="mb-5">
        <b-col cols="12">
          <b-card class="summary-card">
            <b-row class="align-items-center">
              <b-col md="8">
                <h4 class="mb-3">خلاصه نتایج انتخابات</h4>
                <b-list-group class="summary-stats">
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
                  <b-list-group-item class="d-flex justify-content-between align-items-center">
                    <span>آرای باطله</span>
                    <b-badge variant="secondary" pill>
                      {{ formatNumber(finalResults.invalidVotes) }} رأی
                    </b-badge>
                  </b-list-group-item>
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
              <img v-if="winner.user_photo" :src="`${apiUrlrtb}/${winner.user_photo}`" :alt="winner.first_name"
                class="winner-photo" />
              <div class="winner-crown">
                <b-icon icon="crown-fill"></b-icon>
              </div>
            </div>
          </b-col>

          <b-col md="8">
            <div class="winner-info">
              <h2 class="winner-name">{{ winner.first_name }} {{ winner.last_name }}</h2>
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

              <div class="winner-quote mt-4">
                <b-icon icon="quote" variant="secondary" class="ml-2"></b-icon>
                <em>{{ winner.quote }}</em>
              </div>
            </div>
          </b-col>
        </b-row>

        <div class="victory-message text-center mt-4">
          <b-alert variant="success" show class="d-inline-block">
            <h5 class="alert-heading mb-2">پیروزی با {{ winner.name.split(' ')[1] }}!</h5>
            <p class="mb-0">با کسب {{ winner.percentage }}% از آراء به عنوان عضو جدید هیئت مدیره صندوق ذخیره فرهنگیان
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
                <img :src="`${data.item.photo}`" class="candidate-photo" :alt="data.item.name" />
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
                <template #cell(winner)="data">
                  <div class="region-winner">
                    <img v-if="getCandidatePhoto(data.value)" :src="getCandidatePhoto(data.value)"
                      class="region-winner-photo" :alt="data.value" />
                    <div v-else class="region-winner-photo placeholder">
                      <b-icon icon="person-circle"></b-icon>
                    </div>
                    <span class="region-winner-name">{{ data.value }}</span>
                  </div>
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
    <b-container fluid class="stats-footer text-center py-4" v-if="electionStatusAll === 'ended'">
      صندوق ذخیره فرهنگیان
    </b-container>
  </div>
</template>

<script>
import Chart from 'chart.js';
import { apiUrlrtb } from '../../constants/config'
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: "ElectionFinalResults",
  data() {
    return {
      apiUrlrtb,
      electionDate: '۱۴۰۲/۱۱/۱۵',
      viewMode: 'table',

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
        id: 1,
        name: 'دکتر محمدرضا احمدی',
        position: 'استاد دانشگاه - علوم تربیتی',
        photo: 'assets/img/avatars/image1.png?text=دکتر+احمدی',
        votes: 45680,
        percentage: 36.4,
        margin: 8.2,
        quote: 'سپاسگزار اعتماد فرهنگیان عزیز هستم. این انتخاب، مسئولیتی بزرگ در قبال آینده صندوق ذخیره فرهنگیان است.'
      },

      // Candidates Data
      candidates: [],

      // Region Results
      regionResults: [
        { id: 1, name: 'تهران', votes: 35480, participation: 72.5, winner: 'دکتر محمدرضا احمدی' },
        { id: 2, name: 'مشهد', votes: 18450, participation: 68.3, winner: 'مهندس سید علی حسینی' },
        { id: 3, name: 'اصفهان', votes: 13870, participation: 65.2, winner: 'دکتر فاطمه کریمی' },
        { id: 4, name: 'شیراز', votes: 10650, participation: 61.7, winner: 'دکتر محمدرضا احمدی' },
        { id: 5, name: 'تبریز', votes: 9540, participation: 59.4, winner: 'مهندس سید علی حسینی' },
        { id: 6, name: 'کرج', votes: 8760, participation: 63.1, winner: 'دکتر محمدرضا احمدی' }
      ],

      // Certification Data
      certification: {
        protocolNumber: 'PR-۱۴۰۲-۱۱-۱۶-۰۰۱',
        approvalDate: '۱۴۰۲/۱۱/۱۶',
        committeeHead: 'دکتر سید حسن موسوی'
      },

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
        { key: 'participation', label: 'مشارکت', sortable: true },
        { key: 'winner', label: 'کاندیدای برتر', sortable: false }
      ],

      // Share Link
      shareLink: ''
    };
  },
  computed: {
    ...mapGetters(["electionStatusAll", "ConfigInfo"]),
    sortedCandidates() {
      return [...this.candidates].sort((a, b) => b.votes - a.votes);
    },

    maxVotes() {
      if (!this.candidates.length) return 1;
      return Math.max(...this.candidates.map(c => c.votes));
    },

    topCandidates() {
      return this.sortedCandidates.slice(0, 5);
    }
  },
  async mounted() {
    if (!this.ConfigInfo) {
      await this.getConfig();
    }
    await this.loadFinalResults();
    this.generateShareLink();
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
    ...mapActions(["getConfig", "getInfoVote"]),
    // Formatting
    formatNumber(num) {
      return new Intl.NumberFormat('fa-IR').format(num);
    },
    async loadFinalResults() {
      const data = await this.getInfoVote();
      this.infoVote = data;

      const totalVotes = Number(data?.totalVotes) || 0;
      const listCandidates = data?.listCan || [];

      const colors = ['#3F51B5', '#4CAF50', '#FF9800', '#9C27B0', '#2196F3', '#E91E63', '#795548', '#607D8B'];

      this.candidates = listCandidates.map((candidate, index) => {
        const votes = Number(candidate.vote_count) || 0;
        const percentage = totalVotes ? Number(((votes / totalVotes) * 100).toFixed(1)) : 0;
        return {
          id: candidate.id ?? index,
          name: `${candidate.first_name || ''} ${candidate.last_name || ''}`.trim(),
          position: candidate.org_position_desc || '',
          photo: candidate.user_photo ? `${this.apiUrlrtb}/${candidate.user_photo}` : null,
          votes,
          percentage,
          color: colors[index % colors.length],
          status: index === 0 ? 'winner' : 'qualified',
          quote: 'به امید روزهای خوب برای صندوق',
          previousMember: false
        };
      });

      this.candidates = this.candidates.sort((a, b) => b.votes - a.votes);

      const winner = this.candidates[0];
      const runnerUp = this.candidates[1];
      const margin = winner && runnerUp && totalVotes
        ? Number((((winner.votes - runnerUp.votes) / totalVotes) * 100).toFixed(1))
        : 0;

      this.winner = winner
        ? {
          ...winner,
          margin
        }
        : {
          id: null,
          name: '',
          position: '',
          photo: null,
          votes: 0,
          percentage: 0,
          margin: 0,
          quote: this.winner.quote
        };

      this.finalResults = {
        ...this.finalResults,
        totalVotes,
        participationRate: Number(data?.voterParticipation) || 0,
        totalCandidates: Number(data?.Candidates) || this.candidates.length,
        invalidVotes: Number(data?.invalidVotes) || 0
      };

      if (this.ConfigInfo?.EndDate) {
        this.electionDate = new Date(this.ConfigInfo.EndDate).toLocaleDateString('fa-IR');
      }

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

    getCandidatePhoto(name) {
      const candidate = this.candidates.find(c => c.name === name);
      return candidate ? candidate.photo : null;
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
      // In real app, this would download a CSV/PDF file
      alert('نتایج استانی در حال دانلود...');
      // Simulate download
      setTimeout(() => {
        this.$bvToast.toast('نتایج استانی با موفقیت دانلود شد', {
          title: 'موفقیت',
          variant: 'success',
          solid: true
        });
      }, 1000);
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
      const text = `نتایج انتخابات صندوق ذخیره فرهنگیان\nبرنده: ${this.winner.name.split(' ')[1]} با ${this.winner.percentage}% آرا\nمشارکت: ${this.finalResults.participationRate}%\n#انتخابات_فرهنگیان`;
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
};
</script>

<style scoped>
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