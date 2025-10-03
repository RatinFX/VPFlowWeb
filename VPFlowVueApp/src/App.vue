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
  <div class="flex flex-col min-h-screen">
    <header>
      <!-- TODO: rethink: keep current top row and open web windows -->
      <VPFMenuBar />
    </header>

    <main class="flex-1 flex items-stretch min-h-0">
      <SplitContainer>
        <template #primary>
          <CanvasArea />

          <div class="mt-2 flex items-center gap-3">
            <div>FX selector</div>
            <div>Parameter selector</div>
          </div>

          <div class="mt-3 flex items-center gap-3">
            <!-- TODO: change into a toggle icon -->
            <VPFDropdownMenu :items="itemsDisplayed" />

            <Button @click="messaging.sendButtonClick">Apply</Button>
          </div>
        </template>

        <template #secondary>
          <div>bottom/right section</div>
        </template>
      </SplitContainer>
    </main>

    <footer>
      <LoggingTextarea />
    </footer>
  </div>
</template>
