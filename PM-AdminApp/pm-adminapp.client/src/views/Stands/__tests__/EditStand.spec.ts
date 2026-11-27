import { flushPromises, shallowMount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { computed, defineComponent, h, inject, provide, ref, type Ref } from 'vue'
import EditStand from '../EditStand.vue'

const data = vi.hoisted(() => ({
  stand: {
    brandId: 1,
    standTypeId: 1,
    cols: 1,
    rows: 1,
    columnList: [{ position: 1, width: 10 }],
    rowList: [{ position: 1, height: 10 }],
    regions: [],
    countries: [],
    layoutStyle: 1,
    shelfIncrement: 10,
  },
}))

vi.mock('@/components/composables/locationFilters', () => ({
  useLocationFilters: () => ({
    getRegions: async () => [],
    getCountriesForRegions: async () => [],
  }),
}))
vi.mock('@/components/composables/multiSelectList.composable', () => ({
  useMultiSelectLists: () => ({}),
}))
vi.mock('@/components/composables/standTypes.composable', () => ({
  useStandTypes: () => ({ getStandTypes: async () => [{ id: 1, name: 'Test' }] }),
}))
vi.mock('@/stores/brandStore', () => ({
  useBrandStore: () => ({ activeBrand: { id: 1 }, brands: [] }),
}))
vi.mock('@/stores/standStore', () => ({
  useStandStore: () => ({ stand: data.stand, initialize: async () => {} }),
}))
vi.mock('@/stores/systemStore', () => ({
  useSystemStore: () => ({ layoutState: {} }),
}))
vi.mock('pinia', () => ({
  storeToRefs: (store: Record<string, unknown>) =>
    Object.fromEntries(Object.entries(store).map(([key, value]) => [key, ref(value)])),
}))
vi.mock('vue-router', () => ({
  useRouter: () => ({
    currentRoute: ref({ name: 'editStand', params: { id: '1' } }),
    push: vi.fn(),
    back: vi.fn(),
  }),
}))
vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: vi.fn() }),
}))

const fields = {
  cols: { states: { value: 1, invalid: false, valid: true } },
  rows: { states: { value: 1, invalid: false, valid: true } },
  shelfIncrement: { states: { value: 1, invalid: false, valid: true } },
}

const FormStub = defineComponent({
  name: 'Form',
  emits: ['submit'],
  setup(_, { expose, slots }) {
    expose({
      fields,
      setFieldValue: vi.fn(),
      validate: async () => ({ values: {} }),
    })
    return () => h('form', slots.default?.({}))
  },
})

const TabsStub = defineComponent({
  name: 'Tabs',
  props: ['value'],
  setup(props, { slots }) {
    provide(
      'activeTab',
      computed(() => props.value),
    )
    return () => h('div', slots.default?.())
  },
})

const TabPanelStub = defineComponent({
  name: 'TabPanel',
  props: ['value'],
  setup(props, { slots }) {
    const activeTab = inject<Ref<string>>('activeTab')
    return () =>
      h(
        'div',
        { role: 'tabpanel', 'data-p-active': String(activeTab?.value === props.value) },
        slots.default?.(),
      )
  },
})

const ButtonStub = defineComponent({
  name: 'Button',
  props: ['label'],
  setup(props) {
    return () => h('button', props.label)
  },
})

const ToggleButtonStub = defineComponent({
  props: ['onLabel', 'offLabel', 'onIcon', 'offIcon'],
  setup: () => () => h('div'),
})

