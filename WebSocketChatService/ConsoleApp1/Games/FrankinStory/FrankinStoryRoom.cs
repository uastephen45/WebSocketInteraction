using ChatService.ChatSupport;
using CSharpPacheCore.Streamers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Games.FrankinStory
{
    public class FrankinStoryRoom : IGameRoom
    {
        public string name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string topic => throw new NotImplementedException();

        public string instance => throw new NotImplementedException();

        public void addLine(ChatLine chatLine, CPacheStream cPacheStream)
        {
            throw new NotImplementedException();
        }

        public string handleClientAppMessage(ChatLine chat)
        {
            throw new NotImplementedException();
        }

        public void subscribeToChat(CPacheStream cPacheStream)
        {
            throw new NotImplementedException();
        }
    }
}
