using System;
using System.IO;

namespace common
{
	public class TeeStream : Stream
	{
		protected TeeStream(Stream primary, Stream secondary, bool autoClose)
		{
			Primary = primary;
			Secondary = secondary;
			AutoClose = autoClose;
		}

		protected Stream Primary { get; private set; }
		protected Stream Secondary { get; private set; }
		private bool AutoClose { get; set; }

		public override void Flush()
		{
			Primary.Flush();
			try { Secondary.Flush(); }
			catch (Exception ex) { throw new TeeException(ex); }
		}

		public override void Close()
		{
			Primary.Close();
			if (AutoClose)
			{
				try { Secondary.Close(); }
				catch (Exception ex) { throw new TeeException(ex); }
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Primary.Dispose();
				if (AutoClose)
				{
					try { Secondary.Dispose(); }
					catch (Exception ex) { throw new TeeException(ex); }
				}
			}
		}

		public override bool CanRead { get { return false; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return false; } }

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}