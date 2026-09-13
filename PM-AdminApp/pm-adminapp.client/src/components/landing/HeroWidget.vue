<template>
  <div
    id="hero"
    class="flex flex-col pt-6 px-6 lg:px-20 overflow-hidden"
    style="
      background:
        linear-gradient(0deg, rgba(255, 255, 255, 0.2), rgb(160 187 227 / 20%)),
        radial-gradient(77.36% 256.97% at 77.36% 57.52%, rgb(189 221 221) 0%, rgb(181 190 251) 100%);
      /* clip-path: ellipse(150% 87% at 93% 13%); */
    "
  >
    <div class="mx-6 md:mx-20 mt-0 md:mt-6">
      <h1 class="text-6xl font-bold text-gray-900 leading-tight">
        <span class="font-light block">Administration System</span> by PlanMatr
      </h1>
      <p class="font-normal text-2xl leading-normal md:mt-4 text-gray-700">
        For administrators only. This system is designed to help administrators manage and organize
        their planograms efficiently, providing tools for creating, editing, and analyzing planogram
        inventory.
      </p>
      <Button
        label="Get Started"
        as="router-link"
        to="/home"
        rounded
        class="!text-xl mt-8 !px-4"
      ></Button>
    </div>
    <div class="flex justify-center md:justify-end h-20">
      <!-- <img src="/demo/images/landing/screen-1.png" alt="Hero Image" class="w-9/12 md:w-auto" /> -->
    </div>
  </div>
</template>
