using System.ComponentModel.DataAnnotations.Schema;

namespace ModelBaseAPI.Models.Response
{
    public class EmployeeResponse
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("Name")]
        public required string Name { get; set; }
        [Column("Email")]
        public required string Email { get; set; }
        [Column("Age")]
        public int Age { get; set; }
        [Column("Occupation")]
        public required string Occupation { get; set; }
    }
}
