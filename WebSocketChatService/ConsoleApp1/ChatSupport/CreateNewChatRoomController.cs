using CSharpPacheCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.ChatSupport
{
    public class CreateNewChatRoomController : AbstractUserController
    {
        public CreateNewChatRoomController()
        {

        }

        public override UserControllerConfig Config()
        {
            return new UserControllerConfig() { HttpMethod = HttpMethod.GET, Url = "/api/CreateNewChatRoom" };
        }

        protected override HttpResponse Response(HttpRequest req)
        {
            var retid = ChatServiceRouter.CreateNewChatRoom();
            var ret = JsonConvert.SerializeObject(retid);
            return new HttpResponse() { ContentType = "application/json", ByteArrayResponseBody = UTF8Encoding.UTF8.GetBytes(ret), ResponseBody = ret, SC = StatusCode.Ok };

        }
    }
}
