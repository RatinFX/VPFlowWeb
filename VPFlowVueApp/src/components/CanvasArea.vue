<script setup lang="ts">
import { log } from "@/lib/logging";
import { computed, onMounted, onUnmounted, ref } from "vue";

interface Point {
  id: string | "start" | "end";
  x: number;
  y: number;
  handleIn?: { x: number; y: number };
  handleOut?: { x: number; y: number };
}

// Constants
const POINT_RADIUS = 2;
const HANDLE_RADIUS = 3;
const CURVE_SIZE = 100; // The curve coordinate system (0-100)
const CANVAS_PADDING = 150; // Extra space around curve for handles to overshoot
const CANVAS_SIZE = CURVE_SIZE + CANVAS_PADDING * 2; // Total canvas size

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
const resizeObserver = ref<ResizeObserver | null>(null);
const isDefaultView = ref(true);
const lastContainerSize = ref({ width: 0, height: 0 });

// ViewBox covers the entire canvas with padding for overshooting handles
const viewBox = computed(() => {
  return `${-CANVAS_PADDING} ${-CANVAS_PADDING} ${CANVAS_SIZE} ${CANVAS_SIZE}`;
});

// Generate bezier curve path
const curvePath = computed(() => {
  if (points.value.length < 2) return "";

  const p = points.value;
  let path = `M ${p[0].x * CURVE_SIZE} ${p[0].y * CURVE_SIZE}`;

  // Generate cubic bezier curves for each segment
  for (let i = 0; i < p.length - 1; i++) {
    const current = p[i];
    const next = p[i + 1];

    const c1x = (current.handleOut?.x ?? current.x) * CURVE_SIZE;
    const c1y = (current.handleOut?.y ?? current.y) * CURVE_SIZE;
    const c2x = (next.handleIn?.x ?? next.x) * CURVE_SIZE;
    const c2y = (next.handleIn?.y ?? next.y) * CURVE_SIZE;
    const endX = next.x * CURVE_SIZE;
    const endY = next.y * CURVE_SIZE;

    path += ` C ${c1x} ${c1y}, ${c2x} ${c2y}, ${endX} ${endY}`;
  }

  return path;
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
  const y1 = (1 - (p1.handleOut?.y ?? p1.y)).toFixed(2); // Invert Y for display
  const x2 = (p2.handleIn?.x ?? p2.x).toFixed(2);
  const y2 = (1 - (p2.handleIn?.y ?? p2.y)).toFixed(2); // Invert Y for display

  return `${x1}, ${y1}, ${x2}, ${y2}`;
});

