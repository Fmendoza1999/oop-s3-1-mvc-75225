namespace VgcCollege.Web.Models;

public class ExamResult
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public int StudentProfileId { get; set; }
    public StudentProfile Student { get; set; } = null!;
    public int Score { get; set; }
    public string Grade { get; set; } = "";
}