using System;
using System.IO;

namespace common
{
	public class TeeInputStream : TeeStream
	{
        //http://en.wikipedia.org/wiki/Tee_(command)
		public TeeInputStream(Stream inputStream, Stream secondary, bool autoClose = false)
			: base(inputStream, secondary, autoClose)
		{
		}

        //Read from primary stream into buffer
        //Write buffer to secondary stream
		public override int Read(byte[] buffer, int offset, int count)
		{
            //read into buffer from primary stream
			int read = Primary.Read(buffer, offset, count);

		    try
		    {
                //write buffer to secondary stream
		        Secondary.Write(buffer, offset, read);
		    }
		    catch (Exception ex)
		    {
		        throw new TeeException(ex);
		    }

		    return read;
		}

		public override bool CanRead { get { return true; } }
	}
}