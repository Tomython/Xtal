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
        // Порт 2025 как ты просил
        listener.Prefixes.Add("http://*:2025/"); 
        listener.Start();
        Console.WriteLine("=== XTAL SERVER STARTED ===");
        Console.WriteLine("Open: http://localhost:2025");

        var youtube = new YoutubeClient();

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Разрешаем запросы со всех источников (фиксит CORS)
            response.AppendHeader("Access-Control-Allow-Origin", "*");
            
            string requestedPath = request.Url.LocalPath.TrimStart('/');

            // --- 1. СТРИМИНГ С YOUTUBE ---
            if (requestedPath == "stream-yt")
            {
                try
                {
                    string videoUrl = request.QueryString["url"];
                    if (string.IsNullOrEmpty(videoUrl)) throw new Exception("URL is empty");
                    
                    // Очистка ссылки
                    if (videoUrl.Contains("?si=")) videoUrl = videoUrl.Split("?si=")[0];

                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                    response.ContentType = "audio/mpeg";
                    using var ytStream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    await ytStream.CopyToAsync(response.OutputStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[YT Error]: {ex.Message}");
                    response.StatusCode = 500;
                }
                finally { response.OutputStream.Close(); }
                continue;
            }

            // --- 2. ПОИСК И РАЗДАЧА HTML ---
            if (string.IsNullOrEmpty(requestedPath) || requestedPath == "XtalHtml.html") 
                requestedPath = "XtalHtml.html";

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] searchPaths = {
                Path.Combine(baseDir, "wwwroot", requestedPath),
                Path.Combine(baseDir, requestedPath),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", requestedPath),
                Path.Combine(Directory.GetCurrentDirectory(), requestedPath)
            };

            string foundPath = null;
            foreach (var path in searchPaths) {
                if (File.Exists(path)) { foundPath = path; break; }
            }

            if (foundPath != null)
            {
                byte[] buffer = await File.ReadAllBytesAsync(foundPath);
                if (foundPath.EndsWith(".html")) response.ContentType = "text/html; charset=utf-8";
                else if (foundPath.EndsWith(".js")) response.ContentType = "application/javascript";
                else if (foundPath.EndsWith(".css")) response.ContentType = "text/css";
                
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = 404;
                string errLog = $"404: Not Found. Checked paths:<br>" + string.Join("<br>", searchPaths);
                byte[] errorBuffer = Encoding.UTF8.GetBytes(errLog);
                response.ContentType = "text/html";
                await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            }
            response.OutputStream.Close();
        }
    }
}