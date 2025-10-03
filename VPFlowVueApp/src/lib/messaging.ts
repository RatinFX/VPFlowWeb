import { log, warn } from "./logging";

export interface WebMessage {
  sender: string;
  payload: object;
}

/** Send data to backend */
function sendButtonClick(payload: WebMessage) {
  // TODO: remove hardcoded payload
  payload = {
    sender: "btnApply",
    payload: {
      coordinates: [0.7, 0.27, 0.5, 1.0],
    },
  };

  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage(payload);
    log("sent message with payload:", payload);
  } else {
    warn("no webview.postMessage available for payload:", payload);
  }
}

function setRecieveFromHost(callback: (data: string) => void) {
  window.ReceiveFromHost = callback;
}

export default {
  sendButtonClick,
  setRecieveFromHost,
};
