import { definePreset } from '@primeuix/themes'
import Lara from '@primeuix/themes/lara'

const PMAdmin = definePreset(Lara, {
  //Your customizations, see the following sections for examples
  semantic: {
    primary: {
      50: '{sky.50}',
      100: '{sky.100}',
      200: '{sky.200}',
      300: '{sky.300}',
      400: '{sky.400}',
      500: '{sky.500}',
      600: '{sky.600}',
      700: '{sky.700}',
      800: '{sky.800}',
      900: '{sky.900}',
      950: '{sky.950}',
    },
  },
  components: {
    DataTable: {
      extend: {},
      css: ({ dt }: any) => `
      .p-datatable-row-hover: hover {
          background: ${dt('button.accent.color')};
          color: ${dt('button.accent.inverse.color')};
          transition-duration: ${dt('my.transition.fast')};
      }
    `,
    },
  },
})

export default PMAdmin
