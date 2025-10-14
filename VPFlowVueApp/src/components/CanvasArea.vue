<script setup lang="ts">
import { useCurvePoints } from "@/composables/useCurvePoints";
import { useKeyboardShortcuts } from "@/composables/useKeyboardShortcuts";
import { useLogging } from "@/composables/useLogging";
import type { Point, PresetCurve } from "@/models/PresetCurve";
import { computed, onMounted, onUnmounted, ref } from "vue";

// Type for coordinate points (used for control points and calculations)
type Coordinate = { x: number; y: number };

// Use composables
const { log } = useLogging();
const {
  points,
  getSelectedPoint,
  setSelectedPoint,
  loadPoints,
  yToDisplayValue,
} = useCurvePoints();

// Constants
const POINT_RADIUS = 4;
const POINT_COLOR = "#fdbf28";
const HANDLE_RADIUS = 4;
const HANDLE_LINE_WIDTH = 2;
const CURVE_PATH_WIDTH = 2;
const CURVE_SIZE = 100; // The curve coordinate system (0-100)
const CANVAS_PADDING = 1000; // Extra space around curve for handles to overshoot
const CANVAS_SIZE = CURVE_SIZE + CANVAS_PADDING * 2; // Total canvas size
const GENERAL_LINE_WIDTH = 1;

const scaledSizes = computed(() => {
  const scale = transform.value.scale;
  return {
    pointRadius: POINT_RADIUS / scale,
    handleRadius: HANDLE_RADIUS / scale,
    handleLineWidth: HANDLE_LINE_WIDTH / scale,
    curvePathWidth: CURVE_PATH_WIDTH / scale,
    centerLineWidth: GENERAL_LINE_WIDTH / scale,
    horizontalLineWidth: GENERAL_LINE_WIDTH / scale,
    rectLineWidth: GENERAL_LINE_WIDTH / scale,
  };
});

const container = ref<HTMLDivElement | null>(null);
const svgRef = ref<SVGSVGElement | null>(null);
const transform = ref({ x: 0, y: 0, scale: 1 });
const isDragging = ref(false);
const isDraggingPoint = ref(false);
const isDraggingHandle = ref(false);
const dragStart = ref({ x: 0, y: 0 });

// Use composable's selected point
const selectedPoint = computed({
  get: () => getSelectedPoint(),
  set: (point: Point | null) => setSelectedPoint(point),
});

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
  let path = `M ${p[0]!.x * CURVE_SIZE} ${p[0]!.y * CURVE_SIZE}`;

  // Generate cubic bezier curves for each segment
  for (let i = 0; i < p.length - 1; i++) {
    const current = p[i]!;
    const next = p[i + 1]!;

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
    p1 = points.value[idx - 1]!;
    p2 = points.value[idx]!;
  } else {
    // Other points: show current and next
    p1 = points.value[idx]!;
    p2 = points.value[idx + 1]!;
  }

  const x1 = (p1.handleOut?.x ?? p1.x).toFixed(2);
  const y1 = yToDisplayValue(p1.handleOut?.y ?? p1.y).toFixed(2); // Invert Y for display
  const x2 = (p2.handleIn?.x ?? p2.x).toFixed(2);
  const y2 = yToDisplayValue(p2.handleIn?.y ?? p2.y).toFixed(2); // Invert Y for display

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
    log(`Selected locked point: ${point.id}`);
    return;
  }

  e.stopPropagation();
  selectedPoint.value = point;
  isDraggingPoint.value = true;
  log(`Started dragging point: ${point.id}`);

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
  if (isDraggingPoint.value && selectedPoint.value) {
    log(
      `Stopped dragging point: ${
        selectedPoint.value.id
      } at (${selectedPoint.value.x.toFixed(2)}, ${(
        1 - selectedPoint.value.y
      ).toFixed(2)})`
    );
  }
  isDraggingPoint.value = false;
  window.removeEventListener("mousemove", onPointDrag);
  window.removeEventListener("mouseup", stopPointDrag);
}

