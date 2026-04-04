namespace VgcCollege.Web.Models;

public class FacultyProfile
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public ICollection<CourseEnrolment> CourseEnrolments { get; set; } = new List<CourseEnrolment>();
}