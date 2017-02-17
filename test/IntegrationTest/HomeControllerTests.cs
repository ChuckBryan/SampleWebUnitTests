using System.Threading.Tasks;
using Shouldly;

namespace IntegrationTest
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Web;
    using Xunit;

    //
    public class HomeControllerTests : IClassFixture<TestFixture<TestStartup>>
    {
        private readonly HttpClient _client;

        public HomeControllerTests(TestFixture<TestStartup> fixture)
        {
            // Note: By default, TestServer hosts the web app in the folder
            // where it is running. In this case, the test project folder. So,
            // it will not be able to find the views.

            // Arrange
            _client = fixture.Client;
        }

        [Fact]
        public async Task ReturnHelloWorld()
        {
            // Arrange - get a session known to exist
            //var testSession = Startup.GetTestSession();

            // Act
            var response = await _client.GetAsync("/");
           
            // Assert
            Should.NotThrow(() => response.EnsureSuccessStatusCode());
            
        }
    }
}