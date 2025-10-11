<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import CanvasArea from "./components/CanvasArea.vue";
import CurveTabs from "./components/CurveTabs.vue";
import LoggingTextarea from "./components/LoggingTextarea.vue";
import SplitContainer from "./components/SplitContainer.vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { useCurvePoints } from "./composables/useCurvePoints";
import { useLogging } from "./composables/useLogging";
import { useMessaging } from "./composables/useMessaging";

// Use composables
const { points } = useCurvePoints();
const { sendApply, onReceiveItems } = useMessaging();
const { log } = useLogging();

const items = ref(["Event", "Track"]);
const canvasAreaRef = ref<InstanceType<typeof CanvasArea> | null>(null);

const itemsDisplayed = computed(() => {
  return items.value;
});

const handleDisplayText = computed(() => {
  return canvasAreaRef.value?.handleDisplayText ?? "0.00, 0.00, 0.00, 0.00";
});

onMounted(() => {
  // Receive items data from backend
  onReceiveItems((data: string) => {
    const parsed = JSON.parse(data) as string[];
    log("receiveItems", parsed);

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
              <Button class="flex-1 min-w-2/3" @click="sendApply(points)"
                >Apply</Button
              >
            </div>
          </div>
        </template>

        <template #secondary>
          <CurveTabs />
        </template>
      </SplitContainer>
    </main>

    <footer>
      <LoggingTextarea />
    </footer>
  </div>
</template>
