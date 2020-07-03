

namespace Blackhawk.Websocket
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Blackhawk.Websocket.Messages;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Timers;
    using WebSocketSharp;
    public class BlackhawkWebsocket
    {
        private static BlackhawkWebsocket instance = null;
        private Dictionary<MessageTypes, Dictionary<int, Action<BaseMessage>>> observers = new Dictionary<MessageTypes, Dictionary<int, Action<BaseMessage>>>();
        private WebSocket ws;
        private System.Timers.Timer pingTimer;

        private BlackhawkWebsocket()
        {
            createObserverList();
        }

        #region Websocket helper methods
        public void startWebsocket(string host)
        {
            pingTimer = new System.Timers.Timer();
            pingTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            pingTimer.Interval = 40000;
            if (ws == null || !ws.IsAlive)
            {
                ws = new WebSocket(host);
                ws.OnOpen += (sender, e) =>
                {
                    Console.WriteLine("BlackhawkWebsocket : Connected to websocket");
                };
                ws.OnMessage += (sender, e) =>
                {
                    try
                    {
                        JObject message = JObject.Parse(e.Data);
                        string messageType = message["type"].ToObject<string>();
                        switch (messageType)
                        {
                            case "NewTitle":
                                NewTitleMessage newTitle = message["data"].ToObject<NewTitleMessage>();
                                Console.Write("BlackhawkWebsocket : Got message " + messageType + ": with data " + newTitle.ToString());
                                callObserver(MessageTypes.NewTitle, newTitle);
                                break;
                            case "NewAchievement":
                                Console.WriteLine(message["data"]);
                                NewAchievementMessage newKillAchievement = message["data"].ToObject<NewAchievementMessage>();
                                Console.Write("BlackhawkWebsocket : Got message " + messageType + ": with data " + newKillAchievement.ToString());
                                callObserver(MessageTypes.NewAchievement, newKillAchievement);
                                break;
                            default:
                                //This should not happen, report error
                                Console.WriteLine("BlackhawkWebsocket : Got message with unrecognized message type: " + messageType);
                                break;
                        }
                    }catch (JsonReaderException)
                    {
                        //Message recieved reset timer.
                        pingTimer.Stop();
                        pingTimer.Start();
                    }
                };
                ws.OnError += (sender, e) =>
                {
                    Console.WriteLine("BlackhawkWebsocket : Error " + e.Message);
                };
                ws.OnClose += WsOnOnClose;
                ws.ConnectAsync();
            }
        }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Ping was never recieved, lets reconnect
            if (ws.IsAlive)
            {
                pingTimer.Stop();
                CloseWs();
            }
        }

        public void CloseWs()
        {
            ws.CloseAsync();
        }
        private void WsOnOnClose(object sender, CloseEventArgs closeEventArgs)
        {
            if (!ws.IsAlive)
            {
                Thread.Sleep(10000);
                ws.ConnectAsync();
            }
        }
        #endregion

        #region Observer methods
        private void createObserverList()
        {
            foreach (MessageTypes messageType in (MessageTypes[])Enum.GetValues(typeof(MessageTypes)))
            {
                observers.Add(messageType, new Dictionary<int, Action<BaseMessage>>());
            }
        }


        public void callObserver(MessageTypes observerType, BaseMessage message)
        {
            Dictionary<int, Action<BaseMessage>> list;
            observers.TryGetValue(observerType, out list);
            foreach (Action<BaseMessage> obs in list.Values)
            {
                obs(message);
            }
        }
        public void addObserver(int observerId, MessageTypes observerType, Action<BaseMessage> callback)
        {
            try
            {
                Dictionary<int, Action<BaseMessage>> list;
                observers.TryGetValue(observerType, out list);
                //Because plugins get loaded and unloaded dynamicly we need to remove any previous one.
                if (list.ContainsKey(observerId))
                {
                    Console.WriteLine(" : Removing prev action");
                    list.Remove(observerId);
                }
                Console.WriteLine("BlackhawkWebsocket : adding action for " + observerId);
                list.Add(observerId, callback);
            }catch(ArgumentNullException e)
            {
                Console.WriteLine("BlackhawkWebsocket : Could not add observer!");
            }
        }
        #endregion

        #region Singleton
        public static BlackhawkWebsocket getInstance(string host)
        {
            if (instance == null)
            {
                instance = new BlackhawkWebsocket(host);
            }
            return instance;
        }
        public static BlackhawkWebsocket getInstance()
        {
            return instance;
        }
        #endregion
    }
}
