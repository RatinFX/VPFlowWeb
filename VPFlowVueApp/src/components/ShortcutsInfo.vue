<script setup lang="ts">
import { Icon } from "@iconify/vue";
import { computed, nextTick, onMounted, onUnmounted, ref } from "vue";
import Kbd from "./ui/kbd/Kbd.vue";
import KbdGroup from "./ui/kbd/KbdGroup.vue";
import ScrollArea from "./ui/scroll-area/ScrollArea.vue";
import Separator from "./ui/separator/Separator.vue";

const isInfoVisible = ref(false);
const overlayRef = ref<HTMLElement | null>(null);
const availableSize = ref({ width: 0, height: 0 });

const PANEL_MIN_WIDTH = 220;
const PANEL_MIN_HEIGHT = 100;
const PANEL_MAX_WIDTH = 220;
const PANEL_MAX_HEIGHT = 300;
const PANEL_BOTTOM_OFFSET = 84;

const panelStyle = computed(() => {
  const widthAvailable = Math.max(0, availableSize.value.width);
  const heightAvailable = Math.max(
    0,
    availableSize.value.height + PANEL_BOTTOM_OFFSET
  );

  const width = Math.min(
    PANEL_MAX_WIDTH,
    Math.max(PANEL_MIN_WIDTH, widthAvailable)
  );

  const height = Math.min(
    PANEL_MAX_HEIGHT,
    Math.max(PANEL_MIN_HEIGHT, heightAvailable)
  );

  return {
    width: `${width}px`,
    height: `${height}px`,
  };
});

let sizeObserver: ResizeObserver | null = null;

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

function getContainer(): HTMLElement | null {
  if (!overlayRef.value) return null;
  const { offsetParent, parentElement } = overlayRef.value;
  if (offsetParent instanceof HTMLElement) return offsetParent;
  return parentElement instanceof HTMLElement ? parentElement : null;
}

function updateAvailableSize(container: HTMLElement) {
  const rect = container.getBoundingClientRect();
  availableSize.value = {
    width: rect.width,
    height: rect.height,
  };
}

onMounted(async () => {
  window.addEventListener("keydown", handleKeydown);

  await nextTick();

  const container = getContainer();
  if (!container) return;

  updateAvailableSize(container);

  sizeObserver = new ResizeObserver((entries) => {
    for (const entry of entries) {
      if (entry.target === container) {
        const { width, height } = entry.contentRect;
        availableSize.value = { width, height: height };
      }
    }
  });
  sizeObserver.observe(container);
});

onUnmounted(() => {
  window.removeEventListener("keydown", handleKeydown);
  if (sizeObserver) {
    sizeObserver.disconnect();
    sizeObserver = null;
  }
});
</script>

<template>
  <div
    ref="overlayRef"
    class="shortcuts-info"
    @mouseenter="showInfo"
    @mouseleave="hideInfo"
  >
    <div class="flex flex-col gap-3">
      <button
        type="button"
        class="w-5 h-5 flex items-center justify-center rounded-full text-muted-foreground hover:text-foreground transition-colors"
      >
        <Icon icon="radix-icons:question-mark-circled" class="size-5" />
      </button>

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
          type="auto"
          class="bg-background/90 backdrop-blur-sm border border-border rounded-lg"
          :style="panelStyle"
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

<style scoped>
.shortcuts-info {
  --shortcuts-offset: 8px;
  position: absolute;
  top: var(--shortcuts-offset);
  left: var(--shortcuts-offset);
  z-index: 20;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.5rem;
  pointer-events: auto;
}
</style>
