using AuthManager;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthManager_Test
{
    public class TestFixture
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public TestFixture()
        {
            ServiceCollection serviceCollection = new();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register your dependencies here
            Startup.AddServices(services);
            Startup.AddMapper(services);

            // Weitere Services und Konfigurationen
        }

        public T? CreateUnitUnderTest<T>()
        {
            return this.ServiceProvider.GetService<T>();
        }
    }
}
