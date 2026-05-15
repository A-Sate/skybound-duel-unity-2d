# AGENTS.md

Guidance for agents working in this Unity project.

## Project Identity

This repository is the active Unity recreation of Skybound Duel.

The earlier Unity 3D / 2.5D foundation was an experiment only. Do not continue that direction unless the user explicitly asks for it.

Build this project as a Unity 2D Core, sprite-based side-view turn-based artillery game.

## Technical Direction

- Use Unity 6000.4.6f1 for verification.
- Use Unity 6000.4.6f1 for batchmode verification.
- Do not use Unity 6000.3.15f1 for this project anymore.
- If Unity 6000.4.6f1 is unavailable, ask before running Unity batchmode.
- Keep the camera orthographic.
- Use 2D side-view gameplay.
- Use sprite-based presentation for vehicles, projectiles, effects, and in-world visuals.
- Use UI elements for HUD, menus, turn indicators, weapon selection, and other interface surfaces.
- Sprites may visually look pseudo-3D, but do not use real 3D models, 3D lighting, or 2.5D scene construction unless explicitly requested.
- Prefer placeholder sprites first. Replace them with final art only when requested or when a task specifically includes asset polish.

## Development Rules

- Do not implement the full game at once.
- Prefer small, testable steps.
- Keep logic modular and separate from visual presentation.
- Keep naming clear and in English.
- Explain how to test every implemented feature in Unity.
- Do not add online multiplayer yet.
- Do not add destructible terrain yet unless explicitly requested.
- Do not change gameplay unless the current task explicitly asks for gameplay changes.

## Architecture Preferences

Separate gameplay state, turn flow, data, controller logic, and visual representation.

Use these foundation concepts unless the user asks for a different structure:

- `GameManager`: high-level game state and scene-level orchestration.
- `TurnManager`: turn order, active unit selection, and turn transitions.
- `UnitController`: unit gameplay behavior and input-facing actions.
- `UnitView`: sprite and presentation updates for a unit.
- `VehicleData`: ScriptableObject for vehicle configuration.
- `WeaponData`: ScriptableObject for weapon configuration.
- `ProjectileController`: projectile movement and hit handling.
- `WindManager`: wind value/state and wind queries used by projectile logic.

Data rules:

- Use ScriptableObjects for `VehicleData` and `WeaponData`.
- Keep tunable values in data assets where reasonable.
- Avoid hard-coding gameplay constants into MonoBehaviours when they belong in data.

Presentation rules:

- Use `SpriteRenderer` for units and projectiles.
- Keep sprite selection, animation hooks, facing, and visual state in view/presentation classes.
- Keep simulation and turn logic independent from sprite implementation where practical.

## First Goal

The current first milestone is a clean 2D Unity foundation containing:

- `GameManager`
- `TurnManager`
- `UnitController`
- `UnitView`
- `WeaponData`
- `VehicleData`
- `ProjectileController`
- `WindManager`
- A simple 2D test scene

For this milestone, keep the scope narrow. Create enough structure to verify the foundation in Unity, but do not build the full Skybound Duel gameplay loop.

## Scene Guidance

For the initial 2D test scene:

- Use an orthographic camera.
- Use simple placeholder sprites or basic generated sprite assets.
- Include only enough scene setup to verify unit display, turn ownership, projectile spawning/movement, and wind state when those features are implemented.
- Keep scene objects clearly named.
- Keep scene hierarchy simple.

## Testing Expectations

Every implemented feature should include a short Unity test note in the response or relevant documentation. Include:

- Which Unity version was used.
- Which scene to open.
- Which objects or assets to inspect.
- Whether to enter Play Mode.
- What visible result or Console output confirms success.

When possible, verify by opening the project in Unity 6000.4.6f1 in batch mode or through the editor. Do not use Unity 6000.3.15f1 for this project anymore. If Unity 6000.4.6f1 is unavailable, ask before running Unity batchmode. If Unity verification cannot be run, clearly say so and explain what was checked instead.

## Repository Hygiene

Do not commit generated Unity or local environment folders, including:

- `Library/`
- `Logs/`
- `Temp/`
- `Obj/`
- `Builds/`
- `UserSettings/`

Also avoid committing generated solution/project files, editor caches, build outputs, crash logs, and temporary artifacts unless the user explicitly asks for them.

Respect existing `.gitignore` rules.

## Agent Workflow

Before changing code or assets:

- Inspect the existing project structure.
- Prefer existing folders and naming patterns.
- Keep edits scoped to the requested feature.

When adding scripts:

- Place runtime scripts under a clear `Assets/Scripts/` structure.
- Use namespaces only if the project has already established one, or if the user asks for one.
- Keep classes small and focused.
- Avoid premature abstraction.

When adding assets:

- Prefer placeholder assets at first.
- Keep generated or temporary assets organized.
- Do not introduce large binary assets unless the task requires them.

When finishing work:

- Summarize what changed.
- Explain how to test it in Unity.
- Mention any verification that could not be completed.
