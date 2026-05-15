# Skybound Duel Project State

## Current Stable Version

Current stable milestone: `v0.5.7 Stable HTML Blueprint`.

Current in-game patch notes in `index.html` go through `v0.5.6.3`. The `v0.5.7` label is a documentation/archive milestone for Unity migration; it does not change gameplay logic or restyle the UI.

Recent major changes:

- `v0.5.5`: Replaced the simple main menu with a Gunbound-inspired lobby UI.
- `v0.5.5`: Added placeholder 4v4 lobby slot scaffolding while preserving current 1v1 gameplay.
- `v0.5.5`: Added lobby placeholders for Team, Items, Vehicle, Game Options, Online Multiplayer, Modifiers, and future chat.
- `v0.5.5.1`: Moved `Start Match` into the central lobby info panel and added a Game Options popup.
- `v0.5.5.2`: Moved Blue/Red Lives settings into the Game Options popup, added lobby DevTool/Wiki shortcut buttons, and polished the lobby options controls.
- `v0.5.6`: Restyled the lobby into a room-style layout with map preview, match info, team slots, chat, status panels, and a placeholder Ready button while preserving current 1v1 startup.
- `v0.5.6.1`: Made the Game Options lives arrows functional with confirm-to-apply behavior for separate blue and red team lives.
- `v0.5.6.2`: Polished Game Options lives row visuals so both arrows align and the confirm tick no longer overlaps Red Lives.
- `v0.5.6.3`: Moved lobby Dev Tool, Options, and Wiki shortcuts into the side panel, moved the side hint into the list area, removed the Vehicle rules row, and made the lobby rules lives summary show confirmed Red : Blue lives.
- `v0.5.7 Stable HTML Blueprint`: Captures the current HTML prototype as the Unity migration reference.

## Repository Structure

Current project layout:

```text
AGENTS.md
DESIGN.md
PROJECT_STATE.md
README.md
UNITY_MIGRATION_PLAN.md
index.html
package.json
package-lock.json
archive/
  skybound_duel_v05_7_stable.html
tests/
  smoke-lobby.mjs
node_modules/
```

The game itself is still intentionally a one-file prototype in `index.html`. CSS, HTML, JavaScript, data, patch notes, rendering, and gameplay logic all live in that file.

The archive copy at `archive/skybound_duel_v05_7_stable.html` is a stable snapshot for migration reference. Do not edit the archive casually; update it only when deliberately creating a new stable blueprint.

## How To Run The Game

Open `index.html` directly in a browser.

The game is also compatible with a temporary local static server, which is how the smoke test opens it.

## How To Run The Smoke Test

Preferred command on Windows:

```powershell
npm.cmd run test:smoke
```

Fallback if `npm.cmd` is not available:

```powershell
node tests/smoke-lobby.mjs
```

The smoke test starts a temporary local static server, opens `index.html` in Playwright Chromium, verifies the stable lobby flow, opens the Game Options popup for lives settings, checks the lobby shortcut buttons and lives summary, starts a match, and checks that the canvas, bottom HUD, PASS button, and Turn List are visible.

## Important AGENTS.md Rules

Future Codex threads should preserve these rules:

- Keep all in-game UI text in English.
- Keep chat-facing patch notes in English because they are copied into the in-game Wiki.
- Do not remove existing gameplay features unless explicitly requested.
- Preserve the current one-file prototype behavior unless the task is specifically a refactor.
- When making changes, update the in-game Patch Notes.
- When adding new variables or helpers, mention them in the patch notes.
- Do not add copyrighted assets directly to the repository.
- Use placeholder assets unless external asset URLs are explicitly provided.
- Keep the game playable by opening `index.html` in a browser.
- After changes to `index.html`, CSS, JavaScript, UI, gameplay logic, or `package.json`, run the smoke test.
- If the smoke test fails, report the failure and do not claim the task is complete.

## Current Gameplay Systems

Skybound Duel is a browser-based 2D turn-based artillery prototype.

The current playable match is still 1v1:

- First occupied blue lobby slot becomes the blue player.
- First occupied red lobby slot becomes the red player.
- Extra lobby slots are scaffolding for future 4v4 support and do not spawn in-game yet.
- The red slot can be toggled between Human and Bot.

Core gameplay systems:

- Turn-based artillery combat.
- Local angle aiming.
- Power charging.
- Weapon selection.
- Wind-influenced projectile motion.
- Destructible floating terrain.
- HP, shield, lives, respawn, and KO flow.
- Camera follow for active player and projectile.
- Manual camera panning.
- Basic bot turn planning and shot execution.

## Current UI Systems

Current UI includes:

- Lobby/main menu.
- Game Options popup.
- Placeholder chat box.
- Placeholder vehicle/status preview.
- Room-style map preview and match rules panel.
- Side item placeholder panel with bottom hint text.
- Top wind dial.
- Gunbound-style bottom HUD.
- Weapon buttons.
- SS charge bar.
- PASS button.
- Angle display.
- Power and move gauges.
- Team lives display.
- Turn List.
- Dev Tool panel.
- Wiki/Encyclopedia modal.
- Toast messages.

The floating Dev Tool and Wiki buttons are hidden while the lobby is active, but lobby-specific shortcut buttons can open those panels from the lobby. In the current lobby, Dev Tool, Options, and Wiki shortcut buttons sit in the bottom-right side panel row.

## Current Lobby / Main Menu State

The lobby is the main menu.

Current lobby behavior:

- Shows up to four blue slots and four red slots.
- Empty slots show `Add Player`.
- Occupied slots show placeholder player/vehicle art, name input, Human/Bot toggle, and Prototype vehicle selector.
- Default setup is Blue `Player 1` as Human and Red `Player 2` as Bot.
- `Start Match` remains functional in the central lobby controls.
- A separate `Ready` button is visible as a placeholder only and has no gameplay effect.
- The center lobby `Options` button opens Game Options.
- The bottom-right side panel shortcut row opens Dev Tool, audio/help Options, and Wiki.
- Game Options currently includes Game Mode, User, Map, Blue Lives, and Red Lives.
- Sudden Death options are intentionally omitted.
- Team Lives are functional through Blue Lives and Red Lives arrow controls in Game Options; the confirmation tick applies the selected values.
- The lobby rules panel shows Game Mode, Teams, Map, Items, and confirmed lives as `Red : Blue`.
- The lobby includes a map preview panel and a placeholder item side panel, but no weather icons, buddy/friend list, or copyrighted logo visuals.

## Vehicle And Weapon Data Overview

Vehicle data lives in `VEHICLE_DATABASE`.

Current functional vehicle:

- `prototype`
- Name: `Prototype`
- HP: 750
- Shield: 250
- Shield regeneration: 10% per turn
- Move speed: 100
- Max movement: 220
- Slope range: 45 degrees
- Standard attack, defense, explosion radius, and pit power modifiers
- `iconUrl` exists for future GitHub-hosted vehicle icons

Weapon data lives in `weapons`.

Current weapons:

- `atk1`: standard attack, higher damage, smaller terrain crater
- `atk2`: larger radius, lower damage
- `atk3`: placeholder alternate attack
- `ss`: special shot, larger projectile, larger radius, stronger terrain destruction

Each weapon defines damage, damage radius, terrain pit power, projectile size, color, and allowed angle range.

## Terrain / Crater / Destruction Behavior

Terrain is generated as a floating sinusoidal island.

Important terrain behavior:

- Terrain surface is sampled from `G.terrain`.
- Base terrain remains separate from crater data.
- Craters are stored in `G.craters`.
- Current crater shape is an ellipse with a 5:3 horizontal-to-vertical ratio.
- Terrain solidity checks account for craters.
- Rendering uses an offscreen terrain layer so crater cutouts do not erase or tint the sky.
- Visible terrain line rendering follows base terrain and visible crater edges.

## Movement, Slope, Falling, Wind, HP, And Shield

Movement:

- Active unit moves with `A` and `D`.
- Movement consumes move gauge.
- Units can face left or right.
- Movement is blocked while charging, falling, airborne, or movement-locked.

Slope:

- Units rotate with terrain slope.
- Active movement is blocked uphill beyond slope range.
- Units can slide down slopes steeper than their slope range.

Falling:

- Units fall if terrain is destroyed beneath them.
- Falling into the void causes KO.
- KO reduces team lives.
- Units respawn safely if lives remain.

Wind:

- Wind has direction and magnitude.
- Wind affects projectile velocity.
- Wind changes after a full player rotation when active returns to blue.

HP and shield:

- Damage hits shield before HP.
- Shields regenerate for all units at end of turn.
- Units gain SS from dealing or taking enemy damage.
- Units also gain passive SS at turn start/end flow.

## Bot Behavior

The bot is only used for the current red-side 1v1 flow.

Current bot behavior:

- Chooses an enemy target.
- Prefers SS when charged.
- Otherwise selects among Attack 1, 2, or 3.
- Uses prediction helpers to search angle and power candidates.
- Can prioritize direct-hit predictions.
- Faces the target, waits, aims, waits again, charges, and fires.
- Uses the same projectile, damage, and turn systems as the player.

Bot behavior should not be expanded into full multi-unit team AI until playable 4v4 is intentionally implemented.

## Dev Tool And Wiki Behavior

Dev Tool:

- Can be opened in-match and from the lobby.
- Shows match state, mouse position, global params, vehicle angle gauge settings, wind, projectile, player stats, and camera state.
- Includes toggles for projectile prediction, hitboxes, and vehicle angle gauge.
- Includes cheat buttons for SS, HP, shield, wind, and move gauge.
- Supports Off, 1 s, and LIVE refresh modes.

Wiki:

- Can be opened in-match and from the lobby.
- Includes Vehicles, Items, Perks, and Patch Notes.
- Vehicle page shows Prototype stats and attack values.
- Items and Perks are placeholder categories.
- Patch Notes render from `PATCH_NOTES`.

## Design Decisions To Preserve

Preserve these decisions unless explicitly asked otherwise:

- One-file playable prototype in `index.html`.
- English in-game UI text.
- Patch notes in English.
- Gunbound-inspired lobby and HUD direction.
- Floating terrain with elliptical crater destruction.
- Current 1v1 gameplay despite 4v4 lobby scaffolding.
- No sudden death feature for now.
- Placeholder-only art and icons unless approved external asset URLs are provided.
- Vehicle data should remain easy to move into external files later.
- Future support for online multiplayer should be prepared but not faked as functional.
- Combat gameplay should not be changed during UI-only tasks.
- Smoke test should be run after UI/game/package changes.

## Known Limitations And Planned Future Work

Known limitations:

- Only two units spawn in the actual match.
- Extra lobby slots are visual scaffolding only.
- Only the Prototype vehicle is functional.
- Items and perks are not implemented.
- Online multiplayer is not implemented.
- Chatbox is a placeholder.
- Vehicle selector is a placeholder.
- Lobby Team, Items, Vehicle, Online, and Mods buttons are placeholders.
- No sudden death logic by design.
- README current version may lag behind in-game patch notes unless explicitly updated.
- Some legacy one-file prototype text and mojibake may still exist in hidden or older patch-note strings.

Planned future work:

- Use `UNITY_MIGRATION_PLAN.md` as the phased Unity rebuild guide.
- Preserve the HTML prototype as the v0.5.7 blueprint while Unity work begins separately.
- Move vehicle and weapon data into Unity ScriptableObjects or equivalent structured assets.
- Add real vehicle selection.
- Add items and perks.
- Add proper team/multiplayer support.
- Add original or approved external visual/audio assets.

## Recommended Workflow For Future Changes

1. Read `AGENTS.md`, `PROJECT_STATE.md`, and the relevant sections of `index.html`.
2. Determine whether the task is UI-only, gameplay, data, tooling, or docs.
3. Keep edits scoped to the request.
4. Preserve one-file prototype behavior unless the user explicitly asks for refactor.
5. Update in-game Patch Notes for any gameplay/UI/code change.
6. Mention new variables or helpers in Patch Notes when added.
7. Run the smoke test after changes to `index.html`, CSS, JavaScript, UI, gameplay logic, or `package.json`.
8. If `npm.cmd run test:smoke` is unavailable, run `node tests/smoke-lobby.mjs`.
9. Report the exact smoke test result.
10. For Unity migration work, consult `UNITY_MIGRATION_PLAN.md` before changing the HTML prototype.
11. Do not claim completion if the smoke test fails.
