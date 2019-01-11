using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StreamDeck.Net.Constants;
using StreamDeck.Net.Models;

namespace StreamDeck.Net
{
    public partial class StreamDeckClient
    {
        /// <summary>
        /// Sets the title displayed by an instance of an action.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <param name="title">Action Title.</param>
        /// <param name="destination">Event Destination.</param>
        public async Task SetTitle(string context, string title, Destination destination = Destination.HardwareAndSoftware)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.SetTitle,
                Context = context,
                Payload = new Dictionary<string, object>
                {
                    { "title", title ?? "" },
                    { "target", (int)destination }
                }
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// Sets the image displayed by an instance of an action.
        /// If <see cref="image"/> is null or empty the action is rest to its original image.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <param name="image">Image as a Base64 string.</param>
        /// <param name="destination">Event Destination.</param>
        public async Task SetImage(string context, string image, Destination destination = Destination.HardwareAndSoftware)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.SetImage,
                Context = context,
                Payload = new Dictionary<string, object>
                {
                    { "image", image ?? "" },
                    { "target", (int)destination }
                }
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// Temporarily show an alert icon on the image displayed by an instance of an action.
        /// </summary>
        /// <param name="context">Action context.</param>
        public async Task ShowAlert(string context)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.ShowAlert,
                Context = context
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// Temporarily show an OK checkmark icon on the image displayed by an instance of an action.
        /// </summary>
        /// <param name="context">Action context.</param>
        public async Task ShowOk(string context)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.ShowOk,
                Context = context
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// Save persistent data for the action's instance
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <param name="settings">An object which is persistently saved for the action's instance.</param>
        public async Task SetSettings(string context, Dictionary<string, object> settings)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.SetSettings,
                Context = context,
                Payload = settings
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// Dynamically change the state of an action supporting multiple states
        /// </summary>
        /// <param name="context">Action context</param>
        /// <param name="state">A zero indexed integer representing the desired state</param>
        public async Task SetState(string context, int state)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.SetState,
                Context = context,
                Payload = new Dictionary<string, object>
                {
                    { "state", state }
                }
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// Send a payload to the Property Inspector
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <param name="payload">An object that will be received by the Property Inspector.</param>
        public async Task SendToPropertyInspector(string context, Dictionary<string, object> payload)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.SendToProperyInspector,
                Context = context,
                Payload = payload
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// The name of the profile to switch to.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="profileName">The name of the profile to switch to.</param>
        public async Task SwitchToProfile(string context, string deviceId, string profileName)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.SwitchToProfle,
                Context = context,
                Payload = new Dictionary<string, object>
                {
                    { "profile", profileName}
                }
            };
            await SendEvent(@event);
        }

        /// <summary>
        /// The name of the profile to switch to.
        /// </summary>
        /// <param name="url">A URL to open in the default browser.</param>
        public async Task OpenUrl(string url)
        {
            var @event = new StreamDeckEvent
            {
                Event = SentEvents.OpenUrl,
                Payload = new Dictionary<string, object>
                {
                    { "url", url}
                }
            };
            await SendEvent(@event);
        }


        private async Task SendEvent(StreamDeckEvent @event)
        {
            var json = JsonConvert.SerializeObject(@event, _serialiserSettings);
            var bytes = Encoding.UTF8.GetBytes(json);
            await SocketHandler.Socket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }
}
