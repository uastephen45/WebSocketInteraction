using ChatService.types;
using CSharpPacheCore.Streamers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChatService.ChatSupport
{

    public class CaHRoom : IGameRoom
    {
         string Instance = Guid.NewGuid().ToString().Replace("-", "");
        public string instance { get { return Instance; } }

        public string name { get; set; }
        public string topic { get { return "CaH"; } }

        List<ChatLine> chatContent = new List<ChatLine>();
        Dictionary<String, CPacheStream> cpacheStreams = new Dictionary<string, CPacheStream>();
        List<CPacheStream> disposedRunningList = new List<CPacheStream>();
        List<Card> whiteCards = new List<Card>();
        List<Card> blackCards = new List<Card>();
        Dictionary<String, Card> currentWhiteCards = new Dictionary<string, Card>();
        public List<String> OpenColors = new List<string>();
        public Dictionary<String, Player> players = new Dictionary<string, Player>();
        object locker = new object();

        
        
        public CaHRoom()
        {
            InitCards();
        }
        public string handleClientAppMessage(ChatLine chat)
        {
            switch (chat.content.Substring(7, 3))
            {
                case "STG":
                    GameUpdate gameUpdate = new GameUpdate() { players = new List<Player>() };
                    foreach (KeyValuePair<String, Player> keyValuePair in players)
                    {
                        gameUpdate.players.Add(keyValuePair.Value);
                    }
                    gameUpdate.BlackCard = getBlackCard();
                    var retval = Newtonsoft.Json.JsonConvert.SerializeObject(gameUpdate);
                    foreach (KeyValuePair<String, CPacheStream> streams in cpacheStreams)
                    {
                        streams.Value.Broadcast("SideCarSTG" + retval);
                        // becuase we are starting the game everyone gets their white cards 
                        for (int q = 0; q < 6; q++)
                        {
                            Card card1 = getWhiteCard();
                            Card bcard = getBlackCard();
                            var retString = Newtonsoft.Json.JsonConvert.SerializeObject(card1);
                            streams.Value.Broadcast("SideCarHWC" + retString);
                        }
                    }
                    return "SideCarSTG" + retval;
                //whitecarddelivery
                case "WCD":
                    //todo add card to correct round. 
                    var card = new Card() { content = chat.content.Substring(10, chat.content.Length - 10) };
                    currentWhiteCards.Add(chat.userName, card);
                    var ret = Newtonsoft.Json.JsonConvert.SerializeObject(getWhiteCard());
                    foreach (KeyValuePair<String, CPacheStream> stream in cpacheStreams)
                    {
                        if (chat.userName == stream.Key)
                        {
                            stream.Value.Broadcast("SideCarHWC" + ret);
                        }
                    }
                    if (players.Count - 1 == currentWhiteCards.Count)
                    {
                        foreach (KeyValuePair<String, CPacheStream> stream in cpacheStreams)
                        {
                            foreach (var pWhiteCard in currentWhiteCards)
                            {
                                stream.Value.Broadcast("SideCarPWC" + Newtonsoft.Json.JsonConvert.SerializeObject(pWhiteCard.Value));
                            }
                        }
                    }

                    return "";

                //whitecardselectforwinner
                case "WCS":
                    var winner = "";
                    var cardString = chat.content.Substring(10, chat.content.Length - 10);
                    Card winnerCard = new Card();
                    foreach (KeyValuePair<String, Card> whitecard in currentWhiteCards)
                    {
                        if (whitecard.Value.content == cardString)
                        {
                            winner = whitecard.Key;
                            winnerCard = whitecard.Value;
                        }
                    }
                    players[winner].Score = players[winner].Score + 1;
                    var winnerCardJson = Newtonsoft.Json.JsonConvert.SerializeObject(winnerCard);
                    //TODO update the clients of the winning Card. 
                    foreach (KeyValuePair<String, CPacheStream> c in cpacheStreams)
                    {
                        c.Value.Broadcast("SideCarWWC" + winnerCardJson);
                    }
                    System.Threading.Thread.Sleep(3000);

                    GameUpdate gameUpdate2 = new GameUpdate() { players = new List<Player>() };

                    var i = 1;
                    var lastTurn = 0;
                    var nextTurn = 0;

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
                    gameUpdate2.BlackCard = getBlackCard();
                    var JsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(gameUpdate2);
                    currentWhiteCards.Clear();
                    foreach (KeyValuePair<String, CPacheStream> c in cpacheStreams)
                    {
                        c.Value.Broadcast("SideCarSTG" + JsonValue);
                    }

                    return "";

                //getNewCard TODO
                case "GNC":
                    return "";

                default:
                    return "";

            }

        }

        public Card getWhiteCard()
        {

            var rand = new Random();
            var ret = whiteCards[rand.Next(0, whiteCards.Count - 1)];
            var i = whiteCards.Remove(ret);
            return ret;
        }
        public Card getBlackCard()
        {

            var rand = new Random();
            var ret = blackCards[rand.Next(0, blackCards.Count - 1)];
            var i = blackCards.Remove(ret);
            return ret;
        }


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

        public void InitCards()
        {

            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);
            directory = String.Concat(directory + "//StaticResources//");


            using (StreamReader sr = new StreamReader(string.Concat(directory, "WhiteCards.json")))
            {
                // Read the stream to a string, and write the string to the console.
                String line = sr.ReadToEnd();
                var wCards = JsonConvert.DeserializeObject<Cards>(line);
                foreach (var card in wCards.cards)
                {
                    whiteCards.Add(new Card() { content = card.content, type = card.type });
                }
                using (StreamReader bsr = new StreamReader(string.Concat(directory, "BlackCards.json")))
                {
                    // Read the stream to a string, and write the string to the console.
                    String bline = bsr.ReadToEnd();
                    var bCards = JsonConvert.DeserializeObject<Cards>(bline);
                    foreach (var card in bCards.cards)
                    {
                        blackCards.Add(new Card() { content = card.content, type = card.type });
                    }
                }
            }

        }

        
    }
}
