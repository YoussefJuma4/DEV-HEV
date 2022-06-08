using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ClassesX;
using System.Linq;

namespace somethingg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        static private List<WebSocket> ConnectedClientsList = new List<WebSocket>();
        
        public ChatController(ILogger<ChatController> logger) //,IWebsocketHandler websocketHandler
        {
            _logger = logger;
        }

        [HttpGet("/ws2")]
        public async Task Get()
        {
          if (HttpContext.WebSockets.IsWebSocketRequest)
          {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var context = ControllerContext.HttpContext;

            ConnectedClientsList.Add(webSocket);

            Console.WriteLine($"connected ## clients:");
            foreach(var x in ConnectedClientsList){
                Console.WriteLine($"--- {x}");
            }

            _logger.Log(LogLevel.Information, "WebSocket connection established");

            await Echo(webSocket);
          }
          else
          {
              HttpContext.Response.StatusCode = 400;
          }
        }
        
        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"{Encoding.UTF8.GetString(buffer)}");
                //Console.WriteLine(System.Text.Encoding.Default.GetString(serverMsg));

                foreach(WebSocket x in ConnectedClientsList){
                    if(webSocket != x){
                        await x.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                        _logger.Log(LogLevel.Information, "Message sent to Client");
                        buffer = new byte[1024 * 4];
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
            
            //room.DisconnectClient(webSocket);

            int indexToRemove = ConnectedClientsList.IndexOf(webSocket);
            ConnectedClientsList.RemoveAt(indexToRemove);
        }
    }
}