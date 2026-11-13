using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseRegistration.Core
{
    public class CourseFullException : Exception
    {
        public CourseFullException(string message) : base(message) { }
    }

    public class DuplicateEnrollmentException : Exception
    {
        public DuplicateEnrollmentException(string message) : base(message) { }
    }

    public class CourseRegistrationService
    {
        private readonly string _courseId;
        private readonly int _capacity;
        private readonly HashSet<string> _prerequisites;

        private readonly HashSet<string> _enrolledLearners = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _waitlist = new();
        private readonly Dictionary<string, HashSet<string>> _completedCourses = new(StringComparer.OrdinalIgnoreCase);

        private readonly object _lock = new();

        public CourseRegistrationService(string courseId, int capacity, IEnumerable<string> prerequisites)
        {
            _courseId = courseId;
            _capacity = capacity;
            _prerequisites = new HashSet<string>(prerequisites, StringComparer.OrdinalIgnoreCase);
        }

        public void SetLearnerCompletedCourses(string learnerId, IEnumerable<string> completed)
        {
            string normalizedId = learnerId?.Trim() ?? throw new ArgumentNullException(nameof(learnerId));
            lock (_lock)
            {
                _completedCourses[normalizedId] = new HashSet<string>(completed, StringComparer.OrdinalIgnoreCase);
            }
        }

        public IReadOnlyCollection<string> EnrolledLearners
        {
            get { lock (_lock) { return _enrolledLearners.ToList(); } }
        }

        public IReadOnlyList<string> Waitlist
        {
            get { lock (_lock) { return _waitlist.ToList(); } }
        }

        private bool IsEligible(string learnerId)
        {
            if (!_completedCourses.TryGetValue(learnerId, out var completed))
            {
                completed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            return _prerequisites.IsSubsetOf(completed);
        }

        public string Enroll(string learnerId)
        {
            string normalizedId = learnerId?.Trim() ?? throw new ArgumentNullException(nameof(learnerId));

            lock (_lock)
            {
                if (_enrolledLearners.Contains(normalizedId))
                    throw new DuplicateEnrollmentException($"Learner '{normalizedId}' is already enrolled in {_courseId}.");
                if (_waitlist.Contains(normalizedId))
                    throw new DuplicateEnrollmentException($"Learner '{normalizedId}' is already on the waitlist for {_courseId}.");

                if (_enrolledLearners.Count < _capacity)
                {
                    // Direct enrollment requires meeting prerequisites
                    if (!IsEligible(normalizedId))
                        throw new InvalidOperationException($"Learner '{normalizedId}' does not meet prerequisites for {_courseId}.");

                    _enrolledLearners.Add(normalizedId);
                    return "ENROLLED";
                }
                else
                {
                    // When full, learners join the waitlist. 
                    // Ineligible learners are permitted to join so they can be skipped and retained upon promotion.
                    _waitlist.Add(normalizedId);
                    return "WAITLISTED";
                }
            }
        }

        public string Drop(string learnerId)
        {
            string normalizedId = learnerId?.Trim() ?? throw new ArgumentNullException(nameof(learnerId));

            lock (_lock)
            {
                if (!_enrolledLearners.Contains(normalizedId))
                    throw new InvalidOperationException($"Learner '{normalizedId}' is not enrolled in {_courseId}.");

                _enrolledLearners.Remove(normalizedId);
                return PromoteNextEligible();
            }
        }

        private string PromoteNextEligible()
        {
            if (_waitlist.Count == 0 || _enrolledLearners.Count >= _capacity)
                return null;

            int eligibleIndex = -1;
            for (int i = 0; i < _waitlist.Count; i++)
            {
                if (IsEligible(_waitlist[i]))
                {
                    eligibleIndex = i;
                    break;
                }
            }

            if (eligibleIndex != -1)
            {
                string promoted = _waitlist[eligibleIndex];
                _waitlist.RemoveAt(eligibleIndex);
                _enrolledLearners.Add(promoted);
                return promoted;
            }

            return null;
        }
    }
}