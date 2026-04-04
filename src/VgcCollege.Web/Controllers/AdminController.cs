using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;

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
}