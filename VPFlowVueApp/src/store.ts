import { ref } from "vue";

/** Handled in @see logging.ts */
const logs = ref("");

const theme = ref("auto");

const displayLogs = ref(true);
const checkForUpdatesOnStart = ref(true);
const ignoreLongSectionWarning = ref(false);
const onlyCreateNecessaryKeyframes = ref(true);

export default {
  logs,

  theme,

  displayLogs,
  checkForUpdatesOnStart,
  ignoreLongSectionWarning,
  onlyCreateNecessaryKeyframes,
};
