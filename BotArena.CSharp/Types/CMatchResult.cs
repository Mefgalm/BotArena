namespace BotArena.CSharp.Types
{
    public class CMatchResult
    {
        public CMatchBot FirstBot { get; set; }
        public CMatchBot SecondBot { get; set; }
        public string Winner { get; set; }
    }
}