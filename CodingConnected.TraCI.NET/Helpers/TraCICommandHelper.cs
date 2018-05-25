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

			// erstelle byteliste mit Inhalt: {}
			var bytes = new List<byte> { messageType };

			// fuege id zu byteliste hinzu:
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromASCIIString(id));

			// erstelle command:
			var command = new TraCICommand
			{
				Identifier = commandType,
				Contents = bytes.ToArray()  // --> Variable & VehicleID
			};
			return command;
		}

		internal static T ExecuteCommand<T>(TraCIClient client, string id, byte commandType, byte messageType)
		{
			var command = GetCommand(id, commandType, messageType);
			var response = client.SendMessage(command);

			try
			{
				UnityEngine.Debug.Log("CommandHelper.ExecuteCommand():\n jumping into ExtractDataFromResponse()");
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
			//TODO works so far.
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
			// bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(1));
			bytes.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(messageTypes.Length));
			bytes.AddRange(messageTypes);
			// bytes.Add(TraCIConstants.VAR_SPEED);

			// create and return command
			var command = new TraCICommand
			{
				Identifier = commandType,
				Contents = bytes.ToArray()
			};
			UnityEngine.Debug.Log("GetSubCommand():\nassembled command; returning command!");
			return command;
		}

		internal static T ExecuteSubCommand<T>(TraCIClient client, string id, byte commandType, byte[] messageTypes, int begintime, int endtime)
		{
			var command = GetSubCommand(id, commandType, messageTypes, begintime, endtime);  // TODO works!
			var response = client.SendMessage(command);
			// response is of format TraCIResult[]

			UnityEngine.Debug.Log("ExecuteSubCommand():\nresponse is: " + response);

			if (response == null) {
				UnityEngine.Debug.Log("ExecuteSubCommand():\nresponse is 'null' --> skipping ExtractDataFromResponse()...");
				// return null;
				return default(T);
			}

			try
			{
				UnityEngine.Debug.Log("CommandHelper.ExecuteSubCommand():\n jumping into ExtractDataFromResponse()");
				return (T)TraCIDataConverter.ExtractDataFromResponse(response, commandType, messageTypes[0]);
			}
			catch
			{
				// forwarding exception
				throw;
			}

			UnityEngine.Debug.Log("ExecuteSubCommand():\n EXITING FUNCTION");
		}

		internal static void voidExecuteSubCommand<T>(TraCIClient client, string id, byte commandType, byte[] messageTypes, int begintime, int endtime)
		{
			var command = GetSubCommand(id, commandType, messageTypes, begintime, endtime);
			client.voidSendMessage(command);

			UnityEngine.Debug.Log("ExecuteSubCommand():\n EXITING FUNCTION");
		}

		#endregion  // subscription-helper


	}
}
