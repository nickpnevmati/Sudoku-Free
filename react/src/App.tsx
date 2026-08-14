import { useGlobals } from '@reactunity/renderer';
import MainMenu from './pages/MainMenu';
import GameMenu from './pages/GameMenu';
import GameScreen from './pages/GameScreen';
import SettingsPage from './pages/SettingsPage';
import { settings } from './pages/SettingsPage';
import { Provider } from 'react-redux';
import { store } from './store';

const screenMapping = {
  'menu': <MainMenu />,
  'gameMenu': <GameMenu />,
  'gameScreen': <GameScreen />,
  'settings': <SettingsPage />,
}

export const screens = {
  'MainMenu': 'menu',
  'GameMenu': 'gameMenu',
  'GameScreen': 'gameScreen',
  'Settings': 'settings',
};

export const flags = {
  'continueGame': 'continueGame',
}

export default function App() {
  const globals = useGlobals();
  return (
    // TODO
    <Provider store={store}>
      <div className={`root ${globals[settings.darkTheme] ? 'theme_dark' : ''}`}>
        {screenMapping[globals.screen]}
      </div>
    </Provider>
  )
}