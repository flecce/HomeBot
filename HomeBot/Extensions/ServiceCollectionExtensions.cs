using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HomeBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider UseAutofac(this ServiceCollection services, Action<ContainerBuilder> builderCallback = null)
        {
            var containerBuilder = new ContainerBuilder();
            
            // Populate the Autofac container with services
            containerBuilder.Populate(services);

            // Calls callback method
            builderCallback?.Invoke(containerBuilder);
            
            // Build container
            var container = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(container);
            return serviceProvider;
        }
    }
}
