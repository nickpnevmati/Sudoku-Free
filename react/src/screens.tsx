import { screenKeys } from './bridge';
import MainMenu from './pages/MainMenu';
import GameMenu from './pages/GameMenu';
import GameScreen from './pages/GameScreen';
import SettingsPage from './pages/SettingsPage';

/**
 * Lives here rather than in bridge.tsx so the dependency graph stays a tree: every page
 * imports the bridge, so the bridge must not import the pages back.
 */
export const screenMapping = {
  [screenKeys.MainMenu]: <MainMenu />,
  [screenKeys.GameMenu]: <GameMenu />,
  [screenKeys.GameScreen]: <GameScreen />,
  [screenKeys.Settings]: <SettingsPage />,
};
