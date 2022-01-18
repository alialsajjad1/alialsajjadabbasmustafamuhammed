using System;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using System.IO;
using Telegram.Bot.Types.InputFiles;

namespace Image_Compression_Bot
{
    class Program
    {
        private static TelegramBotClient? Bot;


        public static async Task Main()
        {
            Bot = new TelegramBotClient("5086525010:AAHTD5V4biljPzeNQevwXr-zkfteLZ6euvY");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "Image_Compression_Bot";
            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cancellationToken: cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            

            switch (message.Type)
            {
                case MessageType.Document:
                    {
                        var fileName = message.Document.FileName;
                        string[] fileNameArray = fileName.Split(".");
                        switch (fileNameArray[1].ToUpper())
                        {
                            case "JPG" or "PNG" or "JPEG":
                                Console.WriteLine($"Receive message type: {message.Type}");
                                Console.WriteLine($"The message was sent with id: {message.MessageId}");
                                ImagCompression(botClient, message);
                                break;
                            default:
                                await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Please send the image you want to compress as a file .\n" +
                                                            "Acceptable image types are: JPG - PNG - JPEG");
                                return;
                        }

                           
                        break;
                    }
                default:
                    {
                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Please send the image you want to compress as a file .\n" +
                                                            "Acceptable image types are: JPG - PNG - JPEG");
                        return;
                    }
            }


        }



        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }



        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        static async Task<Message> ImagCompression(ITelegramBotClient botClient, Message message)
        {

            //var files = message.MessageId
            await botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    replyToMessageId: message.MessageId,
                                    text: "Please wait..."
                                        );
            var fileId = message.Document.FileId;


            var webClient = new System.Net.WebClient();
            var json = webClient.DownloadString("https://api.telegram.org/bot5086525010:AAHTD5V4biljPzeNQevwXr-zkfteLZ6euvY/getFile?file_id=" + fileId);

            var jsonData = JsonConvert.DeserializeObject<Root>(json);

            string filePath = jsonData.result.file_path;
            string fullPath = "https://api.telegram.org/file/bot5086525010:AAHTD5V4biljPzeNQevwXr-zkfteLZ6euvY/" + filePath;

    

            System.Net.WebRequest request =
            System.Net.WebRequest.Create(fullPath);
            System.Net.WebResponse response = request.GetResponse();
            System.IO.Stream responseStream =
                response.GetResponseStream();

            // Get a bitmap. The using statement ensures objects  
            // are automatically disposed from memory after use.  
            using (Bitmap bmp1 = new Bitmap(responseStream))
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                // Create an Encoder object based on the GUID  
                // for the Quality parameter category.  
                System.Drawing.Imaging.Encoder myEncoder =
                    System.Drawing.Imaging.Encoder.Quality;

                // Create an EncoderParameters object.  
                // An EncoderParameters object has an array of EncoderParameter  
                // objects. In this case, there is only one  
                // EncoderParameter object in the array.  
                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp1.Save(@"d:\PhotoQualityFifty.jpg", jpgEncoder, myEncoderParameters);
                /*
                myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp1.Save(@"d:\PhotoQualityHundred.jpg", jpgEncoder, myEncoderParameters);
                */
                // Save the bitmap as a JPG file with zero quality level compression.  
                myEncoderParameter = new EncoderParameter(myEncoder, 0L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp1.Save(@"d:\PhotoQualityZero.jpg", jpgEncoder, myEncoderParameters);

            }


            using (FileStream stream = System.IO.File.OpenRead(@"d:\PhotoQualityFifty.jpg"))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "PhotoQualityFifty.jpg");
                await botClient.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: inputOnlineFile,
                    caption: "<b>Photo Quality: Fifty</b>",
                    parseMode: ParseMode.Html
                    );
            }
            /*
            using (FileStream stream = System.IO.File.OpenRead(@"d:\PhotoQualityHundred.jpg"))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "PhotoQualityHundred.jpg");
                await botClient.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: inputOnlineFile,
                    caption: "<b>Photo Quality: Hundred</b>",
                    parseMode: ParseMode.Html
                    );
            }
            */
            using (FileStream stream = System.IO.File.OpenRead(@"d:\PhotoQualityZero.jpg"))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "PhotoQualityZero.jpg");
                return await botClient.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: inputOnlineFile,
                    caption: "<b>Photo Quality: Zero</b>",
                    parseMode: ParseMode.Html
                    );
            }




            //var Result = new IronTesseract().Read("D:\\cc.jpg");
            //Console.WriteLine(Result.Text);


        }


        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }


        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Result
        {
            public string file_id { get; set; }
            public string file_unique_id { get; set; }
            public int file_size { get; set; }
            public string file_path { get; set; }
        }

        public class Root
        {
            public bool ok { get; set; }
            public Result result { get; set; }
        }

    }
}
