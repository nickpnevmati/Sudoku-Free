# Release TODO

## Build blockers
- [x] Remove `using UnityEditor.Search;` from `UIMasterController.cs` (breaks player builds)
- [x] Fix `LightTheme.asset` / `DarkTheme.asset` — their script (`ThemeSO.cs`) was deleted; recreate it or replace with USS themes

## Game screen (port from old `GameWindowController.cs`, see commit `cde6a20`)
- [x] Cell click handling: selection, highlight same numbers — all visuals rebuilt by `Refresh()`
  - [x] Fast-mode paths call `HighlightCells()` without `ClearHighlights()` first — highlights pile up
  - [x] Non-fast `OnNumpadClicked` doesn't re-highlight after placing a number
  - [x] 3×3 box isn't highlighted (only row + column) — intended?
  - [x] Hard-coded `Color.blue`/`Color.cyan` — now USS classes `cell_selected` / `cell_related` / `cell_same_number` in `GridStyles.uss`
- [x] Numpad input
- [x] Fast Mode button
  - [x] `HighlightNumpad` off by one: compares `m_selectedNumber == i` (0-based) — should be `i + 1`; selecting 9 highlights nothing
  - [x] Turning fast mode off doesn't clear the highlight (`m_selectedNumber` still set) — clear it or check `m_fastMode` in `HighlightNumpad`
  - [x] `.numpad_button_active` USS rule only repeats layout props — now uses the accent colour, incl. `:hover`/`:active`
  - [x] Fast-mode tap on a filled cell switches the number but then falls through and overwrites/erases that cell (incl. givens) — `return` after switching
  - [x] Entering fast mode always picks 1 — old behaviour used the selected cell's number if any
  - [x] **`int.Parse` crashes entering fast mode when the selected cell is empty** — fall back to 1 when `number` is `""` — *still unfixed: `OnFastModeToggled` parses with no empty check*
  - [x] Fast-mode tap on a filled cell changes `m_selectedNumber` but doesn't call `HighlightNumpad()` — numpad shows the old number
- [x] Fast / Note / Erase buttons show their on state (`game_button_active`, accent colour) via `HighlightModeButtons()`
- [x] Note mode button + note toggling
- [x] Erase button — *Note and Erase now switch each other off*
  - [x] **Fast mode + Erase on a filled cell isn't saved**: that branch of `OnCellClicked` returns before `SaveGame()`, so the erase isn't in history (Undo skips it) and Continue brings the number back
- [x] QuickNote button — fills notes from row, column and 3×3 box; saves
- [x] Undo button + history — stops at the starting state and saves after undoing
  - [x] Moves that change nothing (number/note on a filled cell, erase on a given) no longer add history entries — `SaveGame` skips when `GridStateToString()` equals `history.Last`
- [x] Placing a number clears that number from notes in its row and column, but not its 3×3 box (`SetNumber` only uses `ColumnsRows`)
- [x] Exit button with confirmation prompt (reusable `ConfirmModal` + `ShowConfirmation(...)`)
  - [x] Exiting while a puzzle is still generating — `ExitToMenu()` cancels the task (`CancelGeneration`, linked `CancellationTokenSource`)
- [x] Save after every move (numbers + notes; starting state saved on new game; save deleted when solved)
- [x] Error checking (respect "Check Errors" setting) — wrong numbers get `cell_error` (red) in `HighlightCells`
- [x] Win detection + "play again?" prompt — `SetNumber` → `OnPuzzleSolved` → `ShowWinPrompt` (time + "New game", which opens the difficulty picker via `UIMasterController.NavigateToDifficultySelect`)
  - [x] Once solved, the grid is locked (`m_solved` guards cell/numpad/QuickNote/Undo) and the timer stops
- [x] Reset fast / note / erase modes when a new game starts
- [x] Win animation (respect "Disable Win Animation" setting) — `PlayWinAnimation`: cells pulse in rings from the last number placed (`cell_win` / `cell_win_anim` in `GridStyles.uss`), then the prompt; stopped by `StopWinAnimation` on new game / exit
- [x] Game timer (`GameTimer` label, `FormatTime`)
  - [x] Timer isn't saved, so Continue restarts it at 0
  - [x] Timer only ticks while a game is in progress (`IsGameInProgress`): paused on the menu / settings, with the exit or win prompt open, while generating, and once solved
- [x] Loading indicator while a puzzle is generating — `LoadingOverlay.uxml` (spinner + Cancel); opaque, so it also blocks input to the old grid; timer paused meanwhile
  - [x] "Stuck" overlay was the `GameUIController` component being disabled in the scene (Awake ran, Update didn't)
- [x] Prevent editing given clues
- [x] Notes should toggle (use `ToggleNote`), not only set
- [x] Placing a number should clear that cell's notes
- [x] Fast mode + erase on filled cells — handled before the early `return`; `EraseCell` protects givens via `puzzle[cell]`

## Bugs in current code
- [x] **`ConstructGrid`: lambda captures `index` by reference** — every cell calls `OnCellClicked(81)` → out of range
- [x] **`ConstructNumpad`: lambda captures loop variable `i`** — every button calls `OnNumpadClicked(10)`
- [x] **`RegisterButtons`: wrong names** — `"FastNoteButton"` should be `"FastModeButton"`, `"QuickNoteButton"` should be `"QuicknoteButton"` (case-sensitive) → null ref in `Awake`
- [x] Note indexing is now mixed — standardised on 1–9 for `SetNote`/`ClearNote`/`ToggleNote`:
  - [x] **`ClearAll` passes 0–8 to `ClearNote`, which now does `number - 1` → `notes[-1]` crash. `SetNumber` and `EraseCell` call `ClearAll`, so every number placement crashes**
  - [x] `ToggleNote` still uses `notes[number]` with 1–9 → off by one, note 9 throws
  - [x] `SetState` calls `ToggleNote(i)` with 0–8 — will break if `ToggleNote` switches to 1-based
- [x] `EraseCell` only clears notes, not the number
- [x] Picking mode: add `picking-mode="Ignore"` to `NotesContainer` and the three `NoteRow`s in `Cell.uxml` — *typo: written as `"Ingore"`, so Unity ignores it*
- [x] Continue: `RequestStartGame` discards the result of `PuzzleLoader.LoadSaved()`
- [x] Continue: `StartGame` only shows the original givens — the saved progress (last history entry) is never applied to the grid
- [x] Continue: throws when no save exists — Continue is now hidden when `!PuzzleLoader.hasPreviousSave`
- [x] `HStack.Push` uses LINQ `Append`, which never modifies the list
- [x] `GridState.FromCell`: `stateArray.ToString()` yields `"System.Char[]"` — use `new string(stateArray)`
- [x] `Cell.GetState`: `label.text[0]` crashes on empty cells (removed; replaced by `NotesToString`/`NotesFromString`)
- [x] `GridFromString` writes `"0"` into blank cells instead of `""`
- [x] `FinishedCheck` / `IsPlacementCorrect` are `void` — make them return `bool`
- [x] `GameUIController.Awake` missing `return` after destroying a duplicate
- [x] `GridFromString` throws a bare `Exception()` — add a message
- [x] Menu buttons use `MouseUpEvent`; switch to `button.clicked` for reliable touch input
### General
- [ ] Notes aren't highlighted
### Normal Mode
- [x] Erase doesn't work - should turn into a button (not toggle) under normal mode
- [x] Placing a number doesn't re-trigger highlights
- [x] Numpad is highlighted when number selected
### Fast Mode
- [x] Selected Cell doesn't render ColRowBox highlights

## Menu
- [x] Difficulty select (modal in `Menu.uxml`, wired in `MenuUIController`)
- [x] Conditionally hide Continue button (`MenuUIController.Refresh()`, called on `Start` and from `UIMasterController.NavigateToMenu`)
- [x] Settings → menu navigation must go through `NavigateToMenu` so `Refresh()` runs

## Settings screen
- [x] Implement `SettingsUIController` — *each `CustomToggle` row is bound to a `GameSettings` property*
- [x] **Add the `SettingsUIController` component to the Settings UIDocument GameObject in `MainScene`** (it isn't attached yet)
- [x] Back button to menu
- [x] Persist settings — static `GameSettings` class, backed by PlayerPrefs
- [x] Dark Theme toggle (calls `UIMasterController.SetDarkTheme`)
- [x] Use `var(--color-…)` for any new colours instead of hard-coding them, or dark mode will miss them — *audited: no hard-coded colours left in USS/UXML/C# (only a transparent `rgba(0,0,0,0)`, which is theme-independent); keep it that way for new styles*
- [x] Check Errors toggle — read in `HighlightCells` on every `Refresh()`
- [x] Disable Quicknote toggle — hides the QuickNote button when a game starts
  - [x] Mode buttons line up with the numpad with or without QuickNote — footer is one half-height aspect box (`footer_aspect`), both columns share its height and a 10px margin; `game_button_last` removed
- [x] Disable Win Animation toggle — skips straight to the win prompt

## Cleanup
- [x] Delete stray `Assets/UI:call cursor(43,35)` and its `.meta`
- [x] Remove stale `uiframework` entry from `.gitmodules`
- [x] Remove `Resources/puzzles.txt` and `PuzzleLoader.LoadPuzzle` (or keep as offline fallback)
- [x] Remove Adaptive Performance package if not needed (mobile notifications package is gone, but `Assets/Editor/com.unity.mobile.notifications/` settings remain)
- [x] Fix outdated template/style paths in `Game.uxml` (`Assets/UIToolkit/...` → `Assets/UI Toolkit/...`)
- [x] Remove leftover `Debug.Log`s in `StartGame` and `GridFromString` (the latter prints 810 chars on every Continue/Undo)
- [x] `m_quickNote` is assigned but never read (compiler warning CS0414) — remove it
- [x] Save file rewrites the whole history (810 chars per move) on every move — Undo depth capped at `k_maxHistory` (200, ~160 KB save max) via `HStack.TrimOldest` in `SaveGame`

## Testing
- [ ] Test on a real Android device (touch input, safe areas, different aspect ratios)
- [ ] Test save/continue across app restarts and when the app is killed
- [ ] Check generation time for HARD/EVIL on low-end devices

## Play Store
- [x] Set real application identifier: `com.nuclearbiscuitgames.sudokufree` (companyName fixed to `NuclearBiscuitGames`) — *edited in `ProjectSettings.asset` while Unity was open: check Player Settings still shows it*
- [ ] Create upload keystore and enable custom keystore — *Player Settings → Publishing Settings → Keystore Manager; back the keystore + passwords up outside the repo*
- [ ] Build as App Bundle (.aab) — *Build Profiles → Android → "Build App Bundle (Google Play)"; it's a local editor setting, not in the repo*
- [x] Set version name / version code: `1.0.0` / `1` — bump the code for every upload
- [ ] App icon (adaptive icon) — *needs artwork: 512×512 for the store, plus foreground/background layers*
- [x] Splash screen — the Unity splash is already off (`m_ShowUnitySplashScreen: 0`); the background colour is set
- [ ] Store listing: description, screenshots, feature graphic — *text drafted in `docs/store-listing.md`; graphics still to make*
- [ ] Privacy policy URL — *drafted in `docs/privacy-policy.md`; needs hosting (see `docs/store-listing.md`)*
- [ ] Content rating questionnaire + data safety form — *answers drafted in `docs/store-listing.md`*
- [ ] Check `submitAnalytics: 1` (Unity device stats) before declaring "no data collected"
