declare module '*.css';
declare const module: { hot?: { accept(dep: string, cb: () => void): void } };

declare global {
  interface ReactUnityCustomAttributes {
    'custom-command'?: string;
    'custom-noteMode'?: boolean;
    'custom-fastMode'?: boolean;
    'custom-quickNote'?: boolean;

    onGameFinished?: () => void;
    onCellSelected?: (index: number) => void;
    onExitRequested?: () => void;
  }
}

export {};