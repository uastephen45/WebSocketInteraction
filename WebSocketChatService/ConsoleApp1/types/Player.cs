using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.types
{
    public class Player
    {
        public string Username { get; set; }
        public int Score { get; set; }
        public bool myTurn { get; set; }
    }
}
