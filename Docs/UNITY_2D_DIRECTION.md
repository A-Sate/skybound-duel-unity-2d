# Unity 2D Direction

This Unity project is intentionally sprite-based 2D. It is not a continuation of the earlier 3D / 2.5D model-based experiment.

Skybound Duel should be built as a 2D side-view turn-based artillery game using Unity 2D Core conventions and sprite presentation.

## Core Direction

- Keep gameplay on a 2D side-view plane.
- Use an orthographic camera.
- Use `SpriteRenderer` for vehicles and projectiles.
- Use sprites or UI elements for effects, HUD, menus, and other visual feedback.
- Pseudo-3D drawn sprites are allowed when they support the art style.
- Do not use real 3D models unless explicitly requested.
- Do not use 3D lighting unless explicitly requested.

## Sprite-Based Presentation

Vehicles, projectiles, and effects should be represented with sprites first. Placeholder sprites are preferred during early implementation so gameplay systems can be tested before final art exists.

Sprites may be drawn with depth, shading, perspective hints, or pseudo-3D silhouettes, but they should remain 2D assets rendered in a 2D scene.

## Gameplay And Visual Separation

Keep gameplay logic separate from visual sprite presentation.

Gameplay scripts should own state and behavior such as:

- Turn flow
- Unit ownership
- Weapon data
- Projectile simulation
- Wind values
- Damage or hit resolution when implemented

Visual scripts should own presentation details such as:

- Assigned sprites
- Facing direction
- Animation hooks
- Visual state changes
- UI updates

This separation keeps the game testable and makes it easier to replace placeholder sprites with final art later.

## What To Avoid

Unless explicitly requested, do not add:

- Real 3D vehicle or projectile models
- 3D lighting setups
- 2.5D camera staging
- Perspective camera gameplay
- Model-based animation pipelines
- 3D physics for core artillery gameplay

The target foundation is a clean, readable 2D Unity project with modular gameplay logic and sprite-based presentation.
