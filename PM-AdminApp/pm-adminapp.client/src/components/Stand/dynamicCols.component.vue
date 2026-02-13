<script lang="ts" setup>
import { Column } from '@/models/Stands/columns.model'

defineEmits(['update:columns'])
defineEmits(['update:uprights'])
const props = defineProps<{
  column: Column
}>()

const tableRow = {
  props: ['column'],
  template: `<tr>
    <td><input type="number" v-model="column.position" /></td>
    <td><input type="number" v-model="column.width" /></td>

  </tr>`,
}
</script>
