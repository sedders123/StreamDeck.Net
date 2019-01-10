using System;
using System.Threading;
using System.Threading.Tasks;
using StreamDeck.Net.Models;

namespace StreamDeck.Net {
    public class StreamDeckClient
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        public StreamDeckSocketHandler SocketHandler { get; set; }

        public event StreamDeckEventHandler EventOccurredAsync;

        private async Task OnEventOccurredAsync(StreamDeckEventPayload data)
        {
            if (EventOccurredAsync != null)
            {
                await EventOccurredAsync.Invoke(this, data);
            }
        }

        public event StreamDeckEventHandler KeyDownEventAsync;
        private async Task OnKeyDownEventAsync(StreamDeckEventPayload data)
        {
            if (KeyDownEventAsync != null)
            {
                await KeyDownEventAsync.Invoke(this, data);
            }
        }

        public event StreamDeckEventHandler KeyUpEventAsync;

        private async Task OnKeyUpEventAsync(StreamDeckEventPayload data)
        {
            if (KeyUpEventAsync != null)
            {
                await KeyUpEventAsync.Invoke(this, data);
            }
        }


        public StreamDeckClient(string port, string pluginUuid, string registerEvent, string info)
        {
            SocketHandler = new StreamDeckSocketHandler(new Uri($"ws://localhost:{port}"), registerEvent, pluginUuid);

            SocketHandler.EventOccurredAsync += InvokeSpecificEventHandlers;
            SocketHandler.EventOccurredAsync += InvokeGenericEventHandler;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task InvokeGenericEventHandler(object e, StreamDeckEventPayload args)
        {
            await OnEventOccurredAsync(args);
        }

        private async Task InvokeSpecificEventHandlers(object e, StreamDeckEventPayload args)
        {
            switch (args.Event)
            {
                case "keyDown":
                    await OnKeyDownEventAsync(args);
                    break;
                case "keyUp":
                    await OnKeyUpEventAsync(args);
                    break;
            }
        }

        public async Task RunAsync()
        {
            await SocketHandler.ConnectAsync(_cancellationTokenSource.Token);
            await SocketHandler.RunAsync(_cancellationTokenSource.Token);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await SocketHandler.ConnectAsync(cancellationToken);
            await SocketHandler.RunAsync(cancellationToken);
        }

    }
}