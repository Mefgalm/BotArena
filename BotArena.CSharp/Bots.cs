using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BotArena.CSharp.Attributes;
using BotArena.CSharp.Interfaces;
using BotArena.CSharp.Types;

namespace BotArena.CSharp
{
    public class BlottoMethod1 : IBlottoMethod
    {
        public BlottoMethod1()
        {            
        }
        
        [BotName("FirstBot")]
        public IEnumerable<CField> Invoke(string yourBotId, int fieldCount, int tankCount, IEnumerable<CMatchResult> matchResults)
        {
            int botCountPerField = tankCount / fieldCount;
            return Enumerable.Range(0, fieldCount).Select(x => new CField(x, botCountPerField));
        }
    }

    public class BlottoMethod2 : IBlottoMethod
    {
        public BlottoMethod2()
        {            
        }
        
        [BotName("SecondBot")]
        public IEnumerable<CField> Invoke(string yourBotId, int fieldCount, int tankCount, IEnumerable<CMatchResult> matchResults)
        {
            yield return new CField(0, 30);
            yield return new CField(1, 20);
            yield return new CField(2, 10);
        }
    }
    
    public class BlottoMethod3 : IBlottoMethod
    {
        public BlottoMethod3()
        {            
        }
        
        [BotName("ThirdBot")]
        public IEnumerable<CField> Invoke(string yourBotId, int fieldCount, int tankCount, IEnumerable<CMatchResult> matchResults)
        {
            yield return new CField(0, 20);
            yield return new CField(1, 20);
            yield return new CField(2, 0);
        }
    }
}