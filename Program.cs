using System;
using System.IO;
using System.Net;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://*:2025/");
        listener.Start();
        Console.WriteLine("Сервер запущен на порту 2025...");

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string requestedPath = request.Url.LocalPath.TrimStart('/');

            // Если путь пустой, подставляем XtalHtml.html
            if (string.IsNullOrEmpty(requestedPath)) 
            {
                requestedPath = "XtalHtml.html"; // ← Твой HTML-файл
            }

            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "wwwroot", 
                requestedPath
            );

            Console.WriteLine($"Ищем файл: {filePath}"); // ← добавить перед проверкой File.Exists

            // После объявления filePath:
            Console.WriteLine($"Содержимое папки wwwroot: {string.Join(", ", Directory.GetFiles("/app/wwwroot"))}");

            if (File.Exists(filePath))
            {
                byte[] buffer = File.ReadAllBytes(filePath);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                string notFoundResponse = "<html><body><h1>404 - Not Found</h1></body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(notFoundResponse);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            response.OutputStream.Close();
        }
    }
}
