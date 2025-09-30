<script setup lang="ts">
import store from "@/store";
import Textarea from "./ui/textarea/Textarea.vue";
import { nextTick, ref, watch } from "vue";

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
</script>

<template>
  <Textarea
    ref="textareaRef"
    v-model="store.logs.value"
    id="logs"
    class="border-sky-500 border-2"
    style="font-size: 12px"
  />
</template>
