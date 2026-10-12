<script setup lang="ts">
import { Job } from '@/models/Jobs/Job.model'
import { JobFolder } from '@/models/Jobs/JobFolder.model'
import { default as jobsService } from '@/services/Jobs/JobsService'
import { useBrandStore } from '@/stores/brandStore'
import { useSystemStore } from '@/stores/systemStore'

import type { FormInstance } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import { defineEmits, defineProps, onMounted, ref, useTemplateRef } from 'vue'
import { z } from 'zod'

const toast = useToast()
const layout = useSystemStore()
const brandStore = useBrandStore()
const brand = storeToRefs(brandStore).activeBrand

const submitted = ref(false)
const jobDateRange = ref<Date[] | null>(null)
const jobFormTemplateRef = useTemplateRef<FormInstance>('job-form')

const initialJobValues = ref({
  jobCode: '',
  jobDescription: '',
  jobFolder: '',
  jobDateRange: [] as Date[] | null,
})

const props = defineProps({
  jobFolder: JobFolder,
  job: Job,
})

const emits = defineEmits(['save', 'update', 'cancel'])

function formatDate(date: string): string {
  if (!date) return ''
  const [dateString, timeString] = date.split(' ')
  const [day, month, year] = dateString.split('/')
  const [hour, minute, second] = timeString.split(':')

  const dateObj = new Date(+year, +month - 1, +day, +hour, +minute, +second)

  return dateObj.toLocaleDateString('en-GB')
}

function formatDateTime(date: string): Date {
  // if (!date) return ''
  const [dateString, timeString] = date.split(' ')
  const [day, month, year] = dateString.split('/')
  const [hour, minute, second] = timeString.split(':')

  const dateObj = new Date(+year, +month - 1, +day, +hour, +minute, +second)

  return dateObj
}

onMounted(async () => {
  await jobsService.initialise()
  let brandid = brandStore.activeBrand?.id ?? 0
  setInitialJobFormValues()
})

////////////////////////////////////////////
// Job Dialog functions
////////////////////////////////////////////
// async function saveJob() {
//   submitted.value = true

//   const formData = new FormData()

//   if (job?.value.jobCode?.trim()) {
//     if (jobDateRange.value && jobDateRange.value.length === 2) {
//       job.value.dateFrom = jobDateRange.value[0]
//       job.value.dateTo = jobDateRange.value[1]
//     }
//     job.value.uploadedBy = 0 // Replace with actual user info
//     job.value.uploadedOn = new Date()
//     job.value.brandId = brandStore.activeBrand?.id ?? 0
//     job.value.brandName = brandStore.activeBrand?.name ?? ''

//     if (job.value.id) {
//       await jobsService
//         .updateJob(job.value)
//         .then((response) => {
//           if (response && response.data) {
//             console.log(response.data)
//           }
//         })
//         .catch((error) => {
//           console.error('Error updating job:', error)
//         })
//       toast.add({
//         severity: 'success',
//         summary: 'Successful',
//         detail: 'Job Updated',
//         life: 3000,
//       })
//     } else {
//       job.value.id = 0
//       var newJob = await jobsService.createJob(job.value).then((response) => {
//         return response
//       })
//       if (jobFolder.value.jobs) {
//         jobFolder.value.jobs.push(newJob)
//       }
//       toast.add({
//         severity: 'success',
//         summary: 'Successful',
//         detail: 'Job Created',
//         life: 3000,
//       })
//     }

