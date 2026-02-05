import { ThemeColors } from '../utils'
const colors = ThemeColors()

export const lineChartData = {
  labels: ['عالی', 'خوب', 'متوسط', 'ضعیف'],
  datasets: [
    {
      label: '',
      data: [1, 3, 2, 4],
      borderColor: colors.themeColor1,
      pointBackgroundColor: colors.foregroundColor,
      pointBorderColor: colors.themeColor1,
      pointHoverBackgroundColor: colors.themeColor1,
      pointHoverBorderColor: colors.foregroundColor,
      pointRadius: 6,
      pointBorderWidth: 2,
      pointHoverRadius: 8,
      fill: false
    }
  ]
}

export const polarAreaChartData = {
  labels: ['خالص دریافتی', 'ناخالص', 'حق التدریس'],
  datasets: [
    {
      data: [80, 90, 70],
      borderWidth: 2,
      borderColor: [colors.themeColor1, colors.themeColor2, colors.themeColor3],
      backgroundColor: [
        colors.themeColor1_10,
        colors.themeColor2_10,
        colors.themeColor3_10
      ]
    }
  ]
}

export const areaChartData = {
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  datasets: [
    {
      label: '',
      data: [54, 63, 60, 65, 60, 68, 60],
      borderColor: colors.themeColor1,
      pointBackgroundColor: colors.foregroundColor,
      pointBorderColor: colors.themeColor1,
      pointHoverBackgroundColor: colors.themeColor1,
      pointHoverBorderColor: colors.foregroundColor,
      pointRadius: 4,
      pointBorderWidth: 2,
      pointHoverRadius: 5,
      fill: true,
      borderWidth: 2,
      backgroundColor: colors.themeColor1_10
    }
  ]
}

export const scatterChartData = {
  datasets: [
    {
      borderWidth: 2,
      showLine: false,
      label: 'Cakes',
      borderColor: colors.themeColor1,
      backgroundColor: colors.themeColor1_10,
      data: [
        { x: 62, y: -78 },
        { x: -0, y: 74 },
        { x: -67, y: 45 },
        { x: -26, y: -43 },
        { x: -15, y: -30 },
        { x: 65, y: -68 },
        { x: -28, y: -61 }
      ]
    },
    {
      borderWidth: 2,
      showLine: false,
      label: 'Desserts',
      borderColor: colors.themeColor2,
      backgroundColor: colors.themeColor2_10,
      data: [
        { x: 79, y: 62 },
        { x: 62, y: 0 },
        { x: -76, y: -81 },
        { x: -51, y: 41 },
        { x: -9, y: 9 },
        { x: 72, y: -37 },
        { x: 62, y: -26 }
      ]
    }
  ]
}

export const barChartData = {
  labels: ['1403', '1402', '1401', '1400', '1399', '1398'],
  datasets: [
    {
      label: 'درصد قبولی مدرسه A',
      borderColor: colors.themeColor1,
      backgroundColor: colors.themeColor1_10,
      data: [40, 70, 30, 90, 50, 48],
      borderWidth: 2
    },
    {
      label: 'درصد قبولی مدرسه B',
      borderColor: colors.themeColor2,
      backgroundColor: colors.themeColor2_10,
      data: [67, 87, 66, 30, 79, 98],
      borderWidth: 2
    }
  ]
}

export const radarChartData = {
  datasets: [
    {
      label: 'ساعت',
      borderWidth: 2,
      pointBackgroundColor: colors.themeColor1,
      borderColor: colors.themeColor1,
      backgroundColor: colors.themeColor1_10,
      data: [80, 80, 80,80,80]
    },{
      label: 'روز',
      borderWidth: 2,
      pointBackgroundColor: colors.themeColor2,
      borderColor: colors.themeColor2,
      backgroundColor: colors.themeColor2_10,
      data: [70, 70, 70,70,70]
    },{
      label: 'هفته',
      borderWidth: 2,
      pointBackgroundColor: colors.themeColor4,
      borderColor: colors.themeColor4,
      backgroundColor: colors.themeColor4_10,
      data: [60, 60, 60,60,60]
    },{
      label: 'ماه',
      borderWidth: 2,
      pointBackgroundColor: colors.themeColor5,
      borderColor: colors.themeColor5,
      backgroundColor: colors.themeColor5_10,
      data: [90, 90, 90,90,90]
    }
  ],
  labels: ['مدرسه', 'منطقه', 'استان', 'ستاد', 'متفرقه']
}

