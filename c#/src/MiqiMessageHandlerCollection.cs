using System;
using System.Collections;
using System.Collections.Generic;

namespace Miqi.Net
{
	public class MiqiMessageHandlerCollection
	{
		public delegate void MiqiMessageHandler(WebSocketClient client, MiqiMessage message);
		
		private Dictionary<string, MiqiMessageHandler> messageHandlers;
		
		public MiqiMessageHandlerCollection()
		{
			this.messageHandlers = new Dictionary<string, MiqiMessageHandler>();
		}
		
		public void AddHandler(string command, MiqiMessageHandler handler)
		{
			if ((command != null && command.Length > 0) && handler != null)
				messageHandlers[command] = handler;
		}
		
		public void HandleMessage(WebSocketClient client, MiqiMessage message) 
		{
			MiqiMessageHandler msgDelegate = messageHandlers[message.Command];
			msgDelegate(client, message);
		}
	}
}
