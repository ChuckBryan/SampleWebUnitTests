namespace Web.Data
{
    using Microsoft.EntityFrameworkCore;

    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
            :base(options)
        {
            
        }
        public DbSet<Employee> Employees { get; set; }
    }


    public class Employee
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

    }
}