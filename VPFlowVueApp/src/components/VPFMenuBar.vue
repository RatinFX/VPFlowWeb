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
import type { Menu, MenuItem } from "@/models/Menu";
import { useSettings } from "@/composables/useSettings";
import { onMounted, ref } from "vue";
import { log } from "@/lib/logging";
import ThemeSelector from "./ThemeSelector.vue";
import messaging, { MessageType, type SettingsPayload } from "@/lib/messaging";

const {
  theme,
  displayLogs,
  checkForUpdatesOnStart,
  ignoreLongSectionWarning,
  onlyCreateNecessaryKeyframes,
} = useSettings();

const _separator: MenuItem = { isSeparator: true };

const state = ref<{
  displayLogs: MenuItem;
  checkForUpdatesOnStart: MenuItem;
  ignoreLongSectionWarning: MenuItem;
  onlyCreateNecessaryKeyframes: MenuItem;
}>({
  displayLogs: {
    label: "Display logs",
    isCheckboxItem: true,
    action: displayLogsToggle,
    checkboxValue: displayLogs.value,
  },
  checkForUpdatesOnStart: {
    label: "Check for updates on Start",
    isCheckboxItem: true,
    action: checkForUpdatesOnStartToggle,
    checkboxValue: true,
  },
  ignoreLongSectionWarning: {
    label: "Ignore long section warning",
    isCheckboxItem: true,
    action: ignoreLongSectionWarningToggle,
    checkboxValue: false,
  },
  onlyCreateNecessaryKeyframes: {
    label: "Only create necessary Keyframes",
    isCheckboxItem: true,
    action: onlyCreateNecessaryKeyframesToggle,
    checkboxValue: true,
  },
});

onMounted(() => {
  messaging.setReceiveSettings((data) => {
    const parsed = JSON.parse(data) as SettingsPayload;
    log("recieved settings:", parsed);

    theme.value = parsed.theme;

    displayLogs.value = parsed.displayLogs;
    state.value.displayLogs.checkboxValue = displayLogs.value;

    checkForUpdatesOnStart.value = parsed.checkForUpdatesOnStart;
    state.value.checkForUpdatesOnStart.checkboxValue =
      checkForUpdatesOnStart.value;

    ignoreLongSectionWarning.value = parsed.ignoreLongSectionWarning;
    state.value.ignoreLongSectionWarning.checkboxValue =
      ignoreLongSectionWarning.value;

    onlyCreateNecessaryKeyframes.value = parsed.onlyCreateNecessaryKeyframes;
    state.value.onlyCreateNecessaryKeyframes.checkboxValue =
      onlyCreateNecessaryKeyframes.value;
  });
});

const menus: Menu[] = [
  {
    label: "Settings",
    items: [
      state.value.displayLogs,
      _separator,
      state.value.checkForUpdatesOnStart,
      _separator,
      state.value.ignoreLongSectionWarning,
      state.value.onlyCreateNecessaryKeyframes,
      _separator,
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

function displayLogsToggle(state: boolean) {
  log("display logs toggle to:", state);
  displayLogs.value = state;
  messaging.sendMessage(MessageType.Settings);
}

function checkForUpdatesOnStartToggle(state: boolean) {
  log("check for updates on start toggle to:", state);
  checkForUpdatesOnStart.value = state;
  messaging.sendMessage(MessageType.Settings);
}

function ignoreLongSectionWarningToggle(state: boolean) {
  log("ignore long section warning toggle to:", state);
  ignoreLongSectionWarning.value = state;
  messaging.sendMessage(MessageType.Settings);
}

function onlyCreateNecessaryKeyframesToggle(state: boolean) {
  log("only create necessary Keyframes toggle to:", state);
  onlyCreateNecessaryKeyframes.value = state;
  messaging.sendMessage(MessageType.Settings);
}

function preferencesOnClick() {
  log("Preferences item click");
  alert("Settings modal");
}

function creatorOnClick() {
  log("Creator menu click");
  alert("Creator modal");
}

function checkForUpdate() {
  log("Check for update item click");
  alert("Check for update result");
}

function aboutOnClick() {
  log("About item click");
  alert("About modal");
}
</script>

<template>
  <Menubar>
    <MenubarMenu v-for="menu in menus" :key="menu.label">
      <MenubarTrigger @click="menu.action">
        {{ menu.label }}
      </MenubarTrigger>

      <MenubarContent v-if="menu.items" class="min-w-0">
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

    <ThemeSelector />
  </Menubar>
</template>
