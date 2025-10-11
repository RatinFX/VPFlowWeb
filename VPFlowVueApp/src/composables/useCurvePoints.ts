import { ref } from "vue";
import type { Point } from "@/models/PresetCurve";

// Shared state - will be the same instance across all components that use this composable
const points = ref<Point[]>([
  {
    id: "start",
    x: 0,
    y: 1,
    handleOut: { x: 0.72, y: 0.82 }, // SVG coords (0=top, 1=bottom)
  },
  {
    id: "end",
    x: 1,
    y: 0,
    handleIn: { x: 0.28, y: 0.18 },
  },
]);

const selectedPointId = ref<string | null>(null);

export function useCurvePoints() {
  // Get the currently selected point
  const getSelectedPoint = () => {
    if (!selectedPointId.value) return null;
    return points.value.find((p) => p.id === selectedPointId.value) ?? null;
  };

  // Set the selected point
  const setSelectedPoint = (point: Point | null) => {
    selectedPointId.value = point?.id ?? null;
  };

  // Reset points to default state
  const resetPoints = () => {
    points.value = [
      {
        id: "start",
        x: 0,
        y: 1,
        handleOut: { x: 0.72, y: 0.82 },
      },
      {
        id: "end",
        x: 1,
        y: 0,
        handleIn: { x: 0.28, y: 0.18 },
      },
    ];
    selectedPointId.value = "start";
  };

  // Load points from a preset or external source
  const loadPoints = (newPoints: Point[]) => {
    points.value = JSON.parse(JSON.stringify(newPoints)) as Point[];
    // Select first point after loading
    if (points.value.length > 0) {
      selectedPointId.value = points.value[0]!.id;
    }
  };

  // Helper to convert from display values (0=bottom, 1=top) to internal SVG values (0=top, 1=bottom)
  const yFromDisplayValues = (displayY: number): number => {
    return 1 - displayY;
  };

  // Helper to convert from internal SVG values (0=top, 1=bottom) to display values (0=bottom, 1=top)
  const yToDisplayValue = (svgY: number): number => {
    return 1 - svgY;
  };

  return {
    // State
    points,
    selectedPointId,

    // Computed/Getters
    getSelectedPoint,

    // Actions
    setSelectedPoint,
    resetPoints,
    loadPoints,

    // Utilities
    yFromDisplayValues,
    yToDisplayValue,
  };
}