// Convert screen coordinates to SVG coordinates
function screenToSVG(screenX: number, screenY: number) {
  if (!svgRef.value) return { x: 0, y: 0 };

  const pt = svgRef.value.createSVGPoint();
  pt.x = screenX;
  pt.y = screenY;

  const svgP = pt.matrixTransform(svgRef.value.getScreenCTM()?.inverse());
  return { x: svgP.x / CURVE_SIZE, y: svgP.y / CURVE_SIZE };
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
    isDefaultView.value = false; // User is manually adjusting view
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
  const newX = clamp(svgCoords.x);
  const newY = svgCoords.y;

  // Calculate offset
  const deltaX = newX - selectedPoint.value.x;
  const deltaY = newY - selectedPoint.value.y;

  // Move handles with the point
  if (selectedPoint.value.handleIn) {
    selectedPoint.value.handleIn.x += deltaX;
    selectedPoint.value.handleIn.y += deltaY;
  }
  if (selectedPoint.value.handleOut) {
    selectedPoint.value.handleOut.x += deltaX;
    selectedPoint.value.handleOut.y += deltaY;
  }

  // Update point position
  selectedPoint.value.x = newX;
  selectedPoint.value.y = newY;
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

// Add point on canvas click with Ctrl
function addPointOnCanvas(e: MouseEvent) {
  if (
    e.button !== 0 ||
    !e.ctrlKey ||
    e.altKey ||
    isDraggingPoint.value ||
    isDraggingHandle.value
  )
    return;

  e.stopPropagation();

  const svgCoords = screenToSVG(e.clientX, e.clientY);
  const x = clamp(svgCoords.x);
  const y = svgCoords.y;

  // Find insertion position based on x coordinate
  let insertIdx = points.value.findIndex((p) => p.x > x);
  if (insertIdx === -1) insertIdx = points.value.length;

  // Calculate appropriate handle positions to maintain curve smoothness
  const prevPoint = points.value[insertIdx - 1];
  const nextPoint = points.value[insertIdx];

  const handleOffset = 0.1;

  // Create new point with handles that blend into the curve
  const newPoint: Point = {
    id: `point_${Date.now()}`,
    x,
    y,
    handleIn: prevPoint
      ? { x: Math.max(prevPoint.x, x - handleOffset), y }
      : { x: x - handleOffset, y },
    handleOut: nextPoint
      ? { x: Math.min(nextPoint.x, x + handleOffset), y }
      : { x: x + handleOffset, y },
  };

  points.value.splice(insertIdx, 0, newPoint);
  selectedPoint.value = newPoint;
}

// Context menu
function showContextMenu(e: MouseEvent, point: Point) {
  e.preventDefault();
  e.stopPropagation();

  selectedPoint.value = point;

  // Position menu at cursor with offset to prevent immediate re-trigger
  const offset = 2;
  contextMenuPos.value = {
    x: e.clientX + offset,
    y: e.clientY - 40 + offset,
  };
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
    // Select previous point after deletion
    selectedPoint.value = points.value[Math.max(0, idx - 1)] ?? null;
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
  if (
    newScale < 0.1
    // || newScale > 10
  )
    return;

  isDefaultView.value = false; // User is manually adjusting view

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

  // Calculate scale to fit the CURVE (not canvas) nicely in view
  const scale =
    Math.min(containerWidth / CURVE_SIZE, containerHeight / CURVE_SIZE) * 0.8; // 0.8 for comfortable padding

  // Center the canvas (which is larger than the curve due to padding)
  const x = (containerWidth - CANVAS_SIZE * scale) / 2;
  const y = (containerHeight - CANVAS_SIZE * scale) / 2;

  transform.value = { x, y, scale };
  isDefaultView.value = true; // Mark as default view
}

// Handle container resize
function handleResize(entries: ResizeObserverEntry[]) {
  if (!entries.length || !container.value) return;

  const rect = container.value.getBoundingClientRect();
  const newWidth = rect.width;
  const newHeight = rect.height;

  lastContainerSize.value = { width: newWidth, height: newHeight };

  // If in default view, re-center and scale appropriately
  if (isDefaultView.value) {
    resetView();
  }
}

// Keyboard shortcuts
function handleKeydown(e: KeyboardEvent) {
  if (e.key === "r" || e.key === "R") {
    resetView();
  } else if ((e.key === "Delete" || e.key === "d") && selectedPoint.value) {
    deleteSelectedPoint();
  } else if ((e.ctrlKey || e.metaKey) && e.key === "e") {
    e.preventDefault();
    exportCurveData();
  }
}

// Export curve data
function exportCurveData() {
  const data = {
    points: points.value.map((p) => ({
      id: p.id,
      x: p.x,
      y: 1 - p.y, // Invert Y for export
      handleIn: p.handleIn
        ? { x: p.handleIn.x, y: 1 - p.handleIn.y }
        : undefined,
      handleOut: p.handleOut
        ? { x: p.handleOut.x, y: 1 - p.handleOut.y }
        : undefined,
    })),
    handles: [] as { x1: number; y1: number; x2: number; y2: number }[],
  };

  // Generate handle pairs for each segment
  for (let i = 0; i < points.value.length - 1; i++) {
    const p1 = points.value[i];
    const p2 = points.value[i + 1];

    data.handles.push({
      x1: p1.handleOut?.x ?? p1.x,
      y1: 1 - (p1.handleOut?.y ?? p1.y), // Invert Y
      x2: p2.handleIn?.x ?? p2.x,
      y2: 1 - (p2.handleIn?.y ?? p2.y), // Invert Y
    });
  }

  console.log("Curve data exported:", data);
  return data;
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
  // Select first point by default
  selectedPoint.value = points.value[0] ?? null;
  // Center view on mount
  setTimeout(() => resetView(), 10);

  // Setup resize observer
  if (container.value) {
    resizeObserver.value = new ResizeObserver(handleResize);
    resizeObserver.value.observe(container.value);
  }
});

onUnmounted(() => {
  cleanup();
  if (resizeObserver.value) {
    resizeObserver.value.disconnect();
  }
});

