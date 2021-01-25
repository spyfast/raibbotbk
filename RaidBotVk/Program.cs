/*
                                    Разработчик: Владимир Новиков — vk.com/spyfast
                                    Telegram-канал: https://t.me/jungfranco
                                    Бот для группы ВКонтакте. Можно флудить как текстом, так и смайликами,
                                    чтобы бот работал без прав администратора в чате, упомините его и добавьте команду, например,
                                    @club1234567 старт
                                    
                                    По всем вопросам пишите мне в личные сообщения. Отвечу всем. Спасибо.
*/
using System;
using System.IO;
using System.Threading;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace RaidBotVK
{
    class Program
    {
        private static string AccessToken = ""; // Вставить токен группы
        private static ulong GroupId = 0; // Вставить id вашей группы
        private static long FromId = 0; // id ващего профиля, на который будет реагировать бот
        private static bool Working = false;
        
        static void Main(string[] args)
        {
            Console.Title = "RaidBot :: coded by vk.com/spyfast";
            
            if (!File.Exists("messages.txt"))
                File.Create("messages.txt").Close();

            if (!File.Exists("HtmlAgilityPack.dll") || !File.Exists(
                                                        "Microsoft.Extensions.DependencyInjection.Abstractions.dll")
                                                    || !File.Exists("Microsoft.Extensions.DependencyInjection.dll") ||
                                                    !File.Exists("Microsoft.Extensions.Logging.Abstractions.dll")
                                                    || !File.Exists("Newtonsoft.Json.dll") || !File.Exists("VkNet.dll"))
            {
                Console.WriteLine("Отсутствуют необходимые .dll для продолжения работы приложения :с");
                return;
            }
            
            var api = new VkApi();
           

            try
            {
                api.Authorize(new ApiAuthParams()
                    { AccessToken = AccessToken, Settings = Settings.All | Settings.Offline });

                var server = api.Groups.GetLongPollServer(GroupId);
                var message = 
                    File.ReadAllText("messages.txt");
                
                while (true)
                {
                    var response =
                        api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                            {Key = server.Key, Server = server.Server, Ts = server.Ts, Wait = 5});
                    server.Ts = response.Ts;

                    foreach (var item in response.Updates)
                    {
                        var text = item.Message.Text.ToLower();

                        if (item.Type == GroupUpdateType.MessageNew && text.Contains("старт") && item.Message.FromId == FromId)
                        {
                            new Thread(() =>
                            {
                                Working = true;
                                
                                while (Working)
                                {
                                    try
                                    {
                                        api.Messages.Send(new MessagesSendParams()
                                            { Message = message,  PeerId = item.Message.PeerId,  RandomId = new Random().Next() });
                                    }
                                    catch (Exception e)
                                    {
                                        if (!e.Message.Contains("Flood"))
                                            api.Messages.Send(new MessagesSendParams()
                                                 { Message = message,  PeerId = item.Message.PeerId,  RandomId = new Random().Next() });
                                    }
                                }
                            }) {IsBackground = true}.Start();
                        }
                        
                        if (item.Type == GroupUpdateType.MessageNew && text == "стоп" && item.Message.FromId == FromId)
                        {
                            new Thread(() =>
                            {
                                Working = false;
                                api.Messages.Send(new MessagesSendParams()
                                    { Message = "✖ Флуд остановлен.",  PeerId = item.Message.PeerId,  RandomId = new Random().Next() });
                            }) {IsBackground = true}.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error]: " + e.Message);
            }

            Console.ReadKey();
        }
    }
}