<script setup lang="ts">
import { Icon } from "@iconify/vue";
import {
  MenubarContent,
  MenubarMenu,
  MenubarTrigger,
  MenubarRadioGroup,
  MenubarRadioItem,
} from "@/components/ui/menubar";
import store from "@/store";
import messaging, { MessageType } from "@/lib/messaging";

function onThemeUpdate() {
  messaging.sendMessage(MessageType.Settings);
}
</script>

<template>
  <MenubarMenu>
    <MenubarTrigger>
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
    <MenubarContent class="min-w-26">
      <MenubarRadioGroup
        v-model="store.theme.value"
        @update:model-value="onThemeUpdate"
      >
        <MenubarRadioItem value="light"> Light </MenubarRadioItem>
        <MenubarRadioItem value="dark"> Dark </MenubarRadioItem>
      </MenubarRadioGroup>
    </MenubarContent>
  </MenubarMenu>
</template>
