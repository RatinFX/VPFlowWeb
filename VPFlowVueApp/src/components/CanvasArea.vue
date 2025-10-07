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
const HANDLE_RADIUS = 4;
const CANVAS_PADDING = 20; // Padding around the curve area
const CANVAS_SIZE = 400;

const container = ref<HTMLDivElement | null>(null);
const svgRef = ref<SVGSVGElement | null>(null);
const transform = ref({ x: 0, y: 0, scale: 1 });
const isDragging = ref(false);
const isDraggingPoint = ref(false);
const isDraggingHandle = ref(false);
const dragStart = ref({ x: 0, y: 0 });
const points = ref<Point[]>([
  { id: "start", x: 0, y: 1, handleOut: { x: 0.7, y: 0.27 } },
  { id: "end", x: 1, y: 0, handleIn: { x: 0.5, y: 1 } },
]);
const selectedPoint = ref<Point | null>(null);
const selectedHandle = ref<{ pointId: string; type: "in" | "out" } | null>(
  null
);
const contextMenuVisible = ref(false);
const contextMenuPos = ref({ x: 0, y: 0 });

// Computed values for the viewBox to ensure the curve area is fully visible
const viewBox = computed(() => {
  const padding = POINT_RADIUS + CANVAS_PADDING;
  return `-${padding} -${padding} ${100 + padding * 2} ${100 + padding * 2}`;
});

// Generate bezier curve path
const curvePath = computed(() => {
  if (points.value.length < 2) return "";

  const p = points.value;
  const startX = p[0].x * 100;
  const startY = p[0].y * 100;
  const endX = p[1].x * 100;
  const endY = p[1].y * 100;

  const c1x = (p[0].handleOut?.x ?? p[0].x) * 100;
  const c1y = (p[0].handleOut?.y ?? p[0].y) * 100;
  const c2x = (p[1].handleIn?.x ?? p[1].x) * 100;
  const c2y = (p[1].handleIn?.y ?? p[1].y) * 100;

  return `M ${startX} ${startY} C ${c1x} ${c1y}, ${c2x} ${c2y}, ${endX} ${endY}`;
});

// Format handle values for display
const handleDisplayText = computed(() => {
  if (!selectedPoint.value) return "0.00, 0.00, 0.00, 0.00";

  const idx = points.value.findIndex((p) => p.id === selectedPoint.value?.id);
  if (idx === -1) return "0.00, 0.00, 0.00, 0.00";

  let p1, p2;

  if (idx === points.value.length - 1) {
    // Last point: show previous and current
    p1 = points.value[idx - 1];
    p2 = points.value[idx];
  } else {
    // Other points: show current and next
    p1 = points.value[idx];
    p2 = points.value[idx + 1];
  }

  const x1 = (p1.handleOut?.x ?? p1.x).toFixed(2);
  const y1 = (p1.handleOut?.y ?? p1.y).toFixed(2);
  const x2 = (p2.handleIn?.x ?? p2.x).toFixed(2);
  const y2 = (p2.handleIn?.y ?? p2.y).toFixed(2);

  return `${x1}, ${y1}, ${x2}, ${y2}`;
});

// Convert screen coordinates to SVG coordinates
function screenToSVG(screenX: number, screenY: number) {
  if (!svgRef.value) return { x: 0, y: 0 };

  const pt = svgRef.value.createSVGPoint();
  pt.x = screenX;
  pt.y = screenY;

  const svgP = pt.matrixTransform(svgRef.value.getScreenCTM()?.inverse());
  return { x: svgP.x / 100, y: svgP.y / 100 };
}

// Clamp value between 0 and 1
function clamp(val: number) {
  return Math.max(0, Math.min(1, val));
}

// Clamp handle values - X constrained to 0-1, Y can overflow
function clampHandle(x: number, y: number) {
  return {
    x: Math.max(0, Math.min(1, x)),
    y: y, // Allow Y to go outside 0-1 range for overshoot
  };
}

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

// Point interaction
function startPointDrag(e: MouseEvent, point: Point) {
  if (e.button !== 0 || e.altKey) return;

  // Prevent moving default points
  if (point.id === "start" || point.id === "end") {
    selectedPoint.value = point;
    return;
  }

  e.stopPropagation();
  selectedPoint.value = point;
  isDraggingPoint.value = true;

  window.addEventListener("mousemove", onPointDrag);
  window.addEventListener("mouseup", stopPointDrag);
}

