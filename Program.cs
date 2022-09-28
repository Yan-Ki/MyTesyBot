using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;




namespace MyTesyBot
{
    internal class Program
    {
        static public ITelegramBotClient bot = new TelegramBotClient("   "); // ------ введите токен
        /// <summary>
        /// Чтение сообщений
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                //if (string.IsNullOrEmpty());
                string menu = " Привет! Это бот. Если пришлёшь фото, он скачает и будет хранить. Пришлёшь документ, он просто его скачает." +
                              " Напишешь /photo и он все твои фото пришлёт тебе обратно." +
                              " А если напишешь /volf то получишь мемчик про волков. Да, это очень бесполезный бот))))";
                string menu2 = $"/photo - получить фото \n /volf - получить мемчик";
                    switch (message.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.Text:
                        {
                            switch(message.Text.ToLower())
                            {
                                case "/start": await botClient.SendTextMessageAsync(message.Chat, menu); break;
                                case "/photo":
                                    Unload(message.Chat.Id, message, botClient, menu2);
                                    //await botClient.SendTextMessageAsync(message.Chat, $"Санька, Санька в займы не будет у тебя рублей 300 \n {menu2}") ;
                                    break;
                                case "/volf":
                                    MemUnload(message.Chat.Id);
                                    await botClient.SendTextMessageAsync(message.Chat, $"Получай мем \n {menu2}") ; break;
                                    default: await botClient.SendTextMessageAsync(message.Chat, $"Пришли либо фото, либо документ, либо напиши команду, " +
                  $"я же тебе объяснил \n {menu2}"); break;
                            }
                        }
                     
                       break;
                     
                    case Telegram.Bot.Types.Enums.MessageType.Photo:
                        
                            DownloadFoto(message.Photo[message.Photo.Length - 1].FileId);
                            await botClient.SendTextMessageAsync(message.Chat, $"От души \n {menu2}");
                            break;
                        
                    case Telegram.Bot.Types.Enums.MessageType.Document:
                        Download(message.Document.FileId, message.Document.FileName);
                        await botClient.SendTextMessageAsync(message.Chat, $"Всё скачал, давай до связи \n {menu2}");
                        break;
                    default: await botClient.SendTextMessageAsync(message.Chat, $"Сказал же что нужно присылать \n {menu2}");
                        break;
                }
                
            }
        }
        /// <summary>
        /// Ошибки
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        /// <summary>
        /// Сохранение присланных документов
        /// </summary>
        /// <param name="fileID"></param>
        /// <param name="fileName"></param>
        static async void Download (string fileID, string fileName)
        {
            var file = await bot.GetFileAsync(fileID);
            FileStream fs= new FileStream(fileName, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
 
        }
        /// <summary>
        /// Сохранение твоих фото
        /// </summary>
        /// <param name="fotoID"></param>
        static async void DownloadFoto(string fotoID)
        {
            string Path = @"D:\C#\Skillbox - Профессия С# разработчик (2020)\9. Работа с сетью\MyTesyBot\Foto\";// --- введите путь к папке с фото

            DirectoryInfo directoryInfo = new DirectoryInfo(Path);           
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var item in directoryInfo.GetFiles())
            {
                text.Append($"\n{item.Name}  размер файла {item.Length}.");
                i++;
            }
            Console.WriteLine(text.ToString());
            Console.WriteLine(i);
            var foto = await bot.GetFileAsync(fotoID);
            FileStream fs = new FileStream ($"{Path}{i+1}.jpg", FileMode.OpenOrCreate);
            await bot.DownloadFileAsync(foto.FilePath, fs);
            fs.Close();
            
        }
        /// <summary>
        /// метод загрузки всех фото
        /// </summary>
        /// <param name="ChatId"></param>
        static async void Unload (long ChatId, Telegram.Bot.Types.Message message, ITelegramBotClient botClient, string menu2)
        {
            string Path = @" ";// --- введите путь к папке с фото
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);

            if (directoryInfo.GetFiles().Length == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Ты  не прислал ничего ещё \n {menu2}");
            }
            else
            {
                foreach (var item in directoryInfo.GetFiles())
                {
                    FileStream fs = new FileStream($"{Path}{item.Name}", FileMode.Open);
                    await bot.SendPhotoAsync(ChatId, fs);
                    fs.Close();
                    
                }
                await botClient.SendTextMessageAsync(message.Chat, $"Получай \n {menu2}");

            }
        }
        /// <summary>
        /// Метод загрузки мемов
        /// </summary>
        /// <param name="ChatId"></param>
        static async void MemUnload(long ChatId)
        {
            string Path = @" "; // --- введите путь к папке с мемами
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            FileInfo[] mass = directoryInfo.GetFiles();
            Random random = new Random();
            //Console.WriteLine(random.Next(0, 17));
            //Console.WriteLine(mass.Length);
                FileStream fs = new FileStream($"{Path}{mass[random.Next(0,17)].Name}", FileMode.Open);
            await bot.SendPhotoAsync(ChatId, fs, caption: "Безумно можно быть первым...") ;
                fs.Close();
            
        }
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { },
                };
                bot.StartReceiving(HandleUpdateAsync,HandleErrorAsync, receiverOptions,
                    cancellationToken
                );
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}

