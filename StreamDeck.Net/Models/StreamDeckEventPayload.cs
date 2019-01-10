using System;
using System.Collections.Generic;
using System.Text;

namespace StreamDeck.Net.Models
{
    public class StreamDeckEventPayload : EventArgs
    {
        public string Action { get; set; }
        public string Event { get; set; }
        public string Context { get; set; }
        public string Device { get; set; }
        public Payload Payload { get; set; }
    }
}
