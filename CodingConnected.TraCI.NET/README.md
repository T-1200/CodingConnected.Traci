# TraCI.NET

A simple (and rather incomplete) implementation of TraCI (see http://sumo.dlr.de/wiki/TraCI) in C#. 

The library has been written specifically to connect external traffic light controller software to SUMO. Therefore, at this point, only TraCI API functions needed to achieve that goal are exposed through the TraCI.NET API. That basically means: it can run the simulation, read state of "e2" detectors, and set state of traffic lights.

###Usage

Usage of the library is very straightforward. First, create an instance of the client.


    var client = new TraCIClient();

Now, connect to SUMO.
*Note 1:* This presupposes SUMO or SUMO-GUI has been started with the "--remote-port" option as described in the TraCI documentation.
*Note 2:* This function is implemented asynchronously, to allow a GUI to remain responsive; subsequent API calls are (for now) all synchronous (they should not take a noticable amount of time).

    await client.ConnectAsync("127.0.0.1", 4001);

If using SUMO-GUI, press the play button after the network has loaded, otherwise no TraCI commands will be handled.
We can now run the simulation, for example using a Task:

    Task.Run(() =>
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            Thread.Sleep(100);
            client.ControlSimStep();
        }
    }

This assumes a step size of 0,1 second. The approach is quite rudimentary and does not take into account the call to ControlSimStep might take time.

Browse the public methods off the TraCIClient class to find out which functionality is exposed.

### Additional Usage
My goal is to use TraCI within Unity3D to manipulate vehicle models. An example of how I try to retrieve data is given in **Example** below. Subscribing basically does happen, but error-value is returned - connection closes.

###Changes

Files that inherit altered or additional content:
- TraCIClient.cs
    - added function for closing of connection; CloseConnection();
    - added function: voidSendMessage() --> as SendMessage, but no response-parsing, therefore no return.
- ControlCommands.cs
    - added function: returnSimStep() --> Should handle response of SimStep after subscriptions has been invoked. Format of return-value is currently fixed to List<double> for testing purposes - has got to be generalized after successful verification.
    - added function: voidSimStep() --> invokes voidSendMessage()
    - added function: voidClose()
- VehicleCommands.cs
    - added function: VehicleValueSubscription() --> **still to get working**
- TraCICommandHelper.cs
    - added function: GetSubCommand(); derived from GetCommand(), altered to parse additional data
    - added function: ExecuteSubCommand(); derived from ExecuteCommand(), altered to parse additional data --> **still to get working**
    - added function: voidExecuteSubCommand(); as ExecuteSubCommand(), just not trying to parse response, therefore no return.
- TraCIDataConverter.cs
    - added function: ExtractDataFromSubResponse(); derived from ExtractDataFromResponse(); new function has to iterate through defined messageType-array. --> **still to get working**
    - extended GetValueFromTypeAndArray(): added direct retrieval for 2/3D-Position by adding cases 0x01 & 0x03.

###Example
according to above Usage:

```c#
private void main()
{
    client = new TraCIClient ();
    execCommand = new ControlCommands(client);
    vehCommand = new VehicleCommands(client);
    simCommand = new SimulationCommands(client);
    source = new CancellationTokenSource ();

    vehicle_string = vehicle_ID.ToString();     // define vehicle to check information on

    // set subscription variables
    var lt = new List<byte>
    {
        // for testing-period variables have been chosen, that are doubles, so that return-type can be predefined!
        TraCIConstants.VAR_SPEED,
        TraCIConstants.VAR_POSITION
    };
    subscriptionArray = lt.ToArray();

    client.ConnectAsync (IP, port);		// establish connection to SUMO
    Thread.Sleep(step_length);			// give some time to connect

    // check connection:
    if (client._client.Connected) {
        Debug.Log("main():\nconnected to remote host!");
    }

    count = 0;  // to count simSteps
    main_task = Task.Run( () => {
        running = true;

        while (!source.IsCancellationRequested)
        {
            if (count < 1000)
            {
                workFlow();  // stuff happens here!
                Thread.Sleep(step_length);  	// --> defining time between simsteps (more or less)
                count ++;
            }
            else
            {
                running = false;
                closing();
            }
        }
    } );
}
```

```c#
private void workFlow ()
{
    // THIS IS SIMSTEP-HANDLER (-PARSER)
    if (once) {
        // do a usual simStep first:
        execCommand.SimStep();
        vehListe = vehCommand.GetIdList();
    }
    else {
        vehListe = vehCommand.GetIdList();
        if (vehListe.Contains(vehicle_string)) {
            // do a simStep with response-parsing: (!)
            try
            {
                // List<ComposedTypeBase> _list = execCommand.returnSimStep(subscriptionArray);
                List<double> _list = execCommand.returnSimStep(subscriptionArray);
            }
            catch {
            }
        }
        else
        {
            // do a usual simStep
            execCommand.SimStep();
        }
    }

    /*
    THIS IS THE SUBSCRIBER:
    */
    if (once)
    {
        // do subscription!
        var _vsub = vehCommand.VehicleValueSubscription(vehicle_string, subscriptionArray);
        // vehCommand.VehicleValueSubscription(vehicle_string, subscriptionArray);
        if (_vsub == null) {
            Debug.Log("workFlow():\nFAILED! VehicleValueSubscription returned 'null'");
        }
        else {
            Debug.Log("workFlow():\ninvoked: SUBSCRIPTION! seems succesful.");
        }

        once = false;
    }
    else {
        // Debug.Log("workFlow():\njust did simStep and tried parsing data!!");
    }

    // grab further information:
    /*
    Debug.Log("workFlow():\nfetching SimTime!");
    int time = simCommand.GetCurrentTime(vehicle_string);               // works
    Debug.Log("workFlow():\nfetching GetIdCount()!");
    int idCount = vehCommand.GetIdCount();                              // works
    double angle0 = vehCommand.GetAngle(vehicle_string);                // works
    string velocity0 = vehCommand.GetSpeed(vehicle_string).ToString();	// works
    Position2D pos2d = new Position2D();
    pos2d = vehCommand.GetPosition(vehicle_string);                     // works
    Position3D pos3d = new Position3D();
    pos3d = vehCommand.GetPosition3D(vehicle_string);                   // works
    */
}
```
