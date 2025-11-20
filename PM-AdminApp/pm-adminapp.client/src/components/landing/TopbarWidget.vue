<script setup lang="ts">
import { msal } from '@/config/auth'
import { useAuthStore } from '@/stores/auth'
import { onMounted } from 'vue'

const initialize = async () => {
  // let account = authStore.account
  try {
    await msal.initialize()
  } catch (error) {
    console.error('Initialization error: ', error)
  }
}

onMounted(async () => {
  initialize()
})
const auth = useAuthStore()

function login() {
  auth.login()
}

function logout() {
  auth.logout()
}

function smoothScroll(id: string) {
  document.body.click()
  const element = document.getElementById(id)
  if (element) {
    element.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    })
  }
}
</script>

<template>
  <a class="flex items-center" href="#">
    <svg
      svg:contrastcolor="ffffff"
      svg:template="BlackAndHighlightColorHex1"
      svg:presentation="2.5"
      svg:layouttype="undefined"
      svg:specialfontid="undefined"
      svg:id1="129"
      svg:id2="128"
      svg:companyname="PlanMatr"
      svg:companytagline=""
      version="1.1"
      viewBox="0 0 180 180"
      id="svg11"
      sodipodi:docname="P-Logo.svg"
      inkscape:version="1.3.2 (091e20e, 2023-11-25, custom)"
      width="60"
      height="50"
      xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape"
      xmlns:sodipodi="http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd"
      xmlns="http://www.w3.org/2000/svg"
      xmlns:svg="http://www.w3.org/2000/svg"
    >
      <defs id="defs11" />
      <sodipodi:namedview
        id="namedview11"
        pagecolor="#ffffff"
        bordercolor="#000000"
        borderopacity="0.25"
        inkscape:showpageshadow="2"
        inkscape:pageopacity="0.0"
        inkscape:pagecheckerboard="0"
        inkscape:deskcolor="#d1d1d1"
        inkscape:zoom="2.8609756"
        inkscape:cx="275.08099"
        inkscape:cy="205"
        inkscape:window-width="2560"
        inkscape:window-height="1369"
        inkscape:window-x="3448"
        inkscape:window-y="-8"
        inkscape:window-maximized="1"
        inkscape:current-layer="g9"
      />
      <g
        fill="#a2aaad"
        stroke="none"
        stroke-width="1"
        stroke-linecap="butt"
        stroke-linejoin="miter"
        stroke-miterlimit="10"
        stroke-dasharray="none"
        stroke-dashoffset="0"
        font-family="none"
        font-weight="none"
        font-size="none"
        text-anchor="none"
        style="mix-blend-mode: normal"
        id="g11"
      >
        <g
          data-paper-data='{"isGlobalGroup":true,"bounds":{"x":85.67458495848268,"y":115,"width":378.65083008303463,"height":240}}'
          id="g10"
        >
          <g
            data-paper-data='{"isIcon":"true","iconType":"icon","rawIconId":"4JJtJ7aSbFzmCJRGZUqAuJN7QAdU4uvJtmUzl1g_dK","source":"ai","selectedEffects":{"container":"","transformation":"","pattern":""},"isDetailed":false,"iconStyle":"standalone","bounds":{"x":205.61162178262924,"y":115,"width":138.77675643474151,"height":152.17058041835054},"fillRule":"evenodd","suitableAsStandaloneIcon":true}'
            fill-rule="evenodd"
            id="g9"
          >
            <path
              d="m 15.469552,66.348177 v -24.05065 h 9.1527 l 2.94438,2.9248 2.94438,2.92482 3.07929,-2.92482 3.0789,-2.9248 32.10035,0.0024 c 26.0497,0.002 32.844178,0.11455 36.046298,0.59719 24.99907,3.76819 43.7752,21.69778 48.6934,46.49716 0.97692,4.92384 0.9793,14.849123 0.005,20.090983 -2.09175,11.25884 -7.48103,21.6106 -15.32435,29.43401 -10.70871,10.68166 -22.70998,16.35447 -37.87294,17.90144 -2.321788,0.23717 -11.557828,0.43066 -20.524298,0.43066 -13.72388,0 -16.50843,-0.0978 -17.60483,-0.6179 -2.44543,-1.1606 -3.38312,-4.20312 -2.0751,-6.73295 1.23044,-2.37885 1.08214,-2.36251 22.84818,-2.52865 l 19.947128,-0.15187 4.28009,-1.14722 c 18.59512,-4.98359 31.54389,-17.61018 36.16131,-35.26196 1.51725,-5.80032 1.75561,-14.920743 0.543,-20.792683 -1.97911,-9.58556 -6.10614,-17.07432 -13.32473,-24.17747 -6.70472,-6.59805 -14.51921,-10.77959 -24.39871,-13.05627 -3.502,-0.80713 -5.06532,-0.85278 -34.386688,-1.004 l -30.71759,-0.15859 -1.58775,1.10023 c -0.87275,0.60533 -1.58736,1.2229 -1.58736,1.37251 0,0.14961 0.59721,0.85114 1.32689,1.55841 l 1.32724,1.28611 31.58957,0.12718 31.589958,0.12676 3.87262,1.07292 c 7.64895,2.11972 14.32603,5.99677 19.80268,11.49946 3.88303,3.90154 6.1427,7.1322 8.56465,12.24477 4.1416,8.74387 4.90275,18.959503 2.16991,29.135323 -3.76979,14.03685 -16.06529,25.76765 -31.1489,29.71933 -5.33994,1.39896 -10.333038,1.71162 -27.335718,1.71162 -17.77275,0 -18.31308,-0.0684 -19.56046,-2.4805 -1.16238,-2.2469 -0.4045,-5.46031 1.55053,-6.57722 0.69101,-0.39499 5.67043,-0.5754 20.47973,-0.74272 22.287638,-0.25174 22.714138,-0.30732 29.757668,-3.85241 8.89752,-4.47833 15.07559,-12.16978 17.37063,-21.62397 1.14841,-4.73185 1.22628,-11.389613 0.1807,-15.474143 -2.68795,-10.50155 -10.37167,-19.22907 -20.24938,-23.00088 -6.3665,-2.43116 -7.12111,-2.4764 -41.482228,-2.47723 l -31.23281,-0.0012 -2.86039,-2.82289 -2.86042,-2.82292 -3.05034,2.80618 -3.05037,2.80618 -4.58592,0.01754 z m 41.579,7.40794 c 5.39938,-1.75154 6.09336,-1.81677 22.21631,-2.09273 8.74387,-0.14961 17.1822,-0.19037 18.75146,-0.09091 20.812298,1.32025 33.040038,21.78096 24.161828,40.431073 -1.34665,2.83002 -2.29237,4.10742 -5.16013,6.97191 -2.91175,2.90789 -4.1731,3.83191 -7.38414,5.41038 -5.24989,2.58036 -8.02165,3.09275 -16.783048,3.10107 l -7.20403,0.007 -0.13345,-9.89555 -0.13374,-9.89525 -1.34398,-2.72837 c -1.70776,-3.46693 -4.26345,-6.022033 -7.73068,-7.728903 -2.5236,-1.24262 -3.02291,-1.34249 -6.6111,-1.32317 -3.30614,0.01783 -4.22838,0.181 -6.22621,1.09937 -3.07016,1.41115 -6.53115,4.721753 -7.94467,7.598423 l -1.12404,2.28761 -0.40747,87.5599 -5.29952,-0.0437 c -2.91442,-0.0235 -11.58211,-0.0508 -19.26079,-0.0606 l -13.96161,-0.0172 8.1e-4,-54.11388 c 4e-4,-51.474073 -0.16794,-56.032653 1.72716,-57.178103 0.34038,-0.20596 0.74761,-0.30166 1.23393,-0.53943 1.36763,-0.66813 1.71941,-0.69041 3.22483,-0.20389 0.93716,0.30286 2.06874,1.05866 2.54122,1.69706 0.84993,1.14871 0.85195,1.2456 1.05536,50.508443 l 0.20382,49.35765 4.66013,-4.259 0.12554,-37.9841 0.12596,-37.9841 0.92738,-3.05738 c 3.94024,-12.991563 13.36893,-22.815483 25.75285,-26.833743 z m -16.94949,31.698143 c 2.11972,-11.881493 11.96917,-21.579693 24.15203,-23.781413 4.62931,-0.83634 32.63376,-0.87557 36.524798,-0.05082 9.61023,2.03647 16.24896,11.26062 14.68327,20.400673 -0.60868,3.55342 -2.50487,7.47895 -4.79635,9.92854 -3.26484,3.49012 -9.32907,6.01668 -13.665628,5.69243 l -1.83437,-0.13672 -0.2455,-5.70699 c -0.28145,-6.54839 -0.86904,-8.85799 -3.30703,-13.006133 -4.50241,-7.65965 -13.01297,-12.57666 -21.74583,-12.56299 -12.72765,0.01961 -23.18848,9.20632 -25.03679,21.988353 -0.23685,1.64059 -0.40278,17.56351 -0.40765,39.14707 -0.0045,20.00984 -0.14554,36.37798 -0.31388,36.37352 -0.16837,-0.004 -1.26899,-0.91867 -2.44585,-2.03201 l -2.14007,-2.02458 0.01427,-35.57225 c 0.01099,-28.01663 0.13125,-36.2273 0.56458,-38.65668 z"
              data-paper-data='{"isPathIcon":true}'
              id="path9"
              style="fill: #0ea5e9; fill-opacity: 1"
            />
          </g>
        </g>
      </g>
    </svg>

    <span class="text-surface-900 dark:text-surface-0 font-medium text-2xl leading-normal mr-20"
      >PlanMatr</span
    >
  </a>
  <Button
    class="lg:!hidden"
    text
    severity="secondary"
    rounded
    v-styleclass="{
      selector: '@next',
      enterFromClass: 'hidden',
      enterActiveClass: 'animate-scalein',
      leaveToClass: 'hidden',
      leaveActiveClass: 'animate-fadeout',
      hideOnOutsideClick: true,
    }"
  >
    <i class="pi pi-bars !text-2xl"></i>
  </Button>
  <div
    class="items-center bg-surface-0 dark:bg-surface-900 grow justify-between hidden lg:flex absolute lg:static w-full left-0 top-full px-12 lg:px-0 z-20 rounded-border"
  >
    <ul
      class="list-none p-0 m-0 flex lg:items-center select-none flex-col lg:flex-row cursor-pointer gap-8"
    >
      <li>
        <RouterLink
          to="/home"
          class="px-0 py-4 text-surface-900 dark:text-surface-0 font-medium text-xl"
          ><span>Home</span></RouterLink
        >
        <!-- <a
          @click="smoothScroll('hero')"
          class="px-0 py-4 text-surface-900 dark:text-surface-0 font-medium text-xl"
        >
          <span>Home</span>
        </a> -->
      </li>
      <li>
        <a
          @click="smoothScroll('features')"
          class="px-0 py-4 text-surface-900 dark:text-surface-0 font-medium text-xl"
        >
          <span>Features</span>
        </a>
      </li>
      <li>
        <a
          @click="smoothScroll('highlights')"
          class="px-0 py-4 text-surface-900 dark:text-surface-0 font-medium text-xl"
        >
          <span>Highlights</span>
        </a>
      </li>
      <li>
        <a
          @click="smoothScroll('pricing')"
          class="px-0 py-4 text-surface-900 dark:text-surface-0 font-medium text-xl"
        >
          <span>Pricing</span>
        </a>
      </li>
    </ul>
    <div
      v-if="!auth.account"
      class="flex border-t lg:border-t-0 border-surface py-4 lg:py-0 mt-4 lg:mt-0 gap-2"
    >
      <!-- <Button label="Login" text as="router-link" to="/auth.login" rounded></Button> -->
      <Button label="Login" @click="login" rounded></Button>
    </div>
    <div
      v-if="auth.account"
      class="flex border-t lg:border-t-0 border-surface py-4 lg:py-0 mt-4 lg:mt-0 gap-2"
    >
      <!-- <Button label="Logout" text as="router-link" to="/auth.logout" rounded></Button> -->
      <!-- <Button label="Register" to="/login" rounded></Button> -->
      <Button label="Logout" @click="logout" rounded></Button>
    </div>
  </div>
</template>
