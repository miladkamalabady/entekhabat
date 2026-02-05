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
              <div class="time-unit">
                <span class="time-value">{{ timeRemaining.days }}</span>
                <span class="time-label">روز</span>
              </div>
              <div class="time-separator">:</div>
              <div class="time-unit">
                <span class="time-value">{{ timeRemaining.hours }}</span>
                <span class="time-label">ساعت</span>
              </div>
              <div class="time-separator">:</div>
              <div class="time-unit">
                <span class="time-value">{{ timeRemaining.minutes }}</span>
                <span class="time-label">دقیقه</span>
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
        <b-col cols="6" md="3">
          <b-card class="stat-card text-center">
            <div class="stat-icon voters-icon">
              <b-icon icon="people-fill"></b-icon>
            </div>
            <div class="stat-number">{{ formatNumber(totalVoters) }}</div>
            <div class="stat-label">کل واجدین شرایط</div>
            <div class="stat-change text-success">
              <b-icon icon="arrow-up"></b-icon>
              {{ voterParticipation }}% مشارکت
            </div>
          </b-card>
        </b-col>
        
        <b-col cols="6" md="3">
          <b-card class="stat-card text-center">
            <div class="stat-icon vote-icon">
              <b-icon icon="check-circle-fill"></b-icon>
            </div>
            <div class="stat-number">{{ formatNumber(totalVotes) }}</div>
            <div class="stat-label">آرای ثبت شده</div>
            <div class="stat-change">
              <b-icon icon="clock-history"></b-icon>
              آخرین بروزرسانی: {{ lastUpdate }}
            </div>
          </b-card>
        </b-col>
        
        <b-col cols="6" md="3">
          <b-card class="stat-card text-center">
            <div class="stat-icon candidate-icon">
              <b-icon icon="person-badge-fill"></b-icon>
            </div>
            <div class="stat-number">{{ candidates.length }}</div>
            <div class="stat-label">کاندیداها</div>
            <div class="stat-change text-info">
              <b-icon icon="person-plus"></b-icon>
              {{ activeCandidates }} کاندیدای فعال
            </div>
          </b-card>
        </b-col>
        
        <b-col cols="6" md="3">
          <b-card class="stat-card text-center">
            <div class="stat-icon progress-icon">
              <b-icon icon="graph-up"></b-icon>
            </div>
            <div class="stat-number">{{ participationRate }}%</div>
            <div class="stat-label">میزان مشارکت</div>
            <b-progress
              :value="participationRate"
              :max="100"
              height="6px"
              class="mt-2"
              variant="success"
            ></b-progress>
          </b-card>
        </b-col>
      </b-row>

      <!-- Real-time Updates -->
      <b-card class="mb-4 realtime-card">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h5 class="mb-0">
            <b-icon icon="lightning-fill" variant="warning" class="ml-2"></b-icon>
            به‌روزرسانی‌های لحظه‌ای
          </h5>
          <div class="update-rate">
            <b-badge variant="info">
              هر ۳۰ ثانیه بروزرسانی می‌شود
            </b-badge>
          </div>
        </div>
        
        <div class="realtime-updates">
          <div
            v-for="update in realtimeUpdates"
            :key="update.id"
            class="update-item"
            :class="`update-${update.type}`"
          >
            <div class="update-time">{{ update.time }}</div>
            <div class="update-content">
              <b-icon :icon="getUpdateIcon(update.type)" class="ml-2"></b-icon>
              {{ update.message }}
            </div>
          </div>
        </div>
        
        <div class="text-center mt-3">
          <b-button
            variant="outline-primary"
            size="sm"
            @click="loadMoreUpdates"
            :disabled="loadingUpdates"
          >
            <b-spinner small v-if="loadingUpdates" class="ml-1"></b-spinner>
            بارگذاری بیشتر
          </b-button>
        </div>
      </b-card>

      <!-- Main Dashboard -->
      <b-row class="mb-4">
        <!-- Candidates Ranking -->
        <b-col lg="8" class="mb-4">
          <b-card class="ranking-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="mb-0">رتبه‌بندی کاندیداها</h5>
              <div class="ranking-actions">
                <b-button-group size="sm">
                  <b-button
                    :variant="rankingView === 'table' ? 'primary' : 'outline-primary'"
                    @click="rankingView = 'table'"
                  >
                    <b-icon icon="table"></b-icon>
                  </b-button>
                  <b-button
                    :variant="rankingView === 'chart' ? 'primary' : 'outline-primary'"
                    @click="rankingView = 'chart'"
                  >
                    <b-icon icon="bar-chart-fill"></b-icon>
                  </b-button>
                </b-button-group>
              </div>
            </div>
            
            <!-- Table View -->
            <div v-if="rankingView === 'table'" class="table-responsive">
              <b-table
                :items="sortedCandidates"
                :fields="candidateFields"
                striped
                hover
                class="text-right"
              >
                <template #cell(rank)="data">
                  <div class="rank-badge" :class="`rank-${data.index + 1}`">
                    {{ data.index + 1 }}
                  </div>
                </template>
                
                <template #cell(candidate)="data">
                  <div class="candidate-info">
                    <img
                      v-if="data.item.photo"
                      :src="data.item.photo"
                      class="candidate-photo"
                      :alt="data.item.name"
                    />
                    <div v-else class="candidate-photo placeholder">
                      <b-icon icon="person-circle"></b-icon>
                    </div>
                    <div class="candidate-details">
                      <strong>{{ data.item.name }}</strong>
                      <small class="text-muted d-block">{{ data.item.position }}</small>
                    </div>
                  </div>
                </template>
                
                <template #cell(votes)="data">
                  <div class="votes-info">
                    <div class="votes-count">{{ formatNumber(data.item.votes) }}</div>
                    <div class="votes-percentage">
                      <b-progress
                        :value="data.item.votes"
                        :max="maxVotes"
                        height="4px"
                        class="votes-progress"
                      ></b-progress>
                      <small>{{ ((data.item.votes / totalVotes) * 100).toFixed(1) }}%</small>
                    </div>
                  </div>
                </template>
                
                <template #cell(trend)="data">
                  <div class="trend-indicator" :class="getTrendClass(data.item.trend)">
                    <b-icon :icon="getTrendIcon(data.item.trend)"></b-icon>
                    {{ data.item.trend }}%
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
              <div
                v-for="region in regions"
                :key="region.id"
                class="region-item"
                @click="viewRegionDetails(region)"
              >
                <div class="region-name">{{ region.name }}</div>
                <div class="region-stats">
                  <div class="region-progress">
                    <b-progress
                      :value="region.participation"
                      :max="100"
                      height="6px"
                      class="mb-1"
                    ></b-progress>
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

      <!-- Real-time Chart -->
      <b-card class="mb-4">
        <h5 class="mb-3">نمودار لحظه‌ای آرای ثبت شده</h5>
        <div class="chart-wrapper-large">
          <canvas ref="realTimeChart"></canvas>
        </div>
        <div class="chart-controls text-center mt-3">
          <b-button-group>
            <b-button
              :variant="timeRange === '1h' ? 'primary' : 'outline-primary'"
              @click="changeTimeRange('1h')"
              size="sm"
            >
              ۱ ساعت
            </b-button>
            <b-button
              :variant="timeRange === '6h' ? 'primary' : 'outline-primary'"
              @click="changeTimeRange('6h')"
              size="sm"
            >
              ۶ ساعت
            </b-button>
            <b-button
              :variant="timeRange === '24h' ? 'primary' : 'outline-primary'"
              @click="changeTimeRange('24h')"
              size="sm"
            >
              ۲۴ ساعت
            </b-button>
          </b-button-group>
        </div>
      </b-card>

      <!-- Voting Locations Status -->
      <b-row class="mb-4">
        <b-col md="6" class="mb-4">
          <b-card>
            <h5 class="mb-3">وضعیت شعبه‌های رأی‌گیری</h5>
            <div class="locations-status">
              <div class="status-item">
                <div class="status-indicator active"></div>
                <div class="status-info">
                  <div class="status-count">{{ activeLocations }}</div>
                  <div class="status-label">شعبه فعال</div>
                </div>
              </div>
              <div class="status-item">
                <div class="status-indicator busy"></div>
                <div class="status-info">
                  <div class="status-count">{{ busyLocations }}</div>
                  <div class="status-label">شلوغ</div>
                </div>
              </div>
              <div class="status-item">
                <div class="status-indicator offline"></div>
                <div class="status-info">
                  <div class="status-count">{{ offlineLocations }}</div>
                  <div class="status-label">غیرفعال</div>
                </div>
              </div>
            </div>
            
            <div class="mt-3">
              <b-button
                variant="outline-info"
                size="sm"
                block
                @click="showLocationsMap"
              >
                <b-icon icon="geo-alt" class="ml-1"></b-icon>
                مشاهده روی نقشه
              </b-button>
            </div>
          </b-card>
        </b-col>
        
        <b-col md="6">
          <b-card>
            <h5 class="mb-3">پیش‌بینی نهایی</h5>
            <div class="prediction">
              <div class="prediction-item">
                <div class="prediction-label">برآورد مشارکت نهایی:</div>
                <div class="prediction-value">{{ finalParticipationPrediction }}%</div>
              </div>
              <div class="prediction-item">
                <div class="prediction-label">ساعات اوج مشارکت:</div>
                <div class="prediction-value">۱۰:۰۰ - ۱۴:۰۰</div>
              </div>
              <div class="prediction-item">
                <div class="prediction-label">پیش‌بینی زمان اعلام نتایج:</div>
                <div class="prediction-value">ساعت ۲۴:۰۰</div>
              </div>
            </div>
            
            <div class="alert alert-info mt-3">
              <b-icon icon="info-circle" class="ml-1"></b-icon>
              این اطلاعات بر اساس الگوهای مشارکت پیشین برآورد شده است.
            </div>
          </b-card>
        </b-col>
      </b-row>

      <!-- Social Feed -->
      <b-card class="social-feed-card">
        <h5 class="mb-3">
          <b-icon icon="chat-left-text-fill" class="ml-2"></b-icon>
          واکنش‌های کاربران
        </h5>
        
        <div class="social-feed">
          <div
            v-for="feed in socialFeed"
            :key="feed.id"
            class="feed-item"
            :class="`sentiment-${feed.sentiment}`"
          >
            <div class="feed-header">
              <img
                v-if="feed.avatar"
                :src="feed.avatar"
                class="feed-avatar"
                alt="کاربر"
              />
              <div class="feed-user">
                <strong>{{ feed.user }}</strong>
                <small class="text-muted">{{ feed.time }}</small>
              </div>
              <b-badge :variant="getSentimentBadge(feed.sentiment)" class="mr-auto">
                {{ getSentimentText(feed.sentiment) }}
              </b-badge>
            </div>
            <div class="feed-content">{{ feed.content }}</div>
            <div class="feed-footer">
              <div class="feed-actions">
                <span class="mr-3">
                  <b-icon icon="hand-thumbs-up"></b-icon>
                  {{ feed.likes }}
                </span>
                <span class="mr-3">
                  <b-icon icon="chat"></b-icon>
                  {{ feed.comments }}
                </span>
                <span>
                  <b-icon icon="share"></b-icon>
                  اشتراک‌گذاری
                </span>
              </div>
            </div>
          </div>
        </div>
      </b-card>
    </b-container>

    <!-- Region Details Modal -->
    <b-modal
      v-model="showRegionModal"
      :title="selectedRegion ? selectedRegion.name : ''"
      size="lg"
      hide-footer
      centered
      scrollable
    >
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
          <b-col md="6">
            <div class="detail-item">
              <strong>شعبه‌های فعال:</strong>
              <span>{{ selectedRegion.activeLocations }} شعبه</span>
            </div>
            <div class="detail-item">
              <strong>کاندیدای برتر:</strong>
              <span>{{ selectedRegion.topCandidate }}</span>
            </div>
            <div class="detail-item">
              <strong>رشد مشارکت:</strong>
              <span :class="`text-${selectedRegion.growth >= 0 ? 'success' : 'danger'}`">
                {{ selectedRegion.growth >= 0 ? '+' : '' }}{{ selectedRegion.growth }}%
              </span>
            </div>
          </b-col>
        </b-row>
        
        <h6 class="mb-3">توزیع آرا بین کاندیداها</h6>
        <div class="candidates-distribution">
          <div
            v-for="candidate in selectedRegion.candidates"
            :key="candidate.id"
            class="distribution-item"
          >
            <div class="candidate-name">{{ candidate.name }}</div>
            <div class="distribution-bar">
              <div
                class="bar-fill"
                :style="{ width: candidate.percentage + '%' }"
              ></div>
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
    <b-button
      variant="primary"
      class="refresh-button"
      @click="manualRefresh"
      :disabled="refreshing"
    >
      <b-spinner small v-if="refreshing" class="ml-1"></b-spinner>
      <b-icon v-else icon="arrow-clockwise" class="ml-1"></b-icon>
      بروزرسانی
    </b-button>
  </div>
