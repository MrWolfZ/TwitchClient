using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using vtortola.WebSockets;

namespace TwitchBot.Util
{
    public static class WebSocketExtensions
    {
        public static async Task<dynamic> ReadDynamicAsync(this WebSocket ws, CancellationToken cancel)
        {
            var message = await ws.ReadMessageAsync(cancel);
            if (message != null)
            {
                using (var sr = new StreamReader(message, Encoding.UTF8))
                {
                    return (dynamic)JObject.Load(new JsonTextReader(sr));
                }
            }

            return null;
        }

        public static void WriteDynamic(this WebSocket ws, dynamic data)
        {
            var serializer = new JsonSerializer();
            using (var writer = ws.CreateMessageWriter(WebSocketMessageType.Text))
            using (var sw = new StreamWriter(writer, Encoding.UTF8))
            {
                serializer.Serialize(sw, data);
            }
        }
    }
}