describe('EditStand invalid submission', () => {
  beforeEach(() => {
    data.stand.columnList[0]!.width = 10
    data.stand.rowList[0]!.height = 10
    data.stand.layoutStyle = 1
    data.stand.shelfIncrement = 10
    for (const field of Object.values(fields)) {
      field.states.invalid = false
      field.states.valid = true
    }
  })

  async function mountForm() {
    const wrapper = shallowMount(EditStand, {
      global: {
        stubs: {
          Form: FormStub,
          Tabs: TabsStub,
          TabPanel: TabPanelStub,
          Button: ButtonStub,
          ToggleButton: ToggleButtonStub,
        },
        renderStubDefaultSlot: true,
      },
    })
    await flushPromises()
    return wrapper
  }

  it.each([
    ['details', { name: { invalid: true } }, '0'],
    ['layout', { cols: { invalid: true } }, '1'],
    ['row', { rows: { invalid: true } }, '1'],
    ['vertical pitch', { shelfIncrement: { invalid: true } }, '1'],
    ['both tabs', { name: { invalid: true }, cols: { invalid: true } }, '0'],
    ['details and rows', { name: { invalid: true }, rows: { invalid: true } }, '0'],
  ])('opens the first tab with %s errors', async (_, states, expected) => {
    const wrapper = await mountForm()
    if (expected === '0') {
      await wrapper
        .findAll('button')
        .find((button) => button.text() === 'Layout')!
        .trigger('click')
      await flushPromises()
    }
    wrapper.findComponent(FormStub).vm.$emit('submit', { valid: false, states })
    await flushPromises()
    expect(wrapper.findComponent(TabsStub).props('value')).toBe(expected)
  })

  it('opens layout for invalid column widths and prioritizes details errors', async () => {
    data.stand.columnList[0]!.width = 0
    const wrapper = await mountForm()
    wrapper.findComponent(FormStub).vm.$emit('submit', { valid: true, states: {} })
    await flushPromises()
    expect(wrapper.findComponent(TabsStub).props('value')).toBe('1')

    wrapper.findComponent(FormStub).vm.$emit('submit', {
      valid: false,
      states: { name: { invalid: true } },
    })
    await flushPromises()
    expect(wrapper.findComponent(TabsStub).props('value')).toBe('0')
  })

  it('opens layout when row height validation fails', async () => {
    data.stand.rowList[0]!.height = 0
    const wrapper = await mountForm()
    wrapper.findComponent(FormStub).vm.$emit('submit', {
      valid: true,
      states: { rows: fields.rows.states },
    })
    await flushPromises()
    expect(fields.rows.states.invalid).toBe(true)
    expect(wrapper.findComponent(TabsStub).props('value')).toBe('1')
  })

  it.each([2, 3])(
    'opens layout when vertical pitch validation fails for style %i',
    async (style) => {
      data.stand.layoutStyle = style
      data.stand.shelfIncrement = 0
      const wrapper = await mountForm()
      wrapper.findComponent(FormStub).vm.$emit('submit', {
        valid: true,
        states: { shelfIncrement: fields.shelfIncrement.states },
      })
      await flushPromises()
      expect(fields.shelfIncrement.states.invalid).toBe(true)
      expect(wrapper.findComponent(TabsStub).props('value')).toBe('1')

      wrapper.findComponent(FormStub).vm.$emit('submit', {
        valid: false,
        states: { shelfIncrement: fields.shelfIncrement.states, name: { invalid: true } },
      })
      await flushPromises()
      expect(wrapper.findComponent(TabsStub).props('value')).toBe('0')
    },
  )

  it('scrolls to the first error on the selected tab when switching tabs', async () => {
    const wrapper = await mountForm()
    const panels = wrapper.findAll('[role="tabpanel"]')
    const detailsField = document.createElement('div')
    detailsField.className = 'p-invalid'
    detailsField.scrollIntoView = vi.fn()
    panels[0]!.element.append(detailsField)

    const firstLayoutContainer = document.createElement('div')
    firstLayoutContainer.className = 'flex flex-col'
    firstLayoutContainer.scrollIntoView = vi.fn()
    const firstLayoutField = document.createElement('div')
    firstLayoutField.className = 'p-invalid'
    firstLayoutContainer.append(firstLayoutField)
    const secondLayoutField = document.createElement('div')
    secondLayoutField.setAttribute('aria-invalid', 'true')
    secondLayoutField.scrollIntoView = vi.fn()
    panels[1]!.element.append(firstLayoutContainer, secondLayoutField)

    await wrapper
      .findAll('button')
      .find((button) => button.text() === 'Layout')!
      .trigger('click')
    await flushPromises()
    expect(firstLayoutContainer.scrollIntoView).toHaveBeenCalledWith({
      behavior: 'smooth',
      block: 'end',
      inline: 'nearest',
    })
    expect(secondLayoutField.scrollIntoView).not.toHaveBeenCalled()
    expect(detailsField.scrollIntoView).not.toHaveBeenCalled()
  })

  it('scrolls to an error after an invalid submit even when the tab is already selected', async () => {
    const wrapper = await mountForm()
    const field = document.createElement('div')
    field.className = 'p-invalid'
    field.scrollIntoView = vi.fn()
    wrapper.findAll('[role="tabpanel"]')[0]!.element.append(field)

    wrapper.findComponent(FormStub).vm.$emit('submit', {
      valid: false,
      states: { name: { invalid: true } },
    })
    await flushPromises()
    expect(field.scrollIntoView).toHaveBeenCalledOnce()
  })

  it('scrolls to an error after an invalid submit switches tabs', async () => {
    const wrapper = await mountForm()
    const panels = wrapper.findAll('[role="tabpanel"]')
    const field = document.createElement('div')
    field.className = 'p-invalid'
    field.scrollIntoView = vi.fn()
    panels[1]!.element.append(field)

    wrapper.findComponent(FormStub).vm.$emit('submit', {
      valid: false,
      states: { cols: { invalid: true } },
    })
    await flushPromises()
    expect(wrapper.findComponent(TabsStub).props('value')).toBe('1')
    expect(field.scrollIntoView).toHaveBeenCalledOnce()
  })
})
