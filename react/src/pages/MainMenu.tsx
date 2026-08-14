import Button from "src/templates/Button"
import { useGlobals } from "@reactunity/renderer"
import { screens } from "src/App";
import { useDispatch } from "react-redux";
import { setContinue } from "src/slices/difficultySlice";

export default function MainMenu() {
    const globals = useGlobals();

    const dispatch = useDispatch();

    const hasPreviousSave = globals["hasPreviousSave"];

    function continueGame() {
        dispatch(setContinue(true));
        globals.navigateTo(screens.GameScreen);
    }

    return (
        <div className='buttons_container'>
            <Button onClick={() => globals.navigateTo(screens.GameMenu)} text={'New Game'} />
            {
                hasPreviousSave &&
                <Button onClick={continueGame} text="Continue Game" />
            }
            <Button onClick={() => globals.navigateTo(screens.Settings)} text={'Settings'} />
            <Button onClick={globals.exitGame} text={'Exit'} />
        </div>
    )
}