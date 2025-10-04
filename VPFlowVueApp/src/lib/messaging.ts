import store from "@/store";
import { log, warn } from "./logging";

const MessageType = {
  Apply: "Apply",
  Settings: "Settings",
} as const;

type MessageType = (typeof MessageType)[keyof typeof MessageType];

export { MessageType };

export interface WebMessage {
  messageType: MessageType;
  payload: object;
}

/** Send data to backend */

function sendMessage(type: MessageType) {
  const payload: WebMessage = createPayload(type);
  log("payload:", payload);
  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage(payload);
    log("sent message with payload:", payload);
  } else {
    warn("no webview.postMessage available for payload:", payload);
  }
}

export function createPayload(type: MessageType): WebMessage {
  const result: WebMessage = {
    messageType: type,
    payload: {},
  };

  if (type === MessageType.Apply) {
    result.payload = getApplyPayload();
  }

  if (type === MessageType.Settings) {
    result.payload = getSettingsPayload();
  }

  return result;
}

function getApplyPayload() {
  return {
    // TODO: account for multiple points
    coordinates: [0.7, 0.27, 0.5, 1.0],
  };
}

function getSettingsPayload() {
  return {
    theme: store.theme.value,
    displayLogs: store.displayLogs.value,
    checkForUpdatesOnStart: store.checkForUpdatesOnStart.value,
    ignoreLongSectionWarning: store.ignoreLongSectionWarning.value,
    onlyCreateNecessaryKeyframes: store.onlyCreateNecessaryKeyframes.value,
  };
}

function setRecieveFromHost(callback: (data: string) => void) {
  window.ReceiveFromHost = callback;
}

export default {
  sendMessage,
  createPayload,
  setRecieveFromHost,
};
