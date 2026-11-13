# Design Specification

## Service Boundary
Scoped strictly to a single course offering entity (`CourseRegistrationService`). Responsible for managing active enrollments, prerequisite validation, FIFO waitlist queueing, and drop-triggered vacancy promotions.

## Data & API Design
* **In-Memory Storage:**
  * `_enrolledLearners`: `HashSet<string>` configured with `StringComparer.OrdinalIgnoreCase` for $O(1)$ duplicate lookups and active capacity tracking.
  * `_waitlist`: `List<string>` preserving strict FIFO ordering upon overflow.
  * `_completedCourses`: `Dictionary<string, HashSet<string>>` mapping learner IDs to their completed course sets.
* **Public Contracts:**
  * `Enroll(string learnerId)`: Evaluates capacity, uniqueness, and prerequisites; adds to enrollment or waitlist.
  * `Drop(string learnerId)`: Removes learner from active enrollment and triggers synchronous waitlist promotion.
  * `SetLearnerCompletedCourses(string learnerId, IEnumerable<string> completed)`: Configures prerequisites for validation.
  * Property Getters: `EnrolledLearners` and `Waitlist` return lock-protected defensive copies (`ToList()`).

## Invariants & Policies
* **Capacity Invariant:** `EnrolledLearners.Count <= Capacity` is strictly maintained at all times.
* **Uniqueness Invariant:** A learner can exist in at most one active state per course offering (enrolled or waitlisted). All IDs are normalized via `.Trim()` and compared case-insensitively.
* **Waitlist Promotion Policy (Skip-and-Retain):** Upon a seat opening via `Drop()`, the waitlist is scanned sequentially from index `0`. The first eligible learner meeting prerequisites is promoted. Ineligible learners are bypassed for that vacancy but **retained** in their queue position.

## Concurrency Approach
Uses a coarse-grained monitor lock (`lock (_lock)`) wrapping all state reads and mutations to prevent compound check-then-act race conditions across interdependent collections.

## Assumptions & Tradeoffs
Prerequisites are modeled as flat sets of course IDs; complex prerequisite DAGs, external database persistence, and distributed locking are intentionally excluded for bounded simplicity.