// Handle interaction
function startHandleDrag(e: MouseEvent, point: Point, type: "in" | "out") {
  if (e.button !== 0 || e.altKey) return;

  e.stopPropagation();

  // Select the appropriate point based on which handle is being dragged
  if (type === "out") {
    // Dragging handleOut affects the segment FROM this point, so select this point
    selectedPoint.value = point;
  } else {
    // Dragging handleIn affects the segment TO this point, so select the previous point
    const idx = points.value.findIndex((p) => p.id === point.id);
    if (idx > 0) {
      selectedPoint.value = points.value[idx - 1]!;
    } else {
      selectedPoint.value = point; // Fallback if it's the first point
    }
  }

  selectedHandle.value = { pointId: point.id, type };
  isDraggingHandle.value = true;
  log(`Started dragging handle: ${point.id}.${type}`);

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

function stopHandleDrag(_: MouseEvent) {
  if (!isDraggingHandle.value) return;

  if (selectedHandle.value) {
    log(
      `Stopped dragging handle: ${selectedHandle.value.pointId}.${selectedHandle.value.type}`
    );
  }

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

// Start canvas drag - snap nearest handle to cursor if point selected
function startCanvasDrag(e: MouseEvent) {
  if (
    e.button !== 0 ||
    e.ctrlKey ||
    e.altKey ||
    !selectedPoint.value ||
    isDraggingPoint.value ||
    isDraggingHandle.value
  )
    return;

  e.stopPropagation();

  const svgCoords = screenToSVG(e.clientX, e.clientY);
  const clickX = svgCoords.x;
  const clickY = svgCoords.y;

  // Check if clicking near a point - if so, don't snap handle
  const POINT_CLICK_THRESHOLD = 0.03; // Distance threshold for clicking on a point
  const clickingOnPoint = points.value.some((p) => {
    const dx = p.x - clickX;
    const dy = p.y - clickY;
    const distance = Math.sqrt(dx * dx + dy * dy);
    return distance < POINT_CLICK_THRESHOLD;
  });

  if (clickingOnPoint) return; // Let the point's click handler deal with it

  // Find the currently displayed handles (selected point and its neighbor)
  const idx = points.value.findIndex((p) => p.id === selectedPoint.value?.id);
  if (idx === -1) return;

  const visibleHandles: Array<{
    point: Point;
    type: "in" | "out";
    distance: number;
  }> = [];

  // Check handles for selected point and its neighbor
  for (let i = 0; i < points.value.length; i++) {
    const point = points.value[i]!;

    // Only include handles that are currently visible (same logic as template)
    const isVisible =
      point.id === selectedPoint.value.id ||
      i === idx + 1 ||
      (selectedPoint.value.id === points.value[points.value.length - 1]?.id &&
        i === points.value.length - 2);

    if (!isVisible) continue;

    // Check handleOut
    if (point.handleOut) {
      const dx = point.handleOut.x - clickX;
      const dy = point.handleOut.y - clickY;
      const distance = Math.sqrt(dx * dx + dy * dy);
      visibleHandles.push({ point, type: "out", distance });
    }

    // Check handleIn
    if (point.handleIn) {
      const dx = point.handleIn.x - clickX;
      const dy = point.handleIn.y - clickY;
      const distance = Math.sqrt(dx * dx + dy * dy);
      visibleHandles.push({ point, type: "in", distance });
    }
  }

  if (visibleHandles.length === 0) return;

  // Find nearest handle
  visibleHandles.sort((a, b) => a.distance - b.distance);
  const nearest = visibleHandles[0]!;

  // Move the nearest handle to cursor position
  const clamped = clampHandle(clickX, clickY);

  if (nearest.type === "out") {
    if (!nearest.point.handleOut)
      nearest.point.handleOut = { x: nearest.point.x, y: nearest.point.y };
    nearest.point.handleOut.x = clamped.x;
    nearest.point.handleOut.y = clamped.y;
  } else {
    if (!nearest.point.handleIn)
      nearest.point.handleIn = { x: nearest.point.x, y: nearest.point.y };
    nearest.point.handleIn.x = clamped.x;
    nearest.point.handleIn.y = clamped.y;
  }

  // Start dragging this handle and select appropriate point
  selectedHandle.value = { pointId: nearest.point.id, type: nearest.type };

  // Select the appropriate point based on which handle is being dragged
  if (nearest.type === "out") {
    // Dragging handleOut affects the segment FROM this point, so select this point
    selectedPoint.value = nearest.point;
  } else {
    // Dragging handleIn affects the segment TO this point, so select the previous point
    const handlePointIdx = points.value.findIndex(
      (p) => p.id === nearest.point.id
    );
    if (handlePointIdx > 0) {
      selectedPoint.value = points.value[handlePointIdx - 1]!;
    } else {
      selectedPoint.value = nearest.point; // Fallback if it's the first point
    }
  }

  isDraggingHandle.value = true;

  // Add event listeners for dragging
  window.addEventListener("mousemove", onHandleDrag);
  window.addEventListener("mouseup", stopHandleDrag);
}

// Add point on canvas click with Ctrl OR move nearest handle on drag
/**
 * Calculate point on cubic Bezier curve at parameter t
 */
function evaluateBezierCurve(
  p0: Coordinate,
  p1: Coordinate,
  p2: Coordinate,
  p3: Coordinate,
  t: number
): Coordinate {
  const u = t;
  const v = 1 - t;
  const v2 = v * v;
  const v3 = v2 * v;
  const u2 = u * u;
  const u3 = u2 * u;

  return {
    x: v3 * p0.x + 3 * v2 * u * p1.x + 3 * v * u2 * p2.x + u3 * p3.x,
    y: v3 * p0.y + 3 * v2 * u * p1.y + 3 * v * u2 * p2.y + u3 * p3.y,
  };
}

/**
 * Find the closest curve segment to a given point
 */
function findClosestSegment(x: number, y: number): number {
  let closestSegmentIdx = 0;
  let minDistance = Infinity;

  for (let i = 0; i < points.value.length - 1; i++) {
    const p0 = points.value[i]!;
    const p3 = points.value[i + 1]!;
    const p1 = p0.handleOut || p0;
    const p2 = p3.handleIn || p3;

    // Sample the bezier curve and find minimum distance
    const samples = 20;
    for (let t = 0; t <= samples; t++) {
      const u = t / samples;
      const curvePoint = evaluateBezierCurve(p0, p1, p2, p3, u);

      const dx = curvePoint.x - x;
      const dy = curvePoint.y - y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance < minDistance) {
        minDistance = distance;
        closestSegmentIdx = i;
      }
    }
  }

  return closestSegmentIdx;
}

/**
 * Find the parameter t on a curve segment closest to a given point
 */
function findParameterOnSegment(
  p0: Coordinate,
  p1: Coordinate,
  p2: Coordinate,
  p3: Coordinate,
  x: number,
  y: number
): number {
  let bestT = 0.5;
  let minDist = Infinity;
  const samples = 20;

  for (let i = 0; i <= samples; i++) {
    const t = i / samples;
    const curvePoint = evaluateBezierCurve(p0, p1, p2, p3, t);

    const dx = curvePoint.x - x;
    const dy = curvePoint.y - y;
    const dist = dx * dx + dy * dy;

    if (dist < minDist) {
      minDist = dist;
      bestT = t;
    }
  }

  return bestT;
}

/**
 * Calculate the tangent (derivative) of a Bezier curve at parameter t
 * Formula: B'(t) = 3(1-t)²(P1-P0) + 6(1-t)t(P2-P1) + 3t²(P3-P2)
 */
function calculateBezierTangent(
  p0: Coordinate,
  p1: Coordinate,
  p2: Coordinate,
  p3: Coordinate,
  t: number
): Coordinate {
  const mt = 1 - t;
  const mt2 = mt * mt;
  const t2 = t * t;

  return {
    x:
      3 * mt2 * (p1.x - p0.x) +
      6 * mt * t * (p2.x - p1.x) +
      3 * t2 * (p3.x - p2.x),
    y:
      3 * mt2 * (p1.y - p0.y) +
      6 * mt * t * (p2.y - p1.y) +
      3 * t2 * (p3.y - p2.y),
  };
}

/**
 * Calculate handles for a new point based on curve tangent
 */
function calculateHandlesFromTangent(
  x: number,
  y: number,
  tangent: Coordinate,
  segmentLength: number
): { handleIn: Coordinate; handleOut: Coordinate } {
  // Normalize the tangent vector
  const tangentLength = Math.sqrt(
    tangent.x * tangent.x + tangent.y * tangent.y
  );
  const normTangentX = tangentLength > 0 ? tangent.x / tangentLength : 1;
  const normTangentY = tangentLength > 0 ? tangent.y / tangentLength : 0;

  // Calculate handle offset distance (proportional to segment length)
  const handleLength = Math.min(0.15, segmentLength * 0.3);

  return {
    handleIn: {
      x: x - normTangentX * handleLength,
      y: y - normTangentY * handleLength,
    },
    handleOut: {
      x: x + normTangentX * handleLength,
      y: y + normTangentY * handleLength,
    },
  };
}

/**
 * Check if a point is too close to existing points
 */
function isPointTooCloseToExisting(x: number, y: number): boolean {
  const MIN_DISTANCE = 0.05;
  return points.value.some((p) => {
    const dx = p.x - x;
    const dy = p.y - y;
    const distance = Math.sqrt(dx * dx + dy * dy);
    return distance < MIN_DISTANCE;
  });
}

// Add point on canvas click with Ctrl
function addPointOnCanvas(e: MouseEvent) {
  // Only respond to Ctrl+Click (no Alt, no dragging)
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

  // Prevent creating points at the boundaries
  const BOUNDARY_MARGIN = 0.02;
  if (x < BOUNDARY_MARGIN || x > 1 - BOUNDARY_MARGIN) return;

  // Prevent creating points too close to existing points
  if (isPointTooCloseToExisting(x, y)) return;

  // Find which curve segment this point should be inserted into
  const closestSegmentIdx = findClosestSegment(x, y);
  const insertIdx = closestSegmentIdx + 1;

  // Get the segment endpoints
  const prevPoint = points.value[closestSegmentIdx];
  const nextPoint = points.value[insertIdx];
  if (!prevPoint || !nextPoint) return;

  // Get the control points for the bezier segment
  const p0 = prevPoint;
  const p3 = nextPoint;
  const p1 = p0.handleOut || p0;
  const p2 = p3.handleIn || p3;

  // Find the exact position on the curve and calculate tangent
  const t = findParameterOnSegment(p0, p1, p2, p3, x, y);
  const tangent = calculateBezierTangent(p0, p1, p2, p3, t);

  // Calculate segment length for proportional handle sizing
  const segmentLength = Math.sqrt(
    Math.pow(p3.x - p0.x, 2) + Math.pow(p3.y - p0.y, 2)
  );

  // Create handles aligned with the curve's tangent
  const handles = calculateHandlesFromTangent(x, y, tangent, segmentLength);

  // Create and insert the new point
  const newPoint: Point = {
    id: `point_${Date.now()}`,
    x,
    y,
    handleIn: handles.handleIn,
    handleOut: handles.handleOut,
  };

  points.value.splice(insertIdx, 0, newPoint);
  selectedPoint.value = newPoint;
  log(
    `Added new point: ${newPoint.id} at (${x.toFixed(2)}, ${(1 - y).toFixed(
      2
    )}) in segment ${closestSegmentIdx}`
  );
}

// Context menu for points
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
    const deletedId = selectedPoint.value.id;
    points.value.splice(idx, 1);
    // Select previous point after deletion
    selectedPoint.value = points.value[Math.max(0, idx - 1)] ?? null;
    log(`Deleted point: ${deletedId}`);
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

  log(
    `Reset view to scale: ${scale.toFixed(2)} ` +
      `from ${transform.value.scale.toFixed(2)}`
  );
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

// Setup keyboard shortcuts using composable
useKeyboardShortcuts({
  selectedPoint,
  points,
  handlers: {
    onResetView: () => resetView(),
    onDeletePoint: () => deleteSelectedPoint(),
    onExportData: () => exportCurveData(),
    onNavigatePrevious: () => {
      const currentIdx = points.value.findIndex(
        (p) => p.id === selectedPoint.value?.id
      );
      if (currentIdx > 0) {
        selectedPoint.value = points.value[currentIdx - 1]!;
        log(`Selected previous point: ${selectedPoint.value.id}`);
      }
    },
    onNavigateNext: () => {
      const currentIdx = points.value.findIndex(
        (p) => p.id === selectedPoint.value?.id
      );
      if (currentIdx < points.value.length - 1) {
        selectedPoint.value = points.value[currentIdx + 1]!;
        log(`Selected next point: ${selectedPoint.value.id}`);
      }
    },
    onMovePoint: (deltaX: number, deltaY: number, _step: number) => {
      if (!selectedPoint.value) return;

      // Update point position
      const newX = clamp(selectedPoint.value.x + deltaX);
      const actualDeltaX = newX - selectedPoint.value.x;
      const newY = selectedPoint.value.y + deltaY;

      // Move handles with the point
      if (selectedPoint.value.handleIn) {
        selectedPoint.value.handleIn.x += actualDeltaX;
        selectedPoint.value.handleIn.y += deltaY;
      }
      if (selectedPoint.value.handleOut) {
        selectedPoint.value.handleOut.x += actualDeltaX;
        selectedPoint.value.handleOut.y += deltaY;
      }

      // Update point position
      selectedPoint.value.x = newX;
      selectedPoint.value.y = newY;
    },
  },
});

// Load a preset curve
function loadPreset(preset: PresetCurve) {
  // Use the composable's loadPoints method
  loadPoints(preset.points);

  log(`Loaded preset curve: ${preset.name}`);
}

// Export curve data
function exportCurveData() {
  const data = {
    points: points.value.map((p) => ({
      id: p.id,
      x: p.x,
      y: yToDisplayValue(p.y), // Invert Y for export
      handleIn: p.handleIn
        ? { x: p.handleIn.x, y: yToDisplayValue(p.handleIn.y) }
        : undefined,
      handleOut: p.handleOut
        ? { x: p.handleOut.x, y: yToDisplayValue(p.handleOut.y) }
        : undefined,
    })),
    handles: [] as { x1: number; y1: number; x2: number; y2: number }[],
  };

  // Generate handle pairs for each segment
  for (let i = 0; i < points.value.length - 1; i++) {
    const p1 = points.value[i]!;
    const p2 = points.value[i + 1]!;

    data.handles.push({
      x1: p1.handleOut?.x ?? p1.x,
      y1: yToDisplayValue(p1.handleOut?.y ?? p1.y), // Invert Y
      x2: p2.handleIn?.x ?? p2.x,
      y2: yToDisplayValue(p2.handleIn?.y ?? p2.y), // Invert Y
    });
  }

  console.log("Curve data exported:", data);
  log(
    `Exported curve with ${points.value.length} points and ${data.handles.length} segments`,
    data
  );
  return data;
}

// Cleanup
function cleanup() {
  // Keyboard shortcuts cleanup is handled by useKeyboardShortcuts composable
  window.removeEventListener("mousemove", onDrag);
  window.removeEventListener("mouseup", stopDrag);
  window.removeEventListener("mousemove", onPointDrag);
  window.removeEventListener("mouseup", stopPointDrag);
  window.removeEventListener("mousemove", onHandleDrag);
  window.removeEventListener("mouseup", stopHandleDrag);
}

onMounted(() => {
  // Keyboard shortcuts setup is handled by useKeyboardShortcuts composable
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
  loadPreset,
});
</script>

<template>
  <div
    ref="container"
    class="relative bg-muted min-h-10 w-full h-full overflow-hidden select-none"
    :class="{ 'cursor-grabbing': isDragging, 'cursor-default': !isDragging }"
    @mousedown="startDrag"
    @wheel.prevent="handleWheel"
  >
    <div
      class="absolute"
      :style="{
        width: `${CANVAS_SIZE}px`,
        height: `${CANVAS_SIZE}px`,
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
          @click.ctrl="addPointOnCanvas"
          @mousedown="startCanvasDrag"
        >
          <!-- Curve boundary (0-100 square) -->
          <rect
            x="0"
            y="0"
            :width="CURVE_SIZE"
            :height="CURVE_SIZE"
            fill="none"
            stroke="currentColor"
            :stroke-width="scaledSizes.rectLineWidth"
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
              :stroke-width="scaledSizes.horizontalLineWidth"
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
              :stroke-width="scaledSizes.horizontalLineWidth"
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
              :stroke-width="scaledSizes.centerLineWidth"
            />
            <line
              x1="0"
              :y1="CURVE_SIZE / 2"
              :x2="CURVE_SIZE"
              :y2="CURVE_SIZE / 2"
              stroke="currentColor"
              :stroke-width="scaledSizes.centerLineWidth"
            />
          </g>

          <!-- Bezier curve -->
          <path
            :d="curvePath"
            fill="none"
            stroke="currentColor"
            :stroke-width="scaledSizes.curvePathWidth"
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
                  :stroke-width="scaledSizes.handleLineWidth"
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
                  :stroke-width="scaledSizes.handleLineWidth"
                  opacity="0.5"
                />

                <!-- Handle Out point -->
                <circle
                  v-if="point.handleOut"
                  :cx="point.handleOut.x * CURVE_SIZE"
                  :cy="point.handleOut.y * CURVE_SIZE"
                  :r="scaledSizes.handleRadius"
                  stroke="none"
                  :class="[
                    'fill-blue-500',
                    isDraggingHandle ? 'cursor-grabbing' : 'cursor-grab',
                  ]"
                  @mousedown="startHandleDrag($event, point, 'out')"
                />

                <!-- Handle In point -->
                <circle
                  v-if="point.handleIn"
                  :cx="point.handleIn.x * CURVE_SIZE"
                  :cy="point.handleIn.y * CURVE_SIZE"
                  :r="scaledSizes.handleRadius"
                  stroke="none"
                  :class="[
                    'fill-green-500',
                    isDraggingHandle ? 'cursor-grabbing' : 'cursor-grab',
                  ]"
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
            :r="scaledSizes.pointRadius"
            stroke="none"
            :fill="
              selectedPoint?.id === point.id ? POINT_COLOR : 'var(--primary)'
            "
            :class="[
              // 'stroke-foreground stroke-2',
              point.id === 'start' || point.id === 'end'
                ? 'cursor-default'
                : isDraggingPoint
                ? 'cursor-grabbing'
                : 'cursor-grab',
            ]"
            @mousedown="startPointDrag($event, point)"
            @click="selectPoint($event, point)"
            @contextmenu="showContextMenu($event, point)"
          />
        </svg>
      </div>
    </div>

    <!-- Context menu
     TODO: replace context menu with:
     - https://www.shadcn-vue.com/docs/components/context-menu
     -->
    <div
      v-if="contextMenuVisible"
      class="absolute bg-background border border-border rounded shadow-lg z-10"
      :style="{ left: `${contextMenuPos.x}px`, top: `${contextMenuPos.y}px` }"
    >
      <button
        v-if="selectedPoint?.id !== 'start' && selectedPoint?.id !== 'end'"
        @click="deleteSelectedPoint"
        class="w-full px-4 py-2 text-left hover:bg-muted text-sm"
      >
        Delete [x]
      </button>
      <div v-else class="px-4 py-2 text-sm text-muted-foreground">
        cannot edit
      </div>
    </div>
  </div>
</template>
