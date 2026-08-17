import { useDispatch } from "react-redux"
import Button from "src/templates/Button"
import { setDifficulty } from "src/slices/difficultySlice";
import { screenKeys, useBridge } from "src/bridge";

export default function GameMenu() {
    const dispatch = useDispatch();
    const { navigateTo } = useBridge();

    const startGame = (difficulty: number) => {
        dispatch(setDifficulty(difficulty));
        navigateTo(screenKeys.GameScreen);
    }

    return (
        <div class="buttons_container">
            <Button onClick={() => startGame(0)} text="Easy" />
            <Button onClick={() => startGame(1)} text="Medium" />
            <Button onClick={() => startGame(2)} text="Hard" />
            <Button onClick={() => navigateTo(screenKeys.MainMenu)} text="Back" />
        </div>
    )
}