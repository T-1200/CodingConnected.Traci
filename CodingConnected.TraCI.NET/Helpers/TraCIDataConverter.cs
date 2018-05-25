using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodingConnected.TraCI.NET.Types;

namespace CodingConnected.TraCI.NET.Helpers
{
	internal static class TraCIDataConverter
	{
		#region Static Methods

		internal static object ExtractDataFromResponse(TraCIResult[] response, byte commandType, byte messageType)
		{
			if (response?.Length > 0)
			{
				UnityEngine.Debug.Log("ExtractDataFromResponse():\nGOT INTO ExtractDataFromResponse! response has content to be working with.");

				var r1 = response.FirstOrDefault(x => x.Identifier == commandType);
				if (r1?.Response[0] == 0x00) // Success
				{
					UnityEngine.Debug.Log("ExtractDataFromResponse():\nsuccess");
					// check if first byte is as requested (it gives the type of data requested)
					var r2 = response.FirstOrDefault(x => x.Identifier == commandType + 0x10);
					if (r2?.Response[0] == messageType)
					{
						// after the type of data, there is the length of the id (a string that we will skip)
						var take = r2.Response.Skip(1).Take(4).Reverse().ToArray();
						var idl = BitConverter.ToInt32(take, 0);
						// after the string is the type of data returned
						var type = r2.Response[5 + idl];

						// now read and translate the data
						UnityEngine.Debug.Log("ExtractDataFromResponse():\nhopping into GetValueFromTypeAndArray");
						return GetValueFromTypeAndArray(type, r2.Response.Skip(6 + idl).ToArray());
					}

					throw new TraCICommandException(commandType, messageType, "No TraCI response was found in the data");
				}

				if (r1?.Response[0] == 0xFF) // Failed
				{
					UnityEngine.Debug.Log("ExtractDataFromResponse():\nfailed.");
					var take = r1.Response.Skip(1).Take(4).Reverse().ToArray();
					var dlen = BitConverter.ToInt32(take, 0);
					var sb = new StringBuilder();
					var k1 = 5;
					for (var j = 0; j < dlen; ++j)
					{
						sb.Append((char)r1.Response[k1]);
						++k1;
					}
					throw new TraCICommandException(commandType, messageType, "TraCI reports command failure: " + sb);
				}

				if (r1?.Response[0] == 0x01) // Not implemented
				{
					UnityEngine.Debug.Log("ExtractDataFromResponse():\nnot implemented...");
					var take = r1.Response.Skip(1).Take(4).Reverse().ToArray();
					var dlen = BitConverter.ToInt32(take, 0);
					var sb = new StringBuilder();
					var k1 = 5;
					for (var j = 0; j < dlen; ++j)
					{
						sb.Append((char)r1.Response[k1]);
						++k1;
					}
					throw new TraCICommandException(commandType, messageType, "TraCI reports command not implemented: " + sb);
				}
			}
			return null;
		}


		/*
		BEGIN OF INSERT!
		new ExtractDataFromResponse!
		*/

		internal static object ExtractDataFromSubResponse(TraCIResult[] response, byte commandType, byte[] messageType)
		{
			byte[] take;

			//TODO/ msgtype[] are variables!

			UnityEngine.Debug.Log("ExtractDataFromSubResponse():\nGOT INTO ExtractDataFromSubResponse! response has content to be working with.");

