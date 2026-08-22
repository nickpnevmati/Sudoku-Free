import { useGlobals } from "@reactunity/renderer";
import { useMemo, useRef } from "react";

export const screenKeys = {
  MainMenu: "mainMenu",
  GameMenu: "gameMenu",
  GameScreen: "gameScreen",
  Settings: "settings",
};

export const settings = {
  darkTheme: "darkTheme",
  checkErrors: "checkErrors",
  disableQuickNote: "disableQuickNote",
  hideEvilWarning: "hideEvilWarning",
};

export const flags = {
  hasPreviousSave: "hasPreviousSave",
};

// Globals keys only. The board's `custom-*` attribute names are not listed here - they are
// JSX attributes, not globals, and ReactUnityCustomAttributes in global.d.ts type-checks them.
export const props = {
  screen: "screen",
  boardPrefab: "boardPrefab",
  solveTime: "solveTime",
};

export const commands = {
  startGame: (difficulty: number) => `start_game:${difficulty}`,
  continueGame: "continue_game",
  numpad: (num: number) => `numpad:${num}`,
  erase: "erase",
  undo: "undo",
};

export function useBridge() {
  const globals = useGlobals();

  // useGlobals() builds a fresh Proxy on every render, so anything closing over it directly
  // is a new reference every time. Keep the newest proxy in a ref and route the calls through
  // it: the functions below can then be created once and still read current values.
  const latest = useRef(globals);
  latest.current = globals;

  const api = useMemo(
    () => ({
      exitGame: () => latest.current.exitGame(),
      navigateTo: (screen: string) => latest.current.navigateTo(screen),
      changeSettings: (action: string, value: unknown) =>
        latest.current.changeSetting(action, value),
      setGlobal: (key: string, value: unknown) => {
        latest.current[key] = value;
      },
      getGlobal: (key: string) => latest.current[key],
    }),
    [],
  );

  // Read during render, not inside the memo: the proxy's get trap is what subscribes this
  // component to a global, and these two drive re-renders.
  const screen = globals[props.screen];
  const boardPrefab = globals[props.boardPrefab];

  return useMemo(
    () => ({ screen, boardPrefab, ...api }),
    [screen, boardPrefab, api],
  );
}
