namespace VgcCollege.Web.Models;

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}