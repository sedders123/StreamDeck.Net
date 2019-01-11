using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using StreamDeck.Net.Constants;
using StreamDeck.Net.Models;

namespace StreamDeck.Net
{
    public partial class StreamDeckClient
    {
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

        private readonly Dictionary<string, string> _eventHandlers;

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
                    await (Task)handler.Method.Invoke(handler.Target, new object[] { this, data });
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
