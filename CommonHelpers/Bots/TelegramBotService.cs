using CommonHelpers.Gardens;
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

        private CancellationTokenSource _onStartCancelationToken;

        public TelegramBotService(string botKey, IGardenService gardenService, ILogger<TelegramBotService> logger)
        {
            _logger = logger;
            _currentBot = new TelegramBotClient(botKey);

            _currentBot.OnMessage += BotOnMessageReceived;
            _currentBot.OnMessageEdited += BotOnMessageReceived;
            _gardenService = gardenService;
            _gardenService.SubscribeOnStart(_gardenOnStartHandler);
            _gardenService.SubscribeOnStop(_gardenOnStopHandler);

            User currentBotInfo = _currentBot.GetMeAsync().GetAwaiter().GetResult();
            _logger.LogDebug($"Connected as: {currentBotInfo.Username}");
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
                // send inline keyboard
                case "/gardenoff":
                    _gardenService.ForceStop();
                    await _currentBot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Chiuso");
                    break;

                // send custom keyboard
                case "/gardenon":
                    _gardenService.ForceStart();
                    await _currentBot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Attivo");
                    break;

                default:
                    const string usage = @"
Usage:
/gardenoff   - Apre
/gardenon - Chiude";

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
    }
}