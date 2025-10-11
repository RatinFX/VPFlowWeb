import { onMounted, onUnmounted, type Ref } from "vue";
import type { Point } from "@/models/PresetCurve";
import { useLogging } from "./useLogging";

export interface KeyboardShortcutHandlers {
  onResetView?: () => void;
  onDeletePoint?: () => void;
  onExportData?: () => void;
  onNavigatePrevious?: () => void;
  onNavigateNext?: () => void;
  onMovePoint?: (deltaX: number, deltaY: number, step: number) => void;
}

export interface KeyboardShortcutOptions {
  selectedPoint?: Ref<Point | null>;
  points?: Ref<Point[]>;
  handlers: KeyboardShortcutHandlers;
  enabled?: Ref<boolean>;
}

export function useKeyboardShortcuts(options: KeyboardShortcutOptions) {
  const {
    selectedPoint,
    points,
    handlers,
    enabled = { value: true } as Ref<boolean>,
  } = options;

  const { log } = useLogging();

  const smallStep = 0.01; // Small movement step
  const bigStep = 0.05; // Big movement step (with Shift)

  function handleKeydown(e: KeyboardEvent) {
    if (!enabled.value) return;

    // R - Reset view
    if (e.key === "r" || e.key === "R") {
      handlers.onResetView?.();
      return;
    }

    // Delete/X - Delete point
    if ((e.key === "Delete" || e.key === "x") && selectedPoint?.value) {
      handlers.onDeletePoint?.();
      return;
    }

    // Ctrl+E / Cmd+E - Export data
    if ((e.ctrlKey || e.metaKey) && e.key === "e") {
      e.preventDefault();
      handlers.onExportData?.();
      return;
    }

    // Q/E - Navigate between points
    if (
      !e.ctrlKey &&
      !e.metaKey &&
      !e.altKey &&
      (e.key.toLowerCase() === "q" || e.key.toLowerCase() === "e")
    ) {
      if (!selectedPoint?.value || !points?.value.length) return;

      const currentIdx = points.value.findIndex(
        (p) => p.id === selectedPoint.value?.id
      );
      if (currentIdx === -1) return;

      if (e.key.toLowerCase() === "q") {
        // Select previous point
        const prevIdx = currentIdx - 1;
        if (prevIdx >= 0) {
          handlers.onNavigatePrevious?.();
          e.preventDefault();
        }
      } else if (e.key.toLowerCase() === "e") {
        // Select next point
        const nextIdx = currentIdx + 1;
        if (nextIdx < points.value.length) {
          handlers.onNavigateNext?.();
          e.preventDefault();
        }
      }
      return;
    }

    // WASD/Arrow keys - Move selected point
    if (selectedPoint?.value && !e.ctrlKey && !e.metaKey && !e.altKey) {
      // Prevent moving locked points (start/end)
      if (
        selectedPoint.value.id === "start" ||
        selectedPoint.value.id === "end"
      ) {
        return;
      }

      const step = e.shiftKey ? bigStep : smallStep;
      let deltaX = 0;
      let deltaY = 0;

      switch (e.key.toLowerCase()) {
        case "w":
        case "arrowup":
          deltaY = -step; // Move up
          e.preventDefault();
          break;
        case "s":
        case "arrowdown":
          deltaY = step; // Move down
          e.preventDefault();
          break;
        case "a":
        case "arrowleft":
          deltaX = -step; // Move left
          e.preventDefault();
          break;
        case "d":
        case "arrowright":
          deltaX = step; // Move right
          e.preventDefault();
          break;
        default:
          return; // Not a movement key, do nothing
      }

      log(`Key pressed: ${e.key}, moving by step: ${step}`);
      handlers.onMovePoint?.(deltaX, deltaY, step);
    }
  }

  // Setup and cleanup
  onMounted(() => {
    window.addEventListener("keydown", handleKeydown);
  });

  onUnmounted(() => {
    window.removeEventListener("keydown", handleKeydown);
  });

  return {
    handleKeydown,
  };
}
