# 👀 GazeFlow – Mosaic of Attention

A calm XR eye‑tracking experience that turns scattered glances into a living mosaic of light, exploring how our attention shapes virtual space in real time.



https://github.com/user-attachments/assets/12852ec9-adb7-4423-9744-9ef938f13a1a


---

## Overview

Modern digital life teaches kids to stare at flat, glowing rectangles from one fixed distance. That's convenient for devices, but it encourages static focus, limited eye movement, and visual fatigue instead of rich, exploratory seeing. GazeFlow flips that pattern: what if our digital tools invited eyes to roam, rewarded healthy movement, and turned attention itself into the main way you interact?

In this hackathon prototype, we build a VR mosaic of floating tiles around the user. Each tile quietly waits in the dark until it's noticed. When you look at a tile, it brightens, grows, and locks in as part of a larger pattern. Over a short session, your tiny micro‑movements—where you look, for how long, and in what sequence—assemble into a glowing, personalized mosaic.

We're inspired by work like [Luminopia](https://www.luminopia.com/) and other VR‑based vision therapies that guide vision intentionally rather than just cutting screen time, but GazeFlow is a hackathon‑scale exploration, not a medical device. Our goal is to prototype interaction patterns and logging tools that make it easier to design healthier, gaze‑aware experiences in the future.

---

## Team

### Team Name: PUJLY GazeFlow  
"Do you see the vision"

<img width="512" height="384" alt="image" src="https://github.com/user-attachments/assets/85998768-4aae-4ab8-abf4-3b1e29d63aeb" />

