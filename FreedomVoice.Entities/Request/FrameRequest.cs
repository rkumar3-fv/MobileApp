namespace FreedomVoice.Entities.Request

{
    public class FrameRequest
    {
        public long From { get; set; }
        public long To { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
    }
}
