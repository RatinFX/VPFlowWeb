<script setup lang="ts">
import { useSettings } from "@/composables/useSettings";
import { SelectedMode } from "@/types/SelectedMode";
import { Icon } from "@iconify/vue";
import { computed } from "vue";
import Toggle from "./ui/toggle/Toggle.vue";
import Tooltip from "./ui/tooltip/Tooltip.vue";
import TooltipContent from "./ui/tooltip/TooltipContent.vue";
import TooltipProvider from "./ui/tooltip/TooltipProvider.vue";
import TooltipTrigger from "./ui/tooltip/TooltipTrigger.vue";

const { selectedMode, setSetting } = useSettings();

const isTrackMode = computed({
  get: () => selectedMode.value === SelectedMode.Track,
  set: (value: boolean) => {
    setSetting({
      selectedMode: value ? SelectedMode.Track : SelectedMode.Event,
    });
  },
});
</script>

<template>
  <Toggle variant="outline" v-model="isTrackMode" class="p-0">
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger as-child>
          <div class="w-full h-full flex items-center justify-center">
            <!-- change icon color to #fdbf28 when enabled -->
            <Icon
              icon="mdi:chart-gantt"
              class="size-5"
              :class="{ 'text-[#fdbf28]': isTrackMode }"
            />
          </div>
        </TooltipTrigger>
        <TooltipContent>
          <p class="mb-1">
            Toggle selection mode [
            {{ isTrackMode ? "Track" : "Event" }}
            ]
          </p>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  </Toggle>
</template>
