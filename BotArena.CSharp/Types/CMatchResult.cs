namespace BotArena.CSharp.Types
{
    public class CMatchResult
    {
        public CMatchBot OffenceBot { get; set; }
        public CMatchBot DefenceBot { get; set; }
        public string Winner { get; set; }
    }
}