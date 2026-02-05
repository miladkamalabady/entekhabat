<template>
  <b-row class="mt-3">
    <b-colxx lg="6" xxs="12" class="mb-4">
      <small-line-chart-card :class="itemClass" label-prefix="$" :data="smallChartData1" />
    </b-colxx>
    <b-colxx lg="6" xxs="12" class="mb-4">
      <small-line-chart-card :class="itemClass" label-prefix="$" :data="smallChartData2" />
    </b-colxx>
    <b-colxx lg="6" xxs="12" class="mb-4">
      <small-line-chart-card :class="itemClass" label-prefix="$" :data="smallChartData3" />
    </b-colxx>
    <b-colxx lg="6" xxs="12" class="mb-4">
      <small-line-chart-card :class="itemClass" label-prefix="$" :data="smallChartData4" />
    </b-colxx>
  </b-row>
</template>
<script>
import SmallLineChartCard from "../../components/Cards/SmallLineChartCard";
import {
  smallChartData2,
  smallChartData3,
  smallChartData4
} from "../../data/charts";

import { ThemeColors } from '../../utils'
const colors = ThemeColors()
import { mapGetters } from "vuex";

export default {
  components: {
    "small-line-chart-card": SmallLineChartCard
  },
  props: ["itemClass"],
  data() {
    return {
      smallChartData1: {
        labels: ['1397', '1398', '1399', '1400', '1401', '1402', '1403'],
        datasets: [
          {
            label: 'حق التدریس سالیانه',
            data: [31800000, 39300000, 43700000, 61200000, 64200000, 75300000, 85000000],
            borderColor: colors.themeColor1,
            pointBorderColor: colors.themeColor1,
            pointHoverBackgroundColor: colors.themeColor1,
            pointHoverBorderColor: colors.themeColor1,
            pointRadius: 2,
            pointBorderWidth: 3,
            pointHoverRadius: 2,
            fill: false,
            borderWidth: 2,
            datalabels: {
              align: 'end',
              anchor: 'end'
            }
          }
        ]
      },
      smallChartData2,
      smallChartData3,
      smallChartData4
    };
  },
  computed: {
    ...mapGetters(["SalarySlipByEmployeeCodeinfo"]),
  },
  watch: {
    SalarySlipByEmployeeCodeinfo: {
      immediate: true,
      deep: true,
      handler(newVal) {

        if (newVal?.salaryResults) {
          
          
        }
      }
    }
  }
};

</script>
