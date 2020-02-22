using ChatService.ChatSupport;
using ChatService.types;
using CSharpPacheCore.Streamers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Games.FrankinStory
{
    public class FrankinStoryRoom : IGameRoom
    {
        string Instance = Guid.NewGuid().ToString().Replace("-", "");
        public string instance { get { return Instance; } }
        public string name { get; set; }
        public string topic { get { return "FrankinStory"; } }
        Dictionary<String, CPacheStream> cpacheStreams = new Dictionary<string, CPacheStream>();
        public Dictionary<String, Player> players = new Dictionary<string, Player>();
        object locker = new object();
        List<ChatLine> chatContent = new List<ChatLine>();
        List<CPacheStream> disposedRunningList = new List<CPacheStream>();
        public void addLine(ChatLine chatLine, CPacheStream cPacheStream)
        {
            if (cpacheStreams.ContainsKey(cPacheStream.StreamId))
            {
                cpacheStreams.Remove(cPacheStream.StreamId);
                cpacheStreams.Add(chatLine.userName, cPacheStream);
                if (players.Count == 0)
                {
                    players.Add(chatLine.userName, new Player() { myTurn = true, Score = 0, Username = chatLine.userName });
                }
                else
                {
                    players.Add(chatLine.userName, new Player() { myTurn = false, Score = 0, Username = chatLine.userName });
                }
            }


            lock (locker)
            {
                //add it to the legder 
                chatContent.Add(chatLine);

                foreach (KeyValuePair<String, CPacheStream> client in cpacheStreams)
                {
                    var ret = JsonConvert.SerializeObject(chatLine);
                    try
                    {
                        client.Value.Broadcast(ret);
                    }
                    catch (Exception)
                    {
                        disposedRunningList.Add(client.Value);
                    }
                }
            }
        }


        public string handleClientAppMessage(ChatLine chat)
        {
            throw new NotImplementedException();
        }

        public void subscribeToChat(CPacheStream cPacheStream)
        {
            lock (locker)
            {
                //add the client to new messages 
                cpacheStreams.Add(cPacheStream.StreamId, cPacheStream);
                //send all the old messages 
                foreach (ChatLine chatLine in chatContent)
                {
                    var ret = JsonConvert.SerializeObject(chatLine);
                    cPacheStream.Broadcast(ret);
                }
            }
        }
    }
}
