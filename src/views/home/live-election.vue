<template>
  <div class="live-election-page">
    <!-- Header with Timer -->
    <b-container fluid class="election-header py-4" v-if="electionStatusAll == 'active'">
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
    <b-container class="election-container" v-if="electionStatusAll == 'active'">
      <b-card class="mb-4 report-type-card">
        <div class="d-flex flex-wrap align-items-center justify-content-between">
          <div>
            <h5 class="mb-1">نوع گزارش زنده</h5>
            <small class="text-muted">یکی از نماهای تحلیلی زنده را انتخاب کنید.</small>
          </div>
          <b-form-radio-group v-model="selectedReportType" :options="availableReportTypes"
            button-variant="outline-primary" buttons name="report-type-radio" class="report-type-switch mt-2 mt-md-0" />
        </div>
        <small class="text-muted d-block mt-2">{{ selectedReportTypeLabel }}</small>
        <b-row class="report-highlight-row mt-3">
          <b-col v-for="highlight in selectedReportHighlights" :key="highlight.label" cols="12" md="4"
            class="mb-2 mb-md-0">
            <div class="report-highlight" :class="highlight.variant">
              <b-icon :icon="highlight.icon" class="report-highlight-icon"></b-icon>
              <div>
                <div class="report-highlight-value">{{ highlight.value }}</div>
                <div class="report-highlight-label">{{ highlight.label }}</div>
              </div>
            </div>
          </b-col>
        </b-row>
      </b-card>
      <!-- Quick Stats -->
      <b-row class="mb-4">
        <b-col cols="6" md="3">
          <b-card class="stat-card text-center">
            <div class="stat-icon voters-icon">
              <b-icon icon="people-fill"></b-icon>
            </div>
            <div class="stat-number">{{ formatNumber(infoVote?.totalVoters) }}</div>
            <div class="stat-label">کل واجدین شرایط</div>
            <div class="stat-change text-success">
              <b-icon icon="arrow-up"></b-icon>
              {{ safeParticipation }}% مشارکت
            </div>
          </b-card>
        </b-col>

        <b-col cols="6" md="3">
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

        <b-col cols="6" md="3">
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
        <b-col cols="6" md="3">
          <b-card class="stat-card text-center">
            <div class="stat-icon progress-icon">
              <b-icon icon="person-check-fill"></b-icon>
            </div>

            <div class="stat-number">
              {{ formatNumber(infoVote?.participants) }}
            </div>

            <div class="stat-label">
              افراد شرکت‌کننده در رأی‌گیری
            </div>

            <div class="stat-change text-info">
              میانگین انتخاب هر نفر:
              {{ avgVotesPerPerson }}
            </div>
          </b-card>
        </b-col>

      </b-row>


      <!-- Main Dashboard -->
      <b-row class="mb-4">
        <!-- Candidates Ranking -->
        <b-col lg="12" class="mb-4" v-if="showCandidateReport">
          <b-card class="ranking-card">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="mb-0">رتبه‌بندی زنده کاندیداها</h5>
            </div>

            <!-- Table View -->
            <div class="table-responsive">
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

              </b-table>
            </div>

          </b-card>
        </b-col>

        <!-- Voting Progress by Region -->
        <b-col lg="12" class="mb-4" v-if="showProvinceReport">
          <b-card class="region-card">
            <h5 class="mb-4">{{ selectedReportType === "admin" ? "گزارش تجمیعی ادمین" : getReportSectionTitle() }}</h5>

            <div class="region-list">
              <div v-for="region in regionDisplayData" :key="region.id" class="region-item"
                @click="viewRegionDetails(region)">
                <div>
                  <div class="region-name">{{ region.name }}</div>
                  <small class="text-muted" v-if="selectedReportType === 'participation'">
                    {{ region.participationNote }}
                  </small>
                </div>
                <div class="region-stats">
                  <div class="region-progress">
                    <b-progress :value="region.participation" :max="100" height="6px" class="mb-1"></b-progress>
                    <small class="text-muted">{{ region.participation }}%</small>
                  </div>
                  <div class="region-votes">
                    {{ formatNumber(region.votes) }} رأی
                  </div>
                  <div class="region-eligible text-muted" style="font-size:11px;">
                    {{ formatNumber(region.eligibleVoters) }} واجد شرایط
                  </div>
                </div>
              </div>
            </div>

          </b-card>
        </b-col>

        <b-col lg="12" class="mb-4" v-if="selectedReportType === 'area'">
          <b-card class="area-report-card">
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap">
              <h5 class="mb-2 mb-md-0">گزارش تفکیکی مناطق</h5>
              <small class="text-muted">مناطق بر اساس تعداد رأی ثبت‌شده مرتب شده‌اند.</small>
            </div>
            <div class="table-responsive">
              <b-table :items="areaReportRows" :fields="areaReportFields" striped hover class="text-right mb-0">
                <template #cell(votes)="data">
                  {{ formatNumber(data.item.votes) }}
                </template>
                <template #cell(participation)="data">
                  <b-badge :variant="getParticipationVariant(data.item.participation)">{{ data.item.participation
                    }}%</b-badge>
                </template>
              </b-table>
            </div>
          </b-card>
        </b-col>

        <b-col lg="12" class="mb-4" v-if="selectedReportType === 'participation'">
          <b-card class="participation-card">
            <h5 class="mb-4">نبض مشارکت لحظه‌ای</h5>
            <b-row>
              <b-col md="4" class="mb-3" v-for="item in participationPulseCards" :key="item.title">
                <div class="pulse-card" :class="item.variant">
                  <b-icon :icon="item.icon" class="pulse-card-icon"></b-icon>
                  <div class="pulse-card-title">{{ item.title }}</div>
                  <div class="pulse-card-value">{{ item.value }}</div>
                  <small>{{ item.description }}</small>
                </div>
              </b-col>
            </b-row>
          </b-card>
        </b-col>
        <!-- جایگزین بخش ایران Heatmap Grid با نقشه SVG -->
        <b-col lg="12" class="mb-4" v-if="showHeatmapReport">
          <b-card class="iran-heatmap-card">
            <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap">
              <h5 class="mb-2 mb-md-0">🗺️ نقشه حرارتی ایران (استان‌ها)</h5>
              <small class="text-muted">با حرکت موس روی هر استان، جزئیات نمایش داده می‌شود</small>
            </div>

            <!-- نقشه SVG ایران - inline از ir.svg با رنگ‌بندی تعاملی -->
            <div class="iran-map-wrapper" ref="iranMapWrapper" v-html="coloredIranMapSvg"
              @mousemove="onMapMouseMove" @mouseleave="onProvinceLeave" @click="onMapClick">
            </div>
            <!-- tooltip استان hover شده -->
            <div v-if="hoveredProvince" class="province-tooltip" :style="tooltipStyle">
              <strong>{{ hoveredProvince.name }}</strong><br/>
              مشارکت: {{ hoveredProvince.participation }}%
            </div>

            <!-- خلاصه آماری استان انتخاب شده -->
            <div v-if="selectedProvinceForMap" class="selected-province-stats mt-4">
              <b-alert :variant="getParticipationVariant(getProvinceParticipation(selectedProvinceForMap))" show
                class="mb-0">
                <div class="d-flex justify-content-between align-items-center flex-wrap">
                  <div>
                    <strong class="fs-5">{{ selectedProvinceForMap.name }}</strong>
                    <div class="mt-2">
                      <span class="badge bg-light text-dark me-2">مشارکت: {{
                        getProvinceParticipation(selectedProvinceForMap)
                        }}%</span>
                      <span class="badge bg-light text-dark me-2">آرا: {{
                        formatNumber(getProvinceVotes(selectedProvinceForMap))
                        }}</span>
                      <span class="badge bg-light text-dark">مناطق: {{ getProvinceAreasCount(selectedProvinceForMap)
                        }}</span>
                    </div>
                  </div>
                  <b-button size="sm" variant="outline-primary" @click="viewProvinceDetails(selectedProvinceForMap)">
                    مشاهده جزئیات استان
                  </b-button>
                </div>
              </b-alert>
            </div>
          </b-card>
        </b-col>
      </b-row>
    </b-container>
    <b-container fluid class="py-4 text-center" v-else-if="loadingElectionStatus">
      <b-spinner variant="primary" label="در حال بررسی وضعیت انتخابات"></b-spinner>
      <div class="mt-3">در حال بررسی وضعیت انتخابات...</div>
    </b-container>
    <b-container fluid class="py-4" v-else>
      <b-alert show class="text-center" variant="danger">انتخابات فعال نمی‌باشد!</b-alert>
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

        <h6 class="mb-3">توزیع آرا بر اساس مناطق استان</h6>
        <div class="candidates-distribution">
          <div v-for="area in selectedRegion.areas" :key="area.id">
            <div class="distribution-item">
              <div class="candidate-name">{{ area.name }}</div>
              <div class="distribution-bar">
                <div class="bar-fill" :style="{ width: area.percentage + '%' }"></div>
              </div>
              <div class="distribution-percentage">{{ formatNumber(area.votes) }} رأی ({{ area.percentage }}%)</div>
            </div>
            <div v-if="selectedReportType === 'admin'" class="mt-4">
              <h6 class="mb-3">آمار کاندیداها در {{ area.name }}</h6>
              <div v-if="getAreaCandidateStats(area).length" class="table-responsive">
                <b-table small striped hover :items="getAreaCandidateStats(area)" :fields="regionCandidateFields"
                  class="text-right mb-0">
                  <template #cell(candidate_id)="data">
                    {{ data.item.candidate_id }}
                  </template>
                  <template #cell(candidate)="data">
                    {{ data.item.first_name }} {{ data.item.last_name }}
                  </template>
                </b-table>
              </div>
              <p v-else class="text-muted mb-0">برای {{ area.name }} هنوز آماری از کاندیداها ثبت نشده است.</p>
            </div>
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
import { apiUrlrtb } from '../../constants/config'
import { mapGetters, mapActions, mapMutations } from "vuex";

