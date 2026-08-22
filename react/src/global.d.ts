declare global {
  const module: { hot?: { accept(dep: string, cb: () => void): void } };

  // Source of truth for the board's custom attribute names - mirrors PropertyKeys in
  // ReactBridge.cs. Keep in sync when adding a case to GameLogicController.SetProperty.
  interface ReactUnityCustomAttributes {
    'custom-command'?: string;
    'custom-noteMode'?: boolean;
    'custom-fastMode'?: boolean;
    'custom-quickNote'?: boolean;
    'custom-eraseMode'?: boolean;
    'custom-gamePaused'?: boolean;

    onGameFinished?: () => void;
    onGameReady?: () => void;
    onCellSelected?: (index: number) => void;
  }
}

export {};