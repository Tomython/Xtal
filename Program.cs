using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://*:2025/"); 
        listener.Start();
        Console.WriteLine("=== XTAL SERVER IS LIVE ===");
        Console.WriteLine("URL: http://localhost:2025");

        var youtube = new YoutubeClient();

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Разрешаем CORS, чтобы браузер не блокировал аудио
            response.AppendHeader("Access-Control-Allow-Origin", "*");
            
            string requestedPath = request.Url.LocalPath.TrimStart('/');

            // --- ОБРАБОТКА YOUTUBE (ПРОКСИ) ---
            if (requestedPath == "stream-yt")
            {
                try
                {
                    string videoUrl = request.QueryString["url"];
                    if (string.IsNullOrEmpty(videoUrl)) throw new Exception("URL empty");
                    
                    // Очистка ссылки от лишних параметров YouTube
                    if (videoUrl.Contains("&")) videoUrl = videoUrl.Split('&')[0];
                    if (videoUrl.Contains("?si=")) videoUrl = videoUrl.Split("?si=")[0];

                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                    response.ContentType = "audio/mpeg";
                    using var ytStream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    
                    // Копируем поток напрямую из YouTube в браузер
                    await ytStream.CopyToAsync(response.OutputStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Ошибка стриминга: {ex.Message}");
                    response.StatusCode = 500;
                }
                finally { response.OutputStream.Close(); }
                continue;
            }

            // --- РАЗДАЧА HTML-СТРАНИЦЫ ---
            if (string.IsNullOrEmpty(requestedPath) || requestedPath == "XtalHtml.html") 
                requestedPath = "XtalHtml.html";

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] searchPaths = {
                Path.Combine(baseDir, "wwwroot", requestedPath),
                Path.Combine(baseDir, requestedPath),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", requestedPath),
                Path.Combine(Directory.GetCurrentDirectory(), requestedPath)
            };

            string finalPath = null;
            foreach (var p in searchPaths) { if (File.Exists(p)) { finalPath = p; break; } }

            if (finalPath != null)
            {
                byte[] buffer = await File.ReadAllBytesAsync(finalPath);
                response.ContentType = finalPath.EndsWith(".html") ? "text/html; charset=utf-8" : "application/octet-stream";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = 404;
                byte[] error = Encoding.UTF8.GetBytes("404 - File Not Found");
                await response.OutputStream.WriteAsync(error, 0, error.Length);
            }
            response.OutputStream.Close();
        }
    }
}