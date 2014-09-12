﻿using System.IO;

namespace WFS210.IO
{
	public class MessageWriter
	{
		protected readonly Stream Stream;

		protected readonly MessageSerializer Serializer;

		public MessageWriter(Stream stream, MessageSerializer serializer)
		{
			this.Stream = stream;
			this.Serializer = serializer;
		}

		public void Write (Message message)
		{
			Serializer.Serialize (Stream, message);
		}
	}
}

