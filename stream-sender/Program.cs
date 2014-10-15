using System;
using System.IO;
using System.ServiceModel;
using contract;

namespace stream_sender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Sender is up and running. Hit <ENTER> to begin the stream transmission...");
            Console.WriteLine();
            Console.ReadLine();

            var channel = new ChannelFactory<IStreaming>("stream-forwarder-ep").CreateChannel();

            try
            {
                var fileStream = File.OpenRead(@"c:\temp\MP3.rar");

                //use a rar or zip compressed file
                //request-response operation so thread is blocked here
                channel.ReceiveStream(fileStream);
                ((IClientChannel)channel).Close();

                fileStream.Close();

                Console.WriteLine("The stream was sent. Hit <ENTER> to stop the sender...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                ((IClientChannel)channel).Abort();

                Console.WriteLine(ex);
            }
        }
    }
}
