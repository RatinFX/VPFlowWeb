import type { Point } from "@/models/PresetCurve";
import type { SelectedMode } from "@/types/SelectedMode";
import { useLogging } from "./useLogging";
import { useSettings } from "./useSettings";

// Message types
export const MessageType = {
  Apply: "Apply",
  Settings: "Settings",
} as const;

export type MessageType = (typeof MessageType)[keyof typeof MessageType];

// Payload interfaces
export interface ApplyPayload {
  points: Point[];
}

export interface SettingsPayload {
  theme: string;
  displayLogs: boolean;
  checkForUpdatesOnStart: boolean;
  ignoreLongSectionWarning: boolean;
  onlyCreateNecessaryKeyframes: boolean;
  selectedMode: SelectedMode;
}

export interface WebMessage<T = any> {
  messageType: MessageType;
  payload: T | null;
}

// Callback types
type ReceiveSettingsCallback = (data: string) => void;
type ReceiveItemsCallback = (data: string) => void;

/**
 * Composable for WebView2 messaging between frontend and backend
 * Provides type-safe message sending and receiving capabilities
 */
export function useMessaging() {
  const { log, warn } = useLogging();

  /**
   * Send a message to the backend via WebView2
   */
  function sendMessage(type: MessageType, data?: Point[]): void {
    let payload: WebMessage;

    switch (type) {
      case MessageType.Apply:
        if (!data || !Array.isArray(data)) {
          warn("Apply message requires points array");
          return;
        }
        payload = {
          messageType: type,
          payload: createApplyPayload(data),
        };
        break;

      case MessageType.Settings:
        payload = {
          messageType: type,
          payload: createSettingsPayload(),
        };
        break;

      default:
        warn(`Unknown message type: ${type}`);
        return;
    }

    log("Sending message:", payload);

    if (window.chrome?.webview?.postMessage) {
      window.chrome.webview.postMessage(payload);
      log("Message sent successfully");
    } else {
      warn("WebView2 postMessage not available - message not sent");
    }
  }

  /**
   * Send apply message with curve points to backend
   */
  function sendApply(points: Point[]): void {
    sendMessage(MessageType.Apply, points);
  }

  /**
   * Send current settings to backend
   */
  function sendSettings(): void {
    sendMessage(MessageType.Settings);
  }

  /**
   * Register callback for receiving settings from backend
   */
  function onReceiveSettings(callback: ReceiveSettingsCallback): void {
    window.receiveSettings = callback;
    log("Settings receive callback registered");
  }

  /**
   * Register callback for receiving items from backend
   */
  function onReceiveItems(callback: ReceiveItemsCallback): void {
    window.receiveItems = callback;
    log("Items receive callback registered");
  }

  // TODO:
  // - remove onReceiveItems and receiveItems
  // - receiveEffects here, and to window
  // - receiveParameters here, and to window

  /**
   * Create payload for apply message
   */
  function createApplyPayload(points: Point[]): ApplyPayload {
    return { points };
  }

  /**
   * Create payload for settings message
   * Uses lazy evaluation to avoid circular dependencies
   */
  function createSettingsPayload(): SettingsPayload {
    const settings = useSettings();
    return {
      theme: settings.theme?.value ?? "auto",
      displayLogs: settings.displayLogs.value,
      checkForUpdatesOnStart: settings.checkForUpdatesOnStart.value,
      ignoreLongSectionWarning: settings.ignoreLongSectionWarning.value,
      onlyCreateNecessaryKeyframes: settings.onlyCreateNecessaryKeyframes.value,
      selectedMode: settings.selectedMode.value,
    };
  }

  return {
    // Message sending
    sendMessage,
    sendApply,
    sendSettings,

    // Message receiving
    onReceiveSettings,
    onReceiveItems,

    // Message types export
    MessageType,
  };
}
