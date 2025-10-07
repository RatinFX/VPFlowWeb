export interface PresetCurve {
  name: string;
  points: Point[];
}

export interface Point {
  id: string | "start" | "end";
  x: number;
  y: number;
  handleIn?: { x: number; y: number };
  handleOut?: { x: number; y: number };
}
