using ChatService.types;
using CSharpPacheCore.Streamers;

namespace ChatService.ChatSupport
{
    public interface IGameRoom
    {
        public string name { get; set; }
        public string topic { get;}
        public string instance { get;}

        public void addLine(ChatLine chatLine, CPacheStream cPacheStream);
        public string handleClientAppMessage(ChatLine chat);
        public void subscribeToChat(CPacheStream cPacheStream);
    }
}