namespace VgcCollege.Web.Models;

public class StudentProfile
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime DOB { get; set; }
    public string StudentNumber { get; set; } = "";
    public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();
}