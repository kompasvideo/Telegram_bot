using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Example_941
{
    class Program
    {
        static TelegramBotClient bot;
        static string path;
        static int poz;
        static List<TypeMessage> d_audioPhoto;
        static Dictionary<int, TypeMessage> d_files;
        static string token;
        static void Main(string[] args)
        {
            // Создать бота, позволяющего принимать разные типы файлов, 
            // *Научить бота отправлять выбранный файл в ответ
            // 
            // https://data.mos.ru/
            // https://apidata.mos.ru/
            // 
            // https://vk.com/dev
            // https://vk.com/dev/manuals

            // https://dev.twitch.tv/
            // https://discordapp.com/developers/docs/intro
            // https://discordapp.com/developers/applications/
            // https://discordapp.com/verification

            // Done! Congratulations on your new bot.You will find it at t.me / Test20210404Bot.You can now add a description,
            // about section and profile picture for your bot, see / help for a list of commands.By the way, when you've finished
            // creating your cool bot, ping our Bot Support if you want a better username for it. Just make sure the bot is fully
            // operational before you do this.


            // Use this token to access the HTTP API:
            //1732208764:AAF-JPzVhiq2nOeFV11qd2HETfHDNyUFxoY
            // Keep your token secure and store it safely, it can be used by anyone to control your bot.
            // For a description of the Bot API, see this page: https://core.telegram.org/bots/api

            token = "1732208764:AAF-JPzVhiq2nOeFV11qd2HETfHDNyUFxoY";
            path = Directory.GetCurrentDirectory();
            d_files = new Dictionary<int, TypeMessage>();
            d_audioPhoto = new List<TypeMessage>();
            poz = 0;
            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.OnMessageEdited += MessageListener;
            bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            bot.OnInlineQuery += BotOnInlineQueryReceived;
            bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            bot.OnReceiveError += BotOnReceiveError;

            bot.StartReceiving(Array.Empty<UpdateType>());

            Console.ReadLine();
            bot.StopReceiving();            
        }        

        private static async void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            Console.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");
            int userId = e.Message.From.Id;
            string l_path = path + "\\" + userId.ToString();
            string l_fileName;
            if (!Directory.Exists(l_path))
            {
                Directory.CreateDirectory(l_path);
            }
            Directory.SetCurrentDirectory(l_path);
            TypeMessage fileName = new TypeMessage();
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                Console.WriteLine(e.Message.Document.FileId);
                Console.WriteLine(e.Message.Document.FileName);
                Console.WriteLine(e.Message.Document.FileSize);

                DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);
            }
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo )
            {
                fileName.type = Type.Photo;
                Telegram.Bot.Types.PhotoSize[] photoSizes = e.Message.Photo;
                fileName.fileName = photoSizes[0].FileId;
                d_audioPhoto.Add(fileName);
                Console.WriteLine(photoSizes[0].FileId);                
            }
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Audio)
            {
                fileName.type = Type.Audio;
                fileName.fileName = e.Message.Audio.FileId;
                d_audioPhoto.Add(fileName);
                Console.WriteLine(e.Message.Audio.FileId);
            }

            var messageText = e.Message.Text;

            if (e.Message.Text == null) return;
            if (e.Message.Text == "/start")
            {
                messageText = "Вас приветствует бот 'test20210404_bot'\n" +
                    "Вы можете управлять мной, отправляя эти команды: \n" +
                    "/list - просмотреть список загруженных файлов\n" +
                    "/load_n - скачать выбранный файл, где n - число\n" +
                    "/help - справка по поддерживаемым коммандам\n";

                await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"{messageText}");

            }
            if (e.Message.Text == "/help")
            {
                messageText = "Вы можете управлять мной, отправляя эти команды: \n" +
                    "/list - просмотреть список загруженных файлов\n" +
                    "/load_n - скачать выбранный файл, где n - число\n" +
                    "/help - справка по поддерживаемым коммандам\n";
                await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"{messageText}");

            }
            if (e.Message.Text == "/list")
            {
                messageText = GetFiles(l_path);
                await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"{messageText}");
            }            
            string str_load = e.Message.Text.Substring(0,1);
            if (str_load == "/")
            {
                if (e.Message.Text.Length >= 6)
                {
                    int i = e.Message.Text.CompareTo("/load_");
                    if (i >= 0)
                    {
                        string[] subString = e.Message.Text.Split('_');
                        string str_nomer = subString[1];
                        int nomer;
                        string url;
                        TypeMessage name = null;
                        if (int.TryParse(str_nomer, out nomer))
                        {
                            try
                            {
                                name = d_files[nomer];
                                switch(name.type)
                                {
                                    case Type.Document:
                                        ////l_fileName = string.Format($"attach://")+l_path+"\\"+name.fileName;
                                        //l_fileName = l_path + "\\" + name.fileName;
                                        //using (FileStream stream = System.IO.File.OpenRead(l_fileName))
                                        //{

                                        //    string ssf = l_fileName;
                                        //        //Path.GetFileName(l_fileName); // Получаем имя файла из потока

                                        //    var Iof = new InputOnlineFile(stream, ssf); // Входные данные для отправки

                                        //    string fromsend = $"Файл отправлен от: {Environment.UserName}"; // Имя пользователя

                                        //    //Message ss = 
                                        //        bot.SendDocumentAsync(e.Message.Chat.Id, Iof, fromsend); // Отправка файла с параметрами.

                                        //}

                                        //url = UpLoad(token,e.Message.Chat.Id, l_fileName);
                                        //bot.SendDocumentAsync(e.Message.Chat.Id, url);
                                        await SendDocument(message);
                                        break;
                                    case Type.Audio:                                        
                                        await bot.SendAudioAsync(e.Message.Chat.Id, name.fileName);
                                        break;
                                    case Type.Photo:
                                        await bot.SendPhotoAsync(e.Message.Chat.Id, name.fileName);
                                        break;
                                }
                            }
                            catch (ArgumentException)
                            {
                                Console.WriteLine("An element with Key = \"txt\" already exists.");
                            }
                            catch (KeyNotFoundException)
                            {
                                Console.WriteLine("Key Not Found Exception");
                            }
                        }
                    }
                }
            }
        }

        static async void DownLoad(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();

            fs.Dispose();
        }

        static string GetFiles(string path)
        {
            poz = 0;
            string str_return = "";
            d_files.Clear();
            int photo = 0;
            int audio = 0;
            TypeMessage typeMessage;
            foreach(var aph in d_audioPhoto)
            {
                d_files.Add(poz,aph);
                typeMessage = aph;
                if (typeMessage.type == Type.Photo)
                {
                    str_return += String.Format($"{poz} - (photo) Картинка №{photo}\n");
                    photo++;
                }
                if (typeMessage.type == Type.Audio)
                {
                    str_return += String.Format($"{poz} - (audio) Запись №{audio}\n");
                    audio++;
                }
                poz++;
            }
            string[] files = Directory.GetFiles(path);
            TypeMessage fileName = new TypeMessage();
            foreach(var file in files)
            {
                fileName.fileName = Path.GetFileName(file);
                fileName.type = Type.Document;
                d_files.Add(poz, fileName);
                str_return += String.Format($"{poz} - (document) {fileName.fileName}\n");
                poz++;
            }
            return str_return;
        }
                
        //public static async Task Main2()
        public static void Main2()
        {
#if USE_PROXY
            var Proxy = new WebProxy(Configuration.Proxy.Host, Configuration.Proxy.Port) { UseDefaultCredentials = true };
            Bot = new TelegramBotClient(Configuration.BotToken, webProxy: Proxy);
#else
            //bot = new TelegramBotClient(Configuration.BotToken);
#endif

            //var me = await bot.GetMeAsync();
            var me = bot.GetMeAsync();
            //Console.Title = me.Username;

            bot.OnMessage += BotOnMessageReceived;
            bot.OnMessageEdited += BotOnMessageReceived;
            bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            bot.OnInlineQuery += BotOnInlineQueryReceived;
            bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            bot.OnReceiveError += BotOnReceiveError;

            bot.StartReceiving(Array.Empty<UpdateType>());
            //Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            bot.StopReceiving();
        }
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            switch (message.Text.Split(' ').First())
            {
                // Send inline keyboard
                case "/inline":
                    await SendInlineKeyboard(message);
                    break;

                // send custom keyboard
                case "/keyboard":
                    await SendReplyKeyboard(message);
                    break;

                // send a photo
                case "/photo":
                    await SendPhoto(message);
                    break;
                // send a photo
                case "/doc":
                    await SendDocument(message);
                    break;

                // request location or contact
                case "/request":
                    await RequestContactAndLocation(message);
                    break;

                default:
                    await Usage(message);
                    break;
            }
        }
        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        static async Task SendInlineKeyboard(Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            await Task.Delay(500);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    }
                });
            await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard
            );
        }

        static async Task SendReplyKeyboard(Message message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new KeyboardButton[][]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                },
                resizeKeyboard: true
            );

            await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup

            );
        }

        static async Task SendDocument(Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"AB_Files.xml";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
            await bot.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new InputOnlineFile(fileStream, fileName),
                caption: "AB_Files.xml"
            );
        }

        static async Task SendPhoto(Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
            await bot.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "Nice Picture"
            );
        }
        static async Task RequestContactAndLocation(Message message)
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });
            await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Who or Where are you?",
                replyMarkup: RequestReplyKeyboard
            );
        }

        static async Task Usage(Message message)
        {
            const string usage = "Usage:\n" +
                                    "/inline   - send inline keyboard\n" +
                                    "/keyboard - send custom keyboard\n" +
                                    "/photo    - send a photo\n" +
                                    "/request  - request location or contact";
            await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }


        // Process Inline Keyboard callback data
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}"
            );

            await bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}"
            );
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
            await bot.AnswerInlineQueryAsync(
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