//     jobDialog.value = false
//     job.value = new Job()
//   }
// }
// function editJob(jobModel: Job) {
//   job.value = jobModel
//   if (jobModel.dateTo != null && jobModel.dateFrom != null) {
//     // ukDateTo = formatDate(jobModel.dateTo)
//     // ukDateFrom = formatDate(jobModel.dateFrom)
//     jobDateRange.value = [jobModel.dateFrom, jobModel.dateTo]
//   } else {
//     // if (jobModel.uploadedOn != null) {
//     //   const startDate = formatDateTime(jobModel.uploadedOn.toString())
//     //   const endDateMonth = startDate.getMonth() + 1
//     //   const endDate = startDate
//     //   endDate.setMonth(endDateMonth)
//     //   jobDateRange.value = [startDate, endDate]
//     // } else {
//     //   jobDateRange.value = [new Date(), new Date()]
//     // }
//   }
//   job.value.dateFrom = jobModel.dateFrom
//   job.value.dateTo = jobModel.dateTo
//   job.value.jobFolderId = jobFolder?.value.id ?? 0
//   job.value.jobFolderName = jobFolder?.value.name ?? ''
//   jobDialog.value = true
// }
// function addJob(folder: JobFolder) {
//   job.value = new Job()
//   job.value.jobFolderId = folder.id
//   job.value.jobFolderName = folder.name
//   job.value.brandId = brandStore.activeBrand?.id ?? 0
//   jobDateRange.value = [new Date(), new Date()]
//   initialJobValues.value = { ...job.value }

//   jobDialog.value = true
//   setInitialJobFormValues()
// }

const baseSchema = z.object({
  jobFolder: z.string()!,
  jobCode: z.string().nonempty({ message: 'Job code is required' }),
  jobDescription: z.string().nullable().optional(),
  jobDateRange: z.array(z.date()).optional(),
})
const dateRangeSchema = baseSchema.refine(
  (data) => {
    console.log(data.jobDateRange[0], data.jobDateRange[1])
    if (data.jobDateRange[0] != null && data.jobDateRange[1] == null) {
      return false
    }
    return true
  },
  {
    message: 'Job date range requires a start and end date',
    path: ['jobDateRange'],
    when(payload) {
      return baseSchema.pick({ jobDateRange: true }).safeParse(payload.value).success
    },
  },
)

const resolver = ref(zodResolver(dateRangeSchema))
// const resolver = ({ values }: any) => {
//   const errors = {} as any

//   if (!values.jobCode) {
//     errors.jobCode = [{ message: 'Job Code is required.' }]
//   }
//   if (!values.jobDateRange) {
//     errors.jobDateRange = [{ message: 'Job Date Range is required.' }]
//   }
//   if (values.jobDateRange && values.jobDateRange.length === 2) {
//     if (values.jobDateRange[1] <= values.jobDateRange[0]) {
//       errors.jobDateRange = [
//         { message: 'Job Date Range must have an end date after the start date.' },
//       ]
//     }
//   }

//   return {
//     values, // (Optional) Used to pass current form values to submit event.
//     errors,
//   }
// }

function validateDateRange() {
  const dateRangeField = jobFormTemplateRef.value.fields?.['jobDateRange']
  console.log(dateRangeField)
  const dateRange = dateRangeField.states.value

  if (dateRange && dateRange.length === 2) {
    if (dateRange[1] == null && dateRange[0] != null) {
      dateRangeField.states.invalid = true
      dateRangeField.states.error = {
        message: 'Job Date Range must have an end date after the start date',
      }
      return false
    }
  }
  return true
}
async function submitJobForm({ valid }: any) {
  // resolver.call(jobFormTemplateRef.value)
  const validated = validateDateRange()

  // const validationResult = dateRangeSchema.safeParse(formValues)
  if (valid && validated) {
    //need to check the job date range validity before emitting the save event
    //return a new job object
    const jobCodeField = jobFormTemplateRef.value.fields?.['jobCode']
    const jobDescriptionField = jobFormTemplateRef.value.fields?.['jobDescription']
    const jobFolderField = jobFormTemplateRef.value.fields?.['jobFolder']
    const jobDateRangeField = jobFormTemplateRef.value.fields?.['jobDateRange']

    const newJob = {
      ...props.job,
      jobCode: jobCodeField.states.value ?? '',
      description: jobDescriptionField.states.value ?? '',
      jobFolderName: jobFolderField.states.value ?? '',
      dateFrom: jobDateRangeField.states.value?.[0] ?? null,
      dateTo: jobDateRangeField.states.value?.[1] ?? null,
      // uploadedOn: props.job.uploadedOn
    }
    emits('save', newJob)
  }
}

