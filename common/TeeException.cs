using System;
using System.Runtime.Serialization;

namespace common
{
	[Serializable]
	public class TeeException : System.Exception
	{
		public TeeException()
		{ }

		public TeeException(Exception ex)
			: base(string.Format("Secondary stream: {0}", ex.Message), ex)
		{ }

		protected TeeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}
}