			if (response?.Length > 0)  // response? ermoeglicht rueckgabe von 'null'!
			{
				// get actual response-content:
				var r2 = response.FirstOrDefault(x => x.Identifier == commandType + 0x10);
				UnityEngine.Debug.Log("ExtractDataFromSubResponse():\nEXAMINE BYTE ARRAY FOR PLAUSABILITY!: " + r2.Response.ToString());

				/*
				r2.Response[0]:	ObjectID - string
				r2.Response[1]: Variable Count - ubyte - (Anzahl der zurueckgegebenen Variablen (zB:1))
				r2.Response[2]:	Variable 1 ID - uybte
				r2.Response[3]:	Variable 1 status - ubyte - ok/err
				r2.Response[4]:	Variable 1 return-type - ubyte
				r2.Response[5]: Variable 1 value - <return-type>
				r2.Response[6]:	Variable 2 ...
				*/

				/* nicht auf (einzelnes) byte anwendbar:
				byte[] tempstring = r2.Response[0].Take(4).Reverse().ToArray();
				byte[] tempstring = r2.Response[0].ToArray();
				*/

				byte[] tempstring = new byte[] {r2.Response[0]};
				byte[] vaco = new byte[] {r2.Response[1]};

				string objectID = BitConverter.ToString(tempstring.Reverse().ToArray(), 0);
				int idlength = BitConverter.ToInt32(tempstring.Reverse().ToArray(), 0);
				// int variableCount = BitConverter.ToInt32(r2.Response[1].Reverse().ToArray(), 0);
				int variableCount = BitConverter.ToInt32(vaco.Reverse().ToArray(), 0);


				// retrieving variables out of the msgtypes-array again:
				List<byte> byteslist = messageType.ToList();

				// print above info:
				UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nExtracting Data for ID: "
									+ objectID + ", is this the same(?): " + idlength
									+ ", amount of return values should be 2 is: "
									+ variableCount.ToString()
									);
				UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nlist-length: " + byteslist.Count
									+ ", first msg-type: " + byteslist[0]
									+ ", second msg-type: " + byteslist[1]
									);

				// check for compliance of subscribed msg-types!
				if (byteslist.Count != variableCount)
				{
					UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nERROR: BYTELIST-LENGTH != AMOUNT OF SUBSCRIBED VARIABLES!");
					return null;
				}
				else
				{
					UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nall good: bytelist-length == amount of subscribed variables!");
				}

				int iteriere = 0;

				// get new array from r2.Response without objectID and VarCount:s
				var responseData = r2.Response.Skip(idlength+1).ToArray();

				//TODO: generate list to write in! after foreach: return that list!
				//TODO: this has to be some kind of dynamic later on.
				List<double> liste = new List<double>();

				// iterate through msg-types to retrieve information
				foreach (var bite in byteslist)
				{
					Console.WriteLine("current msgtype-byte is: {0}", bite.ToString());

					//TODO
					//TODO
					//TODO: HIER ALLES UEBERRPRUEFEN!!
					//TODO
					//TODO

					take = new byte[] {0};  // empty filled array!

					/*
					COLLECT RELEVANT DATA
						take relevant stuff of variable n (bite)
						first skip nothing, then skip 8
						x++ gibt x aus, zaehlt dann eins weiter.
					*/
					take = responseData.Skip((iteriere) * 8).Take(8).Reverse().ToArray();
					iteriere++;
					UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nEXAMMINE THIS ARRAY: " + take.ToString() );
					// TODO: check unitl here:
					byte varz = take[0];
					UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nEXAMMINE THIS: variable is (?): " + varz.ToString() );
					// byte success = take.Skip(1).Take(1);
					byte success = take[1];
					UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nEXAMMINE THIS: success(?): " + success.ToString() );
					byte tuep = take[2];
					UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nEXAMMINE THIS: type(?): " + tuep.ToString() );


					/*
					verify success ("variable status")
					and use above collected data
					*/
					if (success == 0x00)
					{
						// verify message-type
						if (varz == bite) {
							// retriev idl
							// retriev type
							// ggfs empty take

							// after the type of data, there is the length of the id (a string that we will skip)
							// var take = responseData.Skip(1).Take(4).Reverse().ToArray();
							// var idl = BitConverter.ToInt32(take, 0);
							// after the string is the type of data returned
							// var type = r2.Response[5 + idl];

							// interpret value by type and array: return GetValueFromTypeAndArray(type, r2.Response.Skip(6 + idl).ToArray());
							//TODO: MORE LIKE APPEND TO ABOVE CREATED LIST!
							var temp = GetValueFromTypeAndArray(tuep, responseData.Skip(3).ToArray());
							// as temp should currently return doubles:
							liste.Add((double)temp);

						} else {
							UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nmsg-type couldnt be verified...");
							throw new TraCICommandException(commandType, bite, "No TraCI response was found in the data");
						}
					}
					else
					{
						UnityEngine.Debug.Log("ExtractDataFromSubResponse()\nSUBSCRIPTION NOT SUCCESSFUL!!");
						// return null;  // cant return or for-loop will be cancelled.
					}						
				}

