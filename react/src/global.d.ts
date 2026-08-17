declare global {
  const module: { hot?: { accept(dep: string, cb: () => void): void } };

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