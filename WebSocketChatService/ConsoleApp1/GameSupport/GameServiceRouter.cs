using ChatService.Games.FrankinStory;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.ChatSupport
{
    static class GameServiceRouter
    {
        public static object locker = new object();
        static List<IGameRoom> gameRooms = new List<IGameRoom>();

        public static IGameRoom getnewGameRoom(string gameMode) {

            switch (gameMode)
            {
                case "CaH":
                    return new CaHRoom();
                case "FrankinStory":
                    return new FrankinStoryRoom();
                default:
                    throw new Exception("GameMode Invalid");
            }
            
        }
        public static string CreateNewChatRoom(string gameMode)
        {
            var ret = getnewGameRoom(gameMode);
            lock (locker)
            {
                gameRooms.Add(ret);
            }
            return ret.instance;
        }
        public static List<IGameRoom> GetAllRooms()
        {
            return gameRooms;
        }

    }
}
