# DEV LOG

## Day 1, 2025/8/1

Got everything up from scratch. DOTS will be a good idea later but it's faster to prototype with familiar GameObjects.

Maybe should use abstract base class for eg IVectorField, so that it's easier to pick in the Editor? I wish there was a way to say "I want an object that has a RigidBody" when
writing a MonoBehaviour's fields.

Singleton might be okay if you really only have one field in the scene, or if you're careful, consistently use a single `SummedVectorField` though in case you want to switch types etc.
However, maybe Red Cubes use the Red Field and Blue Cubes use the Blue Field and so on, that's easy enough to do in the Editor as-is.

RigidBody physics is always tough to tune. Gravity feels floaty or not. My "artificial gravity" acts slower on higher-mass RBs, I think.

## Overview of the new parts today

I ought to learn docstrings convention for MonoBehaviours, but until then I'll outline them in docs like this.

`AirDrag` was useless. RigidBody has dampening which does what I wanted, stabilizing movement.

### The vector fields

`VectorFieldFollower` goes on a RigidBody, then it will follow the influence of a vector field's "gravity".

`ControlPointVectorField` uses a list of plain GameObjects which serve as control points and have an influence over the field,
the transform holds all the data: the forward (z+) is its direction, and its local z scale is its magnitude.
The `ControlPoint` prefab simply has a helpful gizmo on it, making a green cube and a yellow line that points in the control point's direction.
3 Modes:
  1. Add with Gaussian Decay. I think this is most useful, though it's the most computationally expensive. It has a soft Gaussian falloff, so you can control it well.
  Here, "Add" means that control point influences are simply added together. If you duplicate one in-place for example, it would double the forces there. Opposing ones cancel out.
  2. Add with Sharp decay. This is like radioactive decay I think, it's a very steep falloff, and if you turn down the decay factor it is either too soft or too sharp.
  Hard to use. The Gaussian is better, I arrived at that one after being stuck on this.
  3. Interpolate. This doesn't add, so duplicating a control point in-place would have no effect. It's very smooth, but I found it a little strange because unless you just want 
  to flow from a floor to a wall etc, it's kind of too smooth. I wanted a more dramatic "polarization", but Interpolate is very good for smooth places. A drawback is that it
  has no falloff (currently -- could be implemented!) so if you have 2 or 3 control points in the middle of the room, their effect is equal at the far corners of the room.

`EquationVectorField` uses an equation to define a vector field. (https://anvaka.github.io/fieldplay is a cool place to play with vector spaces in 2d. 
Try "randomize" to discover some neat ones. They're only 2D, but give you some ideas.) The equations can do anything: rotations, attractors to a plane or point, repulsion, figure 8 flow, 
etc. They're not flexible, but they can be cool to combine. However as I discovered, things without a central position and falloff "pollute" the scene.
1. Static - useful if you want a "default" direction that other added fields are combined against. I think it uses the Z+ of the transform of the object it's on. Single direction, uniform.
2. Squish - unbounded attractor to a single point. Polluted the scene, which inspired...
3. LocalSquish - bounded attractor with falloff (log decay). (NOTE: could be Gaussian decay instead, might work more nicely.. but an extra Math.pow)
4. Twirl - I think it's unbounded? It's rotational in the horizontal plane
5. Flush - I think it's Twirl with conventional gravity added in -Y

`StaticVectorField` I think is the same as `EquationVectorField` with the Static mode? It was the first one I made, to get things moving. I think it is redundant now.

`SummedVectorField` takes a list of `IVectorField` objects and simply adds them. It's unnecessary to weigh each field by some scalar bc all the field components have a way to scale 
their forces already built-in, so I didn't have to add that feature. Summing the fields works great for distance-bounded fields, but ones like Squish/Twirl/Flush have an 
unbounded influence (I think they get incredibly strong the further away from their centers you are!) so they are not good for additive combining.

`VectorFieldVisualizerGizmo` lets you see what you're doing by having a 3D grid of gizmos that show you the direction and magnitude at many points across the field. I made the gizmo 
colors match XYZ components to RGB with a simple mapping, it really helps visualize the flows across the vector field!

#### Next steps with vector fields

They're still a bit hard to control, though ControlPointVectorField is good, and adds well with a static field.

Gravity is usually either planar (standing on the ground) or radial (Mario Galaxy planet hopping) or maybe "cylindrical / beveled corner" if you walk over a bevelled right angle 
edge/corner and are still standing up. LocalSquish is good for radial, and Static is good for planar, but only, and maybe "twist" (the interpolate is great with that, 
like a spiralled hallway).

I think there's promise of finding better ways to combine them.
- An unbounded one like Flush could be multiplied by a 3d scalar field, eg linear distance up to a max distance from a point, where at the center of the fuzzy sphere it's 1.0 and it
fades out to 0.0. That would effectively bound anything!
- "Nearest neighbor", which makes most sense for ControlPointVectorField. That would allow you to be very planar around corners etc. It could also have a "Soft Nearest Neighbor" where
in a certain threshold (I have 2 neighbors both near 50% total distance, or 3 neighbors all near 33% distance, 4x near 25%, etc) then it's interpolated over that squished range.
- Bounded static could be useful. A circle and a cube. Might be useful with sharp edges, or I could taper it.

### The player controller / pushing mechanism

`PointToVectorDirection` applies a righting force to an object (I only have it on the Player currently) so that the object aligns with the vector field. This allows the Player to 
generally/eventually stay on their feet. If you wanted them head-first, you could flip it, and flip it back with the checkbox. It's also written so you can align any axis with or
against the vector field direction at our given object's position, so instead of Y (up) it could face the vector force in Z (forward), etc etc.
- I initially wrote this setting the rotation with a lerp, but lerps apply a lot of force during collisions esp when resting on a surface and it made the physics very unstable. So far,
the force-based rewrite I have now seems stable. You have to tune it carefully so it doesn't wobble like a buoyant object: with insufficient dampening, it will overshoot like a pendulum
as rotational inertia carries it past the target point.
- It handles 180deg turns poorly, it gets "jammed" bc of the math there. Comments link to a SO article for further refinement.

`RotationController` controls the camera and player rotation. It's maybe an unusual setup, the left/right rotates the player object (unless they're visible in a mirror or something) 
and up/down tilts the camera in a clamped range.

