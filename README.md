# Gravity Field Demo

## Concept

Using `RigidBody` physics, we can use a few simple components to apply a force and/or orient bodies to a direction.

If we imagine "artificial gravity" as a 3d field of `Vector3`s, we can bend, twist, pinch, and otherwise manipulate "gravity".

These fields can be summed or combined in other ways to make a complex and tightly controlled game environment. Given the pieces, a non-technical game designer can mess with gravity and platforms, like an ordinary 3d platformer.

To gamify this, this demo has a mechanic where you can grab walls and free-floating objects with a "tractor beam" and then turn around and kick off to propel yourself to the next. Free floating objects get kicked back when you jump off of them.

## Controls

Facing a surface, hold `space` to engage your "tractor beam" and grab the wall. The reticle in the center of the screen will turn blue when a surface is in range. Turn around until the reticle turns green, indicating you're clear to kick off in that direction, then release `space` to kick off. If you are facing the wall or are at about a 90-degree angle from it, the "tractor beam" will not let you kick off -- you can't propel yourself orthogonally, and you can't propel yourself into the wall you're holding on to.

In progress: `WASD` keys let you walk if you are standing on a surface, not holding the tractor beam, and not tilted too much relative to the local gravity.

Press `Z` to respawn at the last checkpoint. Checkpoints are little blue squares, you start the level on one.