namespace ModelBaseAPI.Models.Response
{
    public class EmployeePagination
    {
        public List<EmployeeResponse>? Employees { get; set;}
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
