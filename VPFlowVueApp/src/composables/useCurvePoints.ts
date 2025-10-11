import type { Point } from "@/models/PresetCurve";
import { ref } from "vue";

// Helper to convert from display values (0=bottom, 1=top) to internal SVG values (0=top, 1=bottom)
const yFromDisplayValues = (displayY: number): number => {
  return parseFloat((1 - displayY).toFixed(4));
};

// Shared state - will be the same instance across all components that use this composable
// Default points defined using display coordinates for clarity
const points = ref<Point[]>([
  {
    id: "start",
    x: 0,
    y: yFromDisplayValues(0), // Display: 0 (bottom) -> SVG: 1 (bottom)
    handleOut: { x: 0.72, y: yFromDisplayValues(0.18) }, // Display coords converted to SVG
  },
  {
    id: "end",
    x: 1,
    y: yFromDisplayValues(1), // Display: 1 (top) -> SVG: 0 (top)
    handleIn: { x: 0.28, y: yFromDisplayValues(0.82) }, // Display coords converted to SVG
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
        y: yFromDisplayValues(0),
        handleOut: { x: 0.72, y: yFromDisplayValues(0.18) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.28, y: yFromDisplayValues(0.82) },
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

  // Helper to convert from internal SVG values (0=top, 1=bottom) to display values (0=bottom, 1=top)
  const yToDisplayValue = (svgY: number): number => {
    return parseFloat((1 - svgY).toFixed(4));
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