`PushOffCollidersEffector` depends on the player object having a collision trigger (on a child) that's bigger than its physics collider. We track what colliders are currently
inside the trigger collider (TODO: somehow the player RB collider is never detected, I just realized this, hmm, maybe should have safeguard), and when the player hits "Jump" (spacebar)
we send a ray at each of the colliders in range (AFAIK, faster and less accidentally-blockable that a ray against everything in the scene) and the closest ray that hits triggers an 
almost-equal-and-opposite force being applied to rigidbody target (if any, fixed colliders have none) and the player. It seems like it's working okay, it's just impossible to aim your 
jump.
Since the vector field in my demo playground is smooth and continuous, PointToVectorDirection gives a nice gradual movement and it's not uncomfortably disorienting to land on a wall.

#### Next steps with player control

- Need to detect standing on a surface and allow walking (maybe walking applies a slight force normal to the surface for "moon bounce"? And high-gravity surface would be rapid short 
hop-hop-hops)
- I want to have a system like, right click to set a target jump vector, left click to kick off. Cancel it if you have too small of an angle between the kickoff direction and the jump 
target -- so you can't jump 90 degrees or less, that wouldn't make sense. Make a little visual for the jump direction, maybe start with a straight cylinder and then make it segments
that follow the vector field as a target prediction. Make crosshairs HUD so you know exactly what you're kicking off of.

#### Summary

The vector field idea is promising. I got much farther today than I expected, after 2 years since I last used Unity, I'm surprised! Visualizing the fields is so important, and if
designing puzzles/platforms there are probably other gizmo ideas that would be very useful in the Editor.

It is still a physics sandbox and the player is not maneuverable in any practical way. That's probably the priority.

The vector field controls need more refinement for a platformer. Bounded and nearest-neighbor ideas for better control seem like a good approach.

Also, I'd like to get some particles going under the influence of the vector field. I think little smoke and floating wires and debris would indicate the field in a helpful and
immersive way. Unity implements "force fields" for particle systems with a 3D texture. I wonder if I could bake my vector field component to a 3D texture and just do it like that. 
It's not dynamic though, if the field changed dynamically you'd have to re-bake over each frame (or Nth frame to cheat) that the field is changing. However maybe that's not too 
expensive, my widget samples the field `X*Y*Z` times every frame in the Editor. Maybe I could also just make sprites with a `VectorFieldFollower` and a non-collision `RigidBody`. 
I wonder if there's an approachable way to do a dynamic particle force field in DOTS.

Ultimately I do want to implement all the vector field stuff in DOTS, I think an ECS could represent this well. But I have to do some DOTS tutorials and read the docs further.

## Day 2, 2025/8/2

Only had the evening. Got the player push-off mechanism working!

It took some thinking to understand the spring anchors, the trick was not using the automatic anchors and setting them with the ray cast. I needed the anchors' positions to find the direction of the spring, to limit the player so they can't jump towards the wall they're holding onto, but only away and orthogonal to it. It's also tricky bc if the spring is connected to another RigidBody it uses local coords, and if it's connected to a wall etc it uses world coords. I drew a bunch of rays to figure it out, and read the docs over and over again. I think what threw me off was trusting that the auto-configure was doing something smart, not finding clear info about it, and having a hard time understanding it before realizing it's better to just use the raycast hit to set the anchor manually.

The state management in `PushOffCollidersEffector` is rocky, it would be nice to use a cleaner state machine pattern.

It took a while to learn how to make the `PushOffCollidersEffector` send events via a singleton event manager for the UI Toolkit UI (via `HUDCrosshair ` component). That was all after I already got the mechanism working, but it was worth pushing for that "nice to have" because it makes the game mechanic make so much more sense instead of guessing why the jump only works at certain angles, and trying to anticipate distances being in or out of range. And having a crosshair/reticle makes a big difference in aiming at things.

