using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Create roles
        foreach (var role in new[] { "Admin", "Faculty", "Student" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Seed branches
        if (!db.Branches.Any())
        {
            db.Branches.AddRange(
                new Branch { Name = "Dublin", Address = "1 Main Street, Dublin" },
                new Branch { Name = "Cork", Address = "5 Patrick Street, Cork" },
                new Branch { Name = "Galway", Address = "10 Shop Street, Galway" }
            );
            await db.SaveChangesAsync();
        }

        // Seed courses
        if (!db.Courses.Any())
        {
            var dublin = db.Branches.First(b => b.Name == "Dublin");
            var cork = db.Branches.First(b => b.Name == "Cork");
            var galway = db.Branches.First(b => b.Name == "Galway");

            db.Courses.AddRange(
                new Course { Name = "Software Development", BranchId = dublin.Id, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) },
                new Course { Name = "Data Analytics", BranchId = dublin.Id, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) },
                new Course { Name = "Cybersecurity", BranchId = cork.Id, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) },
                new Course { Name = "Cloud Computing", BranchId = galway.Id, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) }
            );
            await db.SaveChangesAsync();
        }

        // Seed admin users
        await CreateUserAsync(userManager, "admin@vgc.ie", "Admin123!", "Admin");
        await CreateUserAsync(userManager, "felix@vgc.ie", "Felix123!", "Admin");

        // Seed faculty users + profiles
        var faculty1 = await CreateUserAsync(userManager, "faculty@vgc.ie", "Faculty123!", "Faculty");
        var faculty2 = await CreateUserAsync(userManager, "faculty2@vgc.ie", "Faculty123!", "Faculty");

        if (!db.FacultyProfiles.Any())
        {
            var course1 = db.Courses.First(c => c.Name == "Software Development");
            var course2 = db.Courses.First(c => c.Name == "Data Analytics");

            FacultyProfile? fp1 = null;
            FacultyProfile? fp2 = null;

            if (faculty1 != null)
            {
                fp1 = new FacultyProfile
                {
                    IdentityUserId = faculty1.Id,
                    Name = "Dr. Mary Smith",
                    Email = "faculty@vgc.ie",
                    Phone = "0851234567"
                };
                db.FacultyProfiles.Add(fp1);
            }

            if (faculty2 != null)
            {
                fp2 = new FacultyProfile
                {
                    IdentityUserId = faculty2.Id,
                    Name = "Prof. John Byrne",
                    Email = "faculty2@vgc.ie",
                    Phone = "0861234567"
                };
                db.FacultyProfiles.Add(fp2);
            }

            await db.SaveChangesAsync();
        }

        // Seed student users + profiles
        await SeedStudentAsync(userManager, db, "student1@vgc.ie", "Student123!", "Alice Murphy", "S001");
        await SeedStudentAsync(userManager, db, "student2@vgc.ie", "Student123!", "Bob Ryan", "S002");
        await SeedStudentAsync(userManager, db, "student3@vgc.ie", "Student123!", "Carol Dunne", "S003");
        await SeedStudentAsync(userManager, db, "student4@vgc.ie", "Student123!", "Dave Kelly", "S004");

        // Seed assignments
        if (!db.Assignments.Any())
        {
            var swDev = db.Courses.First(c => c.Name == "Software Development");
            var data = db.Courses.First(c => c.Name == "Data Analytics");

            db.Assignments.AddRange(
                new Assignment { CourseId = swDev.Id, Title = "Project 1 — Web App", MaxScore = 100, DueDate = new DateTime(2026, 2, 1) },
                new Assignment { CourseId = swDev.Id, Title = "Assignment 2 — OOP Basics", MaxScore = 50, DueDate = new DateTime(2026, 3, 1) },
                new Assignment { CourseId = swDev.Id, Title = "Assignment 3 — MVC", MaxScore = 50, DueDate = new DateTime(2026, 4, 1) },
                new Assignment { CourseId = data.Id, Title = "Data Report 1", MaxScore = 100, DueDate = new DateTime(2026, 2, 15) },
                new Assignment { CourseId = data.Id, Title = "SQL Assignment", MaxScore = 75, DueDate = new DateTime(2026, 3, 15) }
            );
            await db.SaveChangesAsync();
        }

        // Seed exams
        if (!db.Exams.Any())
        {
            var swDev = db.Courses.First(c => c.Name == "Software Development");
            var data = db.Courses.First(c => c.Name == "Data Analytics");

            db.Exams.AddRange(
                new Exam { CourseId = swDev.Id, Title = "Midterm Exam", Date = new DateTime(2026, 2, 20), MaxScore = 100, ResultsReleased = true },
                new Exam { CourseId = swDev.Id, Title = "Final Exam", Date = new DateTime(2026, 5, 20), MaxScore = 100, ResultsReleased = false },
                new Exam { CourseId = data.Id, Title = "Data Midterm", Date = new DateTime(2026, 2, 25), MaxScore = 100, ResultsReleased = true },
                new Exam { CourseId = data.Id, Title = "Data Final Exam", Date = new DateTime(2026, 5, 25), MaxScore = 100, ResultsReleased = false }
            );
            await db.SaveChangesAsync();
        }

        // Seed assignment results
        if (!db.AssignmentResults.Any())
        {
            var students = db.StudentProfiles.ToList();
            var assignments = db.Assignments.ToList();

            foreach (var student in students)
            {
                foreach (var assignment in assignments)
                {
                    db.AssignmentResults.Add(new AssignmentResult
                    {
                        AssignmentId = assignment.Id,
                        StudentProfileId = student.Id,
                        Score = new Random().Next(40, assignment.MaxScore),
                        Feedback = "Good work, keep it up!"
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        // Seed exam results
        if (!db.ExamResults.Any())
        {
            var students = db.StudentProfiles.ToList();
            var exams = db.Exams.ToList();

            foreach (var student in students)
            {
                foreach (var exam in exams)
                {
                    var score = new Random().Next(40, 100);
                    db.ExamResults.Add(new ExamResult
                    {
                        ExamId = exam.Id,
                        StudentProfileId = student.Id,
                        Score = score,
                        Grade = score >= 70 ? "A" : score >= 55 ? "B" : score >= 40 ? "C" : "F"
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        // Seed attendance records
        if (!db.AttendanceRecords.Any())
        {
            var enrolments = db.CourseEnrolments.ToList();
            foreach (var enrolment in enrolments)
            {
                for (int week = 1; week <= 8; week++)
                {
                    db.AttendanceRecords.Add(new AttendanceRecord
                    {
                        CourseEnrolmentId = enrolment.Id,
                        WeekNumber = week,
                        Present = new Random().Next(0, 2) == 1
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task<IdentityUser?> CreateUserAsync(
        UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) != null) return null;
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    private static async Task SeedStudentAsync(
        UserManager<IdentityUser> userManager, AppDbContext db,
        string email, string password, string name, string studentNumber)
    {
        var user = await CreateUserAsync(userManager, email, password, "Student");
        if (user == null) return;

        var course = db.Courses.First();
        var faculty = db.FacultyProfiles.FirstOrDefault();

        var profile = new StudentProfile
        {
            IdentityUserId = user.Id,
            Name = name,
            Email = email,
            Phone = "086000000" + studentNumber.Last(),
            Address = "Dublin, Ireland",
            DOB = new DateTime(2002, 6, 15),
            StudentNumber = studentNumber
        };
        db.StudentProfiles.Add(profile);
        await db.SaveChangesAsync();

        db.CourseEnrolments.Add(new CourseEnrolment
        {
            StudentProfileId = profile.Id,
            CourseId = course.Id,
            EnrolDate = DateTime.Now,
            Status = "Active",
            FacultyProfileId = faculty?.Id
        });
        await db.SaveChangesAsync();
    }
}