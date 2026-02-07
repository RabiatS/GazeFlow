# GazeFlow

XR eye‑tracking experience that explores how fragmented visual moments can be measured, understood, and reshaped into a clearer picture in virtual space.

---

## Overview

Modern digital environments train us to stare at flat, static screens from a fixed distance. That’s convenient for devices but not ideal for developing healthy, flexible visual systems. GazeFlow asks a simple question: **what if our digital tools actively guided the eyes to move, focus, and explore, instead of staying frozen on one spot?**

Instead of passive consumption, we use immersive VR to encourage **active visual engagement and guided eye movement**. We prototype small, playful tasks that move targets in depth, change size, and invite users to track, fixate, and read—turning everyday VR time into an opportunity for healthier, more intentional visual behavior rather than just more staring.

We’re inspired by work like [Luminopia](https://www.luminopia.com/) and VR‑based vision therapy, but GazeFlow is a hackathon‑scale exploration, not a medical product.

---
## Team
### Team Name: PUJLY GazeFlow
<img width="512" height="384" alt="image" src="https://github.com/user-attachments/assets/85998768-4aae-4ab8-abf4-3b1e29d63aeb" />


- Rabiat Sadiq - Developer
Email: rabiats@andrew.cmu.edu
GitHub: https://github.com/RabiatS

- Yulkti Poddar - Product designer
Email: Ypoddar@andrew.cmu.edu
GitHub: https://github.com/Yuktipoddar

- Uma Dhamija - UI/UX Designer
Email: urd@andrew.cmu.edu
GitHub: https://github.com/Umaracheldhamija

- Priyal Shrivastava - Developer
Email: pshriva2@andrew.cmu.edu

## Vision

Because so much of kids’ visual world happens on 2D screens, they often don’t get the full range of oculomotor experiences—changing depth, convergence, and rich tracking—that their visual systems evolved for. Our goal isn’t to eliminate screens; that ship has sailed. It’s to **design digital experiences that work with, rather than against, healthy visual development**.

GazeFlow explores how immersive XR can:

- Guide **intentional eye movement** through interactive exercises.  
- Measure where attention lands and how long it stays there.  
- Turn those tiny measurements into **“fragments” of a larger mosaic** of visual function.  

We’re still early in the build, but the prototype already includes moving targets, simple “eye‑chart” style reading, and logging tools that capture how users respond.

---

## Current Prototype Features
<img width="1222" height="811" alt="image" src="https://github.com/user-attachments/assets/1be62838-b9a1-490f-93e0-0b6452aa8196" />


### 1. Moving Focus Ball

We place a simple object (currently a ball; soon a butterfly) in front of the user, then gradually move it farther away in steps.

- The user is asked to **keep their eyes on the object** as it retreats in depth.  
- A controller script moves the object forward from the user and then back by a configurable distance every few seconds.  
- This simulates a very rough version of “how far can you comfortably follow a target?” and hints at convergence and distance‑based performance.

### 2. Gaze Fragment Logger

We use a script called `GazeFragmentLogger` to turn “are they looking at it?” into simple, measurable fragments:

- Uses the **head’s forward direction** (Quest 3) as a stand‑in for eye gaze for now.  
- Checks each frame whether the user is looking roughly at a target object.  
- Measures **how long they keep their gaze on that object** (fixation time).  
- Detects when they look away and resets the timer.  
- Logs to the Unity Console:
  - When they start looking  
  - How long they’ve been looking  
  - When they stop looking and the total fixation duration  

This gives us a first, concrete signal of **focus endurance** we can later export, visualize, or feed into more advanced training games and analytics.

### 3. Early Letter‑Chart‑Style Test (Planned / In Progress)

We’re building a VR version of familiar letter‑chart tests:

- A TextMeshPro “chart” in VR shows rows of letters (e.g., E / F P / T O Z / …).  
- The chart can:
  - Move farther away from the user, and/or  
  - Shrink the letter size over time.  
- The user will use their controller ray to indicate whether they can still read a line or not.  

This isn’t calibrated like a full Snellen or logMAR system; it’s an **interaction pattern** we can build on: distance + clarity + user response, captured as another fragment in the mosaic.

### 4. Butterfly / Follow‑the‑Target Path (Planned / In Progress)

We’re adding a more playful, natural target—a butterfly or similar object—that:

- Flies along a smooth, curved path in front of the user.  
- Encourages **smooth pursuit** eye movements instead of jerky jumps.  
- Can be combined with gaze logging so we can see how well users track a moving, non‑linear target.

---

## Why This Could Help

We’re not making clinical claims, but our direction is grounded in existing research and products:

- VR and eye‑tracking are already being used in pediatric ophthalmology for **assessment, therapy, and visual‑field testing**, with promising results.  
- VR‑based vision therapy has shown **comparable or better outcomes** than some traditional office‑based therapies in trials, especially for convergence insufficiency and amblyopia.  
- VR lazy‑eye treatment products like Luminopia demonstrate that **engaging, gamified visual tasks in a headset can improve visual acuity and adherence**.

GazeFlow sits at the intersection of these ideas and hackathon feasibility: we prototype the **interaction patterns, logging tools, and UX** needed to make these kinds of experiences approachable and repeatable.

---

## How It Works (Technical High‑Level)

- **Engine:** Unity 6000 (URP)  
- **Platforms:** Meta Quest 3(non eye tracking); designed to extend to Meta Quest Pr( With eye tracking)  
- **XR SDK:** Meta XR All‑in‑One / Core SDK  
- **Tracking:**
  - Currently: head direction from the XR rig / OVRCameraRig  
  - Future: eye tracking via `OVREyeGaze` on Quest Pro  

Key building blocks:

- A **moving target controller** that spawns an object in front of the user and gradually moves it farther away over time.  
- The **GazeFragmentLogger** that:
  - Detects when the user is looking at the target (angle threshold)  
  - Tracks fixation duration  
  - Logs “start”, “ongoing”, and “stop” events to the Console  
  - Designed to save to CSV (timestamp, event type, fixation length) so fragments can be analyzed later.  
- A **response logger** (in progress) tied to simple UI buttons so users can ray‑select answers (“Can see”, “Can’t see”, or letter choices) and have them stored with the current difficulty level.

---

## Future / Planned Features

We’re still actively building. Some of the things on our near‑term roadmap:

- Swap from head‑based gaze approximation to **true eye tracking** on Quest Pro.  
- Hook up CSV export for both gaze events and user responses so we can analyze patterns across sessions.  
- Build a visual **“mosaic” summary screen** that reveals tiles as tasks are completed, and colors them based on how well the user performed.  
- Add more mini‑tests: peripheral target detection, smooth pursuit along paths, divided attention tasks (two targets competing for gaze).  
- Explore simple, adaptive difficulty: if a user does well, we automatically move to harder distances or smaller targets; if they struggle, we keep things easier.

---

## Working with the Repo (For Teammates)

### Cloning the project

```bash
git clone https://github.com/RabiatS/GazeFlow.git
cd GazeFlow
