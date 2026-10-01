# Multi-Floor Intelligent Elevator Management System (DFA Engine)

A deterministic, multi-floor, multi-car intelligent elevator simulation built with **C# (.NET Framework 4.7.2)** and **Windows Forms**, modeled entirely on **Deterministic Finite Automata (DFA)** and Moore machine principles.

Developed as a term project for the **Formal Languages and Automata Theory** course.

---

## 📌 Project Overview

Traditional software models for reactive systems often suffer from race conditions and unmanageable state combinations when using nested `if-else` blocks. This project models elevator behavior deterministically using automata theory:
- Physical sensor events (weight limit, door obstacle photocells, emergency buttons) map directly to strict formal alphabets ($\Sigma$).
- State transitions ($\delta$) guarantee predictable system states under all physical interrupts.
- Centralized multi-car scheduling optimizes dispatching through dynamic distance-cost calculations.

---

## 🏗️ Architecture & Automata Models

To eliminate state explosion, the control logic is decoupled into **three interacting sub-automata**:

```
                       [ External Calls / Hall Requests ]
                                       │
                                       ▼
                         ┌───────────────────────────┐
                         │      Dispatcher DFA       │
                         │   (Avail / Full / Halt)   │
                         └─────────────┬─────────────┘
                                       │ (Optimal Car Assignment)
                                       ▼
                         ┌───────────────────────────┐
                         │         Cabin DFA         │
                         │ (Idle, Up, Down, Emrg...) │
                         └─────────────┬─────────────┘
                                       │ (Arrival Signal: 'A')
                                       ▼
                         ┌───────────────────────────┐
                         │         Door DFA          │
                         │ (Closed, Opening, Open...)│
                         └───────────────────────────┘
```

### 1. Cabin Automaton (`Cabin DFA`)
Controls vertical movement, floor transitions, and safety overrides:
- **States ($Q$):** `Idle`, `Up`, `Down`, `Emrg`, `Maint`
- **Alphabet ($\Sigma$):** `U` (Up Request), `D` (Down Request), `A` (Arrived / Idle), `E` (Emergency), `F` (Maintenance), `R` (Reset)
- **Accept State:** `Idle`

### 2. Door Automaton (`Door DFA`)
Manages door operations alongside safety sensors (photocell and load-cell interrupts):
- **States ($Q$):** `Closed`, `Opening`, `Open`, `Closing`, `OvrLoad`, `Obstcle`
- **Alphabet ($\Sigma$):** `A` (Open Signal), `O` (Fully Open), `C` (Fully Closed), `T` (Timer Expired), `V` (Overload Sensor), `L` (Load Cleared), `B` (Obstacle Detected), `R` (Reset)
- **Accept State:** `Closed`

### 3. Dispatcher Automaton (`Dispatcher DFA`)
Acts as the central orchestrator balancing load across $N$ elevators:
- **States ($Q$):** `Avail`, `Full`, `Halt`
- **Alphabet ($\Sigma$):** `F` (Capacity Full), `A` (Available), `E` (Global Panic), `S` (Restart), `R` (Reset)
- **Accept State:** `Avail`

---

## ✨ Key Features

- **Live DFA Visualizer Deck:** Custom GDI+ rendering engine (`DfaVisualizerPanel.cs`) that projects nodes, transitions, self-loops, and actively highlighted edges in real time.
- **Dynamic Shaft & Workspace Generation:** Configurable building matrix ($N$ elevators: 1–6, $M$ floors: 3–15) with auto-scrolling UI panels.
- **Cost-Based Dispatching Algorithm:** Calculates real-time distance and penalty weights (e.g., in-transit penalties) to select the optimal cabin for external calls.
- **Interactive Cabin Modal:** Real-time cabin interior simulation (`CabinControlForm.cs`) to test passenger floor selection, obstacle signals, overload triggers, and emergency locks.
- **10 Automated Academic Verification Scenarios:** Built-in test suite executing deterministic edge-case sequences (e.g., door obstruction recovery, simultaneous multi-car faults, global emergency halts).
- **Subsystem Event Terminal:** Integrated event logger outputting timestamps, subsystems, severity levels, and state transitions.

---

## 🧪 Automated Test Scenarios

The simulator includes pre-programmed automated integration tests:
1. **Upward Movement Cycle (U -> A)**
2. **Downward Movement Cycle (D -> A)**
3. **Door Obstacle Recovery (B -> Obstcle -> O -> Open)**
4. **Overload Lock (V -> OvrLoad -> L -> Open)**
5. **Emergency Stop Lock (E -> Emrg -> R -> Idle)**
6. **Maintenance Mode Isolation (F -> Maint -> R -> Idle)**
7. **Distance-Cost Optimization Selection**
8. **Dispatcher Full Capacity Transition (F -> Full)**
9. **Global System Panic Shutdown (Halt & All Emrg)**
10. **Independent Asynchronous Concurrency Test**

---

## 🚀 Getting Started

### Prerequisites
- Windows OS
- Visual Studio 2019 / 2022
- .NET Framework 4.7.2 Developer Pack

### Build & Run
1. Clone the repository:
   ```bash
   git clone [https://github.com/](https://github.com/)<your-username>/IntelligentElevatorSystem.git
   cd IntelligentElevatorSystem
   ```
2. Open `IntelligentElevatorSystem.sln` (or `IntelligentElevatorSystem.csproj`) in Visual Studio.
3. Set the build configuration to `Release` and platform to `Any CPU`.
4. Press `F5` or click **Start** to run the simulator.
