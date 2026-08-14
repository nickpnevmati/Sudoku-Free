/// <reference path="./global.d.ts" />
import { render } from '@reactunity/renderer';
import App from './App';
import './index.css';

render(<App />);

if (module.hot) {
    module.hot.accept('./App', () => {
        render(<App />);
    });
}