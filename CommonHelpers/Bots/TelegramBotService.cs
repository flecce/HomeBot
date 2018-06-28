using CommonHelpers.Gardens;
using CommonHelpers.Powers;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CommonHelpers.Bots
{
    public class TelegramBotService : IBotService
    {
        private readonly TelegramBotClient _currentBot = null;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly IGardenService _gardenService;
        private readonly IPowerService _powerService;
        private CancellationTokenSource _onStartCancelationToken;

        private PowerDataInfo _lastProductionValue = new PowerDataInfo
        {
            DailyProductionValue = 0.0,
            ConsumationValue = 0.0,
            DateRef = DateTime.MinValue
        };

        public TelegramBotService(string botKey, IGardenService gardenService, ILogger<TelegramBotService> logger, IPowerService powerService)
        {
            _logger = logger;
            _powerService = powerService;
            _currentBot = new TelegramBotClient(botKey);

            _currentBot.OnMessage += BotOnMessageReceived;
            _currentBot.OnMessageEdited += BotOnMessageReceived;
            _gardenService = gardenService;
            _gardenService.SubscribeOnStart(_gardenOnStartHandler);
            _gardenService.SubscribeOnStop(_gardenOnStopHandler);

            _powerService.SubscribeOnValueAcquired(_powerValueAcquiredHandler);

            User currentBotInfo = _currentBot.GetMeAsync().GetAwaiter().GetResult();
            _logger.LogDebug($"Connected as: {currentBotInfo.Username}");
        }

        private void _powerValueAcquiredHandler(PowerDataBase data)
        {
            if (data.GetType() == typeof(ProductionDataInfo))
            {
                _lastProductionValue.DailyProductionValue = ((ProductionDataInfo)data).DailyValue;

                _lastProductionValue.CurrentProductionValue = ((ProductionDataInfo)data).CurrentValue;
            }
        }

        public void Start()
        {
            _currentBot.StartReceiving(Array.Empty<UpdateType>());
            _logger.LogDebug($"Start recv messages");
        }

        public void Stop()
        {
            _currentBot.StopReceiving();
            _logger.LogDebug($"Stop Telegram");
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            _logger.LogDebug($"Data recv: {message.Text}");
            if (message == null || message.Type != MessageType.Text) return;

            switch (message.Text.Split(' ').First())
            {
                case "/gardenoff":
                    _gardenService.ForceStop();
                    await _currentBot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Chiuso");
                    break;

                case "/gardenon":
                    _gardenService.ForceStart();
                    await _currentBot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Attivo");
                    break;

                case "/power":
                    string value = $"Prod gg:{_lastProductionValue.DailyProductionValue} - Prod cur:{_lastProductionValue.CurrentProductionValue} - Consumo corrente:{_lastProductionValue.ConsumationValue}";
                    await _currentBot.SendTextMessageAsync(
                        message.Chat.Id,
                        value);
                    break;

                default:
                    const string usage = @"
Usage:
/gardenoff   - Apre
/gardenon - Chiude
/power - Dati di consumo/generazione";

                    await _currentBot.SendTextMessageAsync(
                        message.Chat.Id,
                        usage,
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        private void _gardenOnStartHandler()
        {
            _logger.LogDebug($"_gardenOnStartHandler::ON");
            _onStartCancelationToken = new CancellationTokenSource();
            //1200000 - 20 minuti
            Task.Delay(1200000, _onStartCancelationToken.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    _gardenService.ForceStop();
                    _currentBot.SendTextMessageAsync(
                         195243681,
                         "Fermo io");
                }
            });

            Task.Run(() =>
            {
                while (_onStartCancelationToken != null)
                {
                    _currentBot.SendTextMessageAsync(
                          195243681,
                          "Occhio acqua aperta");
                    Thread.Sleep(60000);
                }
            });
        }

        private void _gardenOnStopHandler()
        {
            _onStartCancelationToken?.Cancel();
            _currentBot.SendTextMessageAsync(
                         195243681,
                         "Acqua ferma");
            _onStartCancelationToken = null;
        }

        private class PowerDataInfo
        {
            public Double DailyProductionValue { get; set; }

            public Double CurrentProductionValue { get; set; }
            public Double ConsumationValue { get; set; }
            public DateTime DateRef { get; set; }
        }
    }
}