				return liste;

				/* or:
				for (var i = 0; i < myMoney.Count; i++) {
					Console.WriteLine("Amount is {0} and type is {1}", myMoney[i].amount, myMoney[i].type);
				}
				*/
			}
			else
			{
				UnityEngine.Debug.Log("ExtractDataFromSubResponse():\nSOMETHING WENT WRONG - NO RESPONSE-CONTENT. RETURNING null");
				return null;
			}
		}

		/*
		END OF INSERT!
		*/


		internal static byte[] GetTraCIBytesFromInt32(int i)
        {
            return BitConverter.GetBytes(i).Reverse().ToArray();
        }

        internal static byte[] GetTraCIBytesFromASCIIString(string s)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(s.Length).Reverse());
            bytes.AddRange(Encoding.ASCII.GetBytes(s));
            return bytes.ToArray();
        }

        internal static object GetValueFromTypeAndArray(byte type, byte[] array)
        {
	        byte[] take;
	        int length, count;
	        var sb = new StringBuilder();
			var amount = 0;
			string _array;  // for printing

            switch (type)
            {
				/*
				following stuff new from here...
				*/

				case 0x01:
					/*
					return array is:
					42-00-00-00-01-30-01-40-97-2C-45-69-AF-F8-24-40-99-27-A5-02-E8-D6-52

					response-byte array after parsing:
										 40-97-2C-45-69-AF-F8-24-40-99-27-A5-02-E8-D6-52
					
					*/

					var _pos2d = new Position2D();

					// using 8-byte-steps
					amount = 8;

					// erstelle array "take" aus den ersten 8 byte (skip(k_new)):
					take = array.Take(amount).Reverse().ToArray();
					// parse take --> wandle in Double!
					_pos2d.X = BitConverter.ToDouble(take, 0);

					// erstelle neues "take" --> überspringe die ersten 8 byte, nehme die folgenden 8 byte.
					take = array.Skip(amount).Take(amount).Reverse().ToArray();
					// parse take --> wandle in Double!
					_pos2d.Y = BitConverter.ToDouble(take, 0);

					amount = 0;
					return _pos2d;
				
				case 0x03:
					// _array = BitConverter.ToString(array);
					// UnityEngine.Debug.Log("TraCIDataConverter():\ncase: 0x03: input response-byte array: " + _array);

					var k_new = 8;
					amount = 8;
					var _pos3d = new Position3D();

					take = array.Take(amount).Reverse().ToArray();
					_pos3d.X = BitConverter.ToDouble(take, 0);

					take = array.Skip(k_new).Take(amount).Reverse().ToArray();
					_pos3d.Y = BitConverter.ToDouble(take,0);
					k_new += 8;

					take = array.Skip(k_new).Take(amount).Reverse().ToArray();
					_pos3d.Z = BitConverter.ToDouble(take,0);

					k_new = 0;
					amount = 0;

					return _pos3d;

				/*
				...until here.
				*/

                case 0x07:
                    return array[0];
                case 0x08:
                    return BitConverter.ToChar(array, 0);
                case 0x09:
	                take = array.Take(4).Reverse().ToArray();
                    return BitConverter.ToInt32(take, 0);
                case 0x0A:
	                take = array.Take(4).Reverse().ToArray();
                    return BitConverter.ToSingle(take, 0);
                case 0x0B:
	                take = array.Take(8).Reverse().ToArray();
                    return BitConverter.ToDouble(take, 0);
				case 0x0C:
					sb = new StringBuilder();
	                take = array.Take(4).Reverse().ToArray();
					length = BitConverter.ToInt32(take, 0);
					for (var i = 0; i < length; ++i)
					{
						sb.Append((char)array[i + 4]);
					}
					return sb.ToString();
				case 0x0E:
					sb = new StringBuilder();
					take = array.Take(4).Reverse().ToArray();
					count = BitConverter.ToInt32(take, 0);
					var list = new List<string>();
					var k1 = 4;
					for (var i1 = 0; i1 < count; ++i1)
					{
						sb.Clear();
						take = array.Skip(k1).Take(4).Reverse().ToArray();
						k1 += 4;
						length = BitConverter.ToInt32(take, 0);
						for (var j = 0; j < length; ++j)
						{
							sb.Append((char)array[k1]);
							++k1;
						}
						list.Add(sb.ToString());
					}
					return list;
				case 0x0F:
					UnityEngine.Debug.Log("DataConverter():\ninside case 0x0F; trying to parse!");
					take = array.Take(4).Reverse().ToArray();
					count = BitConverter.ToInt32(take, 0);
					var ctlist = new List<ComposedTypeBase>();
					var k2 = 4;
					for (var i = 0; i < count; ++i)
					{
						var ctype = array[k2];
						++k2;
						switch (ctype)
						{
							case 0x00:
								var lonlat = new LonLatPosition();
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								lonlat.Longitude = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								lonlat.Latitude = BitConverter.ToDouble(take, 0);
								ctlist.Add(lonlat);
								break;
							case 0x01:
								var pos2d = new Position2D();
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								pos2d.X = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								pos2d.Y = BitConverter.ToDouble(take, 0);
								ctlist.Add(pos2d);
								break;
							case 0x02:
								var lonlatalt = new LonLatAltPosition();
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								lonlatalt.Longitude = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								lonlatalt.Latitude = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								lonlatalt.Altitude = BitConverter.ToDouble(take, 0);
								ctlist.Add(lonlatalt);
								break;
							case 0x03:
								var pos3d = new Position3D();
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								pos3d.X = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								pos3d.Y = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								pos3d.Z = BitConverter.ToDouble(take, 0);
								ctlist.Add(pos3d);
								break;
							case 0x04:
								var rmp = new RoadMapPosition();
								sb = new StringBuilder();
								take = array.Skip(k2).Take(4).Reverse().ToArray();
								k2 += 4;
								length = BitConverter.ToInt32(take, 0);
								for (var j = 0; j < length; ++j)
								{
									sb.Append((char)array[j + 4]);
									++k2;
								}
								rmp.RoadId = sb.ToString();
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								rmp.Pos = BitConverter.ToDouble(take, 0);
								rmp.LaneId = array[k2];
								ctlist.Add(rmp);
								break;
							case 0x05:
								var bb = new BoundaryBox();
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								bb.LowerLeftX = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								bb.LowerLeftY = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								bb.UpperRightX = BitConverter.ToDouble(take, 0);
								take = array.Skip(k2).Take(8).Reverse().ToArray();
								k2 += 8;
								bb.UpperRightY = BitConverter.ToDouble(take, 0);
								ctlist.Add(bb);
								break;
							case 0x06:
								var pol = new Polygon();
								var plen = array[k2];
								++k2;
								for (var j = 0; j < plen; j++)
								{
									var p = new Position2D();
									take = array.Skip(k2).Take(8).Reverse().ToArray();
									k2 += 8;
									p.X = BitConverter.ToDouble(take, 0);
									take = array.Skip(k2).Take(8).Reverse().ToArray();
									k2 += 8;
									p.Y = BitConverter.ToDouble(take, 0);
									pol.Points.Add(p);
								}
								ctlist.Add(pol);
								break;
							case 0x0D:
								break;
							case 0x11:
								var c = new Color();
								c.R = array[k2++];
								c.G = array[k2++];
								c.B = array[k2++];
								c.A = array[k2++];
								ctlist.Add(c);
								break;
						}
					}
					return ctlist;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static byte[] GetMessageBytes(TraCICommand command)
        {
            return GetMessagesBytes(new[] {command});
        }

        internal static byte[] GetMessagesBytes(IEnumerable<TraCICommand> commands)
        {
            var cmessages = new List<List<byte>>();
            foreach (var c in commands)
            {
				UnityEngine.Debug.Log("GetMessageBytes():\nc.Contents.Length: " + c.Contents.Length);
                var cmessage = new List<byte>();
                if (c.Contents == null)
                {
                    cmessage.Add(2); // no contents: only length self and id => 2
                }
                else if ((c.Contents.Length + 2) <= 255)
                {
                    cmessage.Add((byte)(c.Contents.Length + 2));
                }
                else
                {
                    cmessage.Add(0);
                    cmessage.AddRange(BitConverter.GetBytes(c.Contents.Length + 6).Reverse());
                }
				UnityEngine.Debug.Log("GetMessageBytes():\nadded range accordingly. adding identifier now.");
                cmessage.Add(c.Identifier);
                if (c.Contents != null)
                {
                    cmessage.AddRange(c.Contents);
					UnityEngine.Debug.Log("GetMessageBytes():\nadded Contents");
                }
                cmessages.Add(cmessage);
            }
            var totlength = cmessages.Select(x => x.Count).Sum() + 4;
            var totmessage = new List<byte>();
            totmessage.AddRange(BitConverter.GetBytes(totlength).Reverse());
            cmessages.ForEach(x => totmessage.AddRange(x));
            //totmessage.AddRange(BitConverter.GetBytes('\n'));
            return totmessage.ToArray();
        }

        internal static TraCIResult[] HandleResponse(byte[] response)
        {
			
            try
            {
				// UnityEngine.Debug.Log("HandleResponse():\ninside try --> try parsing...");

                var revLength = response.Take(4).Reverse().ToArray();
                var totlength = BitConverter.ToInt32(revLength, 0);
				// UnityEngine.Debug.Log("HandleResponse():\nrev/totLength is: " + totlength.ToString());

                var i = 4;
                var results = new List<TraCIResult>();
                while (i < totlength)
                {
                    var trresult = new TraCIResult();
                    var j = 0;
                    int len = response[i + j++];
                    trresult.Length = len - 2; // bytes lenght will be: msg - length - id
                    if (len == 0)
                    {
                        if (j + i + 3 < totlength)
                        {
                            revLength = new byte[4];
                            revLength[0] = response[i + j + 3];
                            revLength[1] = response[i + j + 2];
                            revLength[2] = response[i + j + 1];
                            revLength[3] = response[i + j + 0];
                            len = BitConverter.ToInt32(revLength, 0);
                            trresult.Length = len - 6; // bytes lenght will be: msg - length - int32len - id
                            j += 4;
                        }
                        else
                        {
                            break;
                        }
                    }
                    trresult.Identifier = response[i + j++];
                    var cmd = new List<byte>();
                    while (j < len)
                    {
                        cmd.Add(response[i + j++]);
                    }
                    trresult.Response = cmd.ToArray();
                    i += j;
                    results.Add(trresult);
                }
				
				UnityEngine.Debug.Log("HandleResponse():\n returning results.ToArray()!");
                return results.ToArray();
            }
            catch (IndexOutOfRangeException)
            {
				UnityEngine.Debug.Log("HandleResponse():\nthrowing 'Index out of range'-exception");
                return null;
            }
        }

		#endregion // Static Methods
	}
}