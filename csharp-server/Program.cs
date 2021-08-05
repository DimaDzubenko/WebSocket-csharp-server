using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Server;

/*
 2) WebService should track when device goes online and offline, 
    get information from device every 5 minutes 
    and send that information to azure function.
 */

namespace csharp_server
{  
    public class Echo : WebSocketBehavior
    {
        private static HttpClient _httpClient = new HttpClient();

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("Received message from client: " + e.Data);

            AzureFunction(e.Data);

            Sessions.Broadcast(e.Data);
        }

        public async Task AzureFunction( string data)
        {
            _httpClient.BaseAddress = new Uri("http://localhost:7071/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();

            var request = new HttpRequestMessage(HttpMethod.Post, "api/Function1");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(data);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var responce = await _httpClient.SendAsync(request);
            Console.WriteLine(responce.EnsureSuccessStatusCode()); 

           //var content = await responce.Content.ReadAsStringAsync();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer wssv = new WebSocketServer("ws://127.0.0.1:7890");

            wssv.AddWebSocketService<Echo>("/Echo");

            wssv.Start();
            Console.WriteLine("WebServer started on ws://127.0.0.1:7890/Echo");

            Console.ReadKey();
            wssv.Stop();
        }
    }
}
