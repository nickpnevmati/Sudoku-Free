import Button from "src/templates/Button";
import { ButtonType } from "src/templates/Button";
import styles from './GameScreen.module.css'
import { useEffect, useRef, useState } from "react";
import Modal from "src/templates/Modal";
import { RootState } from "src/store";
import { useDispatch, useSelector } from "react-redux";
import { setContinue } from "src/slices/flagsSlice";
import { commands, settings, screenKeys, useBridge } from "src/bridge";

export default function GameScreen() {
    const { getGlobal, navigateTo, boardPrefab } = useBridge();
    const dispatch = useDispatch();
    
    const [fastMode, setFastMode] = useState(false);
    const [lastNum, setLastNum] = useState(0);
    const [noteMode, setNoteMode] = useState(false);
    const [quickNote, setQuickNote] = useState(false);
    const [eraseMode, setEraseMode] = useState(false);

    // Command Stuff
    const [cmd, setCmd] = useState('');
    const seq = useRef(0);
    const send = (c: string) => setCmd(`${c}:${seq.current++}`);

    const [showExit, setShowExit] = useState(false);
    const [gameOver, setGameOver] = useState(false);

    const difficulty = useSelector((state: RootState) => state.flagsReducer.difficulty);
    const continueFlag = useSelector((state: RootState) => state.flagsReducer.continueGame);

    const newGame = () => send(commands.startGame(difficulty));
    const continueGame = () => send(commands.continueGame);
    const exitGame = () => navigateTo(screenKeys.MainMenu);

    const onNumpadClick = (button: number) => {
        setLastNum(button);
        send(commands.numpad(button));
    }
    const onUndo = () => send(commands.undo);

    useEffect(() => {
        continueFlag ? continueGame() : newGame();
        dispatch(setContinue(false));
    }, [])

    // ----- Events ----- 
    function onGameFinished() {
        console.log("React: onGameFinished");
        setGameOver(true);
    }

    
    function onCellSelected(num: number | null) {
        if (!num) return;
        setLastNum(num);
    }

    // ----- Events ----- 

    function handleEraseClicked(_?: boolean) {
        if (fastMode) {
            setNoteMode(false);
            setEraseMode(true);
            return;
        }

        send(commands.erase);
    }

    function handleNoteClicked() {
        if (fastMode) {
            setEraseMode(false);
        }

        setNoteMode(!noteMode);
    }

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
                            className={[
                                styles.numpad_key,
                                fastMode && lastNum === num && styles.numpad_key_selected,
                            ].filter(Boolean).join(' ')}
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
                target={boardPrefab}
                custom-command={cmd}
                custom-noteMode={noteMode}
                custom-fastMode={fastMode}
                custom-eraseMode={eraseMode}
                custom-quickNote={quickNote}
                onCellSelected={onCellSelected}
                onGameFinished={onGameFinished}
            />

            <div className={styles.footer_container}>
                {createNumpad()}

                <div className={styles.mode_buttons}>
                    <Button onClick={setFastMode} text="Fast Mode" type={ButtonType.Toggle} />
                    <Button onClick={handleNoteClicked} text="Note" type={ButtonType.Toggle} toggleStatus={!eraseMode && noteMode} />
                    {
                        !getGlobal(settings.disableQuickNote) &&
                        <Button onClick={setQuickNote} text="Quick Note" type={ButtonType.Toggle} />
                    }
                    <Button onClick={handleEraseClicked} text="Eraser" type={fastMode ? ButtonType.Toggle : ButtonType.Button} toggleStatus={eraseMode} />
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
                    onClick: () => { setGameOver(false); newGame(); }
                }}
                secondaryAction={{
                    text: "No",
                    onClick: () => { setGameOver(false); exitGame(); }
                }}
            />
        </div>
    );
}