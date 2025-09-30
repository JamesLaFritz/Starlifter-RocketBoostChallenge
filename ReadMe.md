# 🚀 Game Design Document

## Title

**Starlifter: Rocket Boost Challenge**

## Tagline / Short Description

*“Precision thrust. Perfect landings. Beat the hazards.”*

## Detailed Description

**Starlifter: Rocket Boost Challenge** is a 2.5D skill-based physics game inspired by classic lunar-lander gameplay and built during the GameDev.tv “Rocket Boost” section. Players pilot a small rocket craft through increasingly challenging environments filled with hazards, moving platforms, and tight spaces.
The core experience is about **precision** and **control**: carefully managing thrust, rotation, and inertia to land safely on designated pads while avoiding obstacles.

* **Objective:** Navigate from launch point to landing pad to complete each level.
* **Core Mechanics:**
  * **Rotation & Thrust:** Player controls left/right rotation and upward thrust with limited fuel.
  * **Physics-based flight:** Momentum, gravity, and collisions determine success.
  * **Hazards & Moving Elements:** Platforms, oscillating obstacles, and environmental features test skill.
* **Progression:** Sequential levels of increasing difficulty.
* **Tone & Style:** Bright, low-poly sci-fi aesthetic (GameDev.tv assets), clean UI, and satisfying particle/audio feedback.
* **Player Experience Goals:**
  * Feel tension and satisfaction mastering tight controls.
  * Feel skill progression from early easy levels to complex “docking” challenges.
  * Enjoy snappy audio-visual feedback (“juice”) for every action.

**Platform:** Unity WebGL build for Itch.io (playable in-browser with keyboard and gamepad).

---

## Course-Based Milestones (Base Game)

Each milestone corresponds to a key section in the Rocket Boost module. You can treat them like sprints:

| Completed | Milestone                   | Description                                                                            |
| --------- | --------------------------- | -------------------------------------------------------------------------------------- |
|   [ x ]   | 1. Setup & Assets           | Import GameDev.tv assets, basic scene with spaceship & landing pad.                    |
|   [ x ]   | 2. Player Controls          | Rotation input, AddRelativeForce thrust, Cinemachine follow camera, Rigidbody tuning.  |
|   [   ]   | 3. Audio & Particles        | Thruster SFX, booster particle effects, multiple audio clips.                          |
|   [   ]   | 4. Game State Management    | Switch statements for state, SceneManager respawn/level load, debug keys.              |
|   [   ]   | 5. Level Design & Obstacles | Add colliders/hazards, oscillating objects, multi-level layouts.                       |
|   [   ]   | 6. Polish & Post-Processing | Quick game tuning, set dressing & lights, intro to post-processing, prefab management. |
|   [   ]   | 7. Build & Share            | Build PC + WebGL, publish WebGL version to Itch.io.                                    |

---

## Post-Course “Juice” Milestones

| Completed | Feature               | Description                                                                                           |
| --------- | --------------------- | ----------------------------------------------------------------------------------------------------- |
|    [ ]    | Enhanced Feedback     | Camera shake on thrust/collision, exhaust glow, landing pad light animations.                         |
|    [ ]    | Progression & Scoring | Fuel/time bonuses, star ratings per level, local high scores.                                         |
|    [ ]    | Sound & Music Upgrade | Layered thruster audio with pitch shifts, short looping background music.                             |
|    [ ]    | UI/UX Improvements    | Animated main menu + level select, in-game tutorial overlay, “Next Level” and “Retry” transitions.    |
|    [ ]    | New Mechanics         | Fuel pickups, environmental hazards (wind gusts, gravity anomalies), moving landing pads/timed doors. |
|    [ ]    | Cosmetic Skins        | Unlockable rocket skins for achievements.                                                             |
|    [ ]    | Post-Processing & VFX | Bloom, color grading, subtle screen shake, thruster heat distortion.                                  |

---

## Branding Ideas for Rocket + Itch.io Page Art

* Use screenshots or GIFs of the rocket flying through levels.
* Use a short 15–30s clip of a level completion for the page banner.
* Brand the game as a “polished student project” with extra features.

