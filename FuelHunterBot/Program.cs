using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace FuelHunterBot
{
    public static class Program
    {
        private static TelegramBotClient Bot;

        public static List<Users> FueslUsers = new List<Users>();

        public static async Task Main()
        {
            Bot = new TelegramBotClient(Configuration.BotToken);

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;


            if (message == null)
                return;
            if (message.Type == MessageType.Location && message.ReplyToMessage != null && message.ReplyToMessage.From.Id == Bot.BotId)
            {
                var user = FueslUsers.FirstOrDefault(item => item.Id == message.From.Id);
                if (user == null)
                {
                    FueslUsers.Add(new Users { Id = message.Chat.Id, CurrentLat = message.Location.Latitude, CurrentLong = message.Location.Longitude, OrderOfNearness = 0 });
                }
                else
                    user.OrderOfNearness = 0;

                await SendFuelTypeChoice(message);
            }

            if (message.Type != MessageType.Text)
                return;
            switch (message.Text.Split(' ').First())
            {

                //request fuel station location
                case "/station":
                    await RequestLocation(message);
                    break;

                // request location or contact
                case "/help":
                    await SendBotDescription(message);
                    break;

                default:
                    await Usage(message);
                    break;
            }

            static async Task SendFuelTypeChoice(Message message)
            {

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Nearest Benzel Station", "Benzel Station"),
                        InlineKeyboardButton.WithCallbackData("Nearest Diseal Station", "Diesel Station"),
                    },
                });
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Choose your type of fuel",
                    replyMarkup: inlineKeyboard
                );
            }

            // Send inline keyboard

            static async Task SendBotDescription(Message message)
            {

                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "This bot allows you to find the nearest fuel station that is not congested."
                );

                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await RequestLocation(message);
            }



            static async Task Usage(Message message)
            {
                const string usage = "Usage:\n" +
                                        "/station  - request nearest station\n" +
                                        "/help   - How it works\n";
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        static async Task RequestLocation(Message message)
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                    KeyboardButton.WithRequestLocation("Location"),
                    //KeyboardButton.WithRequestContact("Contact"),
                });
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Where are you?",
                replyMarkup: RequestReplyKeyboard
            );
        }
        static async Task SendFuelStation(Message message, string type = "")
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            var randomizer = new Random();
            var fuelStations = FuelStation.GetStations();
            var user = FueslUsers.FirstOrDefault(item => item.Id == message.Chat.Id);
            FuelStation station;
            if (user == null)
                station = fuelStations.ElementAt(randomizer.Next(0, fuelStations.Count));
            else
            {
                var orderdStations = fuelStations.OrderBy(item => Math.Abs(user.CurrentLat - item.Lat) + Math.Abs(user.CurrentLong - item.Lon));
                station = orderdStations.ElementAtOrDefault(user.OrderOfNearness);
                if (station == null)
                    station = orderdStations.First();
            }

            if (!string.IsNullOrEmpty(type))
            {
                station.Title = station.Title.Replace("Station", type + "Station");
            }


            await Bot.SendVenueAsync(
                chatId: message.Chat.Id,
                latitude: station.Lat,
                longitude: station.Lon,
                title: station.Title,
                address: station.Address
            );

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                 {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Next "+type+ " Fuel Station", "Next "+type+ " Fuel Station"),
                        InlineKeyboardButton.WithCallbackData("Done", "Done"),
                    },
                });
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "If you want to request the next nearest station, choose below.",
                replyMarkup: inlineKeyboard
            );

        }

        // Process Inline Keyboard callback data
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            if (callbackQuery.Data.Contains("Next", StringComparison.CurrentCultureIgnoreCase))
            {

                var user = FueslUsers.FirstOrDefault(item => item.Id == callbackQuery.Message.Chat.Id);
                if (user != null)
                {
                    user.OrderOfNearness++;
                }
            }

            if (callbackQuery.Data.Contains("Diesel", StringComparison.CurrentCultureIgnoreCase))
                await SendFuelStation(callbackQuery.Message, "Diesel");
            else if (callbackQuery.Data.Contains("Benzel", StringComparison.CurrentCultureIgnoreCase))
                await SendFuelStation(callbackQuery.Message, "Benzel");
            else
            {

                await Bot.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Thanks for using the fuel hunters, hope this was useful to you."
                );

                await RequestLocation(callbackQuery.Message);
            }

        }

        #region Inline Mode

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };
            await Bot.AnswerInlineQueryAsync(
                inlineQueryId: inlineQueryEventArgs.InlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        #endregion

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}
