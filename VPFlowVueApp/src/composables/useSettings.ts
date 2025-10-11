import { ref } from "vue";
import {
  type UseColorModeReturn,
  type BasicColorMode,
  useColorMode,
} from "@vueuse/core";

// Shared state - will be the same instance across all components that use this composable
const logs = ref("");
// theme will be set to useColorMode() result in App.vue
// It can handle both BasicColorMode and "auto" mode
let theme: UseColorModeReturn<BasicColorMode | "auto"> | null = null;
const displayLogs = ref(true);
const checkForUpdatesOnStart = ref(true);
const ignoreLongSectionWarning = ref(false);
const onlyCreateNecessaryKeyframes = ref(true);

const a = useColorMode();

export function useSettings() {
  return {
    // State
    logs,
    get theme() {
      return theme;
    },
    set theme(value: UseColorModeReturn<BasicColorMode | "auto"> | null) {
      theme = value;
    },
    displayLogs,
    checkForUpdatesOnStart,
    ignoreLongSectionWarning,
    onlyCreateNecessaryKeyframes,
  };
}
