import { ref } from "vue";

const logs = ref("");

export default {
  logs,
};

function formatLog(data: any[]) {
  return data
    .map((d) => (typeof d === "string" ? `"${d}"` : JSON.stringify(d)))
    .join(" ");
}

export function log(...data: any[]) {
  console.log(...data);
  logs.value += "- " + formatLog(data) + " \r\n";
}

export function warn(...data: any[]) {
  console.warn(...data);
  logs.value += "[!] " + formatLog(data) + " \r\n";
}

export function error(...data: any[]) {
  console.error(...data);
  logs.value += "[ERROR] " + formatLog(data) + " \r\n";
}
