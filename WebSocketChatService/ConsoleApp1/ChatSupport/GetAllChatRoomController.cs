using CSharpPacheCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.ChatSupport
{
    public class GetAllChatRoomController : AbstractUserController
    {
        public GetAllChatRoomController()
        {

        }

        public override UserControllerConfig Config()
        {
            return new UserControllerConfig() { HttpMethod = HttpMethod.GET, Url = "/api/ChatRooms" };
        }

        protected override HttpResponse Response(HttpRequest req)
        {
            var list = ChatServiceRouter.GetAllRooms();
            var ret = JsonConvert.SerializeObject(list);

            return new HttpResponse() { ContentType = "application/json", ByteArrayResponseBody = UTF8Encoding.UTF8.GetBytes(ret), ResponseBody = ret, SC = StatusCode.Ok };

        }
    }
}

