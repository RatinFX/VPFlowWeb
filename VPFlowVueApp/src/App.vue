<script setup lang="ts">
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { computed, onMounted, ref } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";

const items = ref(["Event", "Track"]);

const itemsDisplayed = computed(() => {
  return items.value;
});

// Recieve data
onMounted(() => {
  window.receiveFromHost = (data) => {
    const parsed = JSON.parse(data) as string[];
    console.log("receiveFromHost", parsed);

    items.value = parsed;
  };
});

// Send data
function sendButtonClick() {
  const payload = {
    sender: "btnApply",
    payload: {
      // TODO: change returned coords to numbers instead
      coordinates: "0.70, 0.27, 0.50, 1.00",
    },
  };

  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage(payload);
    console.log("sent message with payload:", payload);
  } else {
    console.warn("no webview.postMessage available");
  }
}
</script>

<template>
  <div class="flex flex-col min-h-screen">
    <header class="mb-4">
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
  </div>
</template>