- **Rabiat Sadiq – Developer**  
  Email: [rabiats@andrew.cmu.edu](mailto:rabiats@andrew.cmu.edu)  
  GitHub: [https://github.com/RabiatS](https://github.com/RabiatS)

- **Yukti Poddar – Product Designer**  
  Email: [Ypoddar@andrew.cmu.edu](mailto:Ypoddar@andrew.cmu.edu)  
  GitHub: [https://github.com/Yuktipoddar](https://github.com/Yuktipoddar)

- **Uma Dhamija – UI/UX Designer**  
  Email: [urd@andrew.cmu.edu](mailto:urd@andrew.cmu.edu)  
  GitHub: [https://github.com/Umaracheldhamija](https://github.com/Umaracheldhamija)

- **Priyal Shrivastava – Developer**  
  Email: [pshriva2@andrew.cmu.edu](mailto:pshriva2@andrew.cmu.edu)

---

## Vision

Because so much of children's visual world now happens on 2D screens, they often don't experience the full range of oculomotor behavior—shifting depth, convergence, smooth tracking, and exploratory scanning—that their visual systems evolved for. Our goal isn't to eliminate screens; that ship has sailed. It's to **design digital experiences that work with, rather than against, healthy visual engagement**.

This iteration of GazeFlow explores how immersive XR can:

- Turn attention into a **first‑class input**, where looking is how you interact.  
- Gently **encourage eye movement** across the scene using curiosity and visual rewards instead of instructions.  
- Measure **where attention lands and how long it stays there**, building up a mosaic of gaze "fragments" over a short session.  

Instead of tests and scores, the prototype focuses on a **calm, aesthetic experience** that could later evolve into research tools or training exercises.

---

## Current Prototype: Mosaic Gaze Scene
https://github.com/user-attachments/assets/70173b5e-bb16-48c7-86ca-c5544a595ccd
### 1. Floating Attention Tiles

Around the user, we place a ring / cloud of floating tiles in VR:

- Each tile starts small and dim, drifting slightly in space.  
- When the user's gaze ray lands on a tile and lingers for a short dwell time, the tile:
  - Brightens and becomes more emissive.  
  - Scales up slightly to signal "you've activated this fragment."  
  - Locks into its completed state as part of the growing mosaic.  

Over time, more and more tiles activate, revealing a unique pattern that reflects how the user visually explored the environment.

### 2. Gaze‑Driven Interaction Loop

A central manager script computes a gaze ray (from true eye tracking on Quest Pro when available, or from head direction as a fallback) and drives the interaction:

- Each frame, we cast a ray forward into the scene.  
- If the ray hits a tile, the manager:
  - Notifies that tile that gaze has entered.  
  - Starts a **dwell timer** for that tile.  
- If the user keeps looking past a configurable threshold, the tile is marked as **completed** and visually "locks in."  
- If the user looks away, the dwell timer resets and the tile returns to its idle state.

This creates a simple but expressive loop: **look → dwell → transform the world**.

### 3. Gaze Fragment Logging (CSV)

Even though this is a visual, relaxing experience, we still care about capturing useful data:

- For each tile that completes, we log a row with:
  - `tileIndex`  
  - `firstLookTime` (when the user first gazed at the tile)  
  - `dwellDuration` (how long they maintained gaze to activate it)  
- Logs are written to CSV in the headset's persistent storage so they can be analyzed later.  
- These rows act as **"gaze fragments"** that can be recombined into richer measures of attention patterns, exploration strategies, or session summaries.

### 4. Session Summary Mosaic

After a short session (for example, once a certain number of tiles are activated or a time limit is reached), the experience fades into a lightweight summary:

- A simple panel shows:
  - How many tiles were activated.  
  - Average dwell time on completed tiles.  
- The surrounding scene stays lit with the user's completed mosaic, acting as a visual memory of their attention path.

---

## Why This Matters (Hackathon Scale)

We're not claiming clinical impact. Instead, GazeFlow is:

- A **design probe** for what gaze‑driven, non‑gamey VR experiences could feel like.
- A starting point for tools that help designers **see and log attention** in VR scenes.
- An example of using XR and eye‑tracking to **challenge the passive, stare‑at‑a‑screen default** and experiment with more dynamic visual behavior.

In future iterations, the same core structure—gaze tiles, dwell detection, and logging—could be extended into:

- Child‑friendly visual "quests" that encourage wider scanning and depth changes.  
- Research prototypes for studying attention, fatigue, or engagement in VR.  
- Adaptive mosaics that change difficulty or layout based on how someone looks around.

---

## How It Works (Technical High‑Level)

- **Engine:** Unity (URP)
- **Target Headset:** Meta Quest Pro (eye tracking capable), with fallback to non‑eye‑tracked devices  
- **XR Stack:** Meta XR / OVRCameraRig (or XR Origin with Meta provider)

Core systems:

- A **MosaicGazeManager** that:
  - Computes a gaze ray each frame (eye tracking when available, head direction otherwise).  
  - Raycasts into the scene to find tiles under gaze.  
  - Tracks dwell times and triggers tile activations.  
  - Writes CSV rows summarizing each activation event.  
- A **GazeTile** component on each tile that:
  - Handles visual transitions for gaze enter, gaze exit, and dwell completion.  
  - Stores its own index and timing info.  
- A lightweight **summary UI** that aggregates activation counts and dwell durations at the end of a session.

The result is an experience where **code, visual design, and data logging all orbit around a single idea: your gaze is the brush that paints the mosaic.**

---

## Features

### Visual Enhancements
- **Starfield Generator**: Immersive starfield surrounding the experience
- **Tile Layout Manager**: Beautiful sphere/grid arrangements of tiles
- **Visual Effects**: Particle systems and glow effects on tile activation
- **Floating Animation**: Gentle floating and rotation animations

### Technical Features
- Eye tracking support (Quest Pro) with head direction fallback (Quest 3)
- Real-time gaze fragment logging to CSV
- Session summary with completion statistics
- Adaptive tile arrangements

---

## Future Directions

After the hackathon, we'd like to explore:

- Richer tile layouts that encode depth and peripheral vision challenges.  
- Adaptive mosaics that rearrange themselves based on where users haven't explored yet.  
- Integration with more formal visual tasks (peripheral detection, smooth pursuit paths) while keeping the calm, art‑like framing.  
- Collaborations with researchers and clinicians to understand what kinds of gaze metrics are most meaningful for real‑world applications.
Youtube link: https://youtu.be/mAjotcGA1RQ
---

## Working with the Repo (For Teammates)

### Cloning the Project

```bash
git clone https://github.com/RabiatS/GazeFlow.git
cd GazeFlow
```

### Requirements

- Unity 6.0 or later
- Meta XR SDK 83.0.4
- Meta Quest Pro (for eye tracking) or Quest 3 (head tracking fallback)

### Setup

1. Open project in Unity
2. Open `Assets/Scenes/MosaicGazeScene.unity`
3. Ensure OVRCameraRig is present in the scene
4. Configure MosaicGazeManager1 component with tile references
5. Build for Android/Quest platform

---

## License

This project is part of Tartan Hacks hackathon submission.
