<script setup lang="ts">
import { ref } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";

const eventTrackItems = ref(["Event", "Track"]);
</script>

<template>
  <div class="flex flex-col min-h-screen">
    <header class="mb-4">
      <VPFMenuBar />
    </header>

    <main class="flex flex-col flex-1 items-center justify-center">
      <div>
        <VPFDropdownMenu :items="eventTrackItems" />
      </div>

      <div>
        <Button>Apply</Button>
      </div>
    </main>
  </div>
</template>
