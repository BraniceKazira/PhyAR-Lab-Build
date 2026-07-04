# Analysis, Discussion & Recommendations

## Analysis of Results

### Objective 1 – Understand the Problem Context

**Status:** ✅ Achieved

The literature review and requirements analysis confirmed that sub-county public secondary schools in Nairobi consistently lack functional physics laboratory equipment. This creates a measurable gap between KLB curriculum requirements (which specify hands-on experiments) and actual classroom practice. PhyAR Lab addresses this gap by providing virtual equivalents of key KCSE experiments using only a student's phone and a flat surface.

### Objective 2 – Develop the AR Intervention

**Status:** ✅ Achieved (with scope adjustments)

Three topics were implemented — **Current Electricity II**, **Electromagnetic Induction**, and **Waves II** — each following the four-step flow: `Learn → AR → Flashcards → Quiz`.

Physics accuracy was verified through unit testing:

- Ohm's Law calculations confirmed correct across all resistance presets
- Wave equation confirmed correct across all amplitude and frequency combinations
- **25/25 unit tests passing**, zero failures

**Scope adjustment:** The Waves II AR scene is not yet complete (see [Known Gaps](#known-gaps)). The Learn content and Quiz for Waves II are fully functional.

### Objective 3 – Evaluate the Intervention

**Status:** 🟡 Partially achieved

The app is technically stable and ready for field evaluation. Unit testing, functional testing (28 manual test cases), and on-device testing were completed. School-based data collection using the System Usability Scale (SUS) and Student Perceived Learning Questionnaire is planned for the next research phase.

#### What Testing Confirmed

| Claim | Evidence |
|-------|----------|
| No crashes across full learning flow | 28 manual test cases passed on device |
| AR plane detection functional | Detects within 10–15 seconds in well-lit indoor spaces |
| Ohm's Law calculations accurate | Live labels verified against manual calculation table |
| EM Induction demonstration clear | Magnet animation shows galvanometer deflection; 3-state cycle verified |
| Session persistence works | Progress dashboard updates correctly after quiz completion |

#### Known Gaps

| Gap | Impact | Planned Resolution |
|-----|--------|--------------------|
| AR annotation labels incomplete | Only formula label (circuit) and state label (EM) implemented; component-level labels missing | Add floating component labels in next iteration |
| Waves II AR not implemented | Placeholder model does not demonstrate reflection, refraction, or interference | Complete wave AR models as next priority |

---

## Discussion

### Architecture Decisions That Paid Off

The **offline-first JSON architecture** proved correct. The app functions without any network connectivity, matching the connectivity constraints of the target schools. It also makes content editable without rebuilding the app — a teacher or content developer can update JSON files and redeploy.

The **single-scene Unity architecture** with panel-based navigation reduced build complexity and load times:

- Screen transitions: **< 0.5 seconds**
- App launch: **~3 seconds** on target hardware

### Scope Decisions

The decision to prioritise the Current Electricity II and Electromagnetic Induction AR scenes over Waves II was agreed with the project supervisor. The rationale: two fully functional AR demonstrations provide stronger evidence of the concept than three partially complete ones. The Waves II placeholder demonstrates framework extensibility while signalling where future work is needed.

---

## Recommendations

### For Contributors

> **1. Multi-language support** — The target population includes students who learn better in Kiswahili. The JSON data structure already supports multiple fields per content block. Adding Kiswahili translations and a language toggle on the Home screen would increase accessibility.

> **2. Adaptive flashcard sequencing** — Currently flashcards cycle in fixed order. A spaced-repetition algorithm prioritising cards marked *Still Learning* would better support retention for KCSE exam preparation.

> **3. Teacher-configured quiz modes** — The current 10-question quiz uses all questions in order. Allowing teachers to select specific sub-topics would make PhyAR Lab more useful as a formative assessment tool.

### For Researchers

> **1. Comparative study design** — This technical feasibility study establishes that the app works and can be used by individuals. A follow-up quasi-experimental study comparing learning outcomes between AR-assisted and traditional instruction would provide stronger evidence for policy recommendations on technology adoption in under-resourced schools.

> **2. Device deployment models** — Many Kenyan secondary schools prohibit personal phones. Researchers should explore whether a secondary-school extension of the government's Digital Literacy Programme could provide tablets for physics lessons. Investigation should cover:
> - Minimum hardware specifications (can low-cost tablets run ARCore?)
> - Sharing logistics during lessons
> - Charging and storage requirements
> - Administrative implications for schools

> **3. Extension to other STEM subjects** — The offline, JSON-driven architecture is not physics-specific. The same framework could support chemistry, biology, or mathematics with equivalent visual benefits in resource-constrained environments.