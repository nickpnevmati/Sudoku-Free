import Button from "src/templates/Button"
import { useDispatch } from "react-redux";
import { setContinue } from "src/slices/flagsSlice";
import { screenKeys, useBridge, flags } from "src/bridge";

export default function MainMenu() {
    const { getGlobal, navigateTo, exitGame } = useBridge();

    const dispatch = useDispatch();

    const hasPreviousSave = getGlobal(flags.hasPreviousSave);

    function continueGame() {
        dispatch(setContinue(true));
        navigateTo(screenKeys.GameScreen);
    }

    return (
        <div className='buttons_container'>
            <Button onClick={() => navigateTo(screenKeys.GameMenu)} text={'New Game'} />
            {
                hasPreviousSave &&
                <Button onClick={continueGame} text="Continue Game" />
            }
            <Button onClick={() => navigateTo(screenKeys.Settings)} text={'Settings'} />
            <Button onClick={exitGame} text={'Exit'} />
        </div>
    )
}