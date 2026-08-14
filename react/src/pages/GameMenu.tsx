import { useDispatch } from "react-redux"
import Button from "src/templates/Button"
import { setDifficulty } from "src/slices/difficultySlice";
import { useGlobals } from "@reactunity/renderer";
import { screens } from "src/App";

export default function GameMenu() {
    const globals = useGlobals();
    const dispatch = useDispatch();

    const startGame = (difficulty: number) => {
        dispatch(setDifficulty(difficulty));
        globals.navigateTo(screens.GameScreen);
    }

    return (
        <div class="buttons_container">
            <Button onClick={() => startGame(0)} text="Easy" />
            <Button onClick={() => startGame(1)} text="Medium" />
            <Button onClick={() => startGame(2)} text="Hard" />
            <Button onClick={() => globals.navigateTo(screens.MainMenu)} text="Back" />
        </div>
    )
}