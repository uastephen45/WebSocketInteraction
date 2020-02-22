using ChatService.ChatSupport;
using ChatService.types;
using ChatService.types.Games.FrankenStory;
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
        List<FrankenStoryLine> Story = new List<FrankenStoryLine>();
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
            switch (chat.content.Substring(7, 3))
            {
                case "STG":
                    FrankenStoryGameUpdate gameUpdate = new FrankenStoryGameUpdate() { players = new List<Player>() };
                    foreach (KeyValuePair<String, Player> keyValuePair in players)
                    {
                        gameUpdate.players.Add(keyValuePair.Value);
                    }
                    var retval = Newtonsoft.Json.JsonConvert.SerializeObject(gameUpdate);
                    foreach (KeyValuePair<String, CPacheStream> streams in cpacheStreams)
                    {
                        streams.Value.Broadcast("SideCarSTG" + retval);
                        // becuase we are starting the game everyone gets their white cards                         
                    }
                    return "SideCarSTG" + retval;
                case "FSU"://frankenstoryUpdate
                    var storyupdate = new FrankenStoryLine() { content = chat.content.Substring(10, chat.content.Length - 10) };
                    storyupdate.username = chat.userName;
                    storyupdate.date = System.DateTime.Now;
                    Story.Add(storyupdate);
                    var ret = Newtonsoft.Json.JsonConvert.SerializeObject(storyupdate);
                    foreach (KeyValuePair<String, CPacheStream> streams in cpacheStreams)
                    {
                        //here's Story Update
                        streams.Value.Broadcast("SideCarHSU" + ret);
                        // becuase we are starting the game everyone gets their white cards                         
                    }

         

                    var i = 1;
                    var lastTurn = 0;
                    var nextTurn = 0;
                    FrankenStoryGameUpdate gameUpdate2 = new FrankenStoryGameUpdate() { players = new List<Player>() };
                    foreach (KeyValuePair<String, Player> keyValuePair in players)
                    {
                        if (keyValuePair.Value.myTurn)
                        {
                            lastTurn = i;
                        }
                        else
                        {
                            i = i + 1;
                        }
                    }
                    if (lastTurn == players.Count)
                    {
                        nextTurn = 1;
                    }
                    else
                    {
                        nextTurn = lastTurn + 1;
                    }
                    i = 1;
                    foreach (KeyValuePair<String, Player> keyValuePair in players)
                    {
                        if (nextTurn == i)
                        {
                            keyValuePair.Value.myTurn = true;
                            i = i + 1;
                        }
                        else
                        {
                            keyValuePair.Value.myTurn = false;
                            i = i + 1;
                        }
                        gameUpdate2.players.Add(keyValuePair.Value);
                    }
                   
                    var JsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(gameUpdate2);
                    foreach (KeyValuePair<String, CPacheStream> c in cpacheStreams)
                    {
                        c.Value.Broadcast("SideCarSTG" + JsonValue);
                    }

                    //tell the next person it's there turn. 
                    return "";
                default:
                    return "nothing";
            }
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
