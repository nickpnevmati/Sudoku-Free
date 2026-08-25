import Button from "src/templates/Button";
import { ButtonType } from "src/templates/Button";
import styles from "./GameScreen.module.css";
import { memo, useCallback, useEffect, useRef, useState } from "react";
import Modal from "src/templates/Modal";
import { RootState } from "src/store";
import { useDispatch, useSelector } from "react-redux";
import { setContinue } from "src/slices/flagsSlice";
import { commands, settings, screenKeys, useBridge } from "src/bridge";
import Timer from "src/templates/Timer";

interface NumpadKeyProps {
  num: number;
  selected: boolean;
  onClick: (num: number) => void;
}

// Split out and memoized on purpose. Inline in the parent, all nine keys handed the host a fresh
// onClick identity on every tap, and each re-bind crosses into C#. Memoized, a tap re-renders only
// the two keys whose `selected` actually flipped, and even those keep a stable handler - so the
// commit is two className writes instead of nine listener rebinds.
const NumpadKey = memo(function NumpadKey({ num, selected, onClick }: NumpadKeyProps) {
  const handleClick = useCallback(() => onClick(num), [onClick, num]);

  return (
    <div
      id={`keypad-${num}`}
      onClick={handleClick}
      className={[styles.numpad_key, selected && styles.numpad_key_selected]
        .filter(Boolean)
        .join(" ")}
    >
      {num}
    </div>
  );
});

const NUMBERS = [1, 2, 3, 4, 5, 6, 7, 8, 9];

export default function GameScreen() {
  const { getGlobal, navigateTo, boardPrefab } = useBridge();
  const dispatch = useDispatch();

  const [fastMode, setFastMode] = useState(false);
  const [lastNum, setLastNum] = useState(0);
  const [noteMode, setNoteMode] = useState(false);
  const [quickNote, setQuickNote] = useState(false);
  const [eraseMode, setEraseMode] = useState(false);

  // Command Stuff
  const [cmd, setCmd] = useState("");
  const seq = useRef(0);
  const send = useCallback((c: string) => setCmd(`${c}:${seq.current++}`), []);

  const [showExit, setShowExit] = useState(false);
  const [gameOver, setGameOver] = useState(false);
  const [generating, setGenerating] = useState(true);

  const gamePaused = showExit;

  const difficulty = useSelector(
    (state: RootState) => state.flagsReducer.difficulty,
  );
  const continueFlag = useSelector(
    (state: RootState) => state.flagsReducer.continueGame,
  );

  const newGame = () => send(commands.startGame(difficulty));
  const continueGame = () => send(commands.continueGame);
  const exitGame = () => navigateTo(screenKeys.MainMenu);

  const onNumpadClick = useCallback(
    (button: number) => {
      setLastNum(button);
      send(commands.numpad(button));
    },
    [send],
  );
  const onUndo = useCallback(() => send(commands.undo), [send]);
  const openExit = useCallback(() => setShowExit(true), []);

  // Deliberately mount-once: this deals the puzzle. The ref guard is what enforces that,
  // not the empty dep array - so a re-run (StrictMode double-invoke, a hot reload, a future
  // dep being added) can't deal a second puzzle over the top of the one in play.
  const dealt = useRef(false);
  useEffect(() => {
    if (dealt.current) return;
    dealt.current = true;

    continueFlag ? continueGame() : newGame();
    dispatch(setContinue(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ----- Events -----
  // NOTE: do NOT wrap the three prefab handlers below in useCallback. ReactUnity's
  // PrefabComponent forwards listeners to its TargetHandler, but a listener bound before the
  // prefab instance resolves lands on the base component and is never replayed onto the handler
  // (ResolveInstance only replays CustomProperties). The fresh identity these get on every render
  // is what re-binds them after the target appears - stabilizing them silently breaks onGameReady
  // and the "Generating Game" modal never dismisses.
  function onGameFinished() {
    console.log("React: onGameFinished");
    setGameOver(true);
  }

  function onCellSelected(num: number | null) {
    if (!num) return;
    setLastNum(num);
  }

  function onGameReady() {
    setGenerating(false);
  }

  // ----- Events -----

  const handleEraseClicked = useCallback(
    (_?: boolean) => {
      if (fastMode) {
        setNoteMode(false);
        setEraseMode((e) => !e);
        return;
      }

      send(commands.erase);
    },
    [fastMode, send],
  );

  const handleNoteClicked = useCallback(() => {
    if (fastMode) {
      setEraseMode(false);
    }

    setNoteMode((n) => !n);
  }, [fastMode]);

  return (
    <div className={styles.game_window_root}>
      <div className={styles.header_buttons}>
        <Button onClick={openExit} text="Exit" />
        <Timer />
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
        custom-gamePaused={gamePaused}
        onCellSelected={onCellSelected}
        onGameFinished={onGameFinished}
        onGameReady={onGameReady}
      />

      <div className={styles.footer_container}>
        <div className={styles.numpad_parent}>
          {NUMBERS.map((num) => (
            <NumpadKey
              key={`keypad-${num}`}
              num={num}
              selected={fastMode && lastNum === num}
              onClick={onNumpadClick}
            />
          ))}
        </div>

        <div className={styles.mode_buttons}>
          <Button
            onClick={setFastMode}
            text="Fast Mode"
            type={ButtonType.Toggle}
          />
          <Button
            onClick={handleNoteClicked}
            text="Note"
            type={ButtonType.Toggle}
            toggleStatus={!eraseMode && noteMode}
          />
          {!getGlobal(settings.disableQuickNote) && (
            <Button
              onClick={setQuickNote}
              text="Quick Note"
              type={ButtonType.Toggle}
            />
          )}
          <Button
            onClick={handleEraseClicked}
            text="Eraser"
            type={fastMode ? ButtonType.Toggle : ButtonType.Button}
            toggleStatus={eraseMode}
          />
        </div>
      </div>

      {/* TEMP build marker - bumped each rebuild so a measurement run can prove which bundle the
          APK actually shipped. Remove once the perf work is signed off. */}
      <Modal show={generating} text="Generating Game - Please wait [b2]" />

      <Modal
        show={showExit}
        text="Are you sure you want to leave the game?"
        primaryAction={{
          onClick: exitGame,
          text: "Yes, Exit",
        }}
        secondaryAction={{
          onClick: () => setShowExit(false),
          text: "Keep Playing",
        }}
      />

      <Modal
        show={gameOver}
        text="Game Complete! Would you like to play again?"
        primaryAction={{
          text: "Yes!",
          onClick: () => {
            setGameOver(false);
            setGenerating(true);
            newGame();
          },
        }}
        secondaryAction={{
          text: "No",
          onClick: () => {
            setGameOver(false);
            exitGame();
          },
        }}
      />
    </div>
  );
}
