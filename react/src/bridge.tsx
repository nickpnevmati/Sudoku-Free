import { useGlobals } from "@reactunity/renderer";
import MainMenu from "./pages/MainMenu";
import GameMenu from "./pages/GameMenu";
import GameScreen from "./pages/GameScreen";
import SettingsPage from "./pages/SettingsPage";

export const screenMapping = {
  'mainMenu': <MainMenu />,
  'gameMenu': <GameMenu />,
  'gameScreen': <GameScreen />,
  'settings': <SettingsPage />,
}

export const screenKeys = {
  'MainMenu': 'mainMenu',
  'GameMenu': 'gameMenu',
  'GameScreen': 'gameScreen',
  'Settings': 'settings',
};

export const settings = {
  'darkTheme': 'darkTheme',
  'checkErrors': 'checkErrors',
  'disableQuickNote': 'disableQuickNote'
}

export const flags = {
  'continueGame': 'continueGame',
  'hasPreviousSave': 'hasPreviousSave',
}

export const props = {
  'screen': 'screen',
  'boardPrefab': 'boardPrefab',

  // GameLogicController
  'noteMode': 'noteMode',
  'fastMode': 'fastMode',
  'quickNote': 'quickNote',
  'eraseMode': 'eraseMode',
  'command': 'command',
}

export const commands = {
  'startGame': (difficulty: number) => `start_game:${difficulty}`,
  'continueGame': 'continue_game',
  'numpad': (num: number) => `numpad:${num}`,
  'erase': 'erase',
  'undo': 'undo',
}

export function useBridge() {
  const globals = useGlobals();
  return {
    globals,
    screen: globals[props.screen],
    boardPrefab: globals[props.boardPrefab],
    startGame: (difficulty: number) => globals.startGame(difficulty),
    exitGame: () => globals.exitGame(),
    navigateTo: (screen: string) => globals.navigateTo(screen),
    changeSettings: (action: string, value: unknown) => globals.changeSetting(action, value),
    setGlobal: (key: string, value: unknown) => globals[key] = value,
    getGlobal: (key: string) => globals[key],
  };
}