using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using StreamDeck.Net;
using StreamDeck.Net.Models;

namespace Counter
{
    public class Program
    {
        [Option(Description = "The port the Elgato StreamDeck software is listening on", ShortName = "port")]
        public string Port { get; set; }

        [Option(Description = "The Unique Identifier used to reference the plugin", ShortName = "pluginUUID")]
        public string PluginUuid { get; set; }

        [Option(Description = "Event type used to register the plugin", ShortName = "registerEvent")]
        public string RegisterEvent { get; set; }

        [Option(Description = "String of JSON containing appliaction and device information", ShortName = "info")]
        public string Info { get; set; }

        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);
        

        private async Task OnExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var streamdeck = new StreamDeckClient(Port, PluginUuid, RegisterEvent, Info);
            streamdeck.KeyDownEventAsync += UpdateCounter;
            await streamdeck.RunAsync(cancellationTokenSource.Token);
        }

        private static int _counter;

        private static async Task UpdateCounter(StreamDeckClient e, StreamDeckEventPayload args)
        {
            _counter++;
            await e.SetTitle(args.Context, _counter.ToString());
        }
    }
}
