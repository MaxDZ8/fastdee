using System;
using CommandLine;
using System.Net.Sockets;
using System.Net;

namespace fastdee
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Args>(args).MapResult(
                options => MainWithParsed(options),
                _ => -1);
        }

        static int MainWithParsed(Args options)
        {
            // Pool server needs some additional parsing while I grok the documentation and find out if the lib can parse for me.
            string poolurl;
            ushort poolport;
            {
                var parts = options.Pool.Split(':');
                if (parts.Length != 2) throw new ApplicationException("Does not look like valid POOL:PORT endpoint!");
                poolurl = parts[0];
                poolport = ushort.Parse(parts[1]);
            }
            var presentingAs = options.SubscribeAs ?? MyCanonicalSubscription();
            var serverInfo = new ServerConnectionInfo(poolurl, poolport, presentingAs, options.UserName, options.WorkerName, options.SillyPassword);
            new Stratificator(serverInfo).PumpForeverAsync().Wait(); // TODO: the other services
            return -2;
        }

        static string MyCanonicalSubscription()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version;
            if (null == version) // apparently this can happen with single-file assembly bundles or whatever they are called
            {
                return "fastdee";
            }
            var major = (uint)version.Major;
            var minor = (uint)version.Minor;
            var patch = (uint)version.Build;
            return $"fastdee/{major}.{minor}.{patch}";
        }
    }
}
