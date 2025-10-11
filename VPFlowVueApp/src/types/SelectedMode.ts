export type SelectedMode = 0 | 1;

export const SelectedMode = {
  Event: 0 as SelectedMode,
  Track: 1 as SelectedMode,
} as const;