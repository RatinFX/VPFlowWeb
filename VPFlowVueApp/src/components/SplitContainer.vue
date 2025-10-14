<script setup lang="ts">
import { useLogging } from "@/composables/useLogging";
import { nextTick, onBeforeUnmount, onMounted, ref } from "vue";

const { log } = useLogging();

const MIN_SPLIT = 0;
const MAX_SPLIT = 95;
const DEFAULT_SPLIT = 50;
/** Percentage (0-100) */
const split = ref<number>(loadSplit());
const containerRef = ref<HTMLElement | null>(null);
/** Orientation is derived from the current container size, not the window */
const isLandscape = ref(false);

let dragging = false;
let ro: ResizeObserver | null = null;

function loadSplit(): number {
  try {
    const raw = localStorage.getItem("vpflow.splitPercent");
    const value = raw ? parseFloat(raw) : DEFAULT_SPLIT;
    log("Loading [vpflow.splitPercent]:", value);
    return isFinite(value) ? clamp(value, MIN_SPLIT, MAX_SPLIT) : DEFAULT_SPLIT;
  } catch {
    return DEFAULT_SPLIT;
  }
}

function saveSplit() {
  try {
    log("Save [vpflow.splitPercent]:", split.value);
    localStorage.setItem("vpflow.splitPercent", String(split.value));
  } catch {}
}

function clamp(v: number, a = 0, b = 100) {
  return Math.min(b, Math.max(a, v));
}

function updateOrientationFromContainer() {
  if (!containerRef.value) return;
  const rect = containerRef.value.getBoundingClientRect();
  isLandscape.value = rect.width > rect.height;
  // ensure split stays valid when dimensions change
  split.value = clamp(split.value, MIN_SPLIT, MAX_SPLIT);
}

function startDrag(ev: PointerEvent) {
  if (!containerRef.value) return;
  (ev.target as Element).setPointerCapture(ev.pointerId);
  dragging = true;
  window.addEventListener("pointermove", onPointerMove);
  window.addEventListener("pointerup", stopDrag);
  onPointerMove(ev);
}

function onPointerMove(ev: PointerEvent) {
  if (!dragging || !containerRef.value) return;
  const rect = containerRef.value.getBoundingClientRect();
  let percent = DEFAULT_SPLIT;
  if (isLandscape.value) {
    const x = ev.clientX - rect.left;
    percent = (x / rect.width) * 100;
  } else {
    const y = ev.clientY - rect.top;
    percent = (y / rect.height) * 100;
  }
  split.value = clamp(percent, MIN_SPLIT, MAX_SPLIT);
}

function stopDrag(_: PointerEvent) {
  dragging = false;
  window.removeEventListener("pointermove", onPointerMove);
  window.removeEventListener("pointerup", stopDrag);
  saveSplit();
}

onMounted(async () => {
  // wait for DOM, then observe container size changes
  await nextTick();

  if (containerRef.value) {
    // set initial orientation based on actual available space
    updateOrientationFromContainer();

    ro = new ResizeObserver(() => {
      updateOrientationFromContainer();
    });
    ro.observe(containerRef.value);
  }
});

onBeforeUnmount(() => {
  if (ro) {
    ro.disconnect();
    ro = null;
  }
  window.removeEventListener("pointermove", onPointerMove);
  window.removeEventListener("pointerup", stopDrag);
});
</script>

<template>
  <div
    ref="containerRef"
    class="split-container"
    :class="{ landscape: isLandscape }"
    :style="{ ['--split']: split + '%' }"
  >
    <!-- top/left content -->
    <section class="panel">
      <slot name="primary" />
    </section>

    <!-- adjustable separator -->
    <div
      class="separator"
      role="separator"
      tabindex="0"
      @pointerdown.prevent="startDrag"
      :aria-orientation="isLandscape ? 'vertical' : 'horizontal'"
    ></div>

    <!-- bottom/right content -->
    <section class="panel">
      <slot name="secondary" />
    </section>
  </div>
</template>

<style scoped>
.split-container {
  --split: 50%;
  display: flex;
  width: 100%;
  flex: 1 1 0%;
  min-height: 0;
  flex-direction: column;
  overflow: hidden;
}

.split-container .panel {
  overflow: auto;
  padding: 12px;
  min-height: 0;
}

/* portrait: vertical split (top | separator | bottom) */
.split-container:not(.landscape) .panel:first-child {
  flex-basis: var(--split);
  flex-shrink: 0;
}
.split-container:not(.landscape) .panel:last-child {
  flex: 1 1 0%;
}

/* landscape: horizontal split (left | separator | right) */
.split-container.landscape {
  flex-direction: row;
}
.split-container.landscape .panel:first-child {
  flex-basis: var(--split);
  flex-shrink: 0;
}
.split-container.landscape .panel:last-child {
  flex: 1 1 0%;
}

/* separator styles */
.separator {
  background: var(--separator);
  flex-shrink: 0;
  transition: background 0.2s;
}
.split-container:not(.landscape) .separator {
  height: 8px;
  cursor: row-resize;
  width: 100%;
}
.split-container.landscape .separator {
  width: 8px;
  cursor: col-resize;
  height: 100%;
}

/* hover and active */
.separator:hover {
  background: var(--separator-hover);
}
.separator:active {
  background: var(--separator-active);
}
</style>