export default {
  name: "LiveElectionDashboard",
  data() {
    return {
      // نقشه
      hoveredProvinceId: null,
      selectedProvinceId: null,
      selectedProvinceForMap: null,
      hoveredProvince: null,
      tooltipStyle: {},
      iranSvgContent: '',

      // نگاشت کد استان IR به شناسه و نام فارسی
      // کلید = id path در ir.svg  |  id = ProvinceCode در دیتابیس
      irCodeMap: {
        'IR01': { id: 18, name: 'آذربایجان شرقی' },
        'IR02': { id: 29, name: 'آذربایجان غربی' },
        'IR03': { id: 19, name: 'اردبیل' },
        'IR04': { id: 17, name: 'اصفهان' },
        'IR05': { id: 34, name: 'ایلام' },
        'IR06': { id: 51, name: 'بوشهر' },
        'IR07': { id: 11, name: 'تهران' },
        'IR08': { id: 31, name: 'چهارمحال و بختیاری' },
        'IR09': { id: 24, name: 'البرز' },
        'IR10': { id: 36, name: 'خوزستان' },
        'IR11': { id: 57, name: 'زنجان' },
        'IR12': { id: 60, name: 'سمنان' },
        'IR13': { id: 49, name: 'سیستان و بلوچستان' },
        'IR14': { id: 23, name: 'فارس' },
        'IR15': { id: 38, name: 'کرمان' },
        'IR16': { id: 58, name: 'کردستان' },
        'IR17': { id: 35, name: 'کرمانشاه' },
        'IR18': { id: 42, name: 'کهگیلویه و بویراحمد' },
        'IR19': { id: 37, name: 'گیلان' },
        'IR20': { id: 54, name: 'لرستان' },
        'IR21': { id: 20, name: 'مازندران' },
        'IR22': { id: 15, name: 'مرکزی' },
        'IR23': { id: 50, name: 'هرمزگان' },
        'IR24': { id: 55, name: 'همدان' },
        'IR25': { id: 44, name: 'یزد' },
        'IR26': { id: 25, name: 'قم' },
        'IR27': { id: 27, name: 'گلستان' },
        'IR28': { id: 21, name: 'خراسان شمالی' },
        'IR29': { id: 22, name: 'خراسان جنوبی' },
        'IR30': { id: 16, name: 'خراسان رضوی' },
      },
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
      regions: [],
      areasByProvince: {},


      // UI State
      loadingElectionStatus: true,
      selectedReportType: 'province',
      reportTypeOptions: [
        { value: 'province', text: 'گزارش استانی' },
        { value: 'area', text: 'گزارش مناطق' },
        { value: 'candidate', text: 'گزارش کاندیداها' },
        { value: 'participation', text: 'نبض مشارکت' },
        { value: 'heatmap', text: 'نقشه حرارتی' },
        { value: 'admin', text: 'گزارش ادمین' }
      ],
      showRegionModal: false,
      selectedRegion: null,
      autoRefresh: true,
      refreshing: false,
      hoveredProvince: null,

      // Chart Instances
      realTimeChart: null,

      // Table Fields
      candidateFields: [
        { key: 'rank', label: 'رتبه', sortable: false },
        { key: 'candidate', label: 'کاندیدا', sortable: false },
      ],
      regionCandidateFields: [
        { key: 'codeentekhabati', label: 'کدکاندید', sortable: false },
        { key: 'candidate', label: 'کاندیدا', sortable: false }
      ], areaReportFields: [
        { key: 'provinceName', label: 'استان', sortable: true },
        { key: 'name', label: 'منطقه', sortable: true },
        { key: 'votes', label: 'آرای ثبت شده', sortable: true },
        { key: 'participation', label: 'مشارکت', sortable: true }
      ],

      // Predictions
      finalParticipationPrediction: 68
    };
  },
  computed: {
    ...mapGetters(["ConfigInfo", "currentUser", "electionStatusAll"]),
    avgVotesPerPerson() {
      if (!this.infoVote || !this.infoVote.participants) return 0;
      return (this.infoVote.totalVotes / this.infoVote.participants).toFixed(2);
    },
    coloredIranMapSvg() {
      if (!this.iranSvgContent || typeof DOMParser === 'undefined') return this.iranSvgContent || '';
      const parser = new DOMParser();
      const doc = parser.parseFromString(this.iranSvgContent, 'image/svg+xml');
      const svgEl = doc.querySelector('svg');
      if (!svgEl) return this.iranSvgContent;

      svgEl.removeAttribute('fill');
      svgEl.setAttribute('class', 'iran-map-svg');

      Object.entries(this.irCodeMap).forEach(([irCode, info]) => {
        const path = doc.getElementById(irCode);
        if (!path) return;
        const region = this.regions.find(r => r.id === info.id);
        const participation = region ? Number(region.participation || 0) : 0;
        const color = this.getHeatmapColor(participation);
        const isHovered = this.hoveredProvinceId === info.id;
        const isSelected = this.selectedProvinceId === info.id;
        const opacity = isSelected ? '1' : (isHovered ? '0.88' : '0.72');
        const stroke = isSelected ? '#0056b3' : (isHovered ? '#222' : '#ffffff');
        const strokeW = isSelected ? '3' : (isHovered ? '2' : '.5');

        path.setAttribute('fill', color);
        path.setAttribute('fill-opacity', opacity);
        path.setAttribute('stroke', stroke);
        path.setAttribute('stroke-width', strokeW);
        path.setAttribute('data-ir', irCode);
        path.setAttribute('style', 'cursor:pointer');
      });

      return new XMLSerializer().serializeToString(svgEl);
    },
    provinceMapData() {
      return Object.entries(this.irCodeMap).map(([irCode, info]) => {
        const regionData = this.regions.find(r => r.id === info.id) || {};
        const participation = regionData.participation || 0;
        return {
          id: info.id,
          name: info.name,
          irCode,
          votes: regionData.votes || 0,
          participation,
          eligibleVoters: regionData.eligibleVoters || 0,
          areasCount: (this.areasByProvince?.[info.id] || []).length,
          color: this.getHeatmapColor(participation)
        };
      });
    },
    sortedCandidates() {
      return [...this.candidates].sort((a, b) => b.vote_count - a.vote_count);
    },

    availableReportTypes() {
      if (this.currentUser?.roles?.includes('ADMIN')) return this.reportTypeOptions;
      return this.reportTypeOptions.filter(opt => opt.value !== 'admin');
    },
    selectedReportTypeLabel() {
      const selected = this.availableReportTypes.find(opt => opt.value === this.selectedReportType);
      return selected ? `در حال نمایش: ${selected.text}` : '';
    },
    provinceHeatmapData() {
      const maxParticipation = Math.max(...this.regions.map(r => Number(r.participation) || 0), 1);
      return this.regions.map(region => {
        const ratio = (Number(region.participation) || 0) / maxParticipation;
        const alpha = 0.25 + (ratio * 0.7);
        return {
          ...region,
          areasCount: (this.areasByProvince?.[region.id] || []).length,
          color: `rgba(220, 53, 69, ${alpha.toFixed(2)})`
        };
      });
    },
    safeTotalVotes() {
      return Number(this.infoVote?.totalVotes || 0);
    },
    safeTotalVoters() {
      return Number(this.infoVote?.totalVoters || 0);
    },
    safeParticipation() {
      if (this.infoVote?.voterParticipation !== undefined && this.infoVote?.voterParticipation !== null) {
        return Number(this.infoVote.voterParticipation).toFixed(2);
      }
      return this.safeTotalVoters ? ((this.safeTotalVotes / this.safeTotalVoters) * 100).toFixed(2) : '0.00';
    },
    topRegion() {
      return [...this.regions].sort((a, b) => Number(b.participation) - Number(a.participation))[0] || null;
    },
    lowestRegion() {
      return [...this.regions].filter(region => Number(region.eligibleVoters) || Number(region.votes))
        .sort((a, b) => Number(a.participation) - Number(b.participation))[0] || null;
    },
    regionDisplayData() {
      if (this.selectedReportType !== 'participation') return this.regions;
      return [...this.regions]
        .sort((a, b) => Number(b.participation) - Number(a.participation))
        .map((region, index) => ({
          ...region,
          participationNote: index === 0 ? 'بالاترین مشارکت فعلی' : `${index + 1} در رتبه مشارکت`
        }));
    },
    areaReportRows() {
      return this.regions.flatMap(region => this.buildRegionAreas(region).map(area => ({
        ...area,
        provinceName: region.name,
        participation: region.eligibleVoters ? Number(((area.votes / region.eligibleVoters) * 100).toFixed(1)) : 0
      }))).sort((a, b) => Number(b.votes) - Number(a.votes));
    },
    showCandidateReport() {
      return ['candidate', 'admin'].includes(this.selectedReportType);
    },
    showProvinceReport() {
      return ['province', 'participation', 'admin'].includes(this.selectedReportType);
    },
    showHeatmapReport() {
      return ['province', 'heatmap', 'admin'].includes(this.selectedReportType);
    },
    participationPulseCards() {
      const top = this.topRegion;
      const low = this.lowestRegion;
      const gap = top && low ? Math.max(Number(top.participation) - Number(low.participation), 0).toFixed(1) : '0.0';

      return [
        {
          title: 'پیشتاز مشارکت',
          value: top ? `${top.name} - ${top.participation}%` : '-',
          description: 'استانی که در این لحظه بالاترین نرخ مشارکت را دارد.',
          icon: 'trophy-fill',
          variant: 'success'
        },
        {
          title: 'نیازمند پیگیری',
          value: low ? `${low.name} - ${low.participation}%` : '-',
          description: 'کمترین نرخ مشارکت بین استان‌های دارای داده.',
          icon: 'exclamation-triangle-fill',
          variant: 'warning'
        },
        {
          title: 'فاصله مشارکت',
          value: `${gap}%`,
          description: 'اختلاف مشارکت بین بالاترین و پایین‌ترین استان.',
          icon: 'activity',
          variant: 'info'
        }
      ];
    },
    selectedReportHighlights() {
      const topCandidate = this.sortedCandidates[0];
      const topRegion = this.topRegion;
      const topArea = this.areaReportRows[0];
      const highlightsByType = {
        province: [
          { label: 'استان‌های دارای داده', value: this.formatNumber(this.regions.length), icon: 'geo-alt-fill', variant: 'primary' },
          { label: 'بیشترین مشارکت استانی', value: topRegion ? `${topRegion.name} (${topRegion.participation}%)` : '-', icon: 'graph-up', variant: 'success' },
          { label: 'کل آرای زنده', value: this.formatNumber(this.safeTotalVotes), icon: 'check2-circle', variant: 'info' }
        ],
        area: [
          { label: 'مناطق پوشش داده‌شده', value: this.formatNumber(this.areaReportRows.length), icon: 'diagram3-fill', variant: 'primary' },
          { label: 'فعال‌ترین منطقه', value: topArea ? `${topArea.name} (${this.formatNumber(topArea.votes)})` : '-', icon: 'pin-map-fill', variant: 'success' },
          { label: 'استان فعال‌ترین منطقه', value: topArea?.provinceName || '-', icon: 'map-fill', variant: 'info' }
        ],
        candidate: [
          { label: 'کاندیداهای فعال', value: this.formatNumber(this.infoVote?.activeCandidates || this.candidates.length), icon: 'person-badge-fill', variant: 'primary' },
          { label: 'پیشتاز فعلی', value: topCandidate ? `${topCandidate.first_name} ${topCandidate.last_name}` : '-', icon: 'award-fill', variant: 'success' },
          { label: 'کل آرای ثبت‌شده', value: this.formatNumber(this.safeTotalVotes), icon: 'bar-chart-fill', variant: 'info' }
        ],
        participation: [
          { label: 'مشارکت کل', value: `${this.safeParticipation}%`, icon: 'people-fill', variant: 'primary' },
          { label: 'شرکت‌کنندگان', value: this.formatNumber(this.infoVote?.participants || 0), icon: 'person-check-fill', variant: 'success' },
          { label: 'میانگین انتخاب هر نفر', value: this.avgVotesPerPerson, icon: 'calculator-fill', variant: 'info' }
        ],
        heatmap: [
          { label: 'نقاط نقشه', value: this.formatNumber(this.provinceHeatmapData.length), icon: 'grid3x3-gap-fill', variant: 'primary' },
          { label: 'پررنگ‌ترین استان', value: topRegion ? topRegion.name : '-', icon: 'fire', variant: 'danger' },
          { label: 'آخرین بروزرسانی', value: this.lastUpdate, icon: 'clock-history', variant: 'info' }
        ],
        admin: [
          { label: 'کل آرا', value: this.formatNumber(this.safeTotalVotes), icon: 'shield-lock-fill', variant: 'primary' },
          { label: 'کاندیداها', value: this.formatNumber(this.candidates.length), icon: 'people-fill', variant: 'success' },
          { label: 'مناطق', value: this.formatNumber(this.areaReportRows.length), icon: 'diagram3-fill', variant: 'info' }
        ]
      };
      return highlightsByType[this.selectedReportType] || highlightsByType.province;
    }
  },
  async mounted() {
    try {
      const config = this.ConfigInfo || await this.getConfig();
      this.syncElectionStatus(config);
    } finally {
      this.loadingElectionStatus = false;
    }

    if (this.electionStatusAll !== 'active') return;

    if (this.currentUser?.roles?.includes('ADMIN')) this.selectedReportType = "admin";
    this.startTimer();
    this.startAutoRefresh();
    await this.loadIranSvg();
    await this.loadRegions();

    this.infoVote = await this.getInfoVote()
    this.candidates = this.infoVote?.listCan || []
    this.updateRegionLiveStats();

  },
  beforeDestroy() {
    clearInterval(this.timerInterval);
    clearInterval(this.refreshInterval);
    if (this.realTimeChart) this.realTimeChart.destroy();
  }, watch: {
    availableReportTypes(options) {
      if (!options.some(opt => opt.value === this.selectedReportType)) {
        this.selectedReportType = 'province';
      }
    }
  },
  methods: {
    ...mapActions(["getConfig", "getInfoVote", "getRegions"]),
    ...mapMutations(["SetelectionStatusAll"]),

    syncElectionStatus(config) {
      if (!config?.startDate || !config?.EndDate) {
        this.SetelectionStatusAll('inactive');
        return;
      }

      const now = Date.now();
      const startDate = new Date(config.startDate).getTime();
      const endDate = new Date(config.EndDate).getTime();

      if (!Number.isFinite(startDate) || !Number.isFinite(endDate)) {
        this.SetelectionStatusAll('inactive');
        return;
      }

      const status = now < startDate ? 'upcoming' : now <= endDate ? 'active' : 'ended';
      this.SetelectionStatusAll(status);
    },

    async loadIranSvg() {
      try {
        const res = await fetch(require('@/assets/img/ir.svg'));
        this.iranSvgContent = await res.text();
      } catch (e) {
        console.warn('Could not load ir.svg', e);
      }
    },

    getProvinceByIrCode(irCode) {
      const info = this.irCodeMap[irCode];
      if (!info) return null;
      const region = this.regions.find(r => r.id === info.id) || {};
      return { ...info, ...region };
    },

    onMapMouseMove(e) {
      const path = e.target.closest('path[data-ir]');
      if (!path) { this.hoveredProvinceId = null; this.hoveredProvince = null; return; }
      const irCode = path.getAttribute('data-ir');
      const info = this.irCodeMap[irCode];
      if (!info) return;
      const region = this.regions.find(r => r.id === info.id) || {};
      this.hoveredProvinceId = info.id;
      this.hoveredProvince = { ...info, participation: region.participation || 0 };
      const rect = this.$refs.iranMapWrapper.getBoundingClientRect();
      this.tooltipStyle = {
        left: (e.clientX - rect.left + 12) + 'px',
        top: (e.clientY - rect.top - 10) + 'px'
      };
    },

    onMapClick(e) {
      const path = e.target.closest('path[data-ir]');
      if (!path) return;
      const irCode = path.getAttribute('data-ir');
      const info = this.irCodeMap[irCode];
      if (!info) return;
      this.selectedProvinceId = info.id;
      const region = this.regions.find(r => r.id === info.id) || {};
      this.selectedProvinceForMap = { ...info, ...region };
    },

    // دریافت رنگ استان بر اساس نرخ مشارکت
  getHeatmapColor(participation) {
    if (participation >= 70) return '#dc3545';      // قرمز تیره (مشارکت بالا)
    if (participation >= 50) return '#fd7e14';      // نارنجی
    if (participation >= 30) return '#ffc107';      // زرد
    if (participation >= 10) return '#20c997';      // سبز
    return '#6c757d';                                // خاکستری (مشارکت کم)
  },
  
  // دریافت رنگ استان (برای نقشه SVG)
  getProvinceColor(province) {
    const regionData = this.regions.find(r => r.id === province.id) || {};
    return this.getHeatmapColor(regionData.participation || 0);
  },

  getProvinceParticipation(province) {
    return (this.regions.find(r => r.id === province.id) || {}).participation || 0;
  },

  getProvinceVotes(province) {
    return (this.regions.find(r => r.id === province.id) || {}).votes || 0;
  },

  getProvinceAreasCount(province) {
    return (this.areasByProvince?.[province.id] || []).length;
  },

  onProvinceLeave() {
    this.hoveredProvinceId = null;
    this.hoveredProvince = null;
  },

  viewProvinceDetails(province) {
    const region = this.regions.find(r => r.id === province.id) || {};
    this.selectedRegion = {
      ...region,
      areas: this.buildRegionAreas(region)
    };
    this.showRegionModal = true;
  },
    async loadRegions() {
      const response = await this.getRegions();
      const provinces = response?.data || [];
      const map = response?.areasByProvince || {};

      this.areasByProvince = map;
      this.regions = provinces.map((province, index) => ({
        id: Number(province.id),
        name: province.name,
        votes: 0,
        participation: 0,
        eligibleVoters: 0,
        activeLocations: 0,
        topCandidate: '-',
        growth: 0
      }));
    },
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

    // Data Functions
    formatNumber(num) {
      return new Intl.NumberFormat('fa-IR').format(Number(num || 0));
    },


    getReportSectionTitle() {
      const titles = {
        province: 'مشارکت بر اساس استان',
        participation: 'رتبه‌بندی استان‌ها بر اساس مشارکت'
      };
      return titles[this.selectedReportType] || 'مشارکت بر اساس استان';
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
    buildRegionAreas(region) {
      const provinceAreas = this.areasByProvince?.[region.id] || [];
      const regionVoteStats = this.infoVote?.regionVoteStats || {};
      const baseAreas = provinceAreas.length
        ? provinceAreas.map(item => item.name)
        : [];
      const rawWeights = baseAreas.map((_, idx) => (baseAreas.length - idx) * 2 + 1);
      const totalWeight = rawWeights.reduce((sum, weight) => sum + weight, 0);

      const areas = baseAreas.map((name, idx) => {
        const areaId = String(provinceAreas[idx]?.id || `${region.id}-${idx + 1}`);
        const stat = regionVoteStats?.[areaId] || regionVoteStats?.[Number(areaId)] || null;
        const votes = Number(stat?.votes || 0);
        return { id: areaId, name, votes, percentage: 0 };
      });

      const regionVotes = Number(region.votes || 0);

      return areas.map(area => ({
        ...area,
        percentage: regionVotes > 0 ? Number(((area.votes / regionVotes) * 100).toFixed(1)) : 0
      }));
    },
    getAreaCandidateStats(area) {
      if (this.selectedReportType !== 'admin') return [];
      return (this.infoVote?.listCan || []).filter(candidate => String(candidate.region_id) === String(area.id));
      // const stats = this.infoVote?.listCan.filter(x=>x.region_id==area.id) || {};
      // return stats;
      // return stats?.[area.id] || stats?.[Number(area.id)] || [];

    },
    viewRegionDetails(region) {
      this.selectedRegion = {
        ...region,
        areas: this.buildRegionAreas(region)
      };
      this.showRegionModal = true;
    },

    startAutoRefresh() {
      this.refreshInterval = setInterval(async () => {
        if (!this.autoRefresh) return;

        const data = await this.getInfoVote();
        this.infoVote = data;
        this.candidates = data?.listCan || [];
        this.updateRegionLiveStats();

        this.lastUpdate = new Date().toLocaleTimeString('fa-IR');

      }, 30000);
    },
    async manualRefresh() {
      this.refreshing = true;

      const data = await this.getInfoVote();
      this.infoVote = data;
      this.candidates = data?.listCan || [];
      this.updateRegionLiveStats();

      this.lastUpdate = new Date().toLocaleTimeString('fa-IR');


      this.refreshing = false;
    },
    updateRegionLiveStats() {
      const provinceVoteStats = this.infoVote?.provinceVoteStats || {};
      const eligiblePerProvince = this.infoVote?.eligiblePerProvince || {};
      const regionVoteStats = this.infoVote?.regionVoteStats || {};
      if (!this.regions.length) return;

      this.regions = this.regions.map((region, idx) => {
        const areas = this.areasByProvince?.[region.id] || [];
        const provinceKey = Number(region.id) * 100;
        const provinceStat = provinceVoteStats?.[provinceKey] || null;
        const areaVotes = areas.reduce((sum, area) => {
          const stat = regionVoteStats?.[area.id] || regionVoteStats?.[Number(area.id)] || null;
          return sum + Number(stat?.votes || 0);
        }, 0);
        const votes = Number(provinceStat?.votes || areaVotes);
        const eligibleVoters = Number(eligiblePerProvince[provinceKey] || provinceStat?.eligible || 0);
        const participation = eligibleVoters ? Number(((votes / eligibleVoters) * 100).toFixed(1)) : 0;

        return {
          ...region,
          votes,
          eligibleVoters,
          participation,
          activeLocations: areas.length,
          growth: Number((Math.random() * 3).toFixed(1)),
          areaVotes
        };
      });

    }

  }
};
</script>

<style scoped>
.report-type-switch .btn {
  min-width: 120px;
  margin-bottom: 4px;
}

.report-highlight-row {
  border-top: 1px solid #eef1f5;
  padding-top: 14px;
}

.report-highlight {
  min-height: 82px;
  border-radius: 14px;
  padding: 14px;
  display: flex;
  align-items: center;
  gap: 12px;
  background: #f8fafc;
  border: 1px solid #eef1f5;
}

.report-highlight.primary {
  background: rgba(0, 123, 255, 0.09);
  color: #0056b3;
}

.report-highlight.success {
  background: rgba(40, 167, 69, 0.09);
  color: #1e7e34;
}

.report-highlight.info {
  background: rgba(23, 162, 184, 0.09);
  color: #117a8b;
}

.report-highlight.danger {
  background: rgba(220, 53, 69, 0.09);
  color: #bd2130;
}

.report-highlight-icon {
  font-size: 1.8rem;
  flex: 0 0 auto;
}

.report-highlight-value {
  font-weight: 800;
  color: #2c3e50;
  line-height: 1.7;
}

.report-highlight-label {
  color: #6c757d;
  font-size: 0.82rem;
}

.area-report-card,
.participation-card {
  border-radius: 12px;
  border: none;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);
}

