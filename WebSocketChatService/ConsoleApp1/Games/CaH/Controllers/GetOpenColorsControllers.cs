using CSharpPacheCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.ChatSupport
{
    public class GetOpenColorsControllers : AbstractUserController
    {
        public GetOpenColorsControllers()
        {

        }

        public override UserControllerConfig Config()
        {
            return new UserControllerConfig() { HttpMethod = HttpMethod.GET, Url = "/api/Colors" };
        }

        protected override HttpResponse Response(HttpRequest req)
        {
            var roomid = req.QueryParameters["roomid"].Replace("%22", "");
            IGameRoom chatRoom = null;
            foreach(var room in GameServiceRouter.GetAllRooms())
            {
                if(room.instance == roomid)
                {
                    chatRoom = room;
                }
            }
            var ret = "gren";
            var JsonRet = Newtonsoft.Json.JsonConvert.SerializeObject(ret);
            return new HttpResponse() { ContentType = "application/json", ByteArrayResponseBody = UTF8Encoding.UTF8.GetBytes(JsonRet), ResponseBody = JsonRet, SC = StatusCode.Ok };

        }
    }
}

