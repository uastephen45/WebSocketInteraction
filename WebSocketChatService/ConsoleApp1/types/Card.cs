using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.types
{
    public class Card
    {
       public string type { get; set; }
        public string content { get; set; }
    }
    public class Cards
    {
       public List<Card> cards { get; set; }
    }
}
