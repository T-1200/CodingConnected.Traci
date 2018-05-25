using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodingConnected.TraCI.NET.Helpers;
using CodingConnected.TraCI.NET.Types;

namespace CodingConnected.TraCI.NET.Commands
{
	public class ControlCommands : TraCICommandsBase
	{
		#region Public Methods

		/// <summary>
		/// Gets an identifying version number as described here: http://sumo.dlr.de/wiki/TraCI/Control-related_commands
		/// </summary>
		public int GetVersionId()
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_GETVERSION, 
				Contents = null
			};
			var response = Client.SendMessage(command);
			if (response?.Length == 2)
			{
				return BitConverter.ToInt32(response[1].Response.Take(4).Reverse().ToArray(), 0);
			}
			return -1;
		}

		/// <summary>
		/// Gets a user friendly string describing the version of SUMO
		/// </summary>
		public string GetVersionString()
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_GETVERSION,
				Contents = null
			};
			var response = Client.SendMessage(command);
			if (response?.Length == 2)
			{
				var strlen = response[1].Response.Skip(4).Take(4).Reverse().ToArray();
				var idl = BitConverter.ToInt32(strlen, 0);
				var ver = Encoding.ASCII.GetString(response[1].Response, 8, idl);
				return ver;
			}
			return null;
		}

		/// <summary>
		/// Instruct SUMO to execute a single simulation step
		/// Note: the size of the step is set via the relevant .sumcfg file
		/// </summary>
		/// <param name="targetTime">If this is not 0, SUMO will run until target time is reached</param>
		public void SimStep(int targetTime = 0)
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_SIMSTEP,
				Contents = TraCIDataConverter.GetTraCIBytesFromInt32(targetTime)
			};

			// ReSharper disable once UnusedVariable
			var response = Client.SendMessage(command);  // returns TraCIResult[]

			UnityEngine.Debug.Log("SimStep():\nEND of function; got response; length: " + response.Length);
		}

		// public T returnSimStep<T>(byte[] subsarray, int targetTime = 0)
		public List<double> returnSimStep(byte[] subsarray, int targetTime = 0)
		{	
			//TODO: return value has to be generalized

			UnityEngine.Debug.Log("returnSimStep():\ninside function!");

			// for correct parsing:
			byte identimus = TraCIConstants.CMD_SUBSCRIBE_VEHICLE_VARIABLE;

			// byte msgtypus = TraCIConstants.TYPE_COMPOUND;
			// wird mit der Funktion uebergeben --> subsarray

			/*
			- this is the usual command for SimStep-request!
			- procedure according to TraCICommandHelper.ExecuteCommand()
				- create command
				- get response via SendMessage()
				- try return TraCIDataConverter.ExtractDataFromResponse(input:
					- response (TraCIResult
					- commandType (TraCICommand)	--> Identifier
					- messageType (TraCIConstant)	--> dunno, is just a constant. usual integrated in Contents i think)
			*/

			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_SIMSTEP,
				Contents = TraCIDataConverter.GetTraCIBytesFromInt32(targetTime)
			};

			var response = Client.SendMessage(command);
			// response is output of (/parsed by) HandleResponse() of format TraCIResult[]

			if (response == null) {
				UnityEngine.Debug.Log("returnSimStep():\nresponse is 'null' --> skipping ExtractDataFromResponse()...");
				return null;
				// return default(T);  // but should return null
			} else {
				UnityEngine.Debug.Log("returnSimStep():\nresponse has content to be working with.");
			}

			try
			{
				UnityEngine.Debug.Log("SimStep():\n stepping into ExtractDataFromResponse()");
				return (List<double>)TraCIDataConverter.ExtractDataFromSubResponse(response, identimus, subsarray);
				// return (List<ComposedTypeBase>)TraCIDataConverter.ExtractDataFromResponse(response, identimus, subsarray);
			}
			catch
			{
				UnityEngine.Debug.Log("SimStep():\nthrowing exception! ...");
				throw;
			}

		}

		public void voidSimStep(int targetTime = 0)
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_SIMSTEP,
				Contents = TraCIDataConverter.GetTraCIBytesFromInt32(targetTime)
			};
			Client.voidSendMessage(command);
			// Console.WriteLine("SimStep()\ndid one step further.");
		}
		
		/// <summary>
		/// Instruct SUMO to stop the simulation and close
		/// </summary>
		public void Close()
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_CLOSE,
				Contents = null
			};
			// ReSharper disable once UnusedVariable
			var response = Client.SendMessage(command);
		}

		public void voidClose()
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_CLOSE,
				Contents = null
			};
			// ReSharper disable once UnusedVariable
			// var response = Client.SendMessage(command);
			Client.voidSendMessage(command);
		}

		/// <summary>
		/// Tells TraCI to reload the simulation with the given optionsvoidSendMessage
		/// <remarks>Loading does not work when using multiple clients, currently</remarks>
		/// </summary>
		/// <param name="options">List of options to pass to SUMO</param>
		public void Load(List<string> options)
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_LOAD
			};
			var n = new List<byte>();
			n.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(options.Count));
			foreach (var opt in options)
			{
				n.AddRange(TraCIDataConverter.GetTraCIBytesFromInt32(opt.Length));
				n.AddRange(TraCIDataConverter.GetTraCIBytesFromASCIIString(opt));
			}
			command.Contents = n.ToArray();
			// ReSharper disable once UnusedVariable
			var response = Client.SendMessage(command);
		}

		/// <summary>
		/// Tells TraCI to reload the simulation with the given options
		/// <remarks>Loading does not work when using multiple clients, currently</remarks>
		/// </summary>
		/// <param name="options">List of options to pass to SUMO</param>
		public void SetOrder(int index)
		{
			var command = new TraCICommand
			{
				Identifier = TraCIConstants.CMD_GETVERSION, 
				Contents = BitConverter.GetBytes(index).Reverse().ToArray()
			};
			var response = Client.SendMessage(command);
		}

		#endregion // Public Methods

		#region Constructor

		public ControlCommands(TraCIClient client) : base(client)
		{
		}

		#endregion // Constructor
	}
}