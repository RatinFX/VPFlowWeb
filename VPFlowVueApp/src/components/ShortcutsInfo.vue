<script setup lang="ts">
import { Icon } from "@iconify/vue";
import { onMounted, onUnmounted, ref } from "vue";
import Kbd from "./ui/kbd/Kbd.vue";
import ScrollArea from "./ui/scroll-area/ScrollArea.vue";
import Separator from "./ui/separator/Separator.vue";

const isInfoVisible = ref(false);

function showInfo() {
  isInfoVisible.value = true;
}

function hideInfo() {
  isInfoVisible.value = false;
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === "Escape") {
    hideInfo();
  }
}

onMounted(() => {
  window.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
  window.removeEventListener("keydown", handleKeydown);
});
</script>

<template>
  <div class="absolute top-13 left-4 z-20">
    <!-- Question mark icon button -->
    <div @mouseenter="showInfo" @mouseleave="hideInfo" class="relative flex">
      <button
        class="w-8 h-8 flex items-center justify-center bg-muted/20 hover:bg-muted/40 rounded-full text-muted-foreground hover:text-foreground transition-colors"
      >
        <Icon icon="radix-icons:question-mark-circled" class="w-5 h-5" />
      </button>

      <!-- Info overlay -->
      <Transition
        enter-active-class="transition-opacity duration-200"
        leave-active-class="transition-opacity duration-200"
        enter-from-class="opacity-0"
        leave-to-class="opacity-0"
      >
        <ScrollArea
          v-if="isInfoVisible"
          @mouseenter="showInfo"
          @mouseleave="hideInfo"
          class="min-h-40 bg-background/90 backdrop-blur-sm border border-border rounded-lg w-60"
        >
          <div class="px-4 py-2 space-y-1 text-xs">
            <h3 class="text-sm font-semibold">Keyboard shortcuts</h3>

            <div class="flex justify-between">
              <span class="text-muted-foreground">
                Select prev / next point
              </span>
              <span>
                <Kbd> Q </Kbd>
                <span class="mx-1 text-muted-foreground">/</span>
                <Kbd> E </Kbd>
              </span>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Move point</span>
              <span>
                <Kbd> WASD </Kbd>
                <span class="mx-1 text-muted-foreground">or</span>
                <Kbd> Arrows </Kbd>
              </span>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Bigger steps</span>
              <Kbd> Shift + </Kbd>
            </div>

            <Separator />

            <div class="flex justify-between">
              <span class="text-muted-foreground">Add point</span>
              <Kbd> Ctrl + Click </Kbd>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Delete point</span>
              <span>
                <Kbd> Del </Kbd>
                <span class="mx-1 text-muted-foreground">or</span>
                <Kbd> X </Kbd>
              </span>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Reset view</span>
              <Kbd class="px-1.5 py-0.5 bg-muted rounded text-xs font-mono">
                R
              </Kbd>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Export data</span>
              <Kbd> Ctrl + E </Kbd>
            </div>
          </div>
        </ScrollArea>
      </Transition>
    </div>
  </div>
</template>
