﻿using System.Collections.Generic;
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
        public IEnumerable<CField> Invoke(int fieldCount, int tankCount, IEnumerable<CMatchResult> matchResults)
        {
            yield return new CField(0, 10);
            yield return new CField(1, 20);
            yield return new CField(2, 30);
        }
    }

    public class BlottoMethod2 : IBlottoMethod
    {
        public BlottoMethod2()
        {            
        }
        
        [BotName("SecondBot")]
        public IEnumerable<CField> Invoke(int fieldCount, int tankCount, IEnumerable<CMatchResult> matchResults)
        {
            yield return new CField(0, 30);
            yield return new CField(1, 20);
            yield return new CField(2, 10);
        }
    }
}