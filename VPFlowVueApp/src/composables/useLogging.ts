import { useSettings } from "./useSettings";

const MAX_LOG_LINES = 50;

/**
 * Composable for logging functionality
 * Provides console logging with automatic persistence to the logs ref
 */
export function useLogging() {
  /**
   * Format data for display in logs
   */
  function format(data: any[]): string {
    return data
      .map((d) => (typeof d === "string" ? `"${d}"` : JSON.stringify(d)))
      .join(" ");
  }

  /**
   * Trim logs to maximum number of lines
   */
  function trim(text: string): string {
    const lines = text.split(/\r?\n/);
    if (lines.length <= MAX_LOG_LINES) {
      return text;
    }
    return lines.slice(-MAX_LOG_LINES).join("\r\n");
  }

  /**
   * Commit a log entry to the logs ref
   * Uses lazy evaluation to avoid circular dependencies
   */
  function commit(prefix: string, data: any[]): void {
    const { logs } = useSettings();
    const entry = prefix + format(data);
    const newText = !logs.value ? entry : logs.value + "\r\n" + entry;
    logs.value = trim(newText);
  }

  /**
   * Log a message to console and logs
   */
  function log(...data: any[]): void {
    console.log(...data);
    commit("> ", data);
  }

  /**
   * Log a warning to console and logs
   */
  function warn(...data: any[]): void {
    console.warn(...data);
    commit("[!] ", data);
  }

  /**
   * Log an error to console and logs
   */
  function error(...data: any[]): void {
    console.error(...data);
    commit("[ERROR] ", data);
  }

  /**
   * Clear all logs
   */
  function clearLogs(): void {
    const { logs } = useSettings();
    logs.value = "";
  }

  return {
    log,
    warn,
    error,
    clearLogs,
  };
}
