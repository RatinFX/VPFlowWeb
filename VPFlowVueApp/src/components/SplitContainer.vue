<script setup lang="ts">
import { log } from "@/store";
import { onBeforeUnmount, onMounted, ref } from "vue";

// percentage (0-100)
const split = ref<number>(loadSplit());
const containerRef = ref<HTMLElement | null>(null);
const isLandscape = ref(window.innerWidth > window.innerHeight);
let dragging = false;

function loadSplit(): number {
  try {
    const raw = localStorage.getItem("vpflow.splitPercent");
    const value = raw ? parseFloat(raw) : 50;
    return isFinite(value) ? clamp(value, 5, 95) : 50;
  } catch {
    return 50;
  }
}

function saveSplit() {
  try {
    log("Save split.value:", split.value);
    localStorage.setItem("vpflow.splitPercent", String(split.value));
  } catch {}
}

function clamp(v: number, a = 0, b = 100) {
  return Math.min(b, Math.max(a, v));
}

function updateOrientation() {
  isLandscape.value = window.innerWidth > window.innerHeight;
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
  let percent = 50;
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

function handleResize() {
  updateOrientation();
}

onMounted(() => {
  window.addEventListener("resize", handleResize);
});

onBeforeUnmount(() => {
  window.removeEventListener("resize", handleResize);
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
/* Basic split container */
.split-container {
  --split: 65%;
  display: flex;
  width: 100%;
  height: calc(
    100vh - 4rem
  ); /* account for header/footer rough heights; adjust as needed */
  flex-direction: column;
  align-items: stretch;
  overflow: hidden;
}

/* panels use flex-basis based on CSS variable --split */
.split-container .panel {
  overflow: auto;
  background: transparent;
  padding: 12px;
}

/* portrait: vertical split (top / separator / bottom) */
.split-container:not(.landscape) .panel:first-child {
  flex-basis: var(--split);
  flex-shrink: 0;
}
.split-container:not(.landscape) .panel:last-child {
  flex: 1 1 0%;
}

/* landscape: horizontal split (left / separator / right) */
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