// Emit handle values to parent
defineExpose({
  handleDisplayText,
  points,
  exportCurveData,
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
      :class="`absolute w-[${CANVAS_SIZE}px] h-[${CANVAS_SIZE}px] bg-muted`"
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
          :class="{ 'cursor-crosshair': false }"
          :viewBox="viewBox"
          preserveAspectRatio="xMidYMid meet"
          @click="addPointOnCanvas"
        >
          <!-- Curve boundary (0-100 square) -->
          <rect
            x="0"
            y="0"
            :width="CURVE_SIZE"
            :height="CURVE_SIZE"
            fill="none"
            stroke="currentColor"
            stroke-width="1"
          />

          <!-- Grid lines -->
          <g opacity="0.2">
            <!-- Vertical lines -->
            <line
              v-for="i in 9"
              :key="`v${i}`"
              :x1="i * (CURVE_SIZE / 10)"
              y1="0"
              :x2="i * (CURVE_SIZE / 10)"
              :y2="CURVE_SIZE"
              stroke="currentColor"
              stroke-width="0.5"
            />
            <!-- Horizontal lines -->
            <line
              v-for="i in 9"
              :key="`h${i}`"
              x1="0"
              :y1="i * (CURVE_SIZE / 10)"
              :x2="CURVE_SIZE"
              :y2="i * (CURVE_SIZE / 10)"
              stroke="currentColor"
              stroke-width="0.5"
            />
          </g>

          <!-- Center lines (stronger) -->
          <g opacity="0.3">
            <line
              :x1="CURVE_SIZE / 2"
              y1="0"
              :x2="CURVE_SIZE / 2"
              :y2="CURVE_SIZE"
              stroke="currentColor"
              stroke-width="1"
            />
            <line
              x1="0"
              :y1="CURVE_SIZE / 2"
              :x2="CURVE_SIZE"
              :y2="CURVE_SIZE / 2"
              stroke="currentColor"
              stroke-width="1"
            />
          </g>

          <!-- Bezier curve -->
          <path
            :d="curvePath"
            fill="none"
            stroke="currentColor"
            stroke-width="1.5"
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
                  :x1="point.x * CURVE_SIZE"
                  :y1="point.y * CURVE_SIZE"
                  :x2="point.handleOut.x * CURVE_SIZE"
                  :y2="point.handleOut.y * CURVE_SIZE"
                  stroke="currentColor"
                  stroke-width="0.5"
                  opacity="0.5"
                />

                <!-- Handle In line -->
                <line
                  v-if="point.handleIn"
                  :x1="point.x * CURVE_SIZE"
                  :y1="point.y * CURVE_SIZE"
                  :x2="point.handleIn.x * CURVE_SIZE"
                  :y2="point.handleIn.y * CURVE_SIZE"
                  stroke="currentColor"
                  stroke-width="0.5"
                  opacity="0.5"
                />

                <!-- Handle Out point -->
                <circle
                  v-if="point.handleOut"
                  :cx="point.handleOut.x * CURVE_SIZE"
                  :cy="point.handleOut.y * CURVE_SIZE"
                  :r="HANDLE_RADIUS"
                  class="fill-blue-500 stroke-foreground stroke-1 cursor-move"
                  @mousedown="startHandleDrag($event, point, 'out')"
                />

                <!-- Handle In point -->
                <circle
                  v-if="point.handleIn"
                  :cx="point.handleIn.x * CURVE_SIZE"
                  :cy="point.handleIn.y * CURVE_SIZE"
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
            :cx="point.x * CURVE_SIZE"
            :cy="point.y * CURVE_SIZE"
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

    <!-- Reset button and shortcuts info -->
    <div class="absolute top-2 left-2 flex flex-col gap-1 items-start">
      <button
        @click="resetView"
        class="px-2 py-1 bg-muted/20 hover:bg-muted/40 rounded text-sm"
      >
        Reset View (R)
      </button>
      <div class="text-xs text-muted-foreground bg-muted/10 px-2 py-1 rounded">
        Ctrl+Click to add point
      </div>
      <div class="text-xs text-muted-foreground bg-muted/10 px-2 py-1 rounded">
        Ctrl+E to export
      </div>
    </div>

    <!-- Context menu -->
    <div
      v-if="contextMenuVisible"
      class="absolute bg-background border border-border rounded shadow-lg z-50"
      :style="{ left: `${contextMenuPos.x}px`, top: `${contextMenuPos.y}px` }"
    >
      <button
        v-if="selectedPoint?.id !== 'start' && selectedPoint?.id !== 'end'"
        @click="deleteSelectedPoint"
        class="w-full px-4 py-2 text-left hover:bg-muted text-sm"
      >
        Delete
      </button>
      <div v-else class="px-4 py-2 text-sm text-muted-foreground">
        cannot edit
      </div>
    </div>
  </div>
</template>
