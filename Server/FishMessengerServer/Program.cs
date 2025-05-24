using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
internal class Program
{
    public readonly string currentVer = "0.2.0";
    private static async Task Main(string[] args)
    {

        if (!File.Exists("data.ini"))
        {
            using (FileStream fs = File.Create(@"data.ini"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("[UserKeys]\n[Messages]");
                fs.Write(info, 0, info.Length);
            }
        }
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();


        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);

        var connections = new List<WebSocket>();

        var connectionsNames = new List<string>();

        var connectionsID = new List<string>();

        var connectionsPFP = new List<string>();

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                string curName = string.Empty;
                string curID = string.Empty;
                string curPFP = string.Empty;
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                connections.Add(webSocket);
                await RecieveMessage(webSocket,
                    async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            try
                            {
                                IniFile myIni = new IniFile("config.ini");
                                IniFile dataIni = new IniFile("data.ini");
                                JObject obj = JObject.Parse(message);
                                if (obj["key"].ToString() == "Join")
                                {
                                    if (obj["pass"].ToString() == myIni.Read("password", "Server"))
                                    {
                                        if (connections.Count < int.Parse(myIni.Read("max", "Server")))
                                        {
                                            if (!dataIni.KeyExists(obj["user"].ToString() + obj["ID"].ToString(), "UserKeys") || dataIni.Read(obj["user"].ToString() + obj["ID"].ToString(), "UserKeys") == obj["userpass"].ToString())
                                            {
                                                Console.WriteLine("{\"pass\":\"200\",\"servname\":\"" + myIni.Read("name", "Server") + "\",\"desc\":\"" + myIni.Read("desc", "Server") + "\",\"max\":\"" + myIni.Read("max", "Server") + "\",\"timeout\":\"" + myIni.Read("chattimeout", "Server") + "\"}");
                                                var bytes = Encoding.UTF8.GetBytes("{\"pass\":\"200\",\"servname\":\"" + myIni.Read("name", "Server") + "\",\"desc\":\"" + myIni.Read("desc", "Server") + "\",\"max\":\"" + myIni.Read("max", "Server") + "\",\"timeout\":\"" + myIni.Read("chattimeout", "Server") + "\",\"charlimit\":\"" + myIni.Read("charlimit", "Server") + "\"}");
                                                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                                                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                                                curName = obj["user"].ToString();
                                                curID = obj["ID"].ToString();
                                                curPFP = obj["pfp"].ToString();
                                                long unixTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                                                await Broadcast("{\"key\":\"ServMess\",\"date\":\"" + DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToString() + "\",\"mess\":\"" + curName + " joined the room" + "\"}");
                                                if (!dataIni.KeyExists(obj["user"].ToString() + obj["ID"].ToString(), "UserKeys"))
                                                {
                                                    dataIni.Write(obj["user"].ToString() + obj["ID"].ToString(), obj["userpass"].ToString(), "UserKeys");
                                                }
                                                dataIni.Write(obj["user"].ToString() + obj["ID"].ToString() + "PFP", obj["pfp"].ToString(), "UserKeys");
                                                connectionsNames.Add(curName);
                                                connectionsID.Add(curID);
                                                connectionsPFP.Add(obj["pfp"].ToString());
                                            }
                                            else
                                            {
                                                var bytes = Encoding.UTF8.GetBytes("{\"pass\":\"401\"" + "}");
                                                if (webSocket.State == WebSocketState.Open)
                                                {
                                                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                                                    await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var bytes = Encoding.UTF8.GetBytes("{\"pass\":\"429\"" + "}");
                                            if (webSocket.State == WebSocketState.Open)
                                            {
                                                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                                                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var bytes = Encoding.UTF8.GetBytes("{\"pass\":\"403\"" + "}");
                                        if (webSocket.State == WebSocketState.Open)
                                        {
                                            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                                            await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                                        }
                                    }
                                }
                                else if (obj["key"].ToString() == "UserMess")
                                {
                                    await Broadcast(message);
                                }
                                else if (obj["key"].ToString() == "MemberRequest")
                                {
                                    string memlist = "{}";
                                    var array = JObject.Parse(memlist);
                                    for (var i = 0; i < connectionsNames.Count; i++)
                                    {
                                        array.Add(i + "Name", connectionsNames[i]);
                                        array.Add(i + "ID", connectionsID[i]);
                                        array.Add(i + "PFP", connectionsPFP[i]);
                                    }
                                    array.Add("Count", connectionsNames.Count.ToString());
                                    array.Add("key", "MemberResponse");
                                    memlist = JsonConvert.SerializeObject(array, Formatting.None);
                                    Console.WriteLine(memlist);
                                    var bytes = Encoding.UTF8.GetBytes(memlist);
                                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                                    await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                                else
                                {
                                    Console.WriteLine("Unknown Request: " + message);
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    Console.WriteLine(ex.InnerException.Message);
                                }
                                catch (NullReferenceException)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }

                        }
                        else if (result.MessageType == WebSocketMessageType.Close || webSocket.State == WebSocketState.Aborted)
                        {
                            connections.Remove(webSocket);
                            connectionsNames.Remove(curName);
                            connectionsID.Remove(curID);
                            connectionsPFP.Remove(curPFP);
                            long unixTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                            await Broadcast("{\"key\":\"ServMess\",\"date\":\"" + DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToString() + "\",\"mess\":\"" + curName + " left the room" + "\"}");
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                        }
                    });
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });

        async Task RecieveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        async Task Broadcast(string message)
        {
            IniFile myIni = new IniFile("data.ini");
            if (!myIni.KeyExists("Count", "Messages"))
            {
                myIni.Write("Count", "0", "Messages");
            }
            myIni.Write(myIni.Read("Count", "Messages"), message, "Messages");
            myIni.Write("Count", (int.Parse(myIni.Read("Count", "Messages")) + 1).ToString(), "Messages");

            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var socket in connections)
            {
                if (socket.State == WebSocketState.Open)
                {
                    Console.WriteLine("Broadcast: " + message);
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        await app.RunAsync();
    }
}