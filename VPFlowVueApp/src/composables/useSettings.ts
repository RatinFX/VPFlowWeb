import { ref } from "vue";
import {
  type UseColorModeReturn,
  type BasicColorMode,
  useColorMode,
} from "@vueuse/core";

// Shared state - will be the same instance across all components that use this composable
const logs = ref("");
// Initialize theme with useColorMode at module level
const theme: UseColorModeReturn<BasicColorMode | "auto"> = useColorMode({
  disableTransition: false,
});
const displayLogs = ref(true);
const checkForUpdatesOnStart = ref(true);
const ignoreLongSectionWarning = ref(false);
const onlyCreateNecessaryKeyframes = ref(true);

// Type for setting updates
export type SettingUpdate = Partial<{
  displayLogs: boolean;
  checkForUpdatesOnStart: boolean;
  ignoreLongSectionWarning: boolean;
  onlyCreateNecessaryKeyframes: boolean;
}>;

export function useSettings() {
  /**
   * Update one or more settings and automatically sync with backend
   * Uses lazy import to avoid circular dependency
   */
  function setSetting(updates: SettingUpdate) {
    // Update the settings
    if (updates.displayLogs !== undefined) {
      displayLogs.value = updates.displayLogs;
    }
    if (updates.checkForUpdatesOnStart !== undefined) {
      checkForUpdatesOnStart.value = updates.checkForUpdatesOnStart;
    }
    if (updates.ignoreLongSectionWarning !== undefined) {
      ignoreLongSectionWarning.value = updates.ignoreLongSectionWarning;
    }
    if (updates.onlyCreateNecessaryKeyframes !== undefined) {
      onlyCreateNecessaryKeyframes.value = updates.onlyCreateNecessaryKeyframes;
    }

    // Send settings to backend (lazy import to avoid circular dependency)
    import("./useMessaging").then(({ useMessaging }) => {
      const { sendSettings } = useMessaging();
      sendSettings();
    });
  }

  return {
    // State
    logs,
    theme,
    displayLogs,
    checkForUpdatesOnStart,
    ignoreLongSectionWarning,
    onlyCreateNecessaryKeyframes,

    // Actions
    setSetting,
  };
}
