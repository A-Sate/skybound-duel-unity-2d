# Skybound Duel Design Notes

## Project Overview

Skybound Duel is a browser-based 2D turn-based artillery game prototype inspired by classic Gunbound-style matches. The current stable blueprint is `v0.5.7 Stable HTML Blueprint`, a one-file HTML prototype in `index.html` with embedded CSS, HTML, and JavaScript.

The prototype focuses on fast iteration over core gameplay feel: room-style lobby setup, two-team artillery combat, floating terrain, destructible craters, turn order, projectile physics, wind, simple bot play, vehicle angle readability, and a Gunbound-inspired HUD.

## Current Gameplay

The playable match is currently 1v1. The lobby shows future 4v4 team scaffolding, but the match starts with the first occupied blue slot and the first occupied red slot only.

The default lobby setup is:

- Blue team: `Player 1`, human controlled
- Red team: `Player 2`, bot controlled
- Vehicle: `Prototype`
- Rules: basic 1v1 prototype match
- Team lives: separate Blue Lives and Red Lives values confirmed through Game Options

The red player can be toggled between Human and Bot in the lobby. If the red slot is set to Bot, the existing bot flow controls the red unit.

Each turn lets the active unit move, aim, select a weapon, charge power, fire, or pass. After a shot resolves, the game advances to the next unit. Shield regeneration and SS charge happen through the existing turn and damage systems.

The current playable slice should be treated as the first Unity target: one blue human unit versus one red human or bot unit, using the first occupied lobby slot on each side.

## Controls

- `A` / `D`: move left or right
- `W` / `S`: adjust firing angle
- Hold `J`: charge shot power
- Release `J`: fire
- `1` / `2` / `3` / `4`: select Attack 1, Attack 2, Attack 3, or SS
- `PASS`: skip the current turn
- Left mouse drag: move camera
- Arrow keys: move camera
- `R`: reset the match
- `Escape`: return to the lobby/menu

## UI Structure

The main menu has been replaced by a lobby-style UI:

- Room-style header and match setup layout
- Map preview panel without weather controls
- Match information panel for game mode, teams, map, items, and confirmed lives
- Left and right team columns with up to four placeholder slots per side
- `Add Player` controls for empty slots
- Human/Bot toggle per occupied slot
- Name input near each player slot
- Prototype vehicle selector placeholder
- Separate Blue Lives and Red Lives arrow controls in Options
- Lobby rules lives summary shown as `Red : Blue`
- Functional `Start Match` button above a placeholder `Ready` button
- Placeholder buttons for Team, Items, Vehicle, Options, Online, and Mods
- Bottom-right lobby shortcut row for Dev Tool, audio/help Options, and Wiki
- Placeholder chat area for future online multiplayer
- Placeholder vehicle/status preview panel
- Placeholder item side panel with bottom hint text and no buddy or friend-list features

The lobby intentionally omits sudden death, weather icons, friend/buddy lists, copied logos, and copyrighted visual assets. Icons remain placeholders.

During the match, the main UI includes:

- Top wind dial
- Gunbound-style bottom HUD
- Weapon buttons for attacks 1, 2, 3, and SS
- SS charge bar
- PASS button
- Angle display
- Power bar and power marker
- Move gauge
- Team lives display
- Turn List showing alive units in turn order
- Dev Tools panel
- Wiki/Encyclopedia modal with vehicles, items, perks, and patch notes

The HUD should be preserved as a Unity reference for layout and information priority: weapon selection on the left, angle readout, power/move gauges, team lives, and turn list visibility.

## Vehicle Angle Gauge

The vehicle angle gauge is a world-space visual above each unit. It rotates with the vehicle's terrain slope and shows:

- Grey semicircular annulus/ring background
- Green allowed local-angle arc for the selected weapon
- Red current-angle needle
- Readable local angle number
- Lower opacity for inactive units

The gauge uses local firing angle, not absolute world shot angle. The bottom HUD angle display also reflects local angle. This distinction is important for the Unity migration because terrain slope and facing direction affect world shot direction.

## Terrain and Camera

The stage is a floating 2D terrain island generated from a sinusoidal height curve. Terrain is queried through ground and slope helpers rather than tile data.

