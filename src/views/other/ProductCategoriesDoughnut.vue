<template>
  <b-card class="dashboard-filled-line-chart" no-body>
    <b-card-body class="p-0">
      <div class="custom-tab">
        <span class="text" v-if="type == 1"> ضمن خدمت</span>
        <span class="text" v-else> حقوق واریزی</span>
        <span class="icon">&#x2611;</span>
      </div>
      <div class="dashboard-donut-chart">
        <doughnut-chart v-if="type == 2" :data="doughnutChartData" shadow />
        <doughnut-chart v-else :data="doughnutChartData1" shadow />
      </div>
    </b-card-body>
  </b-card>
</template>
<script>
import DoughnutChart from "../../components/Charts/Doughnut";
import { ThemeColors } from '../../utils'
const colors = ThemeColors()
import { mapGetters } from "vuex";
export default {
  props: ['data', 'type'],
  components: {
    "doughnut-chart": DoughnutChart
  },
  computed: {
    ...mapGetters(["SalarySlipByEmployeeCodeinfo"]),
  },
  mounted() {

  },
  data() {
    return {
      doughnutChartData: {
        labels: [],
        datasets: [
          {
            label: '',
            borderWidth: 2,
            data: []
          }
        ]
      },
      doughnutChartData1: {
        labels: ['ساعات تخصصی', 'ساعات عمومی'],
        datasets: [
          {
            label: '',
            borderColor: [colors.themeColor3, colors.themeColor2],
            backgroundColor: [
              colors.themeColor3_10,
              colors.themeColor2_10
            ],
            borderWidth: 2,
            data: [this.data?.sumTrainingCourseHourProfessional, this.data?.sumTrainingCourseHourGeneral]
          }
        ]
      }
    };
  },
  methods: {
    getRandomColor() {
      // رنگ RGBA با شفافیت 0.6
      const r = Math.floor(Math.random() * 200);
      const g = Math.floor(Math.random() * 200);
      const b = Math.floor(Math.random() * 200);
      return `rgba(${r}, ${g}, ${b}, 0.6)`;
    }
  },
  watch: {
    SalarySlipByEmployeeCodeinfo: {
      immediate: true,
      deep: true,
      handler(newVal) {

        if (newVal?.salaryResults) {
          
          this.doughnutChartData.labels = newVal.salaryResults
          .filter(x => x.otherPaymentTitle != null && x.benefitsSum != null && x.benefitsSum>0)
            .map(x => x.otherPaymentTitle);
            

          this.doughnutChartData.datasets[0].data = newVal.salaryResults
            .filter(x => x.otherPaymentTitle != null && x.benefitsSum != null && x.benefitsSum>0)
            .map(x => x.benefitsSum);


          this.doughnutChartData.datasets[0].backgroundColor = this.doughnutChartData.labels.map(() => this.getRandomColor());


        }
      }
    }
  }
};
</script>
<style scoped>
.custom-tab {
  background-color: green;
  /* خاکستری */
  color: white;
  padding: 10px 15px;
  float: left;
  clip-path: polygon(0 0, 100% 0, 100% 100%, 20px 100%, 0 80%);
}

.custom-tab .icon {
  margin-left: 8px;
  font-size: 16px;
}
</style>