export const pieChartData = {
  labels: ['Cakes', 'Cupcakes', 'Desserts'],
  datasets: [
    {
      label: '',
      borderColor: [colors.themeColor1, colors.themeColor2, colors.themeColor3],
      backgroundColor: [
        colors.themeColor1_10,
        colors.themeColor2_10,
        colors.themeColor3_10
      ],
      borderWidth: 2,
      data: [15, 25, 20]
    }
  ]
}
export const pieChartDataM = {
  labels: ['ساعت', 'روز', 'هفته', 'ماه'],
  datasets: [
    {
      backgroundColor: ['#2f2', '#dee2f1'],
      data: [264,1440]
    },
    {
      backgroundColor: ['#f22','#dee2f1'],
      data: [11, 60]
    },
    {
      backgroundColor: ['#22f','#dee2f1'],
      data: [1.5, 8]
    },
    {
      backgroundColor: ['#01f1e3', '#dee2f1'],
      data: [0.1, 2]
    }
  ]
}


export const smallChartData1 = {
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
}

export const smallChartData2 = {
  labels: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
  datasets: [
    {
      label: 'دریافتی ماهیانه',
      borderColor: colors.themeColor1,
      pointBorderColor: colors.themeColor1,
      pointHoverBackgroundColor: colors.themeColor1,
      pointHoverBorderColor: colors.themeColor1,
      pointRadius: 2,
      pointBorderWidth: 3,
      pointHoverRadius: 2,
      fill: false,
      borderWidth: 2,
      data: [26300000, 22300000, 23300000, 24300000, 24300000, 28300000, 28300000,28700000, 26300000, 26300000, 21340000, 26300000, 26200000],
      datalabels: {
        align: 'end',
        anchor: 'end'
      }
    }
  ]
}

export const smallChartData3 = {
  labels: ['1397', '1398', '1399', '1400', '1401', '1402', '1403'],
  datasets: [
    {
      label: 'دریافتی سالیانه',
      borderColor: colors.themeColor1,
      pointBorderColor: colors.themeColor1,
      pointHoverBackgroundColor: colors.themeColor1,
      pointHoverBorderColor: colors.themeColor1,
      pointRadius: 2,
      pointBorderWidth: 3,
      pointHoverRadius: 2,
      fill: false,
      borderWidth: 2,
      data: [350, 452, 762, 952, 630, 85, 158],
      datalabels: {
        align: 'end',
        anchor: 'end'
      }
    }
  ]
}

export const smallChartData4 = {
  labels: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
  datasets: [
    {
      label: 'حق التدریس ماهیانه',
      borderColor: colors.themeColor1,
      pointBorderColor: colors.themeColor1,
      pointHoverBackgroundColor: colors.themeColor1,
      pointHoverBorderColor: colors.themeColor1,
      pointRadius: 2,
      pointBorderWidth: 3,
      pointHoverRadius: 2,
      fill: false,
      borderWidth: 2,
      data: [3180000, 3930000, 4370000, 6120000, 6420000, 7530000, 8500000, 6120000, 1420000, 7530000, 8500000, 4370000, 6120000],
      datalabels: {
        align: 'end',
        anchor: 'end'
      }
    }
  ]
}

export const conversionChartData = {
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  datasets: [
    {

      label: '',
      data: [1, 2, 1, 3, 4, 1, 2],
      borderColor: colors.themeColor2,
      pointBackgroundColor: colors.foregroundColor,
      pointBorderColor: colors.themeColor2,
      pointHoverBackgroundColor: colors.themeColor2,
      pointHoverBorderColor: colors.foregroundColor,
      pointRadius: 4,
      pointBorderWidth: 2,
      pointHoverRadius: 5,
      fill: true,
      borderWidth: 2,
      backgroundColor: colors.themeColor2_10
    }
  ]
}
// labels: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر','آبان','آذر','دی','بهمن','اسفند'],
export const ahkam = {
  labels: ['1404', '1403', '1402', '1401', '1400', '1399', '1398'],
  
  datasets: [
    {
      label: 'انتصاب',
      data: [3, 2, 1, 0, 0, 3, 2],
      borderColor: colors.themeColor2,
      pointBackgroundColor: colors.foregroundColor,
      pointBorderColor: colors.themeColor2,
      pointHoverBackgroundColor: colors.themeColor2,
      pointHoverBorderColor: colors.foregroundColor,
      pointRadius: 4,
      pointBorderWidth: 2,
      pointHoverRadius: 5,
      fill: true,
      borderWidth: 2,
      backgroundColor: colors.themeColor2_10
    },{
      label: 'حقوق و مزایا',
      data: [2, 3, 1, 4, 0, 3, 2],
      borderColor: colors.themeColor3,
      pointBackgroundColor: colors.foregroundColor,
      pointBorderColor: colors.themeColor3,
      pointHoverBackgroundColor: colors.themeColor3,
      pointHoverBorderColor: colors.foregroundColor,
      pointRadius: 4,
      pointBorderWidth: 2,
      pointHoverRadius: 5,
      fill: true,
      borderWidth: 2,
      backgroundColor: colors.themeColor3_10
    }
  ],
}