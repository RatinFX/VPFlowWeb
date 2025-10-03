<script setup lang="ts">
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { computed, onMounted, ref } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import { log, warn } from "./store";
import LoggingTextarea from "./components/LoggingTextarea.vue";
import SplitContainer from "./components/SplitContainer.vue";

const items = ref(["Event", "Track"]);

const itemsDisplayed = computed(() => {
  return items.value;
});

onMounted(() => {
  // Recieve data
  window.ReceiveFromHost = (data: string) => {
    const parsed = JSON.parse(data) as string[];
    log("ReceiveFromHost", parsed);

    items.value = parsed;
  };
});

interface WebMessage {
  sender: string;
  payload: object;
}

// Send data
function sendButtonClick() {
  const payload: WebMessage = {
    sender: "btnApply",
    payload: {
      coordinates: [0.7, 0.27, 0.5, 1.0],
    },
  };

  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage(payload);
    log("sent message with payload:", payload);
  } else {
    warn("no webview.postMessage available for payload:", payload);
  }
}
</script>

<template>
  <div class="flex flex-col min-h-screen">
    <header>
      <!-- TODO: rethink: keep current top row and open web windows -->
      <VPFMenuBar />
    </header>

    <main class="flex-1 flex items-stretch">
      <SplitContainer>
        <template #primary>
          <div>canvas area</div>

          <div class="mt-2 flex items-center gap-3">
            <div>FX selector</div>
            <div>Parameter selector</div>
          </div>

          <div class="mt-3 flex items-center gap-3">
        <!-- TODO: change into a toggle icon -->
        <VPFDropdownMenu :items="itemsDisplayed" />
      
      <!-- Apply -->
        <Button @click="sendButtonClick">Apply</Button>
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
