using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StreamDeck.Net.Constants;
using StreamDeck.Net.Models;

namespace StreamDeck.Net {
    /// <summary>
    /// Wrapper for the Stream Deck SDK.
    /// </summary>
    public class StreamDeckClient
    {
        /// <summary>
        /// Manages the WebScoket that the Stream Deck uses.
        /// </summary>
        public StreamDeckSocketHandler SocketHandler { get; set; }

        /// <summary>
        /// Occurs when any event is recieved from the Stream Deck.
        /// </summary>
        public event StreamDeckEventHandler EventOccurredAsync;

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        public event StreamDeckEventHandler KeyDownEventAsync;
        /// <summary>
        /// Occurs when the user releases a key.
        /// </summary>
        public event StreamDeckEventHandler KeyUpEventAsync;
        /// <summary>
        /// Occurs when an instance of an action appears.
        /// </summary>
        public event StreamDeckEventHandler WillAppearEventAsync;
        /// <summary>
        /// Occurs when an instance of an action disappears.
        /// </summary>
        public event StreamDeckEventHandler WillDisappearEventAsync;
        /// <summary>
        /// Occurs when the user changes the title or title parameters.
        /// </summary>
        public event StreamDeckEventHandler TitleParametersDidChangeEventAsync;
        /// <summary>
        /// Occurs when a device is plugged to the computer.
        /// </summary>
        public event StreamDeckEventHandler DeviceDidConnectEventAsync;
        /// <summary>
        /// Occurs when a device is unplugged from the computer.
        /// </summary>
        public event StreamDeckEventHandler DeviceDidDisconnectEventAsync;
        /// <summary>
        /// Occurs when a monitored application is launched.
        /// </summary>
        public event StreamDeckEventHandler ApplicationDidLaunchEventAsync;
        /// <summary>
        /// Occurs when a monitored application is terminated.
        /// </summary>
        public event StreamDeckEventHandler ApplicationDidTerminateEventAsync;

        /// <summary>
        /// The port the Stream Deck is connected to.
        /// </summary>
        public string Port { get; }
        /// <summary>
        /// The plugin's UUID.
        /// </summary>
        public string PluginUuid { get; }
        /// <summary>
        /// The RegistrationEvent payload.
        /// </summary>
        public string RegisterEvent { get; }
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

            Port = port;
            PluginUuid = pluginUuid;
            RegisterEvent = registerEvent;
            Info = JsonConvert.DeserializeObject<StreamDeckInfo>(info, _serialiserSettings);

            _cancellationTokenSource = new CancellationTokenSource();
            _eventHandlers = BuildEventHandlerDictionary();
        }

        /// <summary>
        /// Will handle all messages sent over the WebSocket until the program is terminated.
        /// </summary>
        public async Task RunAsync()
        {
            await SocketHandler.ConnectAsync(_cancellationTokenSource.Token);
            await SocketHandler.RunAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Will handle all messages sent over the WebSocket until the program is terminated.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to be used for all async functions.</param>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await SocketHandler.ConnectAsync(cancellationToken);
            await SocketHandler.RunAsync(cancellationToken);
        }

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<string, string> _eventHandlers;
        private JsonSerializerSettings _serialiserSettings;

        private Dictionary<string, string> BuildEventHandlerDictionary()
        {
            return new Dictionary<string, string>
            {
                { RecievedEvents.KeyDown, nameof(KeyDownEventAsync)},
                { RecievedEvents.KeyUp, nameof(KeyUpEventAsync)},
                { RecievedEvents.WillAppear, nameof(WillAppearEventAsync)},
                { RecievedEvents.WillDisappear, nameof(WillDisappearEventAsync)},
                { RecievedEvents.TitleParametersDidChange, nameof(TitleParametersDidChangeEventAsync)},
                { RecievedEvents.DeviceDidConnect, nameof(DeviceDidConnectEventAsync)},
                { RecievedEvents.DeviceDidDisconnect, nameof(DeviceDidDisconnectEventAsync)},
                { RecievedEvents.ApplicationDidLaunch, nameof(ApplicationDidLaunchEventAsync)},
                { RecievedEvents.ApplicationDidTerminate, nameof(ApplicationDidTerminateEventAsync)},
            };
        }

        private async Task OnEventOccurredAsync(StreamDeckEventPayload data)
        {
            if (EventOccurredAsync != null)
            {
                await EventOccurredAsync.Invoke(this, data);
            }
        }

        private async Task OnSpecificEventAsync(string key, StreamDeckEventPayload data)
        {
            var handlerName = _eventHandlers[key];
            var eventDelegate = (MulticastDelegate)GetType().GetField(handlerName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    await (Task) handler.Method.Invoke(handler.Target, new object[] { this, data });
                }
            }
        }

        private async Task InvokeGenericEventHandler(object e, StreamDeckEventPayload args)
        {
            await OnEventOccurredAsync(args);
        }

        private async Task InvokeSpecificEventHandlers(object e, StreamDeckEventPayload args)
        {
            await OnSpecificEventAsync(args.Event, args);
        }

    }
}