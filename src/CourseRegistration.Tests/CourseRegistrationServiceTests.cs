using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using CourseRegistration.Core;

namespace CourseRegistration.Tests
{
    public class CourseRegistrationServiceTests
    {
        [Fact]
        public void Successful_Enrollment()
        {
            var service = new CourseRegistrationService("CS101", 2, new[] { "MATH101" });
            service.SetLearnerCompletedCourses("alice", new[] { "MATH101" });

            string status = service.Enroll("alice");
            Assert.Equal("ENROLLED", status);
            Assert.Contains("alice", service.EnrolledLearners);
        }

        [Fact]
        public void Prerequisite_Failure_Throws()
        {
            var service = new CourseRegistrationService("CS101", 2, new[] { "MATH101" });
            service.SetLearnerCompletedCourses("bob", Array.Empty<string>());

            Assert.Throws<InvalidOperationException>(() => service.Enroll("bob"));
            Assert.Empty(service.EnrolledLearners);
            Assert.Empty(service.Waitlist);
        }

        [Fact]
        public void Waitlist_Joining_When_Full()
        {
            var service = new CourseRegistrationService("CS101", 1, Array.Empty<string>());
            service.Enroll("alice");

            string status = service.Enroll("bob");
            Assert.Equal("WAITLISTED", status);
            Assert.Contains("bob", service.Waitlist);
        }

        [Fact]
        public void Duplicate_Enrollment_And_Case_Insensitive_Check()
        {
            var service = new CourseRegistrationService("CS101", 2, Array.Empty<string>());
            service.Enroll("Alice");

            Assert.Throws<DuplicateEnrollmentException>(() => service.Enroll("alice"));
            Assert.Throws<DuplicateEnrollmentException>(() => service.Enroll("ALICE"));
        }

        [Fact]
        public void Fifo_Promotion_With_Skip_And_Retain()
        {
            var service = new CourseRegistrationService("CS101", 1, new[] { "MATH101" });
            service.SetLearnerCompletedCourses("alice", new[] { "MATH101" });
            service.SetLearnerCompletedCourses("bob", Array.Empty<string>()); // ineligible
            service.SetLearnerCompletedCourses("charlie", new[] { "MATH101" }); // eligible

            service.Enroll("alice");
            service.Enroll("bob");
            service.Enroll("charlie");

            Assert.Equal(new[] { "bob", "charlie" }, service.Waitlist);

            string promoted = service.Drop("alice");
            Assert.Equal("charlie", promoted);
            Assert.Contains("charlie", service.EnrolledLearners);
            Assert.Equal(new[] { "bob" }, service.Waitlist); // Bob remains skipped and retained
        }

        [Fact]
        public void Invalid_And_Repeated_Drops_Throw()
        {
            var service = new CourseRegistrationService("CS101", 2, Array.Empty<string>());
            Assert.Throws<InvalidOperationException>(() => service.Drop("nonexistent"));

            service.Enroll("alice");
            service.Drop("alice");
            Assert.Throws<InvalidOperationException>(() => service.Drop("alice"));
        }

        [Fact]
        public async Task Concurrent_Enrollments_Respect_Capacity_Using_Barrier()
        {
            var service = new CourseRegistrationService("CS101", 5, Array.Empty<string>());
            int threadCount = 15;
            using var barrier = new Barrier(threadCount);

            var tasks = Enumerable.Range(1, threadCount).Select(i => Task.Run(() =>
            {
                string studentId = $"student_{i}";
                service.SetLearnerCompletedCourses(studentId, Array.Empty<string>());
                barrier.SignalAndWait(); // Force simultaneous execution to maximize lock contention
                try
                {
                    service.Enroll(studentId);
                }
                catch { }
            }));

            await Task.WhenAll(tasks);

            Assert.Equal(5, service.EnrolledLearners.Count);
            Assert.Equal(10, service.Waitlist.Count);
        }

        [Fact]
        public void Duplicate_Waitlist_Attempt_Throws()
        {
            var service = new CourseRegistrationService("CS101", 1, Array.Empty<string>());
            service.Enroll("alice"); // Fills capacity

            service.Enroll("bob"); // Goes to waitlist
            Assert.Throws<DuplicateEnrollmentException>(() => service.Enroll("bob")); // Duplicate waitlist attempt
        }

        [Fact]
        public void Drop_When_Waitlist_Is_Empty_Opens_Seat()
        {
            var service = new CourseRegistrationService("CS101", 2, Array.Empty<string>());
            service.Enroll("alice");
            
            string promoted = service.Drop("alice");
            Assert.Null(promoted);
            Assert.Empty(service.EnrolledLearners);
        }
    }
}