using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StreamDeck.Net.Models;

namespace StreamDeck.Net
{
    public delegate Task StreamDeckSocketEventHandler(StreamDeckSocketHandler source, StreamDeckEventPayload e);
    public delegate Task StreamDeckEventHandler(StreamDeckClient source, StreamDeckEventPayload e);

    /// <summary>
    /// Wrapper for managing WebSocket communcation with the Stream Deck.
    /// </summary>
    public class StreamDeckSocketHandler
    {
        /// <summary>
        /// WebSocket used for communication with Stream Deck
        /// </summary>
        [Obsolete("This will likely be made private. It is exposed to allow for custom usage while the library is being built")]
        public ClientWebSocket Socket { get; }

        /// <summary>
        /// Occurs when an Event is received from the Stream Deck.
        /// </summary>
        public event StreamDeckSocketEventHandler EventOccurredAsync;

        /// <summary>
        /// Wrapper for managing WebSocket communcation with the Stream Deck.
        /// </summary>
        /// <param name="uri">The URI to connect to the websocket on.</param>
        /// <param name="registerEvent">The Registration event payload.</param>
        /// <param name="pluginUuid">The plugin's UUID.</param>
        public StreamDeckSocketHandler(Uri uri, string registerEvent, string pluginUuid)
        {
            _uri = uri;
            _registerEvent = registerEvent;
            _pluginUuid = pluginUuid;
#pragma warning disable 618
            Socket = new ClientWebSocket();
#pragma warning restore 618
            _serialiserSettings = new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()};
            _serialiser = JsonSerializer.Create(_serialiserSettings);
        }

        /// <summary>
        /// Initiate connection with the Stream Deck.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to be used for async requests.</param>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
#pragma warning disable 618
            await Socket.ConnectAsync(_uri, cancellationToken);
            await Socket.SendAsync(GetPluginRegistrationBytes(), WebSocketMessageType.Text, true, CancellationToken.None);
#pragma warning restore 618
            _connected = true;
        }

        /// <summary>
        /// Continually listens to the WebSocket and raises EventsOccurrences when messages are recieved.
        /// </summary>
        /// <param name="cancellationToken">Canellation token used to halt the function.</param>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (!_connected)
            {
                throw new Exception("Not connected to socket");
            }
            var pipe = new Pipe();
            Task.WaitAll(FillPipeAsync(pipe.Writer, cancellationToken), ReadPipe(pipe.Reader, cancellationToken));
        }

        private readonly Uri _uri;
        private bool _connected;
        private readonly string _registerEvent;
        private readonly string _pluginUuid;
        private readonly JsonSerializerSettings _serialiserSettings;
        private readonly JsonSerializer _serialiser;
        private Task OnEventOccurredAsync(StreamDeckEventPayload data) => EventOccurredAsync?.Invoke(this, data);

        private ArraySegment<byte> GetPluginRegistrationBytes()
        {

            var registration = new PluginRegistration
            {
                Event = _registerEvent,
                Uuid = _pluginUuid
            };

            var outString = JsonConvert.SerializeObject(registration, _serialiserSettings);  
            var outBytes = Encoding.UTF8.GetBytes(outString);
            return outBytes;

        }

        private async Task FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            if (!_connected)
            {
                throw new Exception("Not connected to socket");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var memory = writer.GetMemory();
                try
                {
#pragma warning disable 618
                    var socketResult = await Socket.ReceiveAsync(memory, cancellationToken);
#pragma warning restore 618
                    if (socketResult.Count == 0)
                    {
                        continue;
                    }
                    writer.Advance(socketResult.Count);
                    if (socketResult.EndOfMessage)
                    {
                        await writer.FlushAsync(cancellationToken);
                    }
                }
                catch (Exception)
                {
                    // Stay in loop
                }
            }
            writer.Complete();
        }

        private async Task ReadPipe(PipeReader reader, CancellationToken cancellationToken)
        {
            if (!_connected)
            {
                throw new Exception("Not connected to socket");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken);

                var jsonString = Encoding.UTF8.GetString((byte[])result.Buffer.ToArray());

                reader.AdvanceTo(result.Buffer.End);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    using (var sr = new StringReader(jsonString))
                    using (var jsonReader = new JsonTextReader(sr))
                    {
                        jsonReader.SupportMultipleContent = true;

                        while (jsonReader.Read())
                        {
                            if (jsonReader.TokenType == JsonToken.StartObject)
                            {
                                var payload = _serialiser.Deserialize<StreamDeckEventPayload>(jsonReader);
                                await OnEventOccurredAsync(payload);
                            }
                        }
                    }
                }

            }
        }
    }
}
