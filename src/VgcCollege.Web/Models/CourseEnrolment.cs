namespace VgcCollege.Web.Models;

public class CourseEnrolment
{
    public int Id { get; set; }
    public int StudentProfileId { get; set; }
    public StudentProfile Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int? FacultyProfileId { get; set; }
    public FacultyProfile? Faculty { get; set; }
    public DateTime EnrolDate { get; set; }
    public string Status { get; set; } = "Active";
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}