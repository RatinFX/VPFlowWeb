<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";

interface Point {
  id: string;
  x: number;
  y: number;
  handleIn?: { x: number; y: number };
  handleOut?: { x: number; y: number };
}

const container = ref<HTMLDivElement | null>(null);
const transform = ref({ x: 0, y: 0, scale: 1 });
const isDragging = ref(false);
const dragStart = ref({ x: 0, y: 0 });
const points = ref<Point[]>([
  { id: "start", x: 0, y: 1, handleOut: { x: 0.7, y: 0.27 } },
  { id: "end", x: 1, y: 0, handleIn: { x: 0.5, y: 1 } },
]);
const selectedPoint = ref<Point | null>(null);

// Pan handling
function startDrag(e: MouseEvent) {
  if (e.button === 1 || (e.button === 0 && e.altKey)) {
    // Middle click or Alt+Left click
    isDragging.value = true;
    dragStart.value = {
      x: e.clientX - transform.value.x,
      y: e.clientY - transform.value.y,
    };
  }
}

function onDrag(e: MouseEvent) {
  if (!isDragging.value) return;

  transform.value.x = e.clientX - dragStart.value.x;
  transform.value.y = e.clientY - dragStart.value.y;
}

function stopDrag() {
  isDragging.value = false;
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

onMounted(() => {
  window.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
  window.removeEventListener("keydown", handleKeydown);
});
</script>

<template>
  <div
    ref="container"
    class="relative w-full h-full overflow-hidden cursor-grab"
    :class="{ 'cursor-grabbing': isDragging }"
    @mousedown="startDrag"
    @mousemove="onDrag"
    @mouseup="stopDrag"
    @mouseleave="stopDrag"
    @wheel.prevent="handleWheel"
  >
    <div
      class="absolute w-96 h-96 bg-muted/20 border border-muted"
      :style="{
        transform: `translate(${transform.x}px, ${transform.y}px) scale(${transform.scale})`,
      }"
    >
      <!-- Grid and curve will go here -->
      <div class="absolute inset-0">
        <svg width="100%" height="100%" class="stroke-foreground">
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
            r="4"
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
