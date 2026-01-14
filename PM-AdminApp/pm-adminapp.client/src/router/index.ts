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
        path: '/products',
        name: 'products',
        component: () => import('@/views/Products/ProductListView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/stands',
        name: 'stands',
        component: () => import('@/views/Stands/StandListView.vue'),
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
        component: () => import('@/views/Clusters/EditClusterView.vue'),
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
        path: '/user',
        name: 'user',
        component: () => import('@/views/UserView.vue'),
        meta: { requiresAuth: true },
      },
      // { path: '/uikit/formlayout', name: 'formlayout', component: () => import('@/views/uikit/FormLayout.vue') },
      // { path: '/uikit/input', name: 'input', component: () => import('@/views/uikit/InputDoc.vue') },
      // { path: '/uikit/button', name: 'button', component: () => import('@/views/uikit/ButtonDoc.vue') },
      // { path: '/uikit/table', name: 'table', component: () => import('@/views/uikit/TableDoc.vue') },
      // { path: '/uikit/list', name: 'list', component: () => import('@/views/uikit/ListDoc.vue') },
      // { path: '/uikit/tree', name: 'tree', component: () => import('@/views/uikit/TreeDoc.vue') },
      // { path: '/uikit/panel', name: 'panel', component: () => import('@/views/uikit/PanelsDoc.vue') },
      // { path: '/uikit/overlay', name: 'overlay', component: () => import('@/views/uikit/OverlayDoc.vue') },
      // { path: '/uikit/media', name: 'media', component: () => import('@/views/uikit/MediaDoc.vue') },
      // { path: '/uikit/message', name: 'message', component: () => import('@/views/uikit/MessagesDoc.vue') },
      // { path: '/uikit/file', name: 'file', component: () => import('@/views/uikit/FileDoc.vue') },
      // { path: '/uikit/menu', name: 'menu', component: () => import('@/views/uikit/MenuDoc.vue') },
      // { path: '/uikit/charts', name: 'charts', component: () => import('@/views/uikit/ChartDoc.vue') },
      // { path: '/uikit/misc', name: 'misc', component: () => import('@/views/uikit/MiscDoc.vue') },
      // { path: '/uikit/timeline', name: 'timeline', component: () => import('@/views/uikit/TimelineDoc.vue') },
      // { path: '/pages/empty', name: 'empty', component: () => import('@/views/pages/Empty.vue') },
      // { path: '/pages/crud', name: 'crud', component: () => import('@/views/pages/Crud.vue') },
      // { path: '/documentation', name: 'documentation', component: () => import('@/views/pages/Documentation.vue') }
    ],
  },
  {
    path: '/testCluster/:id',
    name: 'testEditCluster',
    component: () => import('@/views/Clusters/EditClusterView.vue'),
    meta: { requiresAuth: true },
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