Next priority is making a better demo, making a little level that makes more sense, tuning the masses of cubes so they push each other, and maybe refining the vector field controls so I can get it to do what I want.

Then, upload it to GitHub and make an overview video.

Git LFS was easy to set up, I wonder how much space this 1.2GB minimal project takes up on GitHub, if there's a lot of compression or not.

Nice to have: I noticed that because the camera is outside the player collider box, the player at some angles will look through walls. That's bad!

It would also be cool to add walking controls, detect being on the ground. It's tricky to walk in a physics sim if you're not kinematic. PID??

The ropes are glitchy, since they swirl around so much and are narrow the restriction on launch angle for kickoff feels weird. Oh well, not important, that's something that could be tuned in a real game.

## Day 3, 2025/08/03

Got a new, cleaner scene. I watched a video about the design of recent Mario games, how they have a gimmick mechanic each level, and the level design introduces the concept in a safe way, then develops it and makes it challenging with higher stakes, then adds a final twist showing a new side of the mechanic. Also, the placement of the first mushroom in Mario has it try to hit you and puts a block where most new players would try to jump so you can't escape it, then you learn it's good and not bad like the similar mushroom Goombas. I thought that would be a good way to show off this little game, in stages:

1. Pushoff basics in uniform gravity. Learn that you have to face away from the wall to jump.
2. Mario Galaxy like planet hopping. Local squish.
	- Twist: you can hop around cubes etc to the other side.
3. Tilted gravity basics. Learn that gravity can point in different directions, like a floor to a wall. ControlPointVectorField, but I need to make a limiter for it.
4. Opposed force basics (zero gravity). Pushing off a floating cube will push the cube away. You need to go out down a path of cubes, flip a switch, and come back.
5. Basic puzzle: maybe some levers move control points around (visualized by some sci-fi cylinder thing and floating no-collision little boxes that are constantly spawned from a "vent")

I got the respawn/checkpoint system working, which was challenging because it's objects across scenes and there's a few different ways to do it. I tried to use events as much as possible, and the Player tag is useful too so that objects can respond to the Player collisions, target the Player etc, though the Player is in the `CommonScene` and those objects are in the levels.
- For example, holding the tractor and restarting starts you off holding it! Needs to respond to respawn.

Need a robust reset mechanism. Maybe write an initial state for everything that has a reset method, but that's easy to miss. Maybe can clone a scene and activate the next one, then delete the old one and load a new one. Maybe I should give up for now and just hard reset everything but the last activated checkpoint.

The tractor spring still needs tuning.
- The player rolls too much when you grab on a wall -- maybe more dampening?
- In uniform gravity, it's hard to aim "up" because even at its tightest the spring is at a 45deg angle as you hang down. Think about how to fix that angle measurement.

I wonder if a fixed joint would be better and more controllable?
- Tried it: it goes crazy because the mouse-look tries to turn, the vector field follower and point to vector direction components add forces, and the fixed joint is not happy. But maybe it's because I'm making the anchor points too close and they're colliding with each other (the player and the wall held by the tractor beam)

Also, it would add a lot to be able to walk!

## Day 4, 2025/08/04

Fixed some bugs with tractor. Got basic walking mechanic, but it's fragile. I think it really needs a PID. It's sensitive to gravity: more gravity = more friction, which makes the player stuck. I think the PID would be in common with the vector follower. The player could have a more expensive PID vector follower that could be adapted for walking (where to apply the force and how much) while `VectorFieldFollower` simply is the P of PID I think, and is cheaper computationally and sufficient for most other things in the game.

Also added simple "particle" pool of plain old cubes, that follows the player around. It is useful for seeing gravity distortions.

Started working with collision layers, so the Player doesn't collide with self, and so things like the `FloorDetector` can exclude "player triggers"" like the checkpoint trigger box.

I realized there's a `RaycastHit.normal`, that would probably work much better than my convoluted "tractor normal". Ought to switch it over later.

I think the big problems left are:
- Resetting quickly (which might require nothing more involved than reloading the scene! That's def worth trying before I continue to pursue the "reloadable" path)
	- I think if you reset while holding tractor, it remains engaged. And camera tilt is preserved, etc. Pretty much anything holding state needs to reset it, which is difficult in OOP where state is everywhere.
- Walking, as discussed above
- I still wish for the "set initial data values, dispatch actions to update the data, read the data with selectors/lenses". Some data is local but most time the processing would be the same everywhere, eg there's 4 pushoff enums in the payload of that action, 2 mean tractor engaged, 2 mean tractor disengaged, so all listeners who care need to duplicate this logic. And there's no way to know the initial state, it's written locally each time.
- Still could use more vector field control. For example, `LocalSquish` is useful but the player can't stand up on a non-spherical platform, it would be nice to have a "soft edges" uniform field and a "rectangular squish" equation.

Also, to really show off the system, I want to add "opposed force basics" pushing off blocks, wall kicking to move a big block, and a final puzzle that moves the fields around with switches.
