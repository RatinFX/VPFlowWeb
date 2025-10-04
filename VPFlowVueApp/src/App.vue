<script setup lang="ts">
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { computed, onMounted, ref } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import LoggingTextarea from "./components/LoggingTextarea.vue";
import SplitContainer from "./components/SplitContainer.vue";
import { log } from "./lib/logging";
import CanvasArea from "./components/CanvasArea.vue";
import messaging from "./lib/messaging";

const items = ref(["Event", "Track"]);

const itemsDisplayed = computed(() => {
  return items.value;
});

onMounted(() => {
  // Recieve data
  messaging.setRecieveFromHost((data: string) => {
    const parsed = JSON.parse(data) as string[];
    log("ReceiveFromHost", parsed);

    items.value = parsed;
  });
});
</script>

<template>
  <div class="flex flex-col h-screen">
    <header>
      <!-- TODO: rethink: keep current top row and open web windows -->
      <VPFMenuBar />
    </header>

    <main class="flex-1 flex min-h-0">
      <SplitContainer>
        <template #primary>
<div class="flex justify-center border-cyan-500 border-2">
          <CanvasArea />
</div>

          <div class="mt-2 flex justify-center gap-3 border-cyan-500 border-2">
            <div>FX selector</div>
            <div>Parameter selector</div>
          </div>

          <div class="mt-2 flex justify-center border-cyan-500 border-2">
            <div>coordinates</div>
          </div>

          <div class="mt-2 flex justify-center gap-3 border-cyan-500 border-2">
            <!-- TODO: change into a toggle icon -->
            <VPFDropdownMenu :items="itemsDisplayed" />

            <Button @click="messaging.sendButtonClick">Apply</Button>
          </div>
        </template>

        <template #secondary>
          <div class="flex flex-col h-full">
            <div class="flex justify-center gap-3 border-cyan-500 border-2">
              <div>library</div>
              <div>custom</div>
            </div>

            <div
              class="mt-2 flex-1 flex flex-wrap gap-2 border-red-400 border-2 overflow-auto"
            >
              <!-- Added flex-1, overflow-auto -->
              <div v-for="_ in 100" class="w-20 h-20 border-cyan-500 border-2">
                item
              </div>
            </div>
          </div>
        </template>
      </SplitContainer>
    </main>

    <footer>
      <LoggingTextarea />
    </footer>
  </div>
</template>
