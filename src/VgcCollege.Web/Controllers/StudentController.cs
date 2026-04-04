using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public StudentController(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _db.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);
        if (profile == null) return NotFound();
        return View(profile);
    }

    public async Task<IActionResult> MyResults()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _db.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

        // Only show RELEASED exam results
        var results = await _db.ExamResults
            .Include(r => r.Exam)
            .Where(r => r.StudentProfileId == profile!.Id && r.Exam.ResultsReleased)
            .ToListAsync();

        return View(results);
    }

    public async Task<IActionResult> MyGrades()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _db.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

        var grades = await _db.AssignmentResults
            .Include(r => r.Assignment)
            .Where(r => r.StudentProfileId == profile!.Id)
            .ToListAsync();

        return View(grades);
    }

    public async Task<IActionResult> MyAttendance()
    {
        var userId = _userManager.GetUserId(User);
        var profile = await _db.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

        var enrolments = await _db.CourseEnrolments
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .Where(e => e.StudentProfileId == profile!.Id)
            .ToListAsync();

        return View(enrolments);
    }
}