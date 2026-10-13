<script lang="ts" setup>
import type { Role } from '@/models/Identity/role.model';
import type { User } from '@/models/Identity/user.model';
import { PlanoWidgetFilter } from '@/models/Widgets/planoWidgetFilter.model';
import type { PopularPartDto } from '@/models/Widgets/PopularPartDto';
import { default as roleService } from '@/services/Identity/RoleService';
import { default as userService } from '@/services/Identity/UserService';
import { default as widgetService } from '@/services/Widgets/widgetService';
import { useAuthStore } from '@/stores/auth';
import { useBrandStore } from '@/stores/brandStore';
import { useSystemStore } from '@/stores/systemStore';
import { storeToRefs } from 'pinia';
import { onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const layout = useSystemStore();
const brandStore = useBrandStore();
const authStore = useAuthStore();
const mappedUser = ref<User | null>(null);
const loading = ref(false);
const popularParts = ref<PopularPartDto[]>([]);

const brand = storeToRefs(brandStore).activeBrand;
const systemRoles = ref<Role[]>([]);
const currentUser = ref<any | null>(null);
const graphUser = ref<User | null>(null);

watch(brand, async (newBrand) => {
    if (newBrand) {
        let filter = new PlanoWidgetFilter();
        filter.brandId = newBrand.id;
        await widgetService.getPopularParts(newBrand.id).then((response) => {
            popularParts.value = response;
            console.log('Popular parts loaded for brand change', popularParts.value);
        });
    }
});

onMounted(async () => {
    // const currentUser = ref(authStore.currentlyLoggedInUser)
    // if (!currentUser.value) {
    //   console.log('No current user found in authStore.')
    //   currentUser.value = await userService.getCurrentUserInfo()
    //   authStore.setCurrentlyLoggedInUserInfo(currentUser.value)
    // }
    loading.value = true;
    layout.layoutState.disableBrandSelect = false;
    await widgetService.initialise();
    await roleService.initialise();
    await userService.initialise();
    currentUser.value = authStore.currentlyLoggedInUser;
    if (!currentUser.value) {
        console.log('No current user found in authStore.');
        currentUser.value = await userService.getCurrentUserInfo();
        authStore.setCurrentlyLoggedInUserInfo(currentUser.value);
    }
    graphUser.value = await userService.getUser(currentUser.value?.id ?? '');
    mappedUser.value = userService.mapUser(currentUser.value);
    systemRoles.value = await roleService.getRoles();

    await widgetService
        .getPopularParts(brand.value?.id ?? 0)
        .then((response) => {
            popularParts.value = response;
            console.log('Popular parts loaded', popularParts.value);
        })
        .catch((error) => {
            console.log('Error loading popular parts', error);
        });
    loading.value = false;
});

function viewPlanogram(planogram) {
    router.push({ name: 'editPlanogram', params: { id: planogram.id } });
}
</script>
<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">Popular Parts</div>
        <DataTable :value="popularParts" :rows="5" :paginator="true" responsiveLayout="scroll">
            <template #empty> No popular parts found. </template>

            <Column style="width: 10%" header="Brand">
                <template #body="slotProps">
                    <span>{{ slotProps.data.brandName }}</span>
                </template>
            </Column>
            <Column field="name" header="Name" :sortable="true" style="width: 20%"></Column>

            <Column field="partCount" header="Usage" :sortable="true" style="width: 20%">
                <template #body="slotProps">
                    <span
                        >Used <span class="font-semibold">{{ slotProps.data.partCount }}</span> times recently</span
                    >
                </template>
            </Column>
            <!-- <Column style="width: 15%" header="Edit">
                <template #body="slotProps">
                    <Button icon="pi pi-search" type="button" class="p-button-text" :onclick="() => viewPlanogram(slotProps.data)"></Button>
                </template>
            </Column> -->
        </DataTable>
    </div>
</template>
