<script setup lang="ts">
import type { PresetCurve } from "@/models/PresetCurve";
import { useCurvePoints } from "@/composables/useCurvePoints";
import { useMessaging } from "@/composables/useMessaging";
import { useLogging } from "@/composables/useLogging";
import Tabs from "./ui/tabs/Tabs.vue";
import TabsContent from "./ui/tabs/TabsContent.vue";
import TabsList from "./ui/tabs/TabsList.vue";
import TabsTrigger from "./ui/tabs/TabsTrigger.vue";

const { yFromDisplayValues, loadPoints } = useCurvePoints();
const { sendApply } = useMessaging();
const { log } = useLogging();

// Library preset curves - defined in display coordinates (0=bottom, 1=top)
const libraryPresets: PresetCurve[] = [
  {
    name: "Linear",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0.33, y: yFromDisplayValues(0.33) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.67, y: yFromDisplayValues(0.67) },
      },
    ],
  },
  {
    name: "Ease In",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0.42, y: yFromDisplayValues(0) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 1, y: yFromDisplayValues(1) },
      },
    ],
  },
  {
    name: "Ease Out",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0, y: yFromDisplayValues(0) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.58, y: yFromDisplayValues(1) },
      },
    ],
  },
  {
    name: "Ease In-Out",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0.42, y: yFromDisplayValues(0) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.58, y: yFromDisplayValues(1) },
      },
    ],
  },
  {
    name: "Bounce",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0.15, y: yFromDisplayValues(0.3) },
      },
      {
        id: "point_1",
        x: 0.5,
        y: yFromDisplayValues(1.2),
        handleIn: { x: 0.35, y: yFromDisplayValues(1.2) },
        handleOut: { x: 0.65, y: yFromDisplayValues(1.2) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.85, y: yFromDisplayValues(0.7) },
      },
    ],
  },
  {
    name: "S-Curve",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0.25, y: yFromDisplayValues(0) },
      },
      {
        id: "point_1",
        x: 0.5,
        y: yFromDisplayValues(0.5),
        handleIn: { x: 0.25, y: yFromDisplayValues(0.5) },
        handleOut: { x: 0.75, y: yFromDisplayValues(0.5) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.75, y: yFromDisplayValues(1) },
      },
    ],
  },
  {
    name: "Elastic",
    points: [
      {
        id: "start",
        x: 0,
        y: yFromDisplayValues(0),
        handleOut: { x: 0.2, y: yFromDisplayValues(-0.3) },
      },
      {
        id: "point_1",
        x: 0.6,
        y: yFromDisplayValues(1.1),
        handleIn: { x: 0.4, y: yFromDisplayValues(1.1) },
        handleOut: { x: 0.7, y: yFromDisplayValues(1.1) },
      },
      {
        id: "end",
        x: 1,
        y: yFromDisplayValues(1),
        handleIn: { x: 0.9, y: yFromDisplayValues(0.9) },
      },
    ],
  },
];

function onPresetClick(preset: PresetCurve) {
  log(`Loading preset curve: "${preset.name}"`);
  // Load preset points into the editor
  loadPoints(preset.points);
}

function onApplyPresetClick(preset: PresetCurve) {
  log(`Applying preset curve directly: "${preset.name}"`);
  // Apply preset directly to backend without loading into editor
  sendApply(preset.points);
}
</script>

<template>
  <Tabs default-value="library" class="flex flex-col h-full">
    <TabsList class="grid w-full grid-cols-2">
      <TabsTrigger value="library">Library</TabsTrigger>
      <TabsTrigger value="custom">Custom</TabsTrigger>
    </TabsList>
    <div class="overflow-auto border-blue-400 border-1 rounded-l">
      <TabsContent value="library">
        <div class="flex flex-wrap gap-2 p-2">
          <button
            v-for="preset in libraryPresets"
            :key="preset.name"
            @click="onPresetClick(preset)"
            @dblclick="onApplyPresetClick(preset)"
            class="w-20 h-20 border-2 border-border hover:border-primary rounded flex items-center justify-center text-sm bg-muted/20 hover:bg-muted/40 transition-colors"
          >
            {{ preset.name }}
          </button>
        </div>
      </TabsContent>
      <TabsContent value="custom">
        <div class="flex flex-wrap gap-2 p-2">
          <div
            v-for="i in 10"
            :key="i"
            class="w-20 h-20 border-2 border-border rounded flex items-center justify-center text-sm bg-muted/20"
          >
            Custom {{ i }}
          </div>
        </div>
      </TabsContent>
    </div>
  </Tabs>
</template>
