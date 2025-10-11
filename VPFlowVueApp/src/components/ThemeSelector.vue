<script setup lang="ts">
import {
  MenubarContent,
  MenubarMenu,
  MenubarTrigger,
} from "@/components/ui/menubar";
import { useMessaging } from "@/composables/useMessaging";
import { useSettings } from "@/composables/useSettings";
import { Icon } from "@iconify/vue";

const { theme } = useSettings();
const { sendSettings } = useMessaging();

function onThemeUpdate() {
  // Toggle between light and dark
  theme.value = theme.value === "light" ? "dark" : "light";
  sendSettings();
}
</script>

<template>
  <MenubarMenu>
    <MenubarTrigger @click="onThemeUpdate">
      <Icon
        icon="radix-icons:sun"
        class="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0"
      />
      <Icon
        icon="radix-icons:moon"
        class="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100"
      />
      <span class="sr-only">Toggle theme</span>
    </MenubarTrigger>
    <MenubarContent class="hidden"></MenubarContent>
  </MenubarMenu>
</template>
