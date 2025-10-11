<script setup lang="ts">
import { useSettings } from "@/composables/useSettings";
import Textarea from "./ui/textarea/Textarea.vue";
import { nextTick, ref, watch } from "vue";

const { logs, displayLogs } = useSettings();

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

watch(logs, () => {
  scrollLogsToBottom();
});
</script>

<template>
  <Textarea
    v-if="displayLogs"
    ref="textareaRef"
    v-model="logs"
    id="logs"
    class="h-24 resize-none p-1 disabled:cursor-default"
    style="font-size: 12px"
    disabled
  />
</template>
