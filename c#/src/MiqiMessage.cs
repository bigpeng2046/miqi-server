using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Miqi.Net
{
	public class MiqiMessage
	{
		public static readonly string GET_SERVER_INFO = "GET-SERVER-INFO";
		public static readonly string GET_SERVER_INFO_RESP = "GET-SERVER-INFO-RESP";
		public static readonly string SET_CREDENTIAL = "SET-CREDENTIAL";

		// public static readonly string LOGIN_2D_BARCODE = "LOGIN-2D-BARCODE";
		// public static readonly string LOGON_2D_BARCODE = "LOGON-2D-BARCODE";
		
		public static readonly string DEFAULT_MIQI_PROTOCOL = "MIQI/1.0";
		
		private string command;
		private string protocol;
		
		private Dictionary<string, string> msgHeaders;
		
		private MiqiMessage(string command, string protocol)
		{
			this.command = command;
			this.protocol = protocol;
			this.msgHeaders = new Dictionary<string, string>();			
		}
		
		public string Command {	get { return this.command; } }
		public string Protocol { get { return this.protocol; } }
		
		public string GetHeader(string key)
		{
			try {
				return msgHeaders[key];
			}
			catch (Exception)
			{
				return "";
			}
		}
		
		public void AddHeader(string key, string value)
		{
			if ((key != null && key.Length > 0) && (value != null))
				msgHeaders[key] = value;
		}
		
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append(String.Format("{0} {1}\r\n", this.command, this.protocol));
			foreach (string key in msgHeaders.Keys) {
				builder.Append(String.Format("{0}:{1}\r\n", key, msgHeaders[key]));
			}
			
			return builder.ToString();
		}
		
		public static MiqiMessage BuildGetSeverInfoResponse(string hosts, int port, string clientId) {
			MiqiMessage miqiMsg = new MiqiMessage(MiqiMessage.GET_SERVER_INFO_RESP,
									MiqiMessage.DEFAULT_MIQI_PROTOCOL);
									
			miqiMsg.AddHeader("Hosts", hosts);
			miqiMsg.AddHeader("Port", "" + port);
			miqiMsg.AddHeader("ClientId", clientId);
			
			return miqiMsg;
		}
		
		public static MiqiMessage BuildFromString(string message)
		{
			string command;
			string protocol = "";
			
			string[] lines = message.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			if (lines.Length <= 0)
				throw new ArgumentException("Invalid Miqi message");
			
			string[] cmds = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (cmds.Length <= 0)
				throw new ArgumentException("No message command");
			
			command = cmds[0].Trim().ToUpper();
			if (command.Length == 0)
				throw new ArgumentException("No message command");
		
			if (cmds.Length > 1)
				protocol = cmds[1].Trim().ToUpper();
			
			if (protocol.Length == 0)
				protocol = MiqiMessage.DEFAULT_MIQI_PROTOCOL;
			
			MiqiMessage miqiMsg = new MiqiMessage(command, protocol);
			
			for (int i = 1; i < lines.Length; i++)
			{
				int index = lines[i].IndexOf(':');
				if (index > 0) {
					string left = lines[i].Substring(0, index).Trim();
					string right = lines[i].Substring(index + 1).Trim();
					miqiMsg.AddHeader(left, right);
				}
			}
			
			return miqiMsg;
		}
	}
}