function onPointDrag(e: MouseEvent) {
  if (!isDraggingPoint.value || !selectedPoint.value) return;

  const svgCoords = screenToSVG(e.clientX, e.clientY);
  selectedPoint.value.x = clamp(svgCoords.x);
  selectedPoint.value.y = clamp(svgCoords.y);
}

function stopPointDrag() {
  isDraggingPoint.value = false;
  window.removeEventListener("mousemove", onPointDrag);
  window.removeEventListener("mouseup", stopPointDrag);
}

// Handle interaction
function startHandleDrag(e: MouseEvent, point: Point, type: "in" | "out") {
  if (e.button !== 0 || e.altKey) return;

  e.stopPropagation();
  selectedPoint.value = point;
  selectedHandle.value = { pointId: point.id, type };
  isDraggingHandle.value = true;

  window.addEventListener("mousemove", onHandleDrag);
  window.addEventListener("mouseup", stopHandleDrag);
}

function onHandleDrag(e: MouseEvent) {
  if (!isDraggingHandle.value || !selectedHandle.value) return;

  const point = points.value.find(
    (p) => p.id === selectedHandle.value?.pointId
  );
  if (!point) return;

  const svgCoords = screenToSVG(e.clientX, e.clientY);
  const clamped = clampHandle(svgCoords.x, svgCoords.y);

  if (selectedHandle.value.type === "out") {
    if (!point.handleOut) point.handleOut = { x: point.x, y: point.y };
    point.handleOut.x = clamped.x;
    point.handleOut.y = clamped.y;
  } else {
    if (!point.handleIn) point.handleIn = { x: point.x, y: point.y };
    point.handleIn.x = clamped.x;
    point.handleIn.y = clamped.y;
  }
}

function stopHandleDrag() {
  isDraggingHandle.value = false;
  selectedHandle.value = null;
  window.removeEventListener("mousemove", onHandleDrag);
  window.removeEventListener("mouseup", stopHandleDrag);
}

// Point selection
function selectPoint(e: MouseEvent, point: Point) {
  if (e.button !== 0 || e.altKey) return;
  selectedPoint.value = point;
}

// Context menu
function showContextMenu(e: MouseEvent, point: Point) {
  e.preventDefault();
  e.stopPropagation();

  selectedPoint.value = point;
  contextMenuPos.value = { x: e.clientX, y: e.clientY };
  contextMenuVisible.value = true;

  // Close on next click
  setTimeout(() => {
    window.addEventListener("click", closeContextMenu, { once: true });
  }, 0);
}

function closeContextMenu() {
  contextMenuVisible.value = false;
}

function deleteSelectedPoint() {
  if (!selectedPoint.value) return;

  // Prevent deletion of default points
  if (selectedPoint.value.id === "start" || selectedPoint.value.id === "end")
    return;

  const idx = points.value.findIndex((p) => p.id === selectedPoint.value?.id);
  if (idx !== -1) {
    points.value.splice(idx, 1);
    selectedPoint.value = null;
  }

  closeContextMenu();
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
  if (!container.value) {
    transform.value = { x: 0, y: 0, scale: 1 };
    return;
  }

  // Get container dimensions
  const rect = container.value.getBoundingClientRect();
  const containerWidth = rect.width;
  const containerHeight = rect.height;

  // Calculate scale to fit and center the canvas
  const scale =
    Math.min(containerWidth / CANVAS_SIZE, containerHeight / CANVAS_SIZE) * 0.9; // 0.9 for padding

  // Center the canvas
  const x = (containerWidth - CANVAS_SIZE * scale) / 2;
  const y = (containerHeight - CANVAS_SIZE * scale) / 2;

  transform.value = { x, y, scale };
}

// Keyboard shortcuts
function handleKeydown(e: KeyboardEvent) {
  if (e.key === "r" || e.key === "R") {
    resetView();
  } else if (e.key === "Delete" && selectedPoint.value) {
    deleteSelectedPoint();
  }
}

// Cleanup
function cleanup() {
  window.removeEventListener("keydown", handleKeydown);
  window.removeEventListener("mousemove", onDrag);
  window.removeEventListener("mouseup", stopDrag);
  window.removeEventListener("mousemove", onPointDrag);
  window.removeEventListener("mouseup", stopPointDrag);
  window.removeEventListener("mousemove", onHandleDrag);
  window.removeEventListener("mouseup", stopHandleDrag);
}

