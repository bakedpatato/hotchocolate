using System.Threading.Tasks;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate
{
    public class DependencyInjectionTests
    {
        [Fact]
        public async Task ScopedServiceIsCorrectlyUsed()
        {
            ISchema schema = SchemaBuilder.New()
                .AddDocumentFromString("type Query { a: Int }")
                .AddResolver("Query", "a", ctx =>
                {
                    ctx.Service<ScopedService>().Increase();
                    return ctx.Service<ScopedService>().Count;
                })
                .Create();

            IQueryExecutor executor = schema.MakeExecutable();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ScopedService>();

            IReadOnlyQueryResult result;

            using (var services = serviceCollection.BuildServiceProvider().CreateRequestServiceScope())
            {
                result = (IReadOnlyQueryResult)await executor.ExecuteAsync(
                    QueryRequestBuilder.New()
                        .SetQuery("{ a b:a c:a }")
                        .SetServices(services.ServiceProvider)
                        .Create());
            }

            result.MatchSnapshot();
        }

        public class ScopedService
        {
            private int _count;

            public int Count => _count;

            public void Increase()
            {
                _count++;
            }
        }
    }
}
