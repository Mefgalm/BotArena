namespace BotArena.CSharp.Types
{
    public class CField
    {
        public CField(int position, int quantity)
        {
            Position = position;
            Quantity = quantity;
        }
        
        public int Position { get; set; }
        public int Quantity { get; set; }
    }
}