Based on your screenshot (white pod with blue glow, red sky, blue towers):

* **Logo:** “Starlifter” in bold, rounded sans serif, white letters with a neon blue outline, small yellow thruster flame icon above the “i”.
* **Color Palette:**

  * Primary: Electric blue (#2FB8FF)
  * Accent: Neon yellow (#FFD800)
  * Background: Deep space red (#3C0A0A)
* **Itch.io Banner:** The rocket on a landing pad with animated thruster particles + glowing tower lights behind it.
* **Icons:** A small rocket silhouette for level select buttons; a neon landing pad icon for “completed” levels.
* **Page Sections:**

  * About (short tagline).
  * Controls (keyboard + gamepad).
  * Screenshots/GIFs of rocket landing and hazards.
  * Credits (built in Unity 6, based on GameDev.tv Rocket Boost section).

---

## Itch.io Release Plan

* **Platform:** WebGL (plus downloadable PC build as optional).
* **Page Content:**
  * Banner art with logo (“Starlifter”).
  * Short description/tagline.
  * Controls info (keyboard/gamepad).
  * Link back to GameDev.tv for learning context (optional).
* **Versioning:**
  * v1.0 = Base course project.
  * v1.1+ = Added juice features.

---

## Credits

**Development & Design:**

* Game built by *James LaFritz* as part of the [**GameDev.tv course Complete Unity 3D Developer: Design & Develop Games in Unity 6 using C#**](https://www.gamedev.tv/courses/unity6-complete-3d) - ***Rocket Boost*** section.

**Assets & Learning Materials:**

* Core 3D models, textures, and prefabs provided by [GameDev.tv](https://www.gamedev.tv) (Unity 6 Complete 3D Developer Course).
* Additional level layouts, effects, and gameplay elements created by *James LaFritz*.
* Additional 2D art created by **ChatGPT** (OpenAI)
* Additional 3D art pruchased from by [Synty Studios](https://syntystore.com/) [Synty Studios Asset Store](https://assetstore.unity.com/publishers/5217)
  * **POLYGON - Sci-Fi Space Pack** [Synty Store](https://syntystore.com/products/polygon-sci-fi-space-pack?_pos=1&_sid=e0a55a0c2&_ss=r) [Asset Store](https://assetstore.unity.com/packages/3d/environments/sci-fi/polygon-sci-fi-space-low-poly-3d-art-by-synty-138857)
  * **INTERFACE - Sci-Fi Soldier HUD** [Synty Store]() [Asset Store](https://assetstore.unity.com/packages/2d/gui/sci-fi-soldier-hud-synty-interface-gui-278336)
  * **POLYGON - Sci-Fi Worlds Pack** [Synty Store](https://syntystore.com/products/polygon-sci-fi-worlds?_pos=16&_sid=e0a55a0c2&_ss=r) [Asset Store](https://assetstore.unity.com/packages/3d/environments/sci-fi/polygon-sci-fi-worlds-low-poly-3d-art-by-synty-206299)
  * **POLYGON - Sci-Fi Cyber City** [Synty Store](https://syntystore.com/products/polygon-sci-fi-cyber-city?_pos=13&_sid=e0a55a0c2&_ss=r) [Asset Store](https://assetstore.unity.com/packages/3d/environments/sci-fi/polygon-sci-fi-cyber-city-low-poly-3d-art-by-synty-259784)

**Music & Sound Effects:**

* Placeholder SFX from GameDev.tv assets; additional SFX and music composed/arranged by *James LaFritz* or sourced from royalty-free libraries (list when finalized).

**Game Design Document & Code Documentation:**

* Drafted, structured, and formatted with assistance from **ChatGPT** (OpenAI) based on your input and the course outline.

**Engine & Tools:**

* Built in Unity 6 with the new Input System.
* Target platform WebGL for Itch.io.

**Special Thanks:**

* Rick Davidson and the GameDev.tv community for the course and assets.
* Playtesters and friends who gave feedback on level design.

*(Update this section as you add new contributors, music sources, or testers.)*

---

This keeps everything transparent for your Itch.io page while making it look polished and professional.
