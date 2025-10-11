import { useSettings } from "@/composables/useSettings";
import { log, warn } from "./logging";
import type { Point } from "@/models/PresetCurve";

const MessageType = {
  Apply: "Apply",
  Settings: "Settings",
} as const;

type MessageType = (typeof MessageType)[keyof typeof MessageType];

export { MessageType };

export interface WebMessage {
  messageType: MessageType;
  payload: any;
}

/** Send data to backend */

function sendMessage(type: MessageType, data?: Point[] | SettingsPayload) {
  const payload: WebMessage = {
    messageType: type,
    payload: data || null,
  };

  if (type === MessageType.Apply && data && Array.isArray(data)) {
    payload.payload = getApplyPayload(data);
  }

  if (type === MessageType.Settings) {
    payload.payload = getSettingsPayload();
  }

  log("payload:", payload);

  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage(payload);
    log("sent message with payload:", payload);
  } else {
    warn("no webview.postMessage available for payload:", payload);
  }
}

export interface ApplyPayload {
  points: Point[];
}

function getApplyPayload(points: Point[]) {
  return {
    points: points,
  };
}

export interface SettingsPayload {
  theme: string;
  displayLogs: boolean;
  checkForUpdatesOnStart: boolean;
  ignoreLongSectionWarning: boolean;
  onlyCreateNecessaryKeyframes: boolean;
}

function getSettingsPayload(): SettingsPayload {
  const settings = useSettings();
  return {
    theme: settings.theme?.value ?? "auto", // Access theme string value from settings
    displayLogs: settings.displayLogs.value,
    checkForUpdatesOnStart: settings.checkForUpdatesOnStart.value,
    ignoreLongSectionWarning: settings.ignoreLongSectionWarning.value,
    onlyCreateNecessaryKeyframes: settings.onlyCreateNecessaryKeyframes.value,
  };
}

function setReceiveSettings(callback: (data: string) => void) {
  window.receiveSettings = callback;
}

function setReceiveItems(callback: (data: string) => void) {
  window.receiveItems = callback;
}

export default {
  sendMessage,
  setReceiveSettings,
  setReceiveItems,
};
