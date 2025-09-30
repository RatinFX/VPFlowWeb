<script setup lang="ts">
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { computed, onMounted, ref } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import { log, warn } from "./store";
import LoggingTextarea from "./components/LoggingTextarea.vue";

const items = ref(["Event", "Track"]);

const itemsDisplayed = computed(() => {
  return items.value;
});

onMounted(() => {
  // Recieve data
  window.ReceiveFromHost = (data) => {
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
    <header class="mb-4">
      <!-- TODO: rethink, keep current top row and open web windows -->
      <VPFMenuBar />
    </header>

    <main class="flex flex-col flex-1 items-center justify-center">
      <div>
        <!-- TODO: change into a toggle icon -->
        <VPFDropdownMenu :items="itemsDisplayed" />
      </div>

      <div>
        <Button @click="sendButtonClick">Apply</Button>
      </div>
    </main>

    <div class="w-full h-30 grid gap-1">
      <LoggingTextarea />
    </div>
  </div>
</template>
