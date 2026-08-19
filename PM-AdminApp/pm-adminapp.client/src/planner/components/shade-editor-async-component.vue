<script setup lang="ts">
import { defineAsyncComponent } from 'vue'

const ShadeEditorAsyncComponent = defineAsyncComponent(
  () => import('@/components/planner/editShades.vue'),
)
</script>
