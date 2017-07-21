using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(new IPEndPoint(IPAddress.Loopback, 5001));

                using (var rawStream = new NetworkStream(socket))
                using (var sslStream = new SslStream(rawStream))
                {
                    Console.WriteLine("Calling SslStream.AuthenticateAsClient with a list of enabled SslProtocols.");

                    Exception authenticationEx = null;
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    try
                    {
                        sslStream.AuthenticateAsClient("127.0.0.1", new X509CertificateCollection(), SslProtocols.Tls11 | SslProtocols.Tls12, checkCertificateRevocation: true);
                    }
                    catch (Exception ex)
                    {
                        authenticationEx = ex;
                    }

                    Console.WriteLine("SslStream.AuthenticateAsClient completed in {0} ms. Ex: {1}", stopWatch.ElapsedMilliseconds, authenticationEx);
                }
            }

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(new IPEndPoint(IPAddress.Loopback, 5001));

                using (var rawStream = new NetworkStream(socket))
                using (var sslStream = new SslStream(rawStream))
                {
                    Console.WriteLine("Calling SslStream.AuthenticateAsClient without extra arguments.");

                    Exception authenticationEx = null;
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    try
                    {
                        sslStream.AuthenticateAsClient("127.0.0.1");
                    }
                    catch (Exception ex)
                    {
                        authenticationEx = ex;
                    }

                    Console.WriteLine("SslStream.AuthenticateAsClient completed in {0} ms. Ex: {1}", stopWatch.ElapsedMilliseconds, authenticationEx);
                }
            }
        }
    }
}
