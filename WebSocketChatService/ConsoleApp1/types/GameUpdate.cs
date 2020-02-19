using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.types
{
    public class GameUpdate
    {
        public List<Player> players { get; set; }
        public Card BlackCard { get; set; }
       
        
    }
}
