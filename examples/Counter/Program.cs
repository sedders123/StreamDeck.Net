using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StreamDeck.Net;
using StreamDeck.Net.Models;

namespace Counter
{
    public class Program
    {
        public Program()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

        }

        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        [Option(Description = "The port the Elgato StreamDeck software is listening on", ShortName = "port")]
        public string Port { get; set; }

        [Option(ShortName = "pluginUUID")]
        public string PluginUUID { get; set; }

        [Option(ShortName = "registerEvent")]
        public string RegisterEvent { get; set; }

        [Option(ShortName = "info")]
        public string Info { get; set; }

        private StreamDeckInfo _info;
        internal StreamDeckInfo StreamDeckInfo => _info ?? (_info = JsonConvert.DeserializeObject<StreamDeckInfo>(Info));

        private ClientWebSocket _socket = new ClientWebSocket();

        private async Task OnExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var streamdeck = new StreamDeckClient(Port, PluginUUID, RegisterEvent, Info);
            _socket = streamdeck.SocketHandler.Socket;
            streamdeck.KeyDownEventAsync += KeyDown;
            await streamdeck.RunAsync(cancellationTokenSource.Token);

        }

        private static int _counter;

        private async Task KeyDown(object e, StreamDeckEventPayload args)
        {
            _counter++;
            await SetTitle(args.Context, _counter.ToString());

        }

        /// <summary>
        /// Set the title on the button passed in the context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private Task SetTitle(string context, string title)
        {

            var json = @"{
					""event"": ""setTitle"",
					""context"": """ + context + @""",
					""payload"": {
						""title"": """ + title + @""",
						""target"": " + (int)Destination.HardwareAndSoftware + @"
					}
				}";

            var bytes = Encoding.UTF8.GetBytes(json);
            return _socket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);

        }

    }
}
