# Course Registration Service

A production-grade, thread-safe .NET 8 / C# domain service managing single-offering course enrollments, prerequisite validation, and FIFO skip-and-retain waitlist promotions.

## Repository Architecture

```text
CourseRegistration/
├── .gitignore
├── README.md
├── CourseRegistration.sln
├── docs/
│   ├── DESIGN_SPECIFICATION.md
│   ├── AI_PROMPT_RECORD.md
│   ├── SETUP.md
│   └── LIMITATIONS.md
└── src/
    ├── CourseRegistration.Core/
    │   ├── CourseRegistration.Core.csproj
    │   └── CourseRegistrationService.cs
    └── CourseRegistration.Tests/
        ├── CourseRegistration.Tests.csproj
        └── CourseRegistrationServiceTests.cs
```

## Quick Start & Verification

1. Ensure the [.NET 8 SDK](https://dotnet.microsoft.com/) is installed.

2. From the solution root directory, execute the automated xUnit test suite:

   ```bash
   dotnet test --logger "console;verbosity=normal"
   ```

## Documentation Index

* [**Setup & Run Instructions**](docs/setup_and_run_instructions.md)**:** Detailed prerequisites, build commands, and test execution filters.
* [**Design Specification**](docs/design_specification.md)**:** Core domain service boundary, data structures, invariants, and concurrency model.
* [**AI Prompt and Use Record**](docs/ai_prompt_and_use_record.md)**:** Complete log of AI interactions, prompts used, senior architectural rationales, and verification methods.
* [**Limitations & Follow-up Risks**](docs/limitations_and_follow_up_risks.md)**:** Production considerations regarding in-memory volatility, process-bound locking, and algorithmic scaling.
