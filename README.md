# PhyAR Lab

> **Offline mobile augmented reality physics learning for KCSE Form 2–4 students in Nairobi, Kenya.**

![PhyAR Lab Logo — atom orbital mark in electric copper with cyan AR arcs]

---

## Description

PhyAR Lab is a standalone Android AR application that overlays interactive 3D physics visualisations onto flat surfaces using markerless augmented reality. It targets Form 2, 3, and 4 students in sub-county public secondary schools that lack functional physics laboratory facilities.

The platform delivers a structured four-step learning flow for each topic:

**Learn → AR Experience → Flashcards → Topic Quiz**

- **Learn** — KLB-aligned concept text, definitions, and formulas
- **AR Experience** — interactive 3D models anchored to a flat surface via ARCore plane detection. Students rotate, scale, tap components to reveal annotations, and play physics animations
- **Flashcards** — per-sub-topic flip cards for active recall. Students mark each card as "Got it" or "Still learning"
- **Topic Quiz** — 10-question MCQ quiz per topic with immediate answer feedback and worked explanations

The app runs entirely offline on any ARCore-compatible Android smartphone. No internet, no server, no specialist hardware required.

---

## Physics topics covered

| Topic | Form | Status |
|---|---|---|
| Waves I — pulses, transverse and longitudinal, wave formula | Form 2 | Shell (placeholder) |
| Electromagnetism — Oersted, motor effect, Fleming's LHR | Form 2 | Shell (placeholder) |
| Current Electricity II — Ohm's law, series/parallel, EMF | Form 3 | Full build |
| Waves II — reflection, refraction, diffraction, interference | Form 3 | Full build |
| Electromagnetic Induction — Faraday, Lenz, generators, transformers | Form 4 | Full build |

---

## GitHub Repository

**https://github.com/[your-username]/PhyAR-Lab**

---

## Colour System

| Role | Name | Hex | Used for |
|---|---|---|---|
| Primary | Electric Copper | `#B87333` | Buttons, CTAs, active states |
| Secondary | Deep Indigo | `#1A1A2E` | Headers, nav bars, body text |
| Background | Warm White | `#FAF9F6` | All screen canvases |
| Highlight | Arc Plasma Cyan | `#00D9FF` | AR overlays, current flow, active indicators |
| Alert | Ember Orange | `#FF6B35` | Errors, wrong answers, warnings |
| Success | Coil Green | `#2EC4B6` | Correct answers, completed steps |

---

## How to set up the environment and project

### Prerequisites

| Tool | Version | Download |
|---|---|---|
| Unity Hub | Latest | https://unity.com/download |
| Unity Editor | 6000.0.x LTS (Unity 6) | Via Unity Hub |
| AR Foundation | 6.x | Installed via Unity Package Manager |
| ARCore XR Plugin | 6.x | Installed via Unity Package Manager |
| Android Build Support | API 26+ | Via Unity Hub — Android module |
| Android NDK | Bundled with Android module | Installed alongside |
| Git | 2.x | https://git-scm.com |
| Visual Studio Code | Latest | https://code.visualstudio.com |

Your Android phone must support ARCore. Check compatibility: https://developers.google.com/ar/devices

---

### Step 1 — Clone the repository

```bash
git clone https://github.com/[your-username]/PhyAR-Lab.git
cd PhyAR-Lab
```

### Step 2 — Open in Unity

1. Open Unity Hub
2. Click **Open** → **Add project from disk**
3. Select the cloned `PhyAR-Lab` folder
4. Unity Hub detects the editor version. Install **Unity 6000.0.x LTS** if prompted
5. Open the project — packages import automatically (allow 3–5 minutes on first open)

### Step 3 — Install AR packages

In Unity: **Window → Package Manager → + → Add package by name**

Add these packages:
```
com.unity.xr.arfoundation
com.unity.xr.arcore
com.unity.inputsystem
com.unity.textmeshpro
```

### Step 4 — Configure Android build

1. **File → Build Settings → Android → Switch Platform**
2. **Edit → Project Settings → Player → Android tab**
   - Minimum API Level: Android 7.0 (API 26)
   - Target API Level: Android 14 (API 34)
3. **Project Settings → XR Plug-in Management → Android tab**
   - Enable ARCore checkbox

### Step 5 — Enable USB debugging on your phone

1. **Settings → About phone** — tap Build number 7 times
2. **Settings → Developer options** — enable USB debugging
3. Connect phone via USB and accept the debugging prompt on the phone

### Step 6 — Build and deploy

**File → Build Settings → Build and Run**

Unity compiles the APK and installs it directly to the connected phone.

### Step 7 — Test in editor (no phone required)

1. Install **XR Device Simulator** via Package Manager
2. **Project Settings → XR Plug-in Management → XR Simulation** — enable
3. Press Play in the Unity Editor
4. Use WASD + mouse to simulate camera movement and plane detection

---

## Designs

### Figma prototype

