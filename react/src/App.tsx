import { settings, useBridge } from './bridge';
import { screenMapping } from './screens';
import { Provider } from 'react-redux';
import { store } from './store';

export default function App() {
  const { getGlobal, screen } = useBridge();

  return (
    <Provider store={store}>
      <div className={`root ${getGlobal(settings.darkTheme) ? 'theme_dark' : ''}`}>
        {screenMapping[screen]}
      </div>
    </Provider>
  )
}