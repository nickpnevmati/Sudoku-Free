import { screenMapping, settings, useBridge } from './bridge';
import { Provider } from 'react-redux';
import { store } from './store';

export default function App() {
  const { getGlobal, screen } = useBridge();

  return (
    // TODO
    <Provider store={store}>
      <div className={`root ${getGlobal(settings.darkTheme) ? 'theme_dark' : ''}`}>
        {screenMapping[screen]}
      </div>
    </Provider>
  )
}