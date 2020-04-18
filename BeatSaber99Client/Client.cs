using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using BeatSaber99Client.Packets;
using BS_Utils.Gameplay;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;
using WebSocket = WebSocket4Net.WebSocket;

namespace BeatSaber99Client
{
    public static class Client
    {
        private static ClientStatus _status;
        public static ClientStatus Status
        {
            get => _status;
            set
            {
                if (value != _status)
                {
                    ClientStatusChanged?.Invoke(null, value);
                }

                _status = value;
            }
        }
        public static event EventHandler<ClientStatus> ClientStatusChanged;
        
        private static WebSocket _client;
        private static Dictionary<string, Type> _packetTypes = new Dictionary<string, Type>();

        public static void Init()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && typeof(IPacket).IsAssignableFrom(x)))
            {
                // var obj = (IPacket) Activator.CreateInstance(type);
                _packetTypes.Add(type.Name, type);
            }
        }

        public static void Send(object o)
        {
            _client.Send(JsonConvert.SerializeObject(o));
        }
        
        public static void ConnectAndMatchmake()
        {
            if (_client != null) return;
            SessionState.Clean();

            Status = ClientStatus.Connecting;

            _client = new WebSocket(PluginConfig.Instance.ServerAddress);
            _client.EnableAutoSendPing = true;

            _client.Opened += (sender, args) =>
            {
                // We are connected and automatically matchmaking.
                Plugin.log.Info("Connection created");

                var id = GetUserInfo.GetUserID();
                var name = GetUserInfo.GetUserName();
                var platform = GetUserInfo.GetPlatformInfo();

                Status = ClientStatus.Matchmaking;
                _client.Send(JsonConvert.SerializeObject(new ConnectionPacket()
                {
                    id = id.ToString(),
                    name = name,
                    platform = platform.serialzedName,
                    version = Version.VersionNumber,
                }));
            };

            _client.Error += (sender, args) =>
            {
                Plugin.log.Error(args.Exception);
                Disconnect();
            };

            _client.Closed += (sender, args) =>
            {
                Cleanup();
            };

            _client.MessageReceived += ClientOnMessageReceived;

            try
            {
                _client.Open();
            }
            catch (Exception ex)
            {
                Plugin.log.Error(ex);
                Status = ClientStatus.Waiting;
                return;
            }
        }

        private static void ClientOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var json = JObject.Parse(e.Message);
            if (!json.ContainsKey("type")) return;

            string type = json["type"]?.Value<string>();

            if (type != null && _packetTypes.TryGetValue(type, out var packetType))
            {
                var packet = json.ToObject(packetType) as IPacket;
                Executor.Enqueue(() => packet?.Dispatch());
            }
        }

        public static void Disconnect()
        {
            if (_client == null) return;
            _client.Close();
        }

        public static void Cleanup()
        {
            _client.Dispose();
            _client = null;

            Status = ClientStatus.Waiting;
        }
    }
}