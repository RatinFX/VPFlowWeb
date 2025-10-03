<script setup lang="ts">
import { log } from "@/store";
import { onBeforeUnmount, onMounted, ref, nextTick } from "vue";

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
    return isFinite(value) ? clamp(value, 5, 95) : DEFAULT_SPLIT;
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
  split.value = clamp(split.value, 5, 95);
}

function startDrag(ev: PointerEvent) {
  if (!containerRef.value) return;
  log("startDrag()");
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
  split.value = clamp(percent, 5, 95);
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
/* basic split container */
.split-container {
  --split: 65%;
  display: flex;
  width: 100%;
  /* fill available flex space of parent */
  flex: 1 1 0%;
  min-height: 0;
  flex-direction: column;
  align-items: stretch;
  overflow: hidden;
}

/* Panels use flex-basis based on CSS variable --split */
.split-container .panel {
  overflow: auto;
  background: transparent;
  padding: 12px;
  /* allow panel contents to shrink inside the flex layout */
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
  background: rgba(0, 0, 0, 0.08);
  flex-shrink: 0;
  transition: background 0.15s;
  display: block;
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
  background: rgba(0, 0, 0, 0.15);
}
.separator:active {
  background: rgba(0, 0, 0, 0.25);
}
</style>