.pulse-card {
  min-height: 170px;
  padding: 18px;
  border-radius: 16px;
  color: #2c3e50;
  border: 1px solid #edf0f4;
  background: #fff;
}

.pulse-card.success {
  background: linear-gradient(135deg, rgba(40, 167, 69, .12), rgba(40, 167, 69, .03));
}

.pulse-card.warning {
  background: linear-gradient(135deg, rgba(255, 193, 7, .18), rgba(255, 193, 7, .04));
}

.pulse-card.info {
  background: linear-gradient(135deg, rgba(23, 162, 184, .14), rgba(23, 162, 184, .04));
}

.pulse-card-icon {
  font-size: 2rem;
  margin-bottom: 12px;
}

.pulse-card-title {
  color: #6c757d;
  font-size: .88rem;
}

.pulse-card-value {
  font-size: 1.2rem;
  font-weight: 800;
  margin: 6px 0;
}

.report-type-card {
  border-right: 4px solid #007bff;
}

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

.iran-heatmap-card {
  border-radius: 16px;
}

.iran-map-wrapper {
  position: relative;
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
  display: block;
  user-select: none;
}

.iran-map-wrapper >>> .iran-map-svg {
  width: 100%;
  height: auto;
  display: block;
  border-radius: 8px;
}

.iran-map-wrapper >>> path {
  transition: fill-opacity 0.15s ease, stroke-width 0.15s ease;
}

.province-tooltip {
  position: absolute;
  background: rgba(0,0,0,0.78);
  color: #fff;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.82rem;
  pointer-events: none;
  z-index: 10;
  white-space: nowrap;
  line-height: 1.6;
}

.province-path-overlay {
  cursor: pointer;
  transition: fill 0.2s ease, filter 0.2s ease;
  pointer-events: auto;
}

.province-path-overlay:hover {
  fill-opacity: 0.3 !important;
  filter: drop-shadow(0 0 8px rgba(0, 0, 0, 0.2));
}

.province-path-overlay.province-hover {
  fill-opacity: 0.4 !important;
}

.province-path-overlay.province-selected {
  fill-opacity: 0.5 !important;
  stroke: #007bff !important;
  stroke-width: 2 !important;
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