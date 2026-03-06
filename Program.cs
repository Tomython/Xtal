using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    // Важно: Main теперь async Task
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://*:2025/");
        listener.Start();
        Console.WriteLine("Сервер запущен на порту 2025...");

        var youtube = new YoutubeClient();

        while (true)
        {
            // Используем GetContextAsync для асинхронности
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string requestedPath = request.Url.LocalPath.TrimStart('/');

            // --- 1. ОБРАБОТКА YOUTUBE ---
            if (requestedPath == "get-yt")
            {
                try
                {
                    string videoUrl = request.QueryString["url"];
                    if (string.IsNullOrEmpty(videoUrl)) throw new Exception("Пустой URL");

                    // Получаем прямую ссылку на аудиопоток
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                    string json = $"{{\"url\": \"{streamInfo.Url}\"}}";
                    byte[] jsonBuffer = Encoding.UTF8.GetBytes(json);

                    // Разрешаем CORS на всякий случай
                    response.AppendHeader("Access-Control-Allow-Origin", "*");
                    response.ContentType = "application/json";
                    response.ContentLength64 = jsonBuffer.Length;
                    await response.OutputStream.WriteAsync(jsonBuffer, 0, jsonBuffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка YouTube: " + ex.Message);
                    response.StatusCode = 500;
                }
                finally
                {
                    response.OutputStream.Close();
                }
                continue; // Идем на следующий цикл, не ищем файлы
            }

            // --- 2. РАЗДАЧА ФАЙЛОВ ---
            if (string.IsNullOrEmpty(requestedPath)) 
            {
                requestedPath = "XtalHtml.html";
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string wwwrootPath = Path.Combine(baseDir, "wwwroot");
            
            // Защита для путей в Docker на Render.com
            if (!Directory.Exists(wwwrootPath) && Directory.Exists("/app/wwwroot"))
            {
                wwwrootPath = "/app/wwwroot";
            }

            string filePath = Path.Combine(wwwrootPath, requestedPath);
            Console.WriteLine($"Ищем файл: {filePath}");

            if (File.Exists(filePath))
            {
                try
                {
                    byte[] buffer = await File.ReadAllBytesAsync(filePath);
                    
                    // Указываем браузеру, какой тип файла мы отдаем
                    if (filePath.EndsWith(".html")) response.ContentType = "text/html; charset=utf-8";
                    else if (filePath.EndsWith(".js")) response.ContentType = "application/javascript";
                    else if (filePath.EndsWith(".css")) response.ContentType = "text/css";
                    else if (filePath.EndsWith(".mp4")) response.ContentType = "video/mp4";
                    else if (filePath.EndsWith(".ico")) response.ContentType = "image/x-icon";

                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка чтения: " + ex.Message);
                    response.StatusCode = 500;
                }
            }
            else
            {
                response.StatusCode = 404;
                string notFoundResponse = "<html><body><h1>404 - Not Found</h1></body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(notFoundResponse);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            response.OutputStream.Close();
        }
    }
}