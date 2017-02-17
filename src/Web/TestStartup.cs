namespace Web
{
    using Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class TestStartup : Startup
    {
        public TestStartup(IHostingEnvironment env) : base(env)
        {
        }

        protected override void SetupDatabase(IServiceCollection services)
        {
             services.AddDbContext<EmployeeDbContext>(
                optionsBuilder => optionsBuilder.UseInMemoryDatabase());
        }

        protected override void EnsureDatabaseCreated(EmployeeDbContext dbContext)
        {
            dbContext.Database.OpenConnection(); // see Resource #2 link why we do this
            dbContext.Database.EnsureCreated();

           dbContext.Database.Migrate();
        }
    }
}