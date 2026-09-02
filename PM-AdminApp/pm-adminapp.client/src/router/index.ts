import { msal } from '@/services/Identity/auth'
import { useAuthStore } from '@/stores/auth'
import { createRouter, createWebHistory, type Router, type RouteRecordRaw } from 'vue-router'
import { VueNavigationClient } from './helpers'
// ---------------------------------------------------------------------------------------------------------------------
// setup
// ---------------------------------------------------------------------------------------------------------------------

// special routes
const unmatched = '/:pathMatch(.*)*'
const unguarded = ['/', '/login', '/logout']
// create router
// const router = createRouter({
//   history: createWebHistory(import.meta.env.BASE_URL),
//   routes: [
//     route('/', 'LandingPage'),
//     route('/user', 'User'),
//     hook('/login', auth.login),
//     hook('/logout', auth.logout),
//     route(unmatched, '404'),
//   ],
// })

const routes: RouteRecordRaw[] = [
  { path: '/', name: 'Welcome', component: () => import('@/views/LandingPageView.vue') },
  // hook('/login', auth.login),
  // hook('/logout', auth.logout),
  {
    path: '/home',
    component: () => import('@/layout/AppLayout.vue'),
    meta: {
      layout: 'AppLayoutAdmin',
    },
    children: [
      {
        path: '/home',
        name: 'home',
        component: () => import('@/views/Dashboard.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/parts',
        name: 'parts',
        component: () => import('@/views/Parts/PartList.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/parts/edit/:id',
        name: 'editPart',
        component: () => import('@/views/Parts/EditPart.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/parts/new/',
        name: 'newPart',
        component: () => import('@/views/Parts/EditPart.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/parts/copy/:id',
        name: 'copyPart',
        component: () => import('@/views/Parts/EditPart.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/products',
        name: 'products',
        component: () => import('@/views/Products/ProductListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/products/edit/:id',
        name: 'editProduct',
        component: () => import('@/views/Products/EditProduct.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/products/new/',
        name: 'newProduct',
        component: () => import('@/views/Products/EditProduct.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/stands',
        name: 'stands',
        component: () => import('@/views/Stands/StandListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/stands/edit/:id',
        name: 'editStand',
        component: () => import('@/views/Stands/EditStand.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/stands/new/',
        name: 'newStand',
        component: () => import('@/views/Stands/EditStand.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/planograms',
        name: 'planograms',
        component: () => import('@/views/planograms/planogramListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/planograms/edit/:id',
        name: 'editPlanogram',
        component: () => import('@/views/planograms/editPlanogram.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/planograms/create',
        name: 'createPlanogram',
        component: () => import('@/views/planograms/createPlanogram.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/clusters',
        name: 'clusters',
        component: () => import('@/views/Clusters/ClusterListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/clusters/edit/:id',
        name: 'editCluster',
        component: () => import('@/views/Clusters/editCluster.vue'),
        meta: { requiresAuth: true },
      },

      {
        path: '/clusters/create',
        name: 'createCluster',
        component: () => import('@/views/Clusters/createCluster.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/brands',
        name: 'brands',
        component: () => import('@/views/Settings/BrandListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/categories',
        name: 'categories',
        component: () => import('@/views/Settings/CategoryListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/standTypes',
        name: 'standtypes',
        component: () => import('@/views/Settings/StandTypeListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/regions',
        name: 'regions',
        component: () => import('@/views/Settings/RegionsListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/countries',
        name: 'countries',
        component: () => import('@/views/Settings/CountriesListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/jobfolders',
        name: 'jobfolders',
        component: () => import('@/views/Settings/JobFolderListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/categories',
        name: 'categories',
        component: () => import('@/views/Settings/CategoryListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/standtypes',
        name: 'standtypes',
        component: () => import('@/views/Settings/StandTypeListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/user',
        name: 'user',
        component: () => import('@/views/UserView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/users',
        name: 'users',
        component: () => import('@/views/Users/UserListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/users/manage',
        name: 'manageUser',
        component: () => import('@/views/Users/ManageUser.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/reporting/useractions',
        name: 'userActions',
        component: () => import('@/views/Reporting/useractions.report.vue'),
        meta: { requiresAuth: true },
      },
    ],
  },
  // {
  //   path: '/pages/notfound',
  //   name: 'notfound',
  //   component: () => import('@/views/pages/NotFound.vue'),
  // },
  // { path: '/auth/login', name: 'login', component: () => import('@/views/pages/auth/Login.vue') },
  // {
  //   path: '/auth/access',
  //   name: 'accessDenied',
  //   component: () => import('@/views/pages/auth/Access.vue'),
  // },
  // { path: '/auth/error', name: 'error', component: () => import('@/views/pages/auth/Error.vue') },
]

const router: Router = createRouter({
  history: createWebHistory(),
  routes,
})
// ---------------------------------------------------------------------------------------------------------------------
// authentication
// ---------------------------------------------------------------------------------------------------------------------

// hook MSAL into router
const client = new VueNavigationClient(router)

// set up auth and guard routes
router.beforeEach(async (to, from, next) => {
  // 404
  if (to.matched[0]?.path === unmatched) {
    return next()
  }

  // guarded
  const guarded = unguarded.every((path) => path !== to.path)
  const auth = useAuthStore()
  const brands = await import('@/services/Brands/BrandService').then((m) => m.default)

  // initialized
  if (!auth.initialized) {
    await msal.initialize()
  }
  if (guarded) {
    await auth.initialize(client)
    await brands.initialise()
    // authorised
    if (auth.account) {
      return next()
    }

    // unauthorised
    try {
      await auth.login()
      return next()
    } catch (err) {
      return next(false)
    }
  }

  // unguarded
  next()
})

// export
export default router
