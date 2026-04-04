namespace VgcCollege.Web.Models;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int CourseEnrolmentId { get; set; }
    public CourseEnrolment Enrolment { get; set; } = null!;
    public int WeekNumber { get; set; }
    public bool Present { get; set; }
}