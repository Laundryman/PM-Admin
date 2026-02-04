<script setup lang="ts">
import { useSystemStore } from '@/stores/systemStore'
import { onMounted, ref, watch } from 'vue'

const { getPrimary, getSurface, isDarkTheme } = useSystemStore()

const chartData = ref()
const chartOptions = ref()

function setChartData() {
  const documentStyle = getComputedStyle(document.documentElement)

  return {
    labels: ['Q1', 'Q2', 'Q3', 'Q4'],
    datasets: [
      {
        type: 'bar',
        label: 'MaxFactor',
        backgroundColor: documentStyle.getPropertyValue('--p-primary-400'),
        data: [40, 60, 80, 50],
        barThickness: 32,
      },
      {
        type: 'bar',
        label: 'LOreal',
        backgroundColor: documentStyle.getPropertyValue('--p-primary-300'),
        data: [21, 84, 24, 75],
        barThickness: 32,
      },
      {
        type: 'bar',
        label: 'Maybelline',
        backgroundColor: documentStyle.getPropertyValue('--p-primary-200'),
        data: [41, 52, 34, 74],
        borderRadius: {
          topLeft: 8,
          topRight: 8,
        },
        borderSkipped: true,
        barThickness: 32,
      },
    ],
  }
}

function setChartOptions() {
  const documentStyle = getComputedStyle(document.documentElement)
  const borderColor = documentStyle.getPropertyValue('--surface-border')
  const textMutedColor = documentStyle.getPropertyValue('--text-color-secondary')

  return {
    maintainAspectRatio: false,
    aspectRatio: 0.8,
    scales: {
      x: {
        stacked: true,
        ticks: {
          color: textMutedColor,
        },
        grid: {
          color: 'transparent',
          borderColor: 'transparent',
        },
      },
      y: {
        stacked: true,
        ticks: {
          color: textMutedColor,
        },
        grid: {
          color: borderColor,
          borderColor: 'transparent',
          drawTicks: false,
        },
      },
    },
  }
}

watch([getPrimary, getSurface, isDarkTheme], () => {
  chartData.value = setChartData()
  chartOptions.value = setChartOptions()
})

onMounted(() => {
  chartData.value = setChartData()
  chartOptions.value = setChartOptions()
})
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">New Planograms</div>
    <Chart type="bar" :data="chartData" :options="chartOptions" class="h-80" />
  </div>
</template>
