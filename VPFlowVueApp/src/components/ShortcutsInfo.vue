<script setup lang="ts">
import { Icon } from "@iconify/vue";
import { onMounted, onUnmounted, ref } from "vue";
import Kbd from "./ui/kbd/Kbd.vue";
import KbdGroup from "./ui/kbd/KbdGroup.vue";
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
        class="w-8 h-8 flex items-center justify-center rounded-full text-muted-foreground hover:text-foreground transition-colors"
      >
        <Icon icon="radix-icons:question-mark-circled" class="size-5" />
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
          class="w-54 max-h-[calc(100vh-3.25rem-1rem)] bg-background/90 backdrop-blur-sm border border-border rounded-lg"
        >
          <div class="px-4 py-2 space-y-1 text-xs">
            <h3 class="text-sm font-semibold">How to</h3>

            <div class="flex justify-between">
              <span class="text-muted-foreground"> Close this panel </span>
              <Kbd> Esc </Kbd>
            </div>

            <Separator />

            <div class="flex justify-between">
              <span class="text-muted-foreground"> Move view </span>
              <Kbd>
                Mid click
                <Icon icon="qlementine-icons:mouse-middle-button-16" />
              </Kbd>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground"> Zoom </span>
              <Kbd>
                Scroll
                <Icon icon="qlementine-icons:mouse-middle-button-16" />
              </Kbd>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Reset view</span>
              <KbdGroup>
                <Kbd> R </Kbd>
              </KbdGroup>
            </div>

            <Separator />

            <div class="flex justify-between">
              <span class="text-muted-foreground"> Prev / next point </span>
              <KbdGroup>
                <Kbd> Q </Kbd>
                <span> / </span>
                <Kbd> E </Kbd>
              </KbdGroup>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Move point</span>
              <KbdGroup>
                <Kbd> WASD </Kbd>
                <span> , </span>
                <Kbd> Arrows </Kbd>
              </KbdGroup>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Bigger steps</span>
              <Kbd> Shift + </Kbd>
            </div>

            <Separator />

            <div class="flex justify-between">
              <span class="text-muted-foreground">Add point</span>
              <KbdGroup>
                <Kbd> Ctrl </Kbd>
                <span> + </span>
                <Kbd> Click </Kbd>
              </KbdGroup>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Delete point</span>
              <KbdGroup>
                <Kbd> Del </Kbd>
                <span> or </span>
                <Kbd> X </Kbd>
              </KbdGroup>
            </div>

            <div class="flex justify-between">
              <span class="text-muted-foreground">Export data</span>
              <KbdGroup>
                <Kbd> Ctrl </Kbd>
                <span> + </span>
                <Kbd> E </Kbd>
              </KbdGroup>
            </div>
          </div>
        </ScrollArea>
      </Transition>
    </div>
  </div>
</template>
