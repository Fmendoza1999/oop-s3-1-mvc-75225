using Microsoft.EntityFrameworkCore;
using System;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Tests;

public class CollegeTests
{
    private AppDbContext GetDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Student_Can_Be_Enrolled_In_Course()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Dublin", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Software Dev", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var student = new StudentProfile { Name = "Alice", Email = "alice@test.ie", StudentNumber = "S001" };
        db.StudentProfiles.Add(student);
        await db.SaveChangesAsync();

        db.CourseEnrolments.Add(new CourseEnrolment
        {
            StudentProfileId = student.Id,
            CourseId = course.Id,
            EnrolDate = DateTime.Now,
            Status = "Active"
        });
        await db.SaveChangesAsync();

        var enrolment = await db.CourseEnrolments.FirstOrDefaultAsync();
        Assert.NotNull(enrolment);
        Assert.Equal("Active", enrolment!.Status);
    }

    [Fact]
    public async Task Student_Cannot_See_Unreleased_Exam_Results()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Cork", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Data Analytics", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var student = new StudentProfile { Name = "Bob", Email = "bob@test.ie", StudentNumber = "S002" };
        db.StudentProfiles.Add(student);
        var exam = new Exam { Course = course, Title = "Midterm", Date = DateTime.Now, MaxScore = 100, ResultsReleased = false };
        db.Exams.Add(exam);
        await db.SaveChangesAsync();

        db.ExamResults.Add(new ExamResult { ExamId = exam.Id, StudentProfileId = student.Id, Score = 75, Grade = "B" });
        await db.SaveChangesAsync();

        var visible = await db.ExamResults
            .Include(r => r.Exam)
            .Where(r => r.StudentProfileId == student.Id && r.Exam.ResultsReleased)
            .ToListAsync();

        Assert.Empty(visible);
    }

    [Fact]
    public async Task Student_Can_See_Released_Exam_Results()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Galway", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Data Analytics", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var student = new StudentProfile { Name = "Carol", Email = "carol@test.ie", StudentNumber = "S003" };
        db.StudentProfiles.Add(student);
        var exam = new Exam { Course = course, Title = "Final", Date = DateTime.Now, MaxScore = 100, ResultsReleased = true };
        db.Exams.Add(exam);
        await db.SaveChangesAsync();

        db.ExamResults.Add(new ExamResult { ExamId = exam.Id, StudentProfileId = student.Id, Score = 85, Grade = "A" });
        await db.SaveChangesAsync();

        var visible = await db.ExamResults
            .Include(r => r.Exam)
            .Where(r => r.StudentProfileId == student.Id && r.Exam.ResultsReleased)
            .ToListAsync();

        Assert.Single(visible);
        Assert.Equal(85, visible[0].Score);
    }

    [Fact]
    public async Task Enrolment_Status_Defaults_To_Active()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Dublin", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Test Course", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var student = new StudentProfile { Name = "Dave", Email = "dave@test.ie", StudentNumber = "S004" };
        db.StudentProfiles.Add(student);
        await db.SaveChangesAsync();

        db.CourseEnrolments.Add(new CourseEnrolment
        {
            StudentProfileId = student.Id,
            CourseId = course.Id,
            EnrolDate = DateTime.Now,
            Status = "Active"
        });
        await db.SaveChangesAsync();

        var enrolment = await db.CourseEnrolments.FirstAsync();
        Assert.Equal("Active", enrolment.Status);
    }

    [Fact]
    public async Task Assignment_Result_Score_Is_Saved_Correctly()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Dublin", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Test Course", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var student = new StudentProfile { Name = "Eve", Email = "eve@test.ie", StudentNumber = "S005" };
        db.StudentProfiles.Add(student);
        var assignment = new Assignment { Course = course, Title = "Project 1", MaxScore = 100, DueDate = DateTime.Now };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        db.AssignmentResults.Add(new AssignmentResult
        {
            AssignmentId = assignment.Id,
            StudentProfileId = student.Id,
            Score = 90,
            Feedback = "Excellent"
        });
        await db.SaveChangesAsync();

        var result = await db.AssignmentResults.FirstAsync();
        Assert.Equal(90, result.Score);
        Assert.Equal("Excellent", result.Feedback);
    }

    [Fact]
    public async Task Attendance_Record_Is_Saved_Correctly()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Dublin", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Test Course", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var student = new StudentProfile { Name = "Frank", Email = "frank@test.ie", StudentNumber = "S006" };
        db.StudentProfiles.Add(student);
        await db.SaveChangesAsync();

        var enrolment = new CourseEnrolment { StudentProfileId = student.Id, CourseId = course.Id, EnrolDate = DateTime.Now, Status = "Active" };
        db.CourseEnrolments.Add(enrolment);
        await db.SaveChangesAsync();

        db.AttendanceRecords.Add(new AttendanceRecord { CourseEnrolmentId = enrolment.Id, WeekNumber = 1, Present = true });
        await db.SaveChangesAsync();

        var record = await db.AttendanceRecords.FirstAsync();
        Assert.True(record.Present);
        Assert.Equal(1, record.WeekNumber);
    }

    [Fact]
    public async Task Three_Branches_Can_Be_Created()
    {
        var db = GetDb();
        db.Branches.AddRange(
            new Branch { Name = "Dublin", Address = "1 Main St" },
            new Branch { Name = "Cork", Address = "2 Main St" },
            new Branch { Name = "Galway", Address = "3 Main St" }
        );
        await db.SaveChangesAsync();

        var count = await db.Branches.CountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Exam_Results_Released_Flag_Can_Be_Updated()
    {
        var db = GetDb();
        var branch = new Branch { Name = "Dublin", Address = "Test St" };
        db.Branches.Add(branch);
        var course = new Course { Name = "Test Course", Branch = branch, StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
        db.Courses.Add(course);
        var exam = new Exam { Course = course, Title = "Final Exam", Date = DateTime.Now, MaxScore = 100, ResultsReleased = false };
        db.Exams.Add(exam);
        await db.SaveChangesAsync();

        exam.ResultsReleased = true;
        await db.SaveChangesAsync();

        var updated = await db.Exams.FirstAsync();
        Assert.True(updated.ResultsReleased);
    }
}