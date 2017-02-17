namespace IntegrationTest
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Web;
    using Web.Data;

    public class TestStartup : Startup
    {
        public TestStartup(IHostingEnvironment env) : base(env)
        {
        }

        protected override void SetupDatabase(IServiceCollection services)
        {

            var options = new DbContextOptionsBuilder<EmployeeDbContext>().UseInMemoryDatabase().Options;
            

            services.AddScoped(_ =>  new EmployeeDbContext(options));

            services.AddDbContext<EmployeeDbContext>(builder => builder.UseInMemoryDatabase());

        }


        protected override void EnsureDatabaseCreated(IApplicationBuilder app)
        {


            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
              .CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<EmployeeDbContext>();
                //dbContext.Database.OpenConnection(); // see Resource #2 link why we do this
                dbContext.Database.EnsureCreated();
                // run Migrations
                //dbContext.Database.Migrate();
            }
            //dbContext.Database.OpenConnection(); // see Resource #2 link why we do this
            //dbContext.Database.EnsureCreated();

            //  dbContext.Database.Migrate();
        }
    }
}