<script setup lang="ts">
import {
  Menubar,
  MenubarContent,
  MenubarItem,
  MenubarMenu,
  MenubarSeparator,
  MenubarTrigger,
} from "@/components/ui/menubar";
import MenubarCheckboxItem from "./ui/menubar/MenubarCheckboxItem.vue";
import type { Menu } from "@/models/Menu";

const menus: Menu[] = [
  {
    label: "Settings",
    items: [
      {
        label: "Check for updates on Start",
        isCheckboxItem: true,
        action: checkForUpdatesOnStartToggle,
      },
      { isSeparator: true },
      {
        label: "Ignore long section warning",
        isCheckboxItem: true,
        action: ignoreLongSectionWarningToggle,
      },
      {
        label: "Only create necessary Keyframes",
        isCheckboxItem: true,
        action: onlyCreateNecessaryKeyframesToggle,
      },
      { isSeparator: true },
      {
        label: "Preferences...",
        action: preferencesOnClick,
      },
    ],
  },
  {
    label: "Help",
    items: [
      {
        label: "Check for Updates",
        action: checkForUpdate,
      },
      {
        label: "Creator",
        action: creatorOnClick,
      },
      {
        label: "About",
        action: aboutOnClick,
      },
    ],
  },
];

function checkForUpdatesOnStartToggle(state: boolean) {
  console.log("check for updates on start toggle click", state);
}

function ignoreLongSectionWarningToggle(state: boolean) {
  console.log("ignore long section warning toggle click", state);
}

function onlyCreateNecessaryKeyframesToggle(state: boolean) {
  console.log("only create necessary Keyframes toggle click", state);
}

function preferencesOnClick() {
  console.log("Preferences item click");
  alert("Settings modal");
}

function creatorOnClick() {
  console.log("Creator menu click");
  alert("Creator modal");
}

function checkForUpdate() {
  console.log("Check for update item click");
  alert("Check for update result");
}

function aboutOnClick() {
  console.log("About item click");
  alert("About modal");
}
</script>

<template>
  <Menubar>
    <MenubarMenu v-for="menu in menus" :key="menu.label">
      <MenubarTrigger @click="menu.action">
        {{ menu.label }}
      </MenubarTrigger>

      <MenubarContent v-if="menu.items">
        <template v-for="item in menu.items" :key="item.label">
          <MenubarSeparator v-if="item.isSeparator" />

          <MenubarCheckboxItem
            v-else-if="item.isCheckboxItem"
            v-model="item.checkboxValue"
            @update:model-value="item.action?.(item.checkboxValue)"
          >
            {{ item.label }}
          </MenubarCheckboxItem>

          <MenubarItem
            v-else
            @click="item.action"
            :inset="menu.items?.some((x) => x.isCheckboxItem)"
          >
            {{ item.label }}
          </MenubarItem>
        </template>
      </MenubarContent>
    </MenubarMenu>
  </Menubar>
</template>
