using System.Collections.Generic;

namespace StreamDeck.Net.Models {
    internal class StreamDeckEvent
    {
        public string Event { get; set; }
        public string Context { get; set; }
        public Dictionary<string, object> Payload { get; set; }

    }
}