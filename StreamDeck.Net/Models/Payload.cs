namespace StreamDeck.Net.Models {
    public class Payload
    {
        public dynamic Settings { get; set; }
        public Coordinates Coordinates { get; set; }
        public int State { get; set; }
        public int UserDesiredState { get; set; }
        public bool IsInMultiAction { get; set; }
    }
}