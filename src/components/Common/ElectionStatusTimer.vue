<template>
  <div>
    <b-badge
      :variant="electionStatus === 'active' ? 'success' : electionStatus === 'upcoming' ? 'warning' : 'secondary'" pill>
      {{ statusText }}
    </b-badge><br />
    <!-- تایمر معکوس -->
    <div v-if="electionStatus !== 'ended'" class="mt-2">
      <small class="text-muted">{{ timerLabel }}:</small>
      <span class="timer-display">
        {{ formattedTime }}
      </span>
    </div>
    <b-badge class="mt-2" v-if="configInfo?.active">{{ configInfo?.startDates }} ساعت {{
      configInfo?.startTime }} الی {{ configInfo?.endDates }} ساعت {{ configInfo?.endTime }}</b-badge>
  </div>
</template>

<script>
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  props: {
    configInfo: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      currentTime: new Date(),
      timer: null,
      electionStatus: 'inactive', // 'upcoming', 'active', 'ended'
      timeRemaining: 0,
      statusText: 'غیرفعال'
    }
  },
  computed: {
    formattedTime() {
      if (this.timeRemaining <= 0) return '00:00:00';

      const hours = Math.floor(this.timeRemaining / (1000 * 60 * 60));
      const minutes = Math.floor((this.timeRemaining % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((this.timeRemaining % (1000 * 60)) / 1000);

      return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    },

    timerLabel() {
      return this.electionStatus === 'active' ? 'زمان باقی‌مانده تا پایان' : 'زمان باقی‌مانده تا شروع';
    }
  },
  watch: {
    configInfo: {
      immediate: true,
      handler() {
        this.calculateStatus();
      }
    }
  },
  mounted() {
    // شروع تایمر به روزرسانی هر ثانیه
    this.timer = setInterval(() => {
      this.currentTime = new Date();
      this.calculateStatus();
    }, 1000);
  },
  beforeDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  },
  methods: {
    ...mapMutations(["SetelectionStatusAll"]),
    calculateStatus() {
      if (!this.configInfo?.startDate || !this.configInfo?.EndDate) {
        this.electionStatus = 'inactive';
        this.statusText = 'غیرفعال';
        return;
      }

      const now = this.currentTime;
      const startDate = new Date(this.configInfo.startDate);
      const endDate = new Date(this.configInfo.EndDate);

      // اگر هنوز شروع نشده
      if (now < startDate) {
        this.electionStatus = 'upcoming';
        this.statusText = 'آغاز به زودی';
        this.timeRemaining = startDate - now;
      }
      // اگر در حال برگزاری است
      else if (now >= startDate && now <= endDate) {
        this.electionStatus = 'active';
        this.statusText = 'در حال برگزاری';
        this.timeRemaining = endDate - now;
      }
      // اگر پایان یافته
      else {
        this.electionStatus = 'ended';
        this.statusText = 'پایان یافته';
        this.timeRemaining = 0;
      }
      this.SetelectionStatusAll(this.electionStatus)
    }
  }
}
</script>

<style scoped>
.timer-display {
  font-family: 'Courier New', monospace;
  font-weight: bold;
  font-size: 1.1rem;
  color: #2c3e50;
  margin-right: 5px;
  padding: 3px 8px;
  background-color: #f8f9fa;
  border-radius: 4px;
  border: 1px solid #dee2e6;
}
</style>