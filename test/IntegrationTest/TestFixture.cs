namespace IntegrationTest
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.PlatformAbstractions;
    using Web;
    using Web.Data;

    /// <summary>
    /// The TestFixture class is responsible for configuring and creating the TestServer, 
    /// setting up an HttpClient to communicate with the TestServer. Each of the integration tests 
    /// uses the Client property to connect to the test server and make a request.
    /// </summary>
    public class TestFixture<TStartup> : IDisposable
    {
        private const string SolutionName = "SampleWebUnitTests.sln";
        private readonly TestServer _server;
        private static IServiceScopeFactory _scopeFactory;

        public HttpClient Client { get; }

        

        public TestFixture() : this(Path.Combine("src"))
        {
        }

        protected TestFixture(string solutionRelativeTargetProjectParentDir)
        {
            // Note...using the actual Startup from the MVC project so that we are getting the actual SUT path
            var startupAssembly = typeof(Startup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath(solutionRelativeTargetProjectParentDir, startupAssembly);

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .ConfigureServices(InitializeServices)
                .UseEnvironment("Development")
                .UseStartup(typeof(TStartup));

            _server = new TestServer(builder);

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            _scopeFactory = provider.GetService<IServiceScopeFactory>();

            /*Iverride other services?*/

            // Since we are using our own tests, we will need to be able to configure our services. This
            // Most likely will need to be reflective of what
            //    services.AddMvc();

//           var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
//
//            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
//            var manager = new ApplicationPartManager();
//            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));
//
//            manager.FeatureProviders.Add(new ControllerFeatureProvider());
//            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());
//
//            services.AddSingleton(manager);
        }

        /// <summary>
        /// Gets the full path to the target project path that we wish to test
        /// </summary>
        /// <param name="solutionRelativePath">
        /// The parent directory of the target project.
        /// e.g. src, samples, test, or test/Websites
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>The full path to the target project.</returns>
        private static string GetProjectPath(string solutionRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;

            // Find the folder which contains the solution file. We then use this information to find the target
            // project which we want to test.
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, SolutionName));
                if (solutionFileInfo.Exists)
                {
                    return Path.GetFullPath(Path.Combine(directoryInfo.FullName, solutionRelativePath, projectName));
                }

                directoryInfo = directoryInfo.Parent;
            } while (directoryInfo.Parent != null);

            throw new Exception($"Solution root could not be located using application root {applicationBasePath}.");
        }

        public Task<T> FindAsync<T, TId>(int id)
            where T : Entity<TId>
        {
            return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id));
        }

        public Task InsertAsync<TId>(params Entity<TId>[] entities)
        {
            return ExecuteDbContextAsync(db =>
            {
                foreach (var entity in entities)
                {
                   // db.Set(entity.GetType()).Add(entity);
                   
                }
                return db.SaveChangesAsync();
            });
        }

        // Wrapper Method to execute DbContext Code.
        public Task ExecuteDbContextAsync(Func<EmployeeDbContext, Task> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<EmployeeDbContext>()));
        }

        public Task<T> ExecuteDbContextAsync<T>(Func<EmployeeDbContext, Task<T>> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<EmployeeDbContext>()));
        }


        /*
         * Fixture Seems for Setup / Execute / Verify
         * Execute Each scope in a Committed Transaction.
         * Using ASP.NET Core's DI, we need to create a Scope for 
         * Scoped Dependencies
         */
        public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            // Using ASP.NET Core's DI. Create a Scope for Scoped Dependencies.
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<EmployeeDbContext>();

                try
                {
                    dbContext.BeginTransaction();

                    await action(scope.ServiceProvider);

                    await dbContext.CommitTransactionAsync();
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            // Using ASP.NET Core's DI. Create a Scope for Scoped Dependencies.
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<EmployeeDbContext>();

                try
                {
                    dbContext.BeginTransaction();

                    var result = await action(scope.ServiceProvider);

                    await dbContext.CommitTransactionAsync();

                    return result;
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }
    }
}