function setInitialJobFormValues() {
  if (props.job.id) {
    initialJobValues.value.jobCode = props.job.jobCode ?? ''
    initialJobValues.value.jobDescription = props.job.description ?? ''
    initialJobValues.value.jobFolder = props.job.jobFolderName ?? ''
    if (props.job.dateFrom && props.job.dateTo) {
      initialJobValues.value.jobDateRange = [props.job.dateFrom, props.job.dateTo]
    }
    jobFormTemplateRef.value?.setValues({ ...initialJobValues.value })
  } else {
    initialJobValues.value.jobFolder = props.job.jobFolderName
    const jobFolderField = jobFormTemplateRef.value.fields?.['jobFolder']

    jobFolderField.states.value = initialJobValues.value.jobFolder ?? ''
  }
  // jobFormTemplateRef.value.setFieldValue('jobCode', initialJobValues.value.jobCode)
  // jobFormTemplateRef.value.setFieldValue('jobDescription', initialJobValues.value.jobDescription)
  // jobFormTemplateRef.value.setFieldValue('jobFolderId', initialJobValues.value.jobFolderId)
  // jobFormTemplateRef.value.setFieldValue('jobFolderName', initialJobValues.value.jobFolderName)
  // jobFormTemplateRef.value.setFieldValue('brandId', initialJobValues.value.brandId)
  // jobFormTemplateRef.value.setFieldValue('jobDateRange', initialJobValues.value.jobDateRange)
}
</script>
<template>
  <!-- <form ref="job-form" @submit.prevent="submitJobForm"> -->
  <Form v-slot="$form" @submit="submitJobForm" ref="job-form" :resolver>
    <div class="flex flex-col gap-6">
      <div>
        <label for="JobFolder" class="block font-bold mb-3">JobFolder</label>
        <InputText name="jobFolder" v-model="job.jobFolderName" disabled fluid />
      </div>
      <div>
        <label for="jobCode" class="block font-bold mb-3">Job Code</label>
        <InputText name="jobCode" :value="job.jobCode" v-model="job.jobCode" autofocus fluid />
        <!-- <small v-if="submitted && !job.jobCode" class="text-red-500"
              >Job Code is required.</small
            > -->
        <Message v-if="$form.jobCode?.invalid" severity="error" size="small" variant="simple">{{
          $form.jobCode.error?.message
        }}</Message>
      </div>
      <div>
        <label for="description" class="block font-bold mb-3">Description</label>
        <InputText name="jobDescription" autofocus :invalid="submitted && !job.description" fluid />
        <!-- <small v-if="submitted && !job.description" class="text-red-500"
                >Description is required.</small
              > -->
        <Message
          v-if="$form.jobDescription?.invalid"
          severity="error"
          size="small"
          variant="simple"
          >{{ $form.jobDescription.error?.message }}</Message
        >
      </div>
      <div>
        <label for="jobDateRange" class="w-full block font-bold mb-3">Date Range</label>
        <DatePicker
          name="jobDateRange"
          v-tooltip="'Select the date range by selecting the start date, then the end date'"
          selectionMode="range"
          :manualInput="false"
          dateFormat="dd/mm/yy"
          class="w-full"
          placeHolder="Select Date Range"
        />
        <!-- <small v-if="submitted && !jobDateRange" class="text-red-500"
                >Date Range is required.</small
              > -->
        <Message
          v-if="$form.jobDateRange?.invalid"
          severity="error"
          size="small"
          variant="simple"
          >{{ $form.jobDateRange.error?.message }}</Message
        >
      </div>
    </div>
    <div class="mt-4 justify-end flex">
      <!-- Additional form fields can be added here -->
      <Button label="Cancel" icon="pi pi-times" text @click="emits('cancel')" />
      <Button label="Save" icon="pi pi-check" type="submit" />
    </div>
  </Form>
  <!-- Form fields go here -->
</template>
