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


            token = Token.token;
            path = Directory.GetCurrentDirectory();
            d_files = new Dictionary<int, TypeMessage>();
            d_audioPhoto = new List<TypeMessage>();
            poz = 0;
            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.OnMessageEdited += MessageListener;
            bot.OnCallbackQuery += BotOnCallbackQueryReceived;            
            bot.OnReceiveError += BotOnReceiveError;
            bot.StartReceiving(Array.Empty<UpdateType>());

            Console.ReadLine();
            bot.StopReceiving();            
        }

        /// <summary>
        /// Обрабатывает все сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void MessageListener(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            SaveToList(e);

            if (message == null || message.Type != MessageType.Text)
                return;

            var messageText = e.Message.Text;
            switch (messageText)
            {
                case null:
                    break;
                case "/start":            
                    messageText = "Вас приветствует бот 'test20210404_bot'\n" +
                        "Вы можете управлять мной, отправляя эти команды: \n" +
                        "/list - просмотреть список загруженных файлов\n" +
                        "/load_n - скачать выбранный файл, где n - число\n" +
                        "/help - справка по поддерживаемым коммандам\n";
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                        $"{messageText}");
                    break;

                case "/help":            
                    messageText = "Вы можете управлять мной, отправляя эти команды: \n" +
                        "/list - просмотреть список загруженных файлов\n" +
                        "/load_n - скачать выбранный файл, где n - число\n" +
                        "/help - справка по поддерживаемым коммандам\n";
                    await bot.SendTextMessageAsync(e.Message.Chat.Id,
                        $"{messageText}");
                    break;

                case "/list":
                    messageText = GetFiles(SetDirectory(e.Message.From.Id));
                    if (messageText != "")
                        await bot.SendTextMessageAsync(e.Message.Chat.Id, $"{messageText}");
                    else
                        await bot.SendTextMessageAsync(e.Message.Chat.Id, $"Список сейчас пустой");
                    break;
                default:
                    MessageDefault(e);
                    break;
            }
        }

        /// <summary>
        /// Устанавливает текущую директория и возвращяет её
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Возвращяет текую директорию типа string</returns>
        static string SetDirectory( int userId)
        {
            string l_path = path + "\\" + userId.ToString();
            if (!Directory.Exists(l_path))
            {
                Directory.CreateDirectory(l_path);
            }
            Directory.SetCurrentDirectory(l_path);
            return l_path;
        }

        /// <summary>
        /// Сохраняет фото и аудио в список, документы в папку
        /// </summary>
        /// <param name="e"></param>
        private static void SaveToList(MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            Console.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");
            TypeMessage fileName = new TypeMessage();
            switch (e.Message.Type)
            {
                case MessageType.Document:            
                    Console.WriteLine(e.Message.Document.FileId);
                    Console.WriteLine(e.Message.Document.FileName);
                    Console.WriteLine(e.Message.Document.FileSize);
                    DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);
                    break;

                case MessageType.Photo:
                    fileName.type = Type.Photo;
                    PhotoSize[] photoSizes = e.Message.Photo;
                    fileName.fileName = photoSizes[0].FileId;
                    d_audioPhoto.Add(fileName);
                    Console.WriteLine(photoSizes[0].FileId);
                    break;

                case MessageType.Audio:            
                    fileName.type = Type.Audio;
                    fileName.fileName = e.Message.Audio.FileId;
                    d_audioPhoto.Add(fileName);
                    Console.WriteLine(e.Message.Audio.FileId);
                    break;
                case MessageType.Voice:
                    fileName.type = Type.Voice;
                    fileName.fileName = e.Message.Voice.FileId;
                    d_audioPhoto.Add(fileName);
                    Console.WriteLine(e.Message.Voice.FileId);
                    break;
                case MessageType.Video:
                    fileName.type = Type.Video;
                    fileName.fileName = e.Message.Video.FileId;
                    d_audioPhoto.Add(fileName);
                    Console.WriteLine(e.Message.Video.FileId);
                    break;
            }
        }

        /// <summary>
        /// Обработка сообщений по умолчания (всё кроме команд /start /help /list)
        /// </summary>
        /// <param name="e"></param>
        private static async void MessageDefault(MessageEventArgs e)
        {
            string str_load = e.Message.Text.Substring(0, 1);
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
                        TypeMessage name = null;
                        if (int.TryParse(str_nomer, out nomer))
                        {
                            try
                            {
                                name = d_files[nomer];
                                switch (name.type)
                                {
                                    case Type.Document:
                                        await SendDocument(e.Message, name.fileName);
                                        break;
                                    case Type.Audio:
                                        await bot.SendAudioAsync(e.Message.Chat.Id, name.fileName);
                                        break;
                                    case Type.Photo:
                                        await bot.SendPhotoAsync(e.Message.Chat.Id, name.fileName);
                                        break;
                                    case Type.Voice:
                                        await bot.SendVoiceAsync(e.Message.Chat.Id, name.fileName);
                                        break;
                                    case Type.Video:
                                        await bot.SendVideoAsync(e.Message.Chat.Id, name.fileName);
                                        break;
                                }
                            }
                            catch (ArgumentException)
                            {
                                Console.WriteLine("Ошибка ArgumentException");
                            }
                            catch (KeyNotFoundException)
                            {
                                //Console.WriteLine("Ошибка KeyNotFoundException");
                                await bot.SendTextMessageAsync(e.Message.Chat.Id,
                                    "Нет документа с таким номером");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Сохраняет документ
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="path"></param>
        static async void DownLoad(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream(path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }

        /// <summary>
        /// Создаёт список документов, аудио, фото и т.д
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Возвращяет список документов, аудио, фото и т.д типа string</returns>
        static string GetFiles(string path)
        {
            poz = 0;
            string str_return = "";
            d_files.Clear();
            int photo = 0;
            int audio = 0;
            int voice = 0;
            int video = 0;
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
                if (typeMessage.type == Type.Voice)
                {
                    str_return += String.Format($"{poz} - (voice) Запись №{voice}\n");
                    voice++;
                }
                if (typeMessage.type == Type.Video)
                {
                    str_return += String.Format($"{poz} - (video) Запись №{video}\n");
                    video++;
                }
                poz++;
            }
            string[] files = Directory.GetFiles(path);
            foreach(var file in files)
            {
                TypeMessage fileName = new TypeMessage();
                fileName.fileName = Path.GetFileName(file);
                fileName.type = Type.Document;
                d_files.Add(poz, fileName);
                str_return += String.Format($"{poz} - (document) {fileName.fileName}\n");
                poz++;
            }
            return str_return;
        }                

        /// <summary>
        /// Отправляет документ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static async Task SendDocument(Message message, string filePath)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
            await bot.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new InputOnlineFile(fileStream, fileName),
                caption: filePath
            );
        }        
        
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

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}
