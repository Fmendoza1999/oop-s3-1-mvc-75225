using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;

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

        // Only show students enrolled in faculty's courses
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
}