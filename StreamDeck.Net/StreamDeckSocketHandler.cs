using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StreamDeck.Net.Models;

namespace StreamDeck.Net
{
    public delegate Task StreamDeckEventHandler(object source, StreamDeckEventPayload e);

    public class StreamDeckSocketHandler
    {
        private readonly Uri _uri;
        private bool _connected;
        private readonly string _registerEvent;
        private readonly string _pluginUuid;
        public ClientWebSocket Socket { get; }

        public event StreamDeckEventHandler EventOccurredAsync;
        private Task OnEventOccurredAsync(StreamDeckEventPayload data) => EventOccurredAsync?.Invoke(this, data);


        public StreamDeckSocketHandler(Uri uri, string registerEvent, string pluginUuid)
        {
            _uri = uri;
            _registerEvent = registerEvent;
            _pluginUuid = pluginUuid;
            Socket = new ClientWebSocket();
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await Socket.ConnectAsync(_uri, cancellationToken);
            await Socket.SendAsync(GetPluginRegistrationBytes(), WebSocketMessageType.Text, true, CancellationToken.None);
            _connected = true;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var pipe = new Pipe();
            Task.WaitAll(FillPipeAsync(pipe.Writer, cancellationToken), ReadPipe(pipe.Reader, cancellationToken));
        }

        private ArraySegment<byte> GetPluginRegistrationBytes()
        {

            var registration = new PluginRegistration
            {
                Event = _registerEvent,
                Uuid = _pluginUuid
            };

            var outString = JsonConvert.SerializeObject(registration);
            var outBytes = Encoding.UTF8.GetBytes(outString);

            return outBytes;

        }

        public async Task FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
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
                    var socketResult = await Socket.ReceiveAsync(memory, cancellationToken);
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

        public async Task ReadPipe(PipeReader reader, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken);

                var jsonString = Encoding.UTF8.GetString((byte[])result.Buffer.ToArray());

                reader.AdvanceTo(result.Buffer.End);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    var payloads = new List<StreamDeckEventPayload>();
                    using (var sr = new StringReader(jsonString))
                    using (var jsonReader = new JsonTextReader(sr))
                    {
                        jsonReader.SupportMultipleContent = true;

                        var serializer = new JsonSerializer();
                        while (jsonReader.Read())
                        {
                            if (jsonReader.TokenType == JsonToken.StartObject)
                            {
                                var payload = serializer.Deserialize<StreamDeckEventPayload>(jsonReader);
                                await OnEventOccurredAsync(payload);
                            }
                        }
                    }
                }

            }
        }
    }
}
