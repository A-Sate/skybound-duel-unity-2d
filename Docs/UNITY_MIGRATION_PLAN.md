# Skybound Duel Unity Migration Plan

## Purpose

Recreate the stable HTML prototype as a Unity project while preserving the feel of the current `v0.5.7 Stable HTML Blueprint`.

Reference sources:

- `index.html`: live playable prototype
- `archive/skybound_duel_v05_7_stable.html`: frozen migration snapshot
- `DESIGN.md`: gameplay and UI design notes
- `PROJECT_STATE.md`: current implementation state and workflow rules

The Unity version should not copy the one-file architecture. It should rebuild the same gameplay and UI behavior with Unity-native systems.

## Phase 1: Unity Foundation

Goals:

- Create a clean Unity project with scenes for Lobby, Match, and test/dev workflows.
- Set up deterministic 2D coordinate conventions for terrain, projectile motion, wind, and camera behavior.
- Define core data models for teams, players, vehicles, weapons, rules, match state, and turn state.
- Add placeholder art only.

Deliverables:

- `LobbyScene`
- `MatchScene`
- Shared game state service
- ScriptableObject definitions for vehicles, weapons, rules, and placeholder item/perk data
- A test harness for spawning the current 1v1 match

## Phase 2: Core Match Loop

Goals:

- Rebuild the current 1v1 playable slice first.
- Spawn the first blue player and first red player from lobby state.
- Preserve Human/Bot red-side selection.
- Implement team lives, KO, respawn, defeat, and reset flow.
- Implement turn order for alive units, prepared for future 4v4.

Acceptance criteria:

- A match can start from default Blue Human vs Red Bot setup.
- Blue and Red lives are independent.
- Turn List reflects the active unit order.
- PASS advances the turn.
- Escape or an equivalent UI action returns to the lobby.

## Phase 3: Artillery Physics And Weapons

Goals:

- Recreate local angle aiming, facing, power charging, wind influence, projectile motion, and projectile camera follow.
- Port current weapon tuning from the HTML `weapons` object.
- Preserve weapon-specific allowed local angle ranges.
- Preserve SS charge and firing restrictions.

Acceptance criteria:

- Attack 1, Attack 2, Attack 3, and SS match the prototype's broad feel.
- Wind visibly affects projectile paths.
- Bot shots use the same projectile, damage, and terrain systems as player shots.
- Local angle and world shot direction remain distinct.

## Phase 4: Terrain And Crater Destruction

Goals:

- Recreate floating terrain from a generated surface.
- Preserve elliptical crater destruction with the current 5:3 horizontal-to-vertical ratio.
- Keep base terrain and crater data separate.
- Implement terrain solidity checks, visible surface rendering, falling, void death, and respawn.

Implementation options:

- 2D mesh terrain with boolean/polygon crater subtraction.
- Texture or render-mask terrain with collision proxy updates.
- Hybrid visual mesh plus physics/collision sampler.

Acceptance criteria:

- Craters look clean and elliptical.
- Terrain holes affect movement, falling, projectile collision, and line rendering.
- Destroying terrain beneath a unit causes falling.
- Falling into the void consumes team lives and respawns when lives remain.

## Phase 5: Lobby UI

Goals:

- Rebuild the room-style lobby as Unity UI.
- Preserve the current layout hierarchy: room header, map preview, rules panel, two team columns, central Start Match/Ready area, item side panel, chat placeholder, and shortcut row.
- Preserve current functional behavior while keeping placeholders non-functional.

Controls to preserve:

- Add Player
- Human/Bot toggle
- Player name input
- Prototype vehicle selector placeholder
- Game Options panel with Blue Lives and Red Lives arrow controls plus confirm tick
- Start Match
- Placeholder Ready button with no match effect
- Dev Tool, Options, and Wiki shortcuts

Omissions to preserve:

- No sudden death
- No weather icons
- No buddy/friend list
- No copied logos or copyrighted assets

## Phase 6: Match HUD And Debug UI

Goals:

- Recreate the Gunbound-inspired bottom HUD.
- Preserve top wind dial, Turn List, weapon buttons, SS charge, PASS button, angle display, power gauge, move gauge, and team lives.
- Recreate the vehicle angle gauge above units.
- Recreate Dev Tool and Wiki equivalents as development/debug overlays.

Acceptance criteria:

- HUD information priority matches the HTML prototype.
- Vehicle angle gauge rotates with terrain slope.
- Gauge shows allowed local angle arc, current needle, and local angle number.
- Dev Tool exposes useful live values for balancing and debugging.

## Phase 7: Bot And Prediction

Goals:

- Rebuild the current red-side bot as the first AI slice.
- Preserve staged behavior: choose target, choose weapon, face target, aim, wait, charge, fire.
- Port prediction/search helpers enough to make bot shots plausible.

Acceptance criteria:

- Bot uses the same weapons and physics as human players.
- Bot remains scoped to the current red-side 1v1 flow until multiplayer/team AI is intentionally designed.

## Phase 8: Multiplayer-Ready Expansion

Goals:

- Extend systems toward the lobby's future 4v4 promise.
- Keep networking and authoritative simulation decisions explicit.
- Move chat from placeholder to real multiplayer only when networking exists.

Future work:

- Real vehicle selection
- Items and perks
- 4v4 spawning and turn order
- Online rooms
- Team chat
- Original art, animation, SFX, and music

## Migration Rules

- Preserve the HTML prototype as the feel reference.
- Do not treat current HTML structure as the Unity architecture.
- Keep gameplay data external and inspectable.
- Keep placeholder assets until original or approved assets exist.
- Do not add sudden death unless it is intentionally redesigned later.
- Validate each phase with a small playable build before adding the next layer.
