using System;
using CommandLine;
using System.Net.Sockets;
using System.Net;

namespace fastdee
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Args();
            if (Parser.Default.ParseArguments<Args>(args).WithParsed(good => options = good).Tag == ParserResultType.NotParsed)
            {
                Environment.ExitCode = 1;
                return;
            }
            // I need to read the parser library documentation and figure out if it can validate those things for me.
            // I'm inclined to believe the question is not really "if" but rather just "how".
            var parts = options.Pool.Split(':');
            var uri = parts[0];
            var port = ushort.Parse(parts[1]);
            var addr = Dns.GetHostAddresses(uri)[0];
            var endpoint = new IPEndPoint(addr, port);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            Console.Write("made a connection");
            System.Threading.Thread.Sleep(1234);
        }
    }
}
