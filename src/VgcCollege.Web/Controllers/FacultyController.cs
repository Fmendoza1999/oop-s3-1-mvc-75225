using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public FacultyController(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var faculty = await _db.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
        if (faculty == null) return NotFound();

        var enrolments = await _db.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.FacultyProfileId == faculty.Id)
            .ToListAsync();

        return View(enrolments);
    }

    public async Task<IActionResult> Gradebook()
    {
        var userId = _userManager.GetUserId(User);
        var faculty = await _db.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);

        var results = await _db.AssignmentResults
            .Include(r => r.Student)
            .Include(r => r.Assignment)
            .ThenInclude(a => a.Course)
            .Where(r => r.Assignment.Course.Enrolments
                .Any(e => e.FacultyProfileId == faculty!.Id))
            .ToListAsync();

        return View(results);
    }

    public async Task<IActionResult> AddResult()
    {
        var userId = _userManager.GetUserId(User);
        var faculty = await _db.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);

        ViewBag.Students = await _db.StudentProfiles.ToListAsync();
        ViewBag.Assignments = await _db.Assignments
            .Include(a => a.Course)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddResult(int assignmentId, int studentProfileId, int score, string feedback)
    {
        var existing = await _db.AssignmentResults
            .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.StudentProfileId == studentProfileId);

        if (existing != null)
        {
            existing.Score = score;
            existing.Feedback = feedback;
        }
        else
        {
            _db.AssignmentResults.Add(new AssignmentResult
            {
                AssignmentId = assignmentId,
                StudentProfileId = studentProfileId,
                Score = score,
                Feedback = feedback
            });
        }
        await _db.SaveChangesAsync();
        return RedirectToAction("Gradebook");
    }

    public async Task<IActionResult> AddExamResult()
    {
        ViewBag.Students = await _db.StudentProfiles.ToListAsync();
        ViewBag.Exams = await _db.Exams
            .Include(e => e.Course)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddExamResult(int examId, int studentProfileId, int score, string grade)
    {
        var existing = await _db.ExamResults
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentProfileId);

        if (existing != null)
        {
            existing.Score = score;
            existing.Grade = grade;
        }
        else
        {
            _db.ExamResults.Add(new ExamResult
            {
                ExamId = examId,
                StudentProfileId = studentProfileId,
                Score = score,
                Grade = grade
            });
        }
        await _db.SaveChangesAsync();
        return RedirectToAction("Gradebook");
    }
}