**https://www.figma.com/design/Jk4aRmpj9aP39VUoBcgCwm**

All 11 screens designed across 3 Figma pages:
- Page 1: Design System + S01 Splash + S02 Home
- Page 2: S03 Learn + S04 AR Experience + S05 Flashcard Deck + S06 Flashcard Summary
- Page 3: S07 Quiz Question + S08 Quiz Feedback + S09 Quiz Results + S10 Progress Dashboard + S11 Teacher Panel


### System architecture

PhyAR Lab uses a four-layer offline-only architecture:

```
Presentation layer    — Unity Canvas UI, AR camera view, annotation overlays
Application layer     — C# controllers (ARSessionManager, ModuleLoader, SessionLogger)
AR engine layer       — Unity AR Foundation, Google ARCore, 3D model renderer
Local data layer      — Bundled JSON assets, session logs in device internal storage
```

No backend server. No cloud. No network calls of any kind.

---

## Project structure

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── AR/               — ARSessionManager, ARPlaneDetector, ModelInteractionHandler
    │   ├── Learning/         — ModuleLoader, FlashcardManager, LearningProgressTracker
    │   ├── Quiz/             — QuizManager, QuizQuestion, QuizResultsHandler
    │   ├── Data/             — SessionLogger, LocalStorageManager, TeacherPanelController
    │   └── UI/               — UINavigator, screen controllers per screen
    ├── Scenes/
    │   ├── Main.unity        — all UI screens as toggled panels
    │   └── AR_View.unity     — AR loaded additively on top of Main
    ├── Prefabs/
    │   ├── UI/               — TopicCard, FlashCard, MCQOption, AnnotationLabel
    │   └── AR/               — ARSessionOrigin, PlacementIndicator, physics modules
    ├── Data/
    │   ├── Flashcards/       — JSON flashcard decks per topic
    │   └── Quiz/             — JSON quiz questions per topic (10 per topic)
    ├── Models/               — FBX 3D models for each physics module
    └── Fonts/                — Inter font family (Regular, Medium, SemiBold)
```

---

## Deployment plan

### Phase 1 — Development (Weeks 1–4)

| Week | Activity |
|---|---|
| 1 | Unity setup, AR Foundation config, folder structure, S01+S02 UI |
| 2 | Learn screen (S03), AR module (S04) — all three full topics |
| 3 | Flashcard system (S05+S06), Quiz system (S07+S08+S09) |
| 4 | Progress dashboard (S10), Teacher panel (S11), JSON data all topics |

### Phase 2 — Internal testing (Weeks 5–6)

- Unit tests for SessionLogger, LocalStorageManager, QuizManager
- Device testing on two ARCore Android phones (Snapdragon 6xx, Android 10+)
- Performance target: stable 30–60 FPS on mid-range devices

### Phase 3 — User acceptance testing (Weeks 7–10)

- Deploy to two sub-county public secondary schools in Nairobi
- 20 Form 3/4 students — pre-test and post-test (KCSE-aligned)
- System Usability Scale (SUS) administered at final session
- Session logs exported via USB / ADB for analysis

### Phase 4 — Analysis and reporting (Weeks 11–12)

- Statistical analysis: paired t-test on pre/post scores
- Thematic analysis of teacher interviews
- Engagement analysis from session JSON logs
- Final dissertation submission and deployment recommendations

### APK distribution

Release APK: `builds/PhyAR-Lab-v1.0.apk`
Sideload on any ARCore-compatible Android device. No Play Store required for research deployment.

Teacher session log export: long-press the app logo on the home screen for 3 seconds to open the PIN-protected teacher panel. Export all JSON logs to the device Downloads folder, then copy via USB.

---

## Assets used

| Asset | Source | Licence | Physics module |
|---|---|---|---|
| Magnetic Field of Solenoid | Sketchfab — yuyalyj | CC-BY | EM Induction |
| 3D Magnet (low-poly) | Unity Asset Store | Asset Store EULA | EM Induction |
| CC0 Electronic Components | Sketchfab — literallylara | CC0 | Electric Circuits |
| Electric Circuit (edu) | Sketchfab — 7D Production | Free | Electric Circuits |
| Longitudinal Waves model | Sketchfab | Free | Waves |
| Electromagnetic Wave model | Sketchfab — UUON Foundation | Free | Waves |
| Free Quick Effects VFX | Unity Asset Store | Asset Store EULA | All topics |
| AR Foundation Samples | Unity Technologies (GitHub) | Apache 2.0 | All topics |

---

## Hardware used

| Hardware | Role |
|---|---|
| ARCore-compatible Android phone (Android 7+) | Primary deployment device |
| USB cable | ADB deployment and session log export |
| Development laptop (16 GB RAM, dedicated GPU) | Unity development and build |
| Second Android device | Compatibility testing |

---

## Team

**Developer and researcher:** Branice Kazira
**Programme:** BSc. Software Engineering — AR/VR Specialisation
**Institution:** African Leadership University
**Academic year:** 2025–2026