using CSharpPacheCore.Streamers;
using CSharpPacheCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.ChatSupport
{
    public class FrankinStoryWebSocket : AbstractWebSocket
    {
        IGameRoom ChatRoom;
        CPacheStream myStream;
        public override WebSocketConfig Config()
        {
            return new WebSocketConfig() { Url = "/api/FrankinStoryService" };
        }

        protected override void ClientStream(HttpRequest request, CPacheStream cPacheStream)
        {
            myStream = cPacheStream;
            var id = request.Url.Split("?roomid=")[1].Replace("%22", "");
            lock (GameServiceRouter.locker)
            {
                foreach (IGameRoom chatRoom in GameServiceRouter.GetAllRooms())
                {
                    if (chatRoom.instance == id)
                    {
                        ChatRoom = chatRoom;
                    }
                }
            }

            ChatRoom.subscribeToChat(cPacheStream);
        }

        protected override void DisposedClientStream(CPacheStream cPacheStream)
        {
            throw new NotImplementedException();
        }

        protected override void MessageReceived(string Message)
        {
            ChatLine chat = JsonConvert.DeserializeObject<ChatLine>(Message);
            chat.dateTime = System.DateTime.Now;
                if (chat.content.Length <= 7)
            {       
                ChatRoom.addLine(chat,myStream);
                return;
            }
           if (chat.content.Substring(0, 7) == "SideCar")
            {
                var ret = ChatRoom.handleClientAppMessage(chat);
                //if(ret != "")
                //{
                //    myStream.Broadcast(ret);
                //}                             
            }
            else
            {
                ChatRoom.addLine(chat,myStream);
                return;
            }
        
        }
    }
}
