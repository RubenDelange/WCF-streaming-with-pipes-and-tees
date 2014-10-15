using System;
using System.ServiceModel;

namespace stream_receiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //self hosting a WCF service
            //in case of IIS hosting: consider IIS 7 compression
            var serviceHost = new ServiceHost(typeof(StreamReceiver));
            serviceHost.Open();

            Console.WriteLine("Stream receiver is up and running. Hit <ENTER> to close...");
            Console.WriteLine();
            Console.ReadLine();

            serviceHost.Close();
        }
    }
}
