using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using common;
using contract;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace stream_receiver
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
                     ConcurrencyMode = ConcurrencyMode.Single)]
    public class StreamReceiver : IStreaming
    {
        public void ReceiveStream(Stream streamFromSender)
        {
            Console.WriteLine("Receiving stream...");

            try
            {
                Directory.CreateDirectory("receiver");
                var teeStream = new TeeInputStream(streamFromSender, File.OpenWrite(@"receiver\received-compressed-file.rar"), true);

                DecompressStream(teeStream);

                teeStream.Close(); //closes primary and secondary stream (autoClose = true)
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Copied .RAR locally + extracted .RAR!");
        }

        private void DecompressStream(Stream teeInputStream)
        {
            //PipeStream: http://www.codeproject.com/Articles/16011/PipeStream-a-Memory-Efficient-and-Thread-Safe-Stre
            //redirecting the output of one process to the input of another in the command line without using any intermediate data storage.
            var decompressPipe = new PipeStream();

            //teeInputStream.CopyTo(decompressPipe) operation = READ bytes from teeInputStream and WRITE these bytes to decompressPipe
            //READ: TeeInputStream(streamFromSender, file).Read() reads bytes from streamFromSender and writes to file on disk
            //WRITE: Bytes read from TeeInputStream(streamFromSender, file).Read() are written into decompressPipe (ie. filesystem)
            var sourceStreamReadingTask = Task.Factory.StartNew(() => teeInputStream.CopyTo(decompressPipe));

            //extract the rar file with SharpCompress
            //ASYNC => start extracting when bytes are being transferred over the pipe
            var reader = ReaderFactory.Open(decompressPipe);

            Console.WriteLine("{0}Files in archive:", "".PadLeft(1));

            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    Console.WriteLine("{0}{1}", "".PadLeft(2), reader.Entry.FilePath);
                    reader.WriteEntryToDirectory(@"receiver", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
            }

            //wait untill all bytes from input are read
            sourceStreamReadingTask.Wait();
        }
    }
}
