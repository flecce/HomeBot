using CommonHelpers;
using CommonHelpers.Bots;
using CommonHelpers.Gardens;
using CommonHelpers.Gardens.Water;
using CommonHelpers.MQTTs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace TelegramBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ServiceProvider iocContainer = _configureServices(serviceCollection);

            var botService = iocContainer.GetService<IBotService>();

            botService.Start();            
            Console.ReadLine();
            botService.Stop();
        }

        private static ServiceProvider _configureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            serviceCollection.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Debug));

            serviceCollection.AddTransient<IConfigurationService, AppSettingsXMLConfigurationService>();
            serviceCollection.AddTransient<IGardenWaterControllerService, ArduinoGardenWaterControllerService>(factory =>
            {
                return new ArduinoGardenWaterControllerService(new Uri(factory.GetService<IConfigurationService>().GetRequiredConfigValue("Arduino:Address")));
            });

            serviceCollection.AddTransient<IMQTTQueueService, MQTTQueueService>(factory =>
            {
                return new MQTTQueueService(
                    factory.GetService<IConfigurationService>().GetRequiredConfigValue("MQTT:Broker:Address"),
                    factory.GetService<ILogger<MQTTQueueService>>());
            });

            serviceCollection.AddSingleton<IGardenService, GardenService>(factory =>
            {
                return new GardenService(
                    factory.GetService<IMQTTQueueService>(), factory.GetService<IGardenWaterControllerService>(),
                    factory.GetService<ILogger<GardenService>>());
            });

            serviceCollection.AddSingleton<IBotService, TelegramBotService>(factory =>
            {
                return new TelegramBotService(
                    factory.GetService<IConfigurationService>().GetRequiredConfigValue("Telegram:BotKey"),
                    factory.GetService<IGardenService>(),
                    factory.GetService<ILogger<TelegramBotService>>());
            });
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetRequiredService<ILoggerFactory>()
                            .AddConsole(LogLevel.Debug);
            return serviceProvider;
        }
    }
}