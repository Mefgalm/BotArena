using System.Collections.Generic;

namespace BotArena.CSharp.Types
{
    public class CMatchBot
    {
        public string PlayerId { get; set; }
        public IEnumerable<CField> Fields { get; set; }
    }
}