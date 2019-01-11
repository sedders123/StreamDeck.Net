using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StreamDeck.Net.Models;

namespace StreamDeck.Net {
    /// <summary>
    /// Wrapper for the Stream Deck SDK.
    /// </summary>
    public partial class StreamDeckClient
    {
        /// <summary>
        /// Manages the WebSocket that the Stream Deck uses.
        /// </summary>
        public StreamDeckSocketHandler SocketHandler { get; set; }

        /// <summary>
        /// Stream Deck application and device information.
        /// </summary>
        internal StreamDeckInfo Info { get; }

        /// <summary>
        /// Wrapper for the Stream Deck SDK.
        /// </summary>
        /// <param name="port">The port that should be used to create the WebSocket.</param>
        /// <param name="pluginUuid">The unique identifier string that should be used to register the plugin once the WebSocket is opened.</param>
        /// <param name="registerEvent">The event payload that should be used to register the plugin once the WebSocket is opened.</param>
        /// <param name="info">A stringified json containing the Stream Deck application information and devices information.</param>
        public StreamDeckClient(string port, string pluginUuid, string registerEvent, string info)
        {
            _serialiserSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            SocketHandler = new StreamDeckSocketHandler(new Uri($"ws://localhost:{port}"), registerEvent, pluginUuid);
            SocketHandler.EventOccurredAsync += InvokeSpecificEventHandlers;
            SocketHandler.EventOccurredAsync += InvokeGenericEventHandler;

            Info = JsonConvert.DeserializeObject<StreamDeckInfo>(info, _serialiserSettings);

            _cancellationTokenSource = new CancellationTokenSource();
            _eventHandlers = BuildEventHandlerDictionary();
        }

        /// <summary>
        /// Will handle all messages sent over the WebSocket until the program is terminated.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to be used for all async functions.</param>
        public async Task RunAsync(CancellationToken? cancellationToken = null)
        {
            if (cancellationToken == null)
            {
                cancellationToken = _cancellationTokenSource.Token;
            }

            await SocketHandler.ConnectAsync(cancellationToken.Value);
            await SocketHandler.RunAsync(cancellationToken.Value);
        }

        private readonly CancellationTokenSource _cancellationTokenSource;
        private JsonSerializerSettings _serialiserSettings;

    }
}
