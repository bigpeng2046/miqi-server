using System;
using System.Text;

using Miqi.Net;

namespace Miqi.Test
{
	public class TestMiqiMessage
	{
		public static void Main(string[] args)
		{
			MiqiMessage msg = MiqiMessage.BuildFromString("login Miqi/2.0 \r\n:456\r\nhost : 123\r\n");
			Console.WriteLine("{0}", msg.Command);
			Console.WriteLine("{0}", msg.Protocol);
			Console.WriteLine("{0}", msg.GetHeader(""));
			Console.WriteLine("{0}", msg.GetHeader("host"));
			
			Console.WriteLine("{0}", msg.ToString());
		}
	}
}
