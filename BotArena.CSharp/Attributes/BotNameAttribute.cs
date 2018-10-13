using System;

namespace BotArena.CSharp.Attributes
{
    public class BotNameAttribute : Attribute
    {
        public string Name { get; private set; }
        
        public BotNameAttribute(string name)
        {
            Name = name;
        }
    }
}