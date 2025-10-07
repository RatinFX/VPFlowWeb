<script setup lang="ts">
import type { PresetCurve } from "@/models/PresetCurve";
import Tabs from "./ui/tabs/Tabs.vue";
import TabsContent from "./ui/tabs/TabsContent.vue";
import TabsList from "./ui/tabs/TabsList.vue";
import TabsTrigger from "./ui/tabs/TabsTrigger.vue";

const emit = defineEmits<{
  loadPreset: [PresetCurve];
}>();

// Library preset curves
const libraryPresets: PresetCurve[] = [
  {
    name: "Linear",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0.33, y: 0.67 } },
      { id: "end", x: 1, y: 0, handleIn: { x: 0.67, y: 0.33 } },
    ],
  },
  {
    name: "Ease In",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0.42, y: 1 } },
      { id: "end", x: 1, y: 0, handleIn: { x: 1, y: 0 } },
    ],
  },
  {
    name: "Ease Out",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0, y: 1 } },
      { id: "end", x: 1, y: 0, handleIn: { x: 0.58, y: 0 } },
    ],
  },
  {
    name: "Ease In-Out",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0.42, y: 1 } },
      { id: "end", x: 1, y: 0, handleIn: { x: 0.58, y: 0 } },
    ],
  },
  {
    name: "Bounce",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0.15, y: 0.7 } },
      {
        id: "point_1",
        x: 0.5,
        y: -0.2,
        handleIn: { x: 0.35, y: -0.2 },
        handleOut: { x: 0.65, y: -0.2 },
      },
      { id: "end", x: 1, y: 0, handleIn: { x: 0.85, y: 0.3 } },
    ],
  },
  {
    name: "S-Curve",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0.25, y: 1 } },
      {
        id: "point_1",
        x: 0.5,
        y: 0.5,
        handleIn: { x: 0.25, y: 0.5 },
        handleOut: { x: 0.75, y: 0.5 },
      },
      { id: "end", x: 1, y: 0, handleIn: { x: 0.75, y: 0 } },
    ],
  },
  {
    name: "Elastic",
    points: [
      { id: "start", x: 0, y: 1, handleOut: { x: 0.2, y: 1.3 } },
      {
        id: "point_1",
        x: 0.6,
        y: -0.1,
        handleIn: { x: 0.4, y: -0.1 },
        handleOut: { x: 0.7, y: -0.1 },
      },
      { id: "end", x: 1, y: 0, handleIn: { x: 0.9, y: 0.1 } },
    ],
  },
];

function onPresetClick(preset: PresetCurve) {
  emit("loadPreset", preset);
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
