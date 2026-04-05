using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var branches = await _db.Branches.ToListAsync();
        return View(branches);
    }

    public async Task<IActionResult> Students()
    {
        var students = await _db.StudentProfiles.ToListAsync();
        return View(students);
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await _db.Courses
            .Include(c => c.Branch)
            .ToListAsync();
        return View(courses);
    }

    public async Task<IActionResult> Enrolments()
    {
        var enrolments = await _db.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync();
        return View(enrolments);
    }

    // Create student
    public IActionResult CreateStudent()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent(StudentProfile student)
    {
        if (ModelState.IsValid)
        {
            _db.StudentProfiles.Add(student);
            await _db.SaveChangesAsync();
            return RedirectToAction("Students");
        }
        return View(student);
    }

    // Edit student
    public async Task<IActionResult> EditStudent(int id)
    {
        var student = await _db.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost]
    public async Task<IActionResult> EditStudent(StudentProfile student)
    {
        if (ModelState.IsValid)
        {
            _db.StudentProfiles.Update(student);
            await _db.SaveChangesAsync();
            return RedirectToAction("Students");
        }
        return View(student);
    }

    // Release exam results
    public async Task<IActionResult> Exams()
    {
        var exams = await _db.Exams
            .Include(e => e.Course)
            .ToListAsync();
        return View(exams);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleRelease(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();
        exam.ResultsReleased = !exam.ResultsReleased;
        await _db.SaveChangesAsync();
        return RedirectToAction("Exams");
    }
    // Attendance
    public async Task<IActionResult> Attendance()
    {
        var enrolments = await _db.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .ToListAsync();
        return View(enrolments);
    }

    public async Task<IActionResult> MarkAttendance(int enrolmentId)
    {
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .FirstOrDefaultAsync(e => e.Id == enrolmentId);
        if (enrolment == null) return NotFound();
        return View(enrolment);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAttendance(int enrolmentId, int weekNumber, bool present)
    {
        var existing = await _db.AttendanceRecords
            .FirstOrDefaultAsync(a => a.CourseEnrolmentId == enrolmentId && a.WeekNumber == weekNumber);

        if (existing != null)
        {
            existing.Present = present;
        }
        else
        {
            _db.AttendanceRecords.Add(new AttendanceRecord
            {
                CourseEnrolmentId = enrolmentId,
                WeekNumber = weekNumber,
                Present = present
            });
        }
        await _db.SaveChangesAsync();
        return RedirectToAction("Attendance");
    }
}