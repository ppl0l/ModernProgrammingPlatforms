# Modern Programming Platforms — Lab 1 🧪

A custom **testing framework** built from scratch in **C# / .NET**, including a **Test Runner**, a **custom Thread Pool**, and an application under test (`TaskManager`).

> **Course:** Modern Programming Platforms

## 📋 About

Laboratory Work #1 — implementation of a miniature testing framework (like NUnit) without third-party libraries. Demonstrates attributes, reflection, expression trees, and multithreading.

### Features
- Custom attributes: `[Test]`, `[Before]`, `[TestCaseSource]`, `[Category]`, `[Priority]`, `[Timeout]`.
- `Check` assertions with **expression-tree failure messages**.
- Custom **Thread Pool** with dynamic scaling, idle reaping, and health checks.
- **Test Runner** that loads assemblies via reflection and filters tests by priority/category.
- Async test support.

## 🛠 Technologies
- C# / .NET, WPF-compatible runtime
- System.Reflection, System.Linq.Expressions, System.Threading

## 📂 Structure
```text
Lab1/TestingFrameworkSolution/
├── CalculatorApp/         # App under test (TaskManager, Statistics)
├── CalculatorTests/       # Tests for the app
├── CustomThreadPool/      # Custom MyThreadPool
├── TestRunner/            # Console app to run tests
├── TestingLibrary/        # Attributes + Check assertions
└── TestingFrameworkSolution.sln
```

## 🚀 Installation and Setup

1. **Clone:**
   ```bash
   git clone https://github.com/your-username/ModernProgrammingPlatforms.git
   cd ModernProgrammingPlatforms
   ```

2. **Open** `Lab1/TestingFrameworkSolution/TestingFrameworkSolution.sln` in Visual Studio 2022.

3. **Build:** `Ctrl + Shift + B`.

4. **Run tests:**
   - Set `TestRunner` as startup project → press `F5`
   - Or CLI:
     ```bash
     cd Lab1/TestingFrameworkSolution/TestRunner
     dotnet run
     ```

5. **Filter tests** in `Program.cs` by priority (`Priority > 3`) or category (`ExpressionTree`).

## ✨ Implementation Details

- **TestingLibrary** — attributes + `Check` class with `Eq`, `True`, `Null`, `That(() => expr)`.
- **CustomThreadPool** — dynamic min/max threads, `HealthCheck` for stuck workers, events `OnPoolChanged` / `OnTaskStarted`, `IDisposable` shutdown.
- **TestRunner** — loads DLLs via `Assembly.LoadFrom`, filters by attributes, enqueues into the pool.
- **CalculatorApp** — `TaskManager` (CRUD + async) and `Statistics` (rate, average priority, distribution).

## 📞 Contacts
- **Repo:** [github.com/ppl0l/ModernProgrammingPlatforms](https://github.com/ppl0l/ModernProgrammingPlatforms)

---
*Laboratory work for the "Modern Programming Platforms" course.*
