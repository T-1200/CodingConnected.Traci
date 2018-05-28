using System;
using System.Collections.Generic;

namespace CodingConnected.TraCI.NET.Helpers
{
	internal static class TraCICommandHelper
	{
		internal static TraCICommand GetCommand(string id, byte commandType, byte messageType)
		{
			/*
			Vehicle command 0xa"X" (commandType) is of following format:
			Variable (ubyte)	--> messageType
			VehicleID (string)	--> id
			*/

			var bytes = new List<byte> { messageType };
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromASCIIString(id));
			var command = new TraCICommand
			{
				Identifier = commandType,
				Contents = bytes.ToArray()
			};
			return command;
		}

		internal static T ExecuteCommand<T>(TraCIClient client, string id, byte commandType, byte messageType)
		{
			var command = GetCommand(id, commandType, messageType);
			var response = client.SendMessage(command);

			try
			{
				return (T)TraCIDataConverter.ExtractDataFromResponse(response, commandType, messageType);
			}
			catch
			{
				throw;
			}
		}


		#region subscription-helper
		internal static TraCICommand GetSubCommand(string id, byte commandType, byte[] messageTypes, int begintime, int endtime)
		{
			/*
			Subscription 0xd"X" (commandType) is of following format:
			beginTime (time)
			endTime (time)
			objectID (string)  --> id
			VariableNumber (ubyte)  --> The size of the variables list must be equal to the field "Variable Number"
			The list of variables to return (ubyte[])
			*/

			// create and fill with above values!
			var bytes = new List<byte> ();
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(begintime));
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(endtime));
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromASCIIString(id));
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(messageTypes.Length));
			bytes.AddRange(messageTypes);

			var command = new TraCICommand
			{
				Identifier = commandType,
				Contents = bytes.ToArray()
			};

			return command;
		}

		internal static T ExecuteSubCommand<T>(TraCIClient client, string id, byte commandType, byte[] messageTypes, int begintime, int endtime)
		{
			var command = GetSubCommand(id, commandType, messageTypes, begintime, endtime);
			var response = client.SendMessage(command);

			if (response == null) {
				// return null;
				return default(T);
			}

			try
			{
				return (T)TraCIDataConverter.ExtractDataFromResponse(response, commandType, messageTypes[0]);
			}
			catch
			{
				throw;
			}
		}

		internal static void voidExecuteSubCommand<T>(TraCIClient client, string id, byte commandType, byte[] messageTypes, int begintime, int endtime)
		{
			var command = GetSubCommand(id, commandType, messageTypes, begintime, endtime);
			client.voidSendMessage(command);
		}

		#endregion  // subscription-helper


	}
}
