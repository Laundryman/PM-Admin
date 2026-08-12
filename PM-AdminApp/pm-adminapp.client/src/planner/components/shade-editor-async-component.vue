<script setup lang="ts">
import { defineAsyncComponent } from 'vue'

const ShadeEditorAsyncComponent = defineAsyncComponent(
  () => import('@/components/Planner/editShades.vue'),
)
</script>
