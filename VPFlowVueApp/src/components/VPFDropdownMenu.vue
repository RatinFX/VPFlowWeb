<script lang="ts" setup>
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ref, watch, type PropType } from "vue";

const props = defineProps({
  items: {
    required: true,
    type: Array as PropType<string[]>,
  },
  buttonClasses: {
    required: false,
    type: String,
    default: "",
  },
});

const selectedItem = ref(getDefaultItem(props.items));

function getDefaultItem(list: string[]) {
  return list?.[0] ?? "NO_ITEMS";
}

watch(
  () => props.items,
  (newItems) => {
    selectedItem.value = getDefaultItem(newItems);
  }
);
</script>

<template>
  <DropdownMenu>
    <DropdownMenuTrigger as-child>
      <Button :class="props.buttonClasses" variant="outline">
        {{ selectedItem }}
      </Button>
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