onMounted(() => {
  window.addEventListener("keydown", handleKeydown);
  // Center view on mount
  setTimeout(() => resetView(), 10);
});

onUnmounted(cleanup);

// Emit handle values to parent
defineExpose({
  handleDisplayText,
  points,
});
</script>

<template>
  <div
    ref="container"
    class="relative w-full h-full overflow-hidden select-none"
    :class="{ 'cursor-grabbing': isDragging, 'cursor-default': !isDragging }"
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
      <!-- Grid and curve -->
      <div class="absolute inset-0">
        <svg
          ref="svgRef"
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
            stroke="currentColor"
            stroke-width="1"
          />

          <!-- Bezier curve -->
          <path
            :d="curvePath"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
          />

          <!-- Handle lines and points for selected point and its neighbor -->
          <g v-if="selectedPoint">
            <template v-for="(point, idx) in points" :key="point.id">
              <!-- Show handles for current point and next point (or previous if last) -->
              <g
                v-if="
                  point.id === selectedPoint.id ||
                  idx ===
                    points.findIndex((p) => p.id === selectedPoint?.id) + 1 ||
                  (selectedPoint?.id === points[points.length - 1]?.id &&
                    idx === points.length - 2)
                "
              >
                <!-- Handle Out line -->
                <line
                  v-if="point.handleOut"
                  :x1="point.x * 100"
                  :y1="point.y * 100"
                  :x2="point.handleOut.x * 100"
                  :y2="point.handleOut.y * 100"
                  stroke="currentColor"
                  stroke-width="1"
                  stroke-dasharray="2 2"
                  opacity="0.5"
                />

                <!-- Handle In line -->
                <line
                  v-if="point.handleIn"
                  :x1="point.x * 100"
                  :y1="point.y * 100"
                  :x2="point.handleIn.x * 100"
                  :y2="point.handleIn.y * 100"
                  stroke="currentColor"
                  stroke-width="1"
                  stroke-dasharray="2 2"
                  opacity="0.5"
                />

                <!-- Handle Out point -->
                <circle
                  v-if="point.handleOut"
                  :cx="point.handleOut.x * 100"
                  :cy="point.handleOut.y * 100"
                  :r="HANDLE_RADIUS"
                  class="fill-blue-500 stroke-foreground stroke-1 cursor-move"
                  @mousedown="startHandleDrag($event, point, 'out')"
                />

                <!-- Handle In point -->
                <circle
                  v-if="point.handleIn"
                  :cx="point.handleIn.x * 100"
                  :cy="point.handleIn.y * 100"
                  :r="HANDLE_RADIUS"
                  class="fill-green-500 stroke-foreground stroke-1 cursor-move"
                  @mousedown="startHandleDrag($event, point, 'in')"
                />
              </g>
            </template>
          </g>

          <!-- Points -->
          <circle
            v-for="point in points"
            :key="point.id"
            :cx="point.x * 100"
            :cy="point.y * 100"
            :r="POINT_RADIUS"
            :class="[
              'stroke-foreground stroke-2',
              point.id === 'start' || point.id === 'end'
                ? 'cursor-pointer'
                : 'cursor-move',
              selectedPoint?.id === point.id
                ? 'fill-primary'
                : 'fill-background',
            ]"
            @mousedown="startPointDrag($event, point)"
            @click="selectPoint($event, point)"
            @contextmenu="showContextMenu($event, point)"
          />
        </svg>
      </div>
    </div>

    <!-- Reset button -->
    <button
      @click="resetView"
      class="absolute top-2 right-2 px-2 py-1 bg-muted/20 hover:bg-muted/40 rounded text-sm"
    >
      Reset View (R)
    </button>

    <!-- Context menu -->
    <div
      v-if="contextMenuVisible"
      class="absolute bg-background border border-border rounded shadow-lg py-1 z-50"
      :style="{ left: `${contextMenuPos.x}px`, top: `${contextMenuPos.y}px` }"
    >
      <button
        v-if="selectedPoint?.id !== 'start' && selectedPoint?.id !== 'end'"
        @click="deleteSelectedPoint"
        class="w-full px-4 py-2 text-left hover:bg-muted text-sm"
      >
        Delete Point
      </button>
      <div v-else class="px-4 py-2 text-sm text-muted-foreground">
        Cannot delete default point
      </div>
    </div>
  </div>
</template>
