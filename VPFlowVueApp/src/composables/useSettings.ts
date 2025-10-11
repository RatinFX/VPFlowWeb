import { ref } from "vue";

// Shared state - will be the same instance across all components that use this composable
const logs = ref("");
// theme will be set to useColorMode() result in App.vue, which has a .value property for the actual theme string
const theme = ref<any>("dark");
const displayLogs = ref(true);
const checkForUpdatesOnStart = ref(true);
const ignoreLongSectionWarning = ref(false);
const onlyCreateNecessaryKeyframes = ref(true);

export function useSettings() {
  return {
    // State
    logs,
    theme,
    displayLogs,
    checkForUpdatesOnStart,
    ignoreLongSectionWarning,
    onlyCreateNecessaryKeyframes,
  };
}
