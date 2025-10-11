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
