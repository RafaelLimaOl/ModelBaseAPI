namespace ModelBaseAPI.Models.Request
{
    public class EmployeeRequest
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public int Age { get; set; }
        public required string Occupation { get; set; }
    }
}
