using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.ChatSupport
{
    static class ChatServiceRouter
    {
        public static object locker = new object();
        static List<ChatRoom> chatRooms = new List<ChatRoom>();


        public static string CreateNewChatRoom()
        {
            var ret = new ChatRoom();
            lock (locker)
            {
                chatRooms.Add(ret);
            }
            return ret.instance;
        }
        public static List<ChatRoom> GetAllRooms()
        {
            return chatRooms;
        }

    }
}
