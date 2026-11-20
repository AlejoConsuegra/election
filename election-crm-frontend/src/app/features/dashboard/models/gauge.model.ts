export interface GaugeTick {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  filled: boolean;
}

export interface GaugeStat {
  label: string;
  value: string;
  paths: string[];
  color: string;
  tint: string;
}
