<script setup lang="ts">
import VPFMenuBar from "./components/VPFMenuBar.vue";
import Button from "./components/ui/button/Button.vue";
import { computed, nextTick, onMounted, ref, watch } from "vue";
import VPFDropdownMenu from "./components/VPFDropdownMenu.vue";
import Textarea from "./components/ui/textarea/Textarea.vue";
import store, { log } from "./store";

const items = ref(["Event", "Track"]);

const itemsDisplayed = computed(() => {
  return items.value;
});

const textareaRef = ref<any>(null);

function scrollLogsToBottom() {
  nextTick(() => {
    const comp = textareaRef.value;
    if (!comp) return;

    // component root element (script-setup SFC root) should be available as $el
    let rootEl: HTMLElement | null = null;
    if (comp.$el instanceof HTMLElement) {
      rootEl = comp.$el;
    } else if (comp instanceof HTMLElement) {
      rootEl = comp;
    }

    const textarea: HTMLTextAreaElement | null =
      (rootEl && rootEl.tagName === "TEXTAREA"
        ? (rootEl as HTMLTextAreaElement)
        : null) ||
      (rootEl &&
        (rootEl.querySelector(
          '[data-slot="textarea"]'
        ) as HTMLTextAreaElement | null)) ||
      (document.querySelector(
        '[data-slot="textarea"]'
      ) as HTMLTextAreaElement | null);

    if (textarea) {
      textarea.scrollTop = textarea.scrollHeight;
    }
  });
}

watch(store.logs, () => {
  console.log("[store.logs changed - scrollLogsToBottom()]");
  scrollLogsToBottom();
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
    console.warn("no webview.postMessage available");
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
      <Textarea
        ref="textareaRef"
        v-model="store.logs.value"
        id="logs"
        class="border-sky-500 border-2"
        style="font-size: 12px"
      />
    </div>
  </div>
</template>
