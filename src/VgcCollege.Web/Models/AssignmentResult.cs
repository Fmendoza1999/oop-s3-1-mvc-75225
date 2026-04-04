namespace VgcCollege.Web.Models;

public class AssignmentResult
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public int StudentProfileId { get; set; }
    public StudentProfile Student { get; set; } = null!;
    public int Score { get; set; }
    public string Feedback { get; set; } = "";
}