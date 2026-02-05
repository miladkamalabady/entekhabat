<template>
  <b-card :header="info" class="mt-4">
    <div class="custom-tab-blue ">
      <span class="text">  میزان قبولی</span>
      <span class="icon">&#x2611;</span>
    </div>
    <div class="dashboard-line-chart">
      <line-chart :data="barChartData" shadow />
    </div>
  </b-card>
</template>
<script> 
import LineChart from "../../components/Charts/Bar";
import { barChartData } from "../../data/charts";

export default {
  components: {
    "line-chart": LineChart
  },
  props: ['info'],
  data() {
    return {
      barChartData
    };
  },
  methods: {
    refreshButtonClick() {
      console.log("refreshButtonClick");
    }
  }
};
</script>
<style scoped>


</style>