Terrain destruction uses stored crater data. Craters are elliptical, with the current gold-standard shape using a 5:3 horizontal-to-vertical axis ratio. Rendering uses an offscreen terrain layer so crater cutouts do not erase or tint the sky.

Units rotate with the terrain slope. Movement is restricted by each vehicle's slope range. Units can fall after terrain is destroyed beneath them, and falling into the void causes a KO/respawn flow or team defeat if lives are exhausted.

The camera can follow the active player, follow projectiles after a short delay, or be manually panned with mouse drag or arrow keys.

Movement details to preserve:

- Active movement consumes each unit's move gauge.
- Movement is blocked while charging, falling, airborne, or movement-locked.
- Active uphill movement is blocked beyond the vehicle slope range.
- Units can slide or fall when terrain under them changes.
- Falling into the void reduces team lives, then respawns the unit if lives remain.

Terrain details to preserve:

- Base terrain and crater data are separate.
- Solidity checks must account for crater holes.
- Surface line rendering follows both base terrain and visible crater edges.
- Crater destruction should feel elliptical and clean rather than circular or tile-based.

## Vehicles

Vehicle data currently lives in `VEHICLE_DATABASE` and is intentionally shaped so it can be moved to external data files later.

Only one functional vehicle exists:

- `Prototype`
- 750 HP
- 250 Shield
- 10% shield regeneration per turn
- Medium movement
- Standard attack, defense, explosion, pit power, and slope values
- Placeholder visual model rendered with canvas/CSS shapes

The lobby and Wiki include placeholder vehicle presentation. Future builds are expected to support real vehicle icons, sprites, sounds, and music through local placeholder assets or approved external URLs.

## Weapons

Weapons are defined in the `weapons` object:

- `atk1`: standard shot with moderate damage and crater size
- `atk2`: larger radius shot with lower damage
- `atk3`: placeholder alternate shot
- `ss`: special shot with larger projectile, damage, blast radius, and terrain destruction

Each weapon defines damage, damage radius, terrain pit power, projectile size, color, and allowed local firing angle range.

SS charge is gained passively and through damage interactions. SS can only be fired when fully charged, then resets back to Attack 1.

## Dev Tools and Wiki

Dev Tools expose live prototype values for balancing and debugging, including:

- Prediction toggle
- Player hitbox toggle
- Vehicle angle gauge toggle
- Cheat buttons for SS, HP, shield, wind, and move gauge
- Live match, player, projectile, wind, camera, and parameter readouts

The Wiki/Encyclopedia is in-game documentation for:

- Vehicles
- Items
- Perks
- Patch Notes

Items and perks are placeholder categories. Patch Notes are stored in `PATCH_NOTES` and rendered into the Wiki.

## Testing

The project includes a Playwright smoke test at `tests/smoke-lobby.mjs`.

Run it with:

```powershell
npm.cmd run test:smoke
```

If `npm.cmd` is not available, run:

```powershell
node tests/smoke-lobby.mjs
```

The smoke test starts a temporary local static server, opens `index.html` in Chromium, verifies the stable lobby flow, opens Game Options, confirms team lives, verifies the lobby shortcut buttons, starts a match, and checks that the canvas, bottom HUD, PASS button, and Turn List are visible.

## Planned Unity Migration

The HTML prototype is intended as a design and feel prototype before a future Unity implementation. `archive/skybound_duel_v05_7_stable.html` is the frozen reference copy for this migration milestone.

Recommended migration direction:

- Treat `index.html` and the archive copy as references for gameplay feel, UI hierarchy, and tuning, not as final architecture.
- Move vehicle, weapon, item, perk, and rules data into structured Unity data assets, likely ScriptableObjects.
- Rebuild artillery physics with deterministic simulation suitable for local and online multiplayer.
- Recreate floating terrain and crater destruction with Unity-friendly terrain mesh, polygon, or texture-mask systems.
- Preserve the Gunbound-inspired lobby, HUD, turn list, wind display, vehicle angle gauge, and current 1v1 game loop as the first Unity slice.
- Keep placeholder art until original or approved external assets are available.
- Design multiplayer rules around future 4v4 support, while preserving the current 1v1 prototype behavior as the first playable slice.
- Use `UNITY_MIGRATION_PLAN.md` as the phased implementation checklist.
