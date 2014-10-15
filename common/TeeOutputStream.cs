using System;
using System.IO;

namespace common
{
	public class TeeOutputStream : TeeStream
	{
        //http://en.wikipedia.org/wiki/Tee_(command)
		public TeeOutputStream(Stream primary, Stream secondary, bool autoClose = false)
			: base(primary, secondary, autoClose)
		{
		}

        //Write from buffer to secondary stream
        //Write from buffer to primary stream
		public override void Write(byte[] buffer, int offset, int count)
		{
		    try
		    {
                //write buffer to secondary stream
		        Secondary.Write(buffer, offset, count);
		    }
		    catch (Exception ex)
		    {
		        throw new TeeException(ex);
		    }

		    //write buffer to primary stream
			Primary.Write(buffer, offset, count);
		}

		public override bool CanWrite { get { return true; } }
	}
}