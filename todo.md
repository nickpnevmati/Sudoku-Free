# Release TODO

## Build blockers
- [x] Remove `using UnityEditor.Search;` from `UIMasterController.cs` (breaks player builds)
- [x] Fix `LightTheme.asset` / `DarkTheme.asset` — their script (`ThemeSO.cs`) was deleted; recreate it or replace with USS themes

## Game screen (port from old `GameWindowController.cs`, see commit `cde6a20`)
- [ ] Cell click handling: selection, highlight same numbers — *in progress: `HighlightCells` implemented*
  - [ ] Fast-mode paths call `HighlightCells()` without `ClearHighlights()` first — highlights pile up
  - [x] Non-fast `OnNumpadClicked` doesn't re-highlight after placing a number
  - [x] 3×3 box isn't highlighted (only row + column) — intended?
  - [ ] Hard-coded `Color.blue`/`Color.cyan` — dark text on blue is unreadable; move to USS classes for theming
- [ ] Numpad input — *in progress: buttons wired, routing in `OnNumpadClicked` done*
- [ ] Fast Mode button — *in progress: toggles flag + numpad highlight, but still ends with `throw new NotImplementedException()`*
  - [x] `HighlightNumpad` off by one: compares `m_selectedNumber == i` (0-based) — should be `i + 1`; selecting 9 highlights nothing
  - [x] Turning fast mode off doesn't clear the highlight (`m_selectedNumber` still set) — clear it or check `m_fastMode` in `HighlightNumpad`
  - [ ] `.numpad_button_active` USS rule only repeats layout props — add the actual highlight (background/colour) plus a `:hover` variant
  - [x] Fast-mode tap on a filled cell switches the number but then falls through and overwrites/erases that cell (incl. givens) — `return` after switching
  - [x] Entering fast mode always picks 1 — old behaviour used the selected cell's number if any
  - [ ] **`int.Parse` crashes entering fast mode when the selected cell is empty** — fall back to 1 when `number` is `""` — *still unfixed: `OnFastModeToggled` parses with no empty check*
  - [ ] Fast-mode tap on a filled cell changes `m_selectedNumber` but doesn't call `HighlightNumpad()` — numpad shows the old number
- [ ] Note mode button + note toggling — *flag toggles; no visual on/off state on the button*
- [ ] Erase button — *flag toggles; no visual on/off state; Note + Erase can both be on (Note wins silently)*
- [x] QuickNote button — fills notes from row, column and 3×3 box; saves
- [ ] Undo button + history — *wired, handler throws*
- [x] Exit button with confirmation prompt (reusable `ConfirmModal` + `ShowConfirmation(...)`)
  - [ ] Exiting while a puzzle is still generating: the task keeps running and fills the grid behind the menu
- [x] Save after every move (numbers + notes; starting state saved on new game; save deleted when solved)
- [ ] Error checking (respect "Check Errors" setting)
- [ ] Win detection + "play again?" prompt — *save is already deleted on solve; prompt can reuse `ShowConfirmation`*
- [ ] Reset fast / note / erase modes when a new game starts
- [ ] Win animation (respect "Disable Win Animation" setting)
- [ ] Game timer (`GameTimer` label) — *label fetched, not updated*
- [ ] Loading indicator while a puzzle is generating
- [ ] Prevent editing given clues
- [x] Notes should toggle (use `ToggleNote`), not only set
- [x] Placing a number should clear that cell's notes
- [ ] Fast mode + erase: the new early `return` on filled cells means erase can't clear a number again — skip the return when `m_eraseMode` (but protect givens)

## Bugs in current code
- [x] **`ConstructGrid`: lambda captures `index` by reference** — every cell calls `OnCellClicked(81)` → out of range
- [x] **`ConstructNumpad`: lambda captures loop variable `i`** — every button calls `OnNumpadClicked(10)`
- [x] **`RegisterButtons`: wrong names** — `"FastNoteButton"` should be `"FastModeButton"`, `"QuickNoteButton"` should be `"QuicknoteButton"` (case-sensitive) → null ref in `Awake`
- [x] Note indexing is now mixed — standardised on 1–9 for `SetNote`/`ClearNote`/`ToggleNote`:
  - [x] **`ClearAll` passes 0–8 to `ClearNote`, which now does `number - 1` → `notes[-1]` crash. `SetNumber` and `EraseCell` call `ClearAll`, so every number placement crashes**
  - [x] `ToggleNote` still uses `notes[number]` with 1–9 → off by one, note 9 throws
  - [x] `SetState` calls `ToggleNote(i)` with 0–8 — will break if `ToggleNote` switches to 1-based
- [ ] `EraseCell` only clears notes, not the number
- [ ] Picking mode: add `picking-mode="Ignore"` to `NotesContainer` and the three `NoteRow`s in `Cell.uxml` — *typo: written as `"Ingore"`, so Unity ignores it*
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

## Menu
- [x] Difficulty select (modal in `Menu.uxml`, wired in `MenuUIController`)
- [x] Conditionally hide Continue button (`MenuUIController.Refresh()`, called on `Start` and from `UIMasterController.NavigateToMenu`)
- [ ] Settings → menu navigation must go through `NavigateToMenu` so `Refresh()` runs

## Settings screen
- [ ] Implement `SettingsUIController` (currently empty)
- [ ] Back button to menu
- [ ] Persist settings (PlayerPrefs or JSON)
- [ ] Dark Theme toggle
- [ ] Check Errors toggle
- [ ] Disable Quicknote toggle
- [ ] Disable Win Animation toggle

## Cleanup
- [x] Delete stray `Assets/UI:call cursor(43,35)` and its `.meta`
- [x] Remove stale `uiframework` entry from `.gitmodules`
- [x] Remove `Resources/puzzles.txt` and `PuzzleLoader.LoadPuzzle` (or keep as offline fallback)
- [x] Remove Adaptive Performance package if not needed (mobile notifications package is gone, but `Assets/Editor/com.unity.mobile.notifications/` settings remain)
- [x] Fix outdated template/style paths in `Game.uxml` (`Assets/UIToolkit/...` → `Assets/UI Toolkit/...`)

## Testing
- [ ] Test on a real Android device (touch input, safe areas, different aspect ratios)
- [ ] Test save/continue across app restarts and when the app is killed
- [ ] Check generation time for HARD/EVIL on low-end devices

## Play Store
- [ ] Set real application identifier (replace `com.DefaultCompany.com.unity.template.mobile2D`) — permanent once published
- [ ] Create upload keystore and enable custom keystore
- [ ] Build as App Bundle (.aab)
- [ ] Set version name / version code
- [ ] App icon (adaptive icon)
- [ ] Splash screen
- [ ] Store listing: description, screenshots, feature graphic
- [ ] Privacy policy URL
- [ ] Content rating questionnaire + data safety form
