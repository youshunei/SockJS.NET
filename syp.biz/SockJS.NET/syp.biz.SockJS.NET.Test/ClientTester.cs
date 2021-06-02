using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Client;
using syp.biz.SockJS.NET.Common.Interfaces;

using WebSocketClient;

namespace syp.biz.SockJS.NET.Test
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    internal class ClientTester : ITestModule
    {
        public async Task Execute()
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                var config = Configuration.Factory.BuildDefault("http://localhost:9999/ws");
                config.Logger = new ConsoleLogger();
                config.DefaultHeaders = new WebHeaderCollection
                {
                    {HttpRequestHeader.UserAgent, "Custom User Agent"},
                    {"application-key", "foo-bar"}
                };

                var sockJs = (IClient)new Client.SockJS(config);
                sockJs.Connected += async (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine("****************** Main: Open");
                        //await sockJs.Send(JsonConvert.SerializeObject(new { foo = "bar" }));
                        //await sockJs.Send("test");

                        StompMessageSerializer serializer = new StompMessageSerializer();

                        var connect = new StompMessage("CONNECT");
                        connect["accept-version"] = "1.2";
                        connect["heart-beat"] = "10000,10000";
                        await sockJs.Send(serializer.Serialize(connect));

                        //var sub = new StompMessage("SUBSCRIBE");
                        //sub["id"] = "sub-" + 111;
                        //sub["destination"] = "/signal/public";
                        //await sockJs.Send(serializer.Serialize(sub));
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };
                sockJs.Message += async (sender, msg) =>
                {
                    try
                    {
                        Console.WriteLine($"****************** Main: Message: {msg}");
                        if (msg.Contains("CONNECTED"))
                        {
                            StompMessageSerializer serializer = new StompMessageSerializer();
                            var sub = new StompMessage("SUBSCRIBE");
                            sub["id"] = "sub-" + 111;
                            sub["destination"] = "/signal/public";
                            await sockJs.Send(serializer.Serialize(sub));
                        }
                        //Console.WriteLine("****************** Main: Got back echo -> sending shutdown");
                        //await sockJs.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };
                sockJs.Disconnected += (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine("****************** Main: Closed");
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };

                await sockJs.Connect();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }

            await tcs.Task;
        }
    }
}
