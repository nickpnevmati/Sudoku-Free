import { useGlobals } from "@reactunity/renderer"
import { screens } from "src/App";
import Button from "src/templates/Button";
import { ButtonType } from "src/templates/Button";
import styles from './GameScreen.module.css'
import { useEffect, useRef, useState } from "react";
import Modal from "src/templates/Modal";
import { RootState } from "src/store";
import { settings } from "./SettingsPage";

import { useDispatch, useSelector } from "react-redux";
import { setContinue } from "src/slices/difficultySlice";

export default function GameScreen() {
    const globals = useGlobals();
    const dispatch = useDispatch();
    
    const [fastMode, setFastMode] = useState(false);
    const [lastNum, setLastNum] = useState(0);
    const [noteMode, SetNoteMode] = useState(false);
    const [quickNote, setQuickNote] = useState(false);
    const [cmd, setCmd] = useState('');
    const seq = useRef(0);
    const send = (c: string) => setCmd(`${c}:${seq.current++}`);

    const [showExit, setShowExit] = useState(false);
    const [gameOver, setGameOver] = useState(false);

    const difficulty = useSelector((state: RootState) => state.flagsReducer.difficulty);
    const continueFlag = useSelector((state: RootState) => state.flagsReducer.continueGame);

    const newGame = () => send(`start_game:${difficulty}`);
    const continueGame = () => send('continue_game');
    const exitGame = () => globals.navigateTo(screens.MainMenu);

    const onNumpadClick = (button: number) => {
        setLastNum(button);
        send(`numpad:${button}`);
    }
    const onUndo = () => send('undo');

    useEffect(() => {
        continueFlag ? continueGame() : newGame();
        dispatch(setContinue(false));
    }, [])

    // ----- Events ----- 
    function onGameFinished() {
        setGameOver(true);
    }

    function onCellSelected() {

    }

    function onExitRequested() {

    }
    // ----- Events ----- 

    function createNumpad() {
        const numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        return (
            <div className={styles.numpad_parent}>
                {numbers.map(num => {
                    const key = `keypad-${num}`;
                    return (
                        <div
                            id={key}
                            key={key}
                            onClick={() => { onNumpadClick(num) }}
                            style={{ backgroundColor: fastMode && lastNum === num ? 'red' : undefined }}
                        >
                            {num}
                        </div>
                    );
                })}
            </div>
        )
    }

    return (
        <div className={styles.game_window_root}>
            <div className={styles.header_buttons}>
                <Button onClick={() => { setShowExit(true) }} text="Exit" />
                <Button onClick={onUndo} text="Undo" />
            </div>

            <prefab
                className={styles.board_prefab}
                target={globals.boardPrefab}
                custom-command={cmd}
                custom-noteMode={noteMode}
                custom-fastMode={fastMode}
                custom-quickNote={quickNote}
                onCellSelected={onCellSelected}
                onExitRequested={onExitRequested}
                onGameFinished={onGameFinished}
            />

            <div className={styles.footer_container}>
                {createNumpad()}

                <div className={styles.mode_buttons}>
                    <Button onClick={setFastMode} text="Fast Mode" type={ButtonType.Toggle} />
                    <Button onClick={SetNoteMode} text="Note" type={ButtonType.Toggle} />
                    {
                        !globals[settings.disableQuickNote] &&
                        <Button onClick={setQuickNote} text="Quick Note" type={ButtonType.Toggle} />
                    }
                    <Button onClick={() => send('erase')} text="Eraser" type={fastMode ? ButtonType.Toggle : ButtonType.Button}/>
                </div>
            </div>

            <Modal
                show={showExit}
                text="Are you sure you want to leave the game?"
                primaryAction={{
                    onClick: exitGame,
                    text: 'Yes, Exit'
                }}
                secondaryAction={{
                    onClick: () => setShowExit(false),
                    text: 'Keep Playing'
                }}
            />

            <Modal
                show={gameOver}
                text="Game Complete! Would you like to play again?"
                primaryAction={{
                    text: "Yes!",
                    onClick: newGame
                }}
                secondaryAction={{
                    text: "No",
                    onClick: exitGame
                }}
            />
        </div>
    );
}