import { useDispatch } from "react-redux"
import Button from "src/templates/Button"
import { setDifficulty } from "src/slices/flagsSlice";
import { screenKeys, useBridge } from "src/bridge";
import Modal from "src/templates/Modal";
import { useState } from "react";

export default function GameMenu() {
    const dispatch = useDispatch();
    const { navigateTo } = useBridge();

    const [warnModal, setWarnModal] = useState(false);

    const startGame = (difficulty: number) => {
        dispatch(setDifficulty(difficulty));
        navigateTo(screenKeys.GameScreen);
    }

    const handleEvilDifficulty = () => {
        // TODO add a settings from globals that doens't show the modal again if already dismissed
        setWarnModal(true);
    }

    return (
        <div class="buttons_container">
            <Button onClick={() => startGame(0)} text="Easy" />
            <Button onClick={() => startGame(1)} text="Medium" />
            <Button onClick={() => startGame(2)} text="Hard" />
            <Button onClick={handleEvilDifficulty} text="Evil" />
            <Button onClick={() => navigateTo(screenKeys.MainMenu)} text="Back" />

            <Modal
                show={warnModal}
                text='"Evil" difficulty games may take some time to generate on older devices'
                primaryAction={{
                    text: "I understand, continue",
                    onClick: () => { startGame(3) }
                }}
                secondaryAction={{
                    text: "Go back",
                    onClick: () => setWarnModal(false)
                }}
            />
        </div>
    )
}