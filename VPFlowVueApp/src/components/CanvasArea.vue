<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";

interface Point {
  id: string;
  x: number;
  y: number;
  handleIn?: { x: number; y: number };
  handleOut?: { x: number; y: number };
}

// Constants
const POINT_RADIUS = 6;
const CANVAS_PADDING = 20; // Padding around the curve area

const container = ref<HTMLDivElement | null>(null);
const transform = ref({ x: 0, y: 0, scale: 1 });
const isDragging = ref(false);
const dragStart = ref({ x: 0, y: 0 });
const points = ref<Point[]>([
  { id: "start", x: 0, y: 1, handleOut: { x: 0.7, y: 0.27 } },
  { id: "end", x: 1, y: 0, handleIn: { x: 0.5, y: 1 } },
]);
const selectedPoint = ref<Point | null>(null);

// Computed values for the viewBox to ensure the curve area is fully visible
const viewBox = computed(() => {
  const padding = POINT_RADIUS + CANVAS_PADDING;
  return `-${padding} -${padding} ${100 + padding * 2} ${100 + padding * 2}`;
});

// Pan handling
function startDrag(e: MouseEvent) {
  if (e.button === 1 || (e.button === 0 && e.altKey)) {
    // Middle click or Alt+Left click
    isDragging.value = true;
    dragStart.value = {
      x: e.clientX - transform.value.x,
      y: e.clientY - transform.value.y,
    };
    // Add global mouse event listeners
    window.addEventListener("mousemove", onDrag);
    window.addEventListener("mouseup", stopDrag);
  }
}

function onDrag(e: MouseEvent) {
  if (!isDragging.value) return;

  transform.value.x = e.clientX - dragStart.value.x;
  transform.value.y = e.clientY - dragStart.value.y;
}

function stopDrag() {
  if (!isDragging.value) return;

  isDragging.value = false;
  // Remove global mouse event listeners
  window.removeEventListener("mousemove", onDrag);
  window.removeEventListener("mouseup", stopDrag);
}

// Zoom handling
function handleWheel(e: WheelEvent) {
  e.preventDefault();

  const delta = -e.deltaY;
  const scaleChange = delta > 0 ? 1.1 : 0.9;
  const newScale = transform.value.scale * scaleChange;

  // Limit zoom range
  if (newScale < 0.1 || newScale > 10) return;

  // Zoom towards cursor position
  const rect = container.value?.getBoundingClientRect();
  if (!rect) return;

  const mouseX = e.clientX - rect.left;
  const mouseY = e.clientY - rect.top;

  transform.value = {
    scale: newScale,
    x: mouseX - (mouseX - transform.value.x) * scaleChange,
    y: mouseY - (mouseY - transform.value.y) * scaleChange,
  };
}

function resetView() {
  transform.value = { x: 0, y: 0, scale: 1 };
}

// Keyboard shortcuts
function handleKeydown(e: KeyboardEvent) {
  if (e.key === "r") resetView();
}

// Cleanup
function cleanup() {
  window.removeEventListener("keydown", handleKeydown);
  window.removeEventListener("mousemove", onDrag);
  window.removeEventListener("mouseup", stopDrag);
}

onMounted(() => {
  window.addEventListener("keydown", handleKeydown);
});

onUnmounted(cleanup);
</script>

<template>
  <div
    ref="container"
    class="relative w-full h-full overflow-hidden select-none"
    :class="{ 'cursor-grabbing': isDragging, 'cursor-grab': !isDragging }"
    @mousedown="startDrag"
    @wheel.prevent="handleWheel"
  >
    <div
      class="absolute w-[400px] h-[400px] bg-muted/20"
      :style="{
        transform: `translate(${transform.x}px, ${transform.y}px) scale(${transform.scale})`,
        transformOrigin: '0 0',
      }"
    >
      <!-- Grid and curve will go here -->
      <div class="absolute inset-0">
        <svg
          width="100%"
          height="100%"
          class="stroke-foreground"
          :viewBox="viewBox"
          preserveAspectRatio="xMidYMid meet"
        >
          <!-- Square boundary -->
          <rect
            x="0"
            y="0"
            width="100"
            height="100"
            fill="none"
            stroke="red"
            stroke-width="1"
          />

          <!-- Base line -->
          <path
            :d="`M ${points[0].x * 100} ${points[0].y * 100} L ${
              points[1].x * 100
            } ${points[1].y * 100}`"
            fill="none"
            stroke-width="1"
            stroke-dasharray="4 4"
          />

          <!-- Points -->
          <circle
            v-for="point in points"
            :key="point.id"
            :cx="point.x * 100"
            :cy="point.y * 100"
            :r="POINT_RADIUS"
            class="fill-background stroke-foreground stroke-2"
          />
        </svg>
      </div>
    </div>

    <!-- Reset button -->
    <button
      @click="resetView"
      class="absolute top-2 right-2 px-2 py-1 bg-muted/20 hover:bg-muted/40 rounded"
    >
      Reset View (R)
    </button>
  </div>
</template>
