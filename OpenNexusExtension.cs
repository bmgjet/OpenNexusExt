using Oxide.Core.Extensions;
using Oxide.Core.Libraries;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Oxide.Core.OpenNexus
{
    public class OpenNexusExtension : Extension
    {
        internal static Assembly Assembly = Assembly.GetExecutingAssembly();
        internal static AssemblyName AssemblyName = Assembly.GetName();
        internal static VersionNumber AssemblyVersion = new VersionNumber(AssemblyName.Version.Major, AssemblyName.Version.Minor, AssemblyName.Version.Build);
        internal static string AssemblyAuthors = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute), false)).Company;

        public override bool IsCoreExtension => true;

        public override string Name => "OpenNexus";

        public override string Author => AssemblyAuthors;

        public override VersionNumber Version => AssemblyVersion;

        public OpenNexusExtension(ExtensionManager manager) : base(manager)
        {
        }

        public override void Load()
        {
            Manager.RegisterLibrary("OpenNexusExt", new OpenNexusExt());
        }

        public class OpenNexusExt : Library
        {

            private Thread server;
            private Thread client;
            private bool listening = false;
            private TcpListener listener;
            public int serverport;


            public OpenNexusExt()
            {
                Console.Write("OpenNexus by BMGJET");
            }

            private void ServerThread()
            {
                listener = new TcpListener(IPAddress.Any, serverport);
                listener.Start();
                listening = true;
                while (listening)
                {
                    Console.Write("Waiting for connection...");
                    TcpClient client = listener.AcceptTcpClient();

                    Console.WriteLine("Connection accepted.");
                    NetworkStream ns = client.GetStream();

                    byte[] byteTime = Encoding.ASCII.GetBytes(DateTime.Now.ToString());

                    try
                    {
                        ns.Write(byteTime, 0, byteTime.Length);
                        ns.Close();
                        client.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    Interface.CallHook("OpenNexusSent", this);
                }
                listener.Stop();
            }

            private void ClientThread(string server, int port)
            {
                try
                {
                    var client = new TcpClient(server, port);

                    NetworkStream ns = client.GetStream();

                    byte[] bytes = new byte[1024];
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    Interface.CallHook("OpenNexusRecieve", this);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                    client.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            public void OpenServer(int port)
            {
                serverport = port;
                server = new Thread(new ThreadStart(ServerThread));
                server.Start();
            }

            public void ConnectServer(string server, int port)
            {
                serverport = port;
                client = new Thread(() => ClientThread(server, port));
                client.Start();
            }

            public void CloseServer()
            {
                listening = false;
                server.Abort();
            }
        }
    }
}
