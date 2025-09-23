<script lang="ts" setup>
import { ref, type PropType } from "vue";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const props = defineProps({
  items: {
    required: true,
    type: Array as PropType<string[]>,
  },
});

const selectedItem = ref(props.items?.[0] ?? "NO_ITEMS");
</script>

<template>
  <DropdownMenu>
    <DropdownMenuTrigger as-child>
      <Button class="w-18" variant="outline">{{ selectedItem }}</Button>
    </DropdownMenuTrigger>
    <DropdownMenuContent>
      <DropdownMenuRadioGroup v-model="selectedItem">
        <DropdownMenuRadioItem
          v-for="item in props.items"
          :key="item"
          :value="item"
        >
          {{ item }}
        </DropdownMenuRadioItem>
      </DropdownMenuRadioGroup>
    </DropdownMenuContent>
  </DropdownMenu>
</template>