</template>

<script>
import Chart from "chart.js";

export default {
  name: "LiveElectionDashboard",
  data() {
    return {
      // Timer
      electionEndTime: new Date(Date.now() + 24 * 60 * 60 * 1000), // 24 hours from now
      timeRemaining: {
        days: 0,
        hours: 0,
        minutes: 0,
        seconds: 0
      },
      
      // Statistics
      totalVoters: 150000,
      totalVotes: 87542,
      lastUpdate: 'لحظاتی پیش',
      participationRate: 58.4,
      voterParticipation: 65.2,
      activeCandidates: 8,
      
      // Candidates Data
      candidates: [
        {
          id: 1,
          name: 'دکتر محمدرضا احمدی',
          position: 'استاد دانشگاه - علوم تربیتی',
          photo: 'assets/img/avatars/image1.png?text=MA',
          votes: 24560,
          trend: 2.5,
          color: '#3F51B5'
        },
        {
          id: 2,
          name: 'مهندس سید علی حسینی',
          position: 'مدیر آموزش و پرورش منطقه ۵',
          photo: 'assets/img/avatars/image2.png?text=SH',
          votes: 19875,
          trend: 1.8,
          color: '#4CAF50'
        },
        {
          id: 3,
          name: 'دکتر فاطمه کریمی',
          position: 'معاون پژوهشی - پژوهشگاه مطالعات',
          photo: 'assets/img/avatars/image3.png?text=FK',
          votes: 15680,
          trend: -0.5,
          color: '#FF9800'
        },
        {
          id: 4,
          name: 'دکتر مهدی محمدی',
          position: 'رئیس اسبق صندوق ذخیره',
          photo: 'assets/img/avatars/image4.png?text=MM',
          votes: 9875,
          trend: 3.2,
          color: '#9C27B0'
        },
        {
          id: 5,
          name: 'مهندس رضا نوروزی',
          position: 'مدیر فناوری اطلاعات آموزش و پرورش',
          photo: 'assets/img/avatars/image5.png?text=RN',
          votes: 7654,
          trend: 1.2,
          color: '#2196F3'
        }
      ],
      
      // Regions Data
      regions: [
        { id: 1, name: 'تهران', votes: 25480, participation: 72.5, eligibleVoters: 35100, activeLocations: 45, topCandidate: 'دکتر محمدرضا احمدی', growth: 2.1 },
        { id: 2, name: 'مشهد', votes: 12450, participation: 65.3, eligibleVoters: 19050, activeLocations: 28, topCandidate: 'مهندس سید علی حسینی', growth: 1.8 },
        { id: 3, name: 'اصفهان', votes: 9870, participation: 61.2, eligibleVoters: 16100, activeLocations: 22, topCandidate: 'دکتر فاطمه کریمی', growth: 0.9 },
        { id: 4, name: 'شیراز', votes: 7650, participation: 58.7, eligibleVoters: 13020, activeLocations: 18, topCandidate: 'دکتر محمدرضا احمدی', growth: 2.5 },
        { id: 5, name: 'تبریز', votes: 6540, participation: 55.4, eligibleVoters: 11800, activeLocations: 16, topCandidate: 'مهندس سید علی حسینی', growth: 1.2 }
      ],
      
      // Voting Locations
      activeLocations: 85,
      busyLocations: 32,
      offlineLocations: 3,
      
      // Realtime Updates
      realtimeUpdates: [
        { id: 1, type: 'vote', time: '۲ دقیقه پیش', message: '۱۰۰ رأی جدید از استان تهران ثبت شد' },
        { id: 2, type: 'announcement', time: '۵ دقیقه پیش', message: 'شعبه رأی‌گیری منطقه ۳ به حالت فعال بازگشت' },
        { id: 3, type: 'candidate', time: '۱۰ دقیقه پیش', message: 'دکتر احمدی در منطقه ۵ در حال پیشی گرفتن است' },
        { id: 4, type: 'system', time: '۱۵ دقیقه پیش', message: 'سیستم با موفقیت به‌روزرسانی شد' },
        { id: 5, type: 'milestone', time: '۲۰ دقیقه پیش', message: 'میزان مشارکت به ۵۰٪ رسید' }
      ],
      
      // Social Feed
      socialFeed: [
        { id: 1, user: 'علی محمدی', avatar: 'assets/img/avatars/image6.png?text=AM', time: '۵ دقیقه پیش', content: 'بسیار انتخابات پر رقابتی شکل گرفته!', sentiment: 'positive', likes: 24, comments: 5 },
        { id: 2, user: 'فاطمه رضایی', avatar: 'assets/img/avatars/image7.png?text=FR', time: '۱۲ دقیقه پیش', content: 'سیستم رأی‌گیری الکترونیکی بسیار روان کار می‌کند', sentiment: 'positive', likes: 18, comments: 3 },
        { id: 3, user: 'محمد حسینی', avatar: 'assets/img/avatars/image8.png?text=MH', time: '۲۵ دقیقه پیش', content: 'امیدوارم نتایج به نفع فرهنگیان باشد', sentiment: 'neutral', likes: 12, comments: 8 },
        { id: 4, user: 'سارا کریمی', avatar: 'assets/img/avatars/imagen7.png?text=SK', time: '۴۰ دقیقه پیش', content: 'شعبه ما کمی شلوغ بود اما روند سریعی داشت', sentiment: 'neutral', likes: 8, comments: 2 }
      ],
      
      // UI State
      rankingView: 'table',
      timeRange: '6h',
      showRegionModal: false,
      selectedRegion: null,
      autoRefresh: true,
      refreshing: false,
      loadingUpdates: false,
      
      // Chart Instances
      votesChart: null,
      realTimeChart: null,
      
      // Table Fields
      candidateFields: [
        { key: 'rank', label: 'رتبه', sortable: false },
        { key: 'candidate', label: 'کاندیدا', sortable: false },
        { key: 'votes', label: 'آرا', sortable: true },
        { key: 'trend', label: 'روند', sortable: true }
      ],
      
      // Predictions
      finalParticipationPrediction: 68
    };
  },
  computed: {
    sortedCandidates() {
      return [...this.candidates].sort((a, b) => b.votes - a.votes);
    },
    
    maxVotes() {
      return Math.max(...this.candidates.map(c => c.votes));
    }
  },
  mounted() {
    this.startTimer();
    this.initializeCharts();
    this.startAutoRefresh();
  },
  beforeDestroy() {
    clearInterval(this.timerInterval);
    clearInterval(this.refreshInterval);
    if (this.votesChart) this.votesChart.destroy();
    if (this.realTimeChart) this.realTimeChart.destroy();
  },
  methods: {
    // Timer Functions
    startTimer() {
      this.updateTimer();
      this.timerInterval = setInterval(this.updateTimer, 1000);
    },
    
    updateTimer() {
      const now = new Date();
      const diff = this.electionEndTime - now;
      
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
    
    // Chart Functions
    initializeCharts() {
      this.createVotesChart();
      this.createRealTimeChart();
    },
    
    createVotesChart() {
      const ctx = this.$refs.votesChart?.getContext('2d');
      if (!ctx) return;
      
      if (this.votesChart) this.votesChart.destroy();
      
      const labels = this.sortedCandidates.map(c => c.name.split(' ').pop());
      const data = this.sortedCandidates.map(c => c.votes);
      const colors = this.sortedCandidates.map(c => c.color);
      
      this.votesChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: labels,
          datasets: [{
            label: 'تعداد آرا',
            data: data,
            backgroundColor: colors,
            borderColor: colors.map(c => c + 'CC'),
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
                  return `${candidate.name}: ${this.formatNumber(context.raw)} رأی`;
                }
              }
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'تعداد آرا'
              }
            },
            x: {
              title: {
                display: true,
                text: 'کاندیداها'
              }
            }
          }
        }
      });
    },
    
    createRealTimeChart() {
      const ctx = this.$refs.realTimeChart?.getContext('2d');
      if (!ctx) return;
      
      if (this.realTimeChart) this.realTimeChart.destroy();
      
      // Generate time labels based on selected range
      const labels = this.generateTimeLabels();
      const data = this.generateVoteData(labels.length);
      
      this.realTimeChart = new Chart(ctx, {
        type: 'line',
        data: {
          labels: labels,
          datasets: [{
            label: 'آرای ثبت شده',
            data: data,
            borderColor: '#4CAF50',
            backgroundColor: 'rgba(76, 175, 80, 0.1)',
            borderWidth: 3,
            tension: 0.4,
            fill: true
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false }
          },
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'تعداد آرا'
              }
            },
            x: {
              title: {
                display: true,
                text: 'زمان'
              }
            }
          }
        }
      });
    },
    
    generateTimeLabels() {
      const now = new Date();
      const labels = [];
      const count = this.timeRange === '1h' ? 12 : this.timeRange === '6h' ? 24 : 48;
      
      for (let i = count - 1; i >= 0; i--) {
        const time = new Date(now - i * 30 * 60 * 1000); // Every 30 minutes
        labels.push(time.getHours().toString().padStart(2, '0') + ':' + 
                   time.getMinutes().toString().padStart(2, '0'));
      }
      
      return labels;
    },
    
    generateVoteData(count) {
      const data = [];
      let base = 80000;
      
      for (let i = 0; i < count; i++) {
        const increment = Math.floor(Math.random() * 500) + 100;
        base += increment;
        data.push(base);
      }
      
      return data;
    },
    
    changeTimeRange(range) {
      this.timeRange = range;
      setTimeout(() => {
        this.createRealTimeChart();
      }, 100);
    },
    
    // Data Functions
    formatNumber(num) {
      return new Intl.NumberFormat('fa-IR').format(num);
    },
    
    getTrendClass(trend) {
      if (trend > 0) return 'trend-up';
      if (trend < 0) return 'trend-down';
      return 'trend-neutral';
    },
    
    getTrendIcon(trend) {
      if (trend > 0) return 'arrow-up';
      if (trend < 0) return 'arrow-down';
      return 'dash';
    },
    
    getUpdateIcon(type) {
      const icons = {
        vote: 'check-circle',
        announcement: 'megaphone',
        candidate: 'person',
        system: 'gear',
        milestone: 'trophy'
      };
      return icons[type] || 'info-circle';
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
    
    loadMoreUpdates() {
      this.loadingUpdates = true;
      setTimeout(() => {
        const newUpdate = {
          id: this.realtimeUpdates.length + 1,
          type: 'vote',
          time: 'همین لحظه',
          message: '۵۰ رأی جدید از استان اصفهان ثبت شد'
        };
        this.realtimeUpdates.unshift(newUpdate);
        this.loadingUpdates = false;
      }, 1000);
    },
    
    // Refresh Functions
    startAutoRefresh() {
      this.refreshInterval = setInterval(() => {
        if (this.autoRefresh) {
          this.simulateDataUpdate();
        }
      }, 30000); // Every 30 seconds
    },
    
    simulateDataUpdate() {
      // Simulate new votes
      const voteIncrement = Math.floor(Math.random() * 100) + 50;
      this.totalVotes += voteIncrement;
      
      // Update candidates votes randomly
      this.candidates.forEach(candidate => {
        const increment = Math.floor(Math.random() * 20) + 5;
        candidate.votes += increment;
        candidate.trend = (Math.random() * 4 - 2).toFixed(1);
      });
      
      // Update participation rate
      this.participationRate = Math.min(100, ((this.totalVotes / this.totalVoters) * 100).toFixed(1));
      
      // Update regions
      this.regions.forEach(region => {
        region.votes += Math.floor(Math.random() * 50) + 20;
        region.participation = Math.min(100, ((region.votes / region.eligibleVoters) * 100).toFixed(1));
      });
      
      // Update last update time
      this.lastUpdate = 'لحظاتی پیش';
      
      // Refresh charts
      if (this.votesChart) {
        this.votesChart.data.datasets[0].data = this.sortedCandidates.map(c => c.votes);
        this.votesChart.update('none');
      }
      
      // Add realtime update
      const update = {
        id: this.realtimeUpdates.length + 1,
        type: 'vote',
        time: 'همین لحظه',
        message: `${voteIncrement} رأی جدید ثبت شد`
      };
      this.realtimeUpdates.unshift(update);
      if (this.realtimeUpdates.length > 10) {
        this.realtimeUpdates.pop();
      }
    },
    
    manualRefresh() {
      this.refreshing = true;
      this.simulateDataUpdate();
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
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
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

.rank-4, .rank-5 {
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

.trend-indicator {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.9rem;
  font-weight: 500;
}

.trend-up {
  background: rgba(76, 175, 80, 0.1);
  color: #2E7D32;
}

.trend-down {
  background: rgba(244, 67, 54, 0.1);
  color: #C62828;
}

.trend-neutral {
  background: rgba(158, 158, 158, 0.1);
  color: #616161;
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