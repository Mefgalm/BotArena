using System;
using System.Collections.Generic;
using BotArena.CSharp.Types;

namespace BotArena.CSharp.Interfaces
{
    public interface IBlottoMethod
    {
        IEnumerable<CField> Invoke(string yourBotId, int fieldCount, int tankCount, IEnumerable<CMatchResult> matchResults);
    }
}