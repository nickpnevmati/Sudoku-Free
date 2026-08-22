import { useDispatch } from "react-redux"
import Button from "src/templates/Button"
import { setDifficulty } from "src/slices/flagsSlice";
import { settings, screenKeys, useBridge } from "src/bridge";
import Modal from "src/templates/Modal";
import { useState } from "react";

export default function GameMenu() {
    const dispatch = useDispatch();
    const { getGlobal, changeSettings, navigateTo } = useBridge();

    const [warnModal, setWarnModal] = useState(false);

    const hideEvilWarning = getGlobal(settings.hideEvilWarning);

    const startGame = (difficulty: number) => {
        dispatch(setDifficulty(difficulty));
        navigateTo(screenKeys.GameScreen);
    }

    const handleEvilDifficulty = () => {
        if (hideEvilWarning) {
            startGame(3);
            return;
        }

        setWarnModal(true);
    }

    const dismissWarnModal = () => {
        changeSettings(settings.hideEvilWarning, true);
        setWarnModal(false);
    }

    return (
        <>
            <div class="buttons_container">
                <Button onClick={() => startGame(0)} text="Easy" />
                <Button onClick={() => startGame(1)} text="Medium" />
                <Button onClick={() => startGame(2)} text="Hard" />
                <Button onClick={handleEvilDifficulty} text="Evil" />
                <Button onClick={() => navigateTo(screenKeys.MainMenu)} text="Back" />
            </div>

            <Modal
                show={warnModal}
                text='"Evil" difficulty games may take some time to generate on older devices'
                primaryAction={{
                    text: "I understand, continue",
                    onClick: () => { dismissWarnModal(); startGame(3); }
                }}
                secondaryAction={{
                    text: "Go back",
                    onClick: dismissWarnModal
                }}
            />
        </>
    )
}