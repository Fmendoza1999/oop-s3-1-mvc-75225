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
            db.Courses.AddRange(
                new Course { Name = "Software Development", BranchId = dublin.Id, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) },
                new Course { Name = "Data Analytics", BranchId = dublin.Id, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) }
            );
            await db.SaveChangesAsync();
        }

        // Seed admin user
        await CreateUserAsync(userManager, "admin@vgc.ie", "Admin123!", "Admin");
        await CreateUserAsync(userManager, "felix@vgc.ie", "Felix123!", "Admin");

        // Seed faculty user + profile
        var faculty = await CreateUserAsync(userManager, "faculty@vgc.ie", "Faculty123!", "Faculty");
        if (faculty != null && !db.FacultyProfiles.Any())
        {
            var course = db.Courses.First();
            var profile = new FacultyProfile
            {
                IdentityUserId = faculty.Id,
                Name = "Dr. Mary Smith",
                Email = "faculty@vgc.ie",
                Phone = "0851234567"
            };
            db.FacultyProfiles.Add(profile);
            await db.SaveChangesAsync();
        }

        // Seed student users + profiles
        await SeedStudentAsync(userManager, db, "student1@vgc.ie", "Student123!", "Alice Murphy", "S001");
        await SeedStudentAsync(userManager, db, "student2@vgc.ie", "Student123!", "Bob Ryan", "S002");
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
            Status = "Active"
        });
        await db.SaveChangesAsync();
    }
}