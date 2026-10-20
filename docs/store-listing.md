# Play Store listing — Sudoku - Free

Draft text and form answers for the Play Console. Limits are Google's.

## App details

- **App name** (max 30): `Sudoku - Free`
- **Category:** Games → Puzzle
- **Contact email:** nickpnevmati@gmail.com
- **Privacy policy URL:** _wherever `docs/privacy-policy.md` is hosted (see below)_

## Short description (max 80)

```
Classic Sudoku with four difficulties, notes and dark mode. No ads, works offline.
```

## Full description (max 4000)

```
Sudoku - Free is a clean, distraction-free Sudoku game. No ads, no accounts, no internet needed.

Every puzzle is freshly generated and has exactly one solution, across four difficulty levels:
Easy, Medium, Hard and Evil.

FEATURES
• Four difficulty levels, from relaxing to brutal
• Pencil notes, plus Quicknote to fill in every candidate at once
• Fast mode: pick a number, then tap cells to place it
• Highlights for the selected row, column, box and matching numbers
• Optional error checking
• Undo
• Your game is saved after every move, so you can pick up where you left off
• Light and dark themes
• Works fully offline
```

> Check this against what actually ships — drop Undo / error checking if they aren't finished.

## Graphics (still to make)

| Asset | Size | Notes |
|---|---|---|
| App icon | 512×512 PNG, 32-bit | Also needed as the adaptive icon in Player Settings |
| Feature graphic | 1024×500 PNG/JPG | No transparency |
| Phone screenshots | 2–8, 9:16, min 1080px short side | Menu, game (light), game (dark), notes, difficulty picker |

## Data safety form

- Does your app collect or share any required user data types? **No**
- Is all user data encrypted in transit? **N/A** (no data leaves the device)
- Do you provide a way for users to request data deletion? **N/A** (data is local; uninstall removes it)

> Before submitting, confirm the build sends nothing: Player Settings has `submitAnalytics: 1`
> (Unity's device statistics). Turn it off if your licence allows, otherwise declare
> "Device or other IDs / App diagnostics" as collected by a third-party SDK (Unity).

## Content rating questionnaire (IARC)

- Category: **Game**
- Violence, fear, sexuality, language, controlled substances, gambling: **No** to all
- Users interact / share info / location / purchases: **No**
- Expected rating: **Everyone / PEGI 3**

## Target audience

- Age groups: **13+** (picking under-13 groups triggers the Families policy requirements;
  only do that deliberately)
- Ads: **No**

## Hosting the privacy policy

The repo is `github.com/nickpnevmati/Sudoku-Free`. If it's public, enable GitHub Pages
(Settings → Pages → deploy from branch, `/docs` folder) and the policy will be at
`https://nickpnevmati.github.io/Sudoku-Free/privacy-policy`. If it's private, paste the
policy into a GitHub Gist or a free Google Sites page instead.
