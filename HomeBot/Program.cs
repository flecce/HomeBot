using CommonHelpers;
using CommonHelpers.Bots;
using CommonHelpers.Gardens;
using CommonHelpers.Gardens.Water;
using CommonHelpers.Inverters.Persisters;
using CommonHelpers.Logs.MQTTProvider;
using CommonHelpers.MQTTs;
using CommonHelpers.Powers;
using CommonHelpers.Schedulers;
using CommonHelpers.Schedulers.Tasks;
using CommonHelpers.Times;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Homebot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ServiceFactory.CurrentServiceProvider = _configureServices(serviceCollection);

            var botService = ServiceFactory.CurrentServiceProvider.GetService<IBotService>();
            var schedulerFactory = ServiceFactory.CurrentServiceProvider.GetService<ISchedulerFactory>();
            schedulerFactory.AddTask("", () => new InverterTask());

            schedulerFactory.Start();
            botService.Start();
        }

        private static ServiceProvider _configureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISchedulerFactory, CronSchedulerFactory>();
            serviceCollection.AddSingleton<ITimeService, RealTimeService>();
            serviceCollection.AddSingleton<IPersisterFactory, PersisterFactory>();
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
                    factory.GetService<IMQTTQueueService>(),
                    factory.GetService<IGardenWaterControllerService>(),
                    factory.GetService<ILogger<GardenService>>());
            });

            serviceCollection.AddSingleton<IBotService, TelegramBotService>(factory =>
            {
                return new TelegramBotService(
                    factory.GetService<IConfigurationService>().GetRequiredConfigValue("Telegram:BotKey"),
                    factory.GetService<IGardenService>(),
                    factory.GetService<ILogger<TelegramBotService>>(),
                    factory.GetService<IPowerService>());
            });

            serviceCollection.AddSingleton<IPowerService, PowerService>(factory =>
                       {
                           return new PowerService(
                              factory.GetService<IMQTTQueueService>(),
                               factory.GetService<ILogger<PowerService>>());
                       });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetRequiredService<ILoggerFactory>()
                            .AddConsole(LogLevel.Debug)
                            .AddMQTTLogger(new MQTTLoggerConfiguration
                            {
                                BrokerServer = serviceProvider.GetService<IConfigurationService>().GetRequiredConfigValue("MQTT:Broker:Address"),
                                LogLevel = LogLevel.Debug
                            },
                            serviceProvider.GetService<IMQTTQueueService>());
            return serviceProvider;
        }
    }
}