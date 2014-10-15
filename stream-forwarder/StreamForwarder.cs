using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using common;
using contract;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace stream_forwarder
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
                     ConcurrencyMode = ConcurrencyMode.Single)]
    public class StreamForwarder : IStreaming
    {
        public void ReceiveStream(Stream streamFromSender)
        {
            Console.WriteLine("Forwarding the stream...");

            try
            {
                //PipeStream: http://www.codeproject.com/Articles/16011/PipeStream-a-Memory-Efficient-and-Thread-Safe-Stre
                //redirecting the output of one process to the input of another in the command line without using any intermediate data storage.
                var pipeToReceiver = new PipeStream();

                //setup a pipe between forwarder (this) and receiver - for now the pipe is still empty
                //ASYNC
                var forwardingTask = Task.Factory.StartNew(() => ForwardStream(pipeToReceiver));

                Directory.CreateDirectory("forwarder");
                var outputFileStream = File.OpenWrite(@"forwarder\received-compressed-file.rar");
                var teeCache = new TeeOutputStream(outputFileStream, pipeToReceiver);
                var tee = new TeeInputStream(streamFromSender, teeCache, true);

                DecompressStream(tee);

                Console.WriteLine("Copied .RAR locally + extracted .RAR!");

                //pipeToReceiver cannot be closed because it can still contain data to be processed at the receiver's end
                teeCache.Flush(); //flushes both streams
                Console.WriteLine("Pipe to receiver flushed...");

                tee.Close(); //closes primary and secondary stream (autoClose = true) ; secondary stream => will only close outputFileStream (autoClose = false)

                //wait untill all bytes from input are transferred to the receiver
                forwardingTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Stream successfully forwarded!");
        }

        private void ForwardStream(Stream pipeStream)
        {
            //forward stream to receiver
            var channel = new ChannelFactory<IStreaming>("stream-receiver-ep").CreateChannel();

            try
            {
                //request-response operation so thread is blocked here
                channel.ReceiveStream(pipeStream);
                ((IClientChannel)channel).Close();

                pipeStream.Close();

                Console.WriteLine("Done streaming to receiver!");
            }
            catch (Exception ex)
            {
                ((IClientChannel)channel).Abort();

                throw;
            }
        }

        private void DecompressStream(Stream teeInputStream)
        {
            //teeInputStream is a TeeInputStream with primary stream = streamFromSender and secondary stream = teeCache (TeeOutputStream)

            //setup a pipe between incoming stream and filesystem
            var decompressPipe = new PipeStream();

            //teeInputStream.CopyTo(decompressPipe) operation = READ bytes from teeInputStream and WRITE these bytes to decompressPipe
            //READ: TeeInputStream(streamFromSender, teeCache).Read() reads bytes from streamFromSender and writes to teeCache (TeeOutputStream)
            //      TeeOutputStream.Write() writes the bytes to the pipe between forwarder and receiver (pipeToReceiver as Secondary stream) and to the file system (outputFileStream as Primary stream)
            //WRITE: Bytes read from TeeInputStream(streamFromSender, teeCache).Read() are written into decompressPipe (ie. filesystem)
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
                    reader.WriteEntryToDirectory(@"forwarder", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
            }

            //wait untill all bytes from input are read
            sourceStreamReadingTask.Wait();
        }
    }
}
