# Limitations & Follow-up Risks

## In-Memory Volatility
* **Risk:** All enrollment, waitlist, and prerequisite completion records are held purely in memory; application restarts or crashes result in complete state loss.
* **Follow-up:** Introduce persistence via Entity Framework Core backed by an ACID-compliant relational database (e.g., PostgreSQL).

## Process-Bound Locking
* **Risk:** The current `lock (_lock)` mechanism only serializes concurrent requests within a single application instance.
* **Follow-up:** In a horizontally scaled or multi-node microservice deployment, replace in-process monitor locks with optimistic concurrency tokens or database row-level locking (`SELECT ... FOR UPDATE`).

## Linear Waitlist Scan Complexity
* **Risk:** Evaluating prerequisites sequentially during `Drop()` runs in $O(N \cdot P)$ time relative to waitlist length $N$ and prerequisite count $P$.
* **Follow-up:** For offerings with extremely large waitlists, pre-index learner eligibility flags on prerequisite completion updates to achieve $O(1)$ promotion lookups.