<script setup lang="ts">
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { computed, onMounted, ref } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import LoggingTextarea from "./components/LoggingTextarea.vue";
import SplitContainer from "./components/SplitContainer.vue";
import { log } from "./lib/logging";
import CanvasArea from "./components/CanvasArea.vue";
import messaging, { MessageType } from "./lib/messaging";
import store from "./store";
import { useColorMode } from "@vueuse/core";
import CurveTabs from "./components/CurveTabs.vue";
import type { PresetCurve } from "./models/PresetCurve";

const items = ref(["Event", "Track"]);
const canvasAreaRef = ref<InstanceType<typeof CanvasArea> | null>(null);

const itemsDisplayed = computed(() => {
  return items.value;
});

const handleDisplayText = computed(() => {
  return canvasAreaRef.value?.handleDisplayText ?? "0.00, 0.00, 0.00, 0.00";
});

function applyClick() {
  messaging.sendMessage(MessageType.Apply);
}

function handleLoadPreset(preset: PresetCurve) {
  canvasAreaRef.value?.loadPreset(preset);
}

store.theme = useColorMode({
  disableTransition: false,
  onChanged: (newMode, defaultHandler) => {
    log("Theme changed to:", newMode);
    defaultHandler(newMode);
  },
});

onMounted(() => {
  // Recieve data
  messaging.setReceiveItems((data: string) => {
    const parsed = JSON.parse(data) as string[];
    log("receiveSettings", parsed);

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
          <div class="flex flex-col h-full">
            <div class="flex flex-1 justify-center">
              <CanvasArea ref="canvasAreaRef" />
            </div>

            <div class="mt-2 flex">
              <Button variant="secondary" class="w-full">
                {{ handleDisplayText }}
              </Button>
            </div>

            <div class="mt-2 flex gap-2">
              <VPFDropdownMenu
                buttonClasses="flex-1"
                :items="['S_Shape', 'S_BlurMoCurves']"
              />
              <VPFDropdownMenu
                buttonClasses="flex-1"
                :items="['Test prop', 'X', 'Y', 'Z']"
              />
            </div>

            <div class="mt-2 flex gap-2">
              <!-- TODO: change into a toggle icon -->
              <VPFDropdownMenu buttonClasses="flex-1" :items="itemsDisplayed" />
              <Button class="flex-1 min-w-2/3" @click="applyClick"
                >Apply</Button
              >
            </div>
          </div>
        </template>

        <template #secondary>
          <!-- <div class="flex flex-col h-full">
            <div class="flex justify-center gap-2 border-cyan-500 border-2">
              <div>library</div>
              <div>custom</div>
            </div>

            <div
              class="mt-2 flex flex-wrap gap-2 overflow-auto border-red-400 border-2"
            >
              <div v-for="_ in 100" class="w-20 h-20 border-cyan-500 border-2">
                item
              </div>
            </div>
          </div> -->
          <CurveTabs @load-preset="handleLoadPreset" />
        </template>
      </SplitContainer>
    </main>

    <footer>
      <LoggingTextarea />
    </footer>
  </div>
</template>
