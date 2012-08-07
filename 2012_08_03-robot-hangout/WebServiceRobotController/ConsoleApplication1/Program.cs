using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// so basic!
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using iRobot;

namespace ConsoleApplication1
{
    
    class Program
    {        

        static void Main(string[] args)
        {            
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine ("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            welcomeMessage();
            TheServer theServer = new TheServer();
            theServer.startServer();

        }
        

        static void welcomeMessage()
        {
            System.Console.WriteLine("Waking up the robot and setting up the web server...");

            //get ip address
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < localIPs.Length; i++)
            {
                if (localIPs[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.Out.WriteLine("Local IP: " + localIPs[i]);                    
                    break;
                }
            }

            // TODO: display power status
            // Console.Out.WriteLine();
        }

        class TheServer
        {
            iRobotCreate robot;
            HttpListener listener;
            string sensorState;
            public TheServer()
            {
                setupRobot();
                connectRobot("COM3");
            }

            public void startServer()
            {
                UPnP.OpenFirewallPort("iRobot", UPnP.Protocol.TCP, 8080);

                string[] prefixes = { "http://+:8080/" };
                // URI prefixes are required,
                // for example "http://contoso.com:8080/index/".
                if (prefixes == null || prefixes.Length == 0)
                    throw new ArgumentException("prefixes");

                // Create a listener.
                listener = new HttpListener();

                // Add the prefixes.
                foreach (string s in prefixes)
                {
                    listener.Prefixes.Add(s);
                }
                listener.Start();
                bool keepListening = true;
                while (keepListening)
                {
                    keepListening = handleRequest(listener);
                    // TODO: display power status
                    // Console.Out.WriteLine();
                }
                listener.Stop();
            }

            private void OnWheelDropChanged(object sender, EventArgs e)
            {
                if (!robot.prevSensorState.WheelDropLeft && robot.sensorState.WheelDropLeft)
                    Console.WriteLine("wheel left dropped!");
                if (!robot.prevSensorState.WheelDropRight && robot.sensorState.WheelDropRight)
                    Console.WriteLine("wheel right dropped!");
                if (!robot.prevSensorState.WheelDropCaster && robot.sensorState.WheelDropCaster)
                    Console.WriteLine("wheel caster dropped!");
            }

            private void OnCliffDetectChanged(object sender, EventArgs e)
            {
                if (!robot.prevSensorState.CliffLeft && robot.sensorState.CliffLeft)
                    Console.WriteLine("cliff left!");
                if (!robot.prevSensorState.CliffFrontLeft && robot.sensorState.CliffFrontLeft)
                    Console.WriteLine("cliff front left!");
                if (!robot.prevSensorState.CliffFrontRight && robot.sensorState.CliffFrontRight)
                    Console.WriteLine("cliff front right!");
                if (!robot.prevSensorState.CliffRight && robot.sensorState.CliffRight)
                    Console.WriteLine("cliff right!");
            }

            private void OnBumperChanged(object sender, EventArgs e)
            {
                if (!robot.prevSensorState.BumpLeft && robot.sensorState.BumpLeft)
                    Console.WriteLine("bump left!");
                if (!robot.prevSensorState.BumpRight && robot.sensorState.BumpRight)
                    Console.WriteLine("bump right!");

            }

            private void OnSensorUpdate(object sender, EventArgs e)
            {
                string sensortemp = "";
                try
                {

                    sensortemp += "<hr>Bumpers: " + (robot.sensorState.BumpLeft ? 0 : 1) + " " + (robot.sensorState.BumpRight ? 0 : 1);
                    sensortemp += "<hr>WheelDrop: " + (robot.sensorState.WheelDropLeft ? 0 : 1) + " " + (robot.sensorState.WheelDropCaster ? 0 : 1) + " " + (robot.sensorState.WheelDropRight ? 0 : 1);
                    sensortemp += "<hr>Cliff: " + (robot.sensorState.CliffLeft ? 0 : 1) + " " + (robot.sensorState.CliffFrontLeft ? 0 : 1) + " " + (robot.sensorState.CliffFrontRight ? 0 : 1) + " " + (robot.sensorState.CliffRight ? 0 : 1);
                    string state = "Idle";
                    if (robot.sensorState.ChargingState == 1)
                        state = "Reconditioning";
                    if (robot.sensorState.ChargingState == 2)
                        state = "Charging";
                    if (robot.sensorState.ChargingState == 3)
                        state = "Trickle";
                    if (robot.sensorState.ChargingState == 4)
                        state = "Waiting";
                    if (robot.sensorState.ChargingState == 5)
                        state = "Fault";

                    int batteryCharge = (robot.sensorState.BatteryCharge * 100 / robot.sensorState.BatteryCapacity);

                    sensortemp += "<hr>iRobot Battery: " + state + " " + batteryCharge + "% (" + ((robot.sensorState.Current > 0) ? "+" : "") + robot.sensorState.Current / 1000.0 + "A " + robot.sensorState.Voltage / 1000.0 + "V " + robot.sensorState.BatteryTempurature + "C)";
                }
                catch (Exception x)
                {
                    Console.Out.WriteLine(x);
                }
                sensorState = sensortemp;
            }

            public void setupRobot()
            {
                robot = new iRobotCreate();
                robot.OnSensorUpdateRecieved += new iRobotCreate.SensorUpdateHandler(OnSensorUpdate);
                robot.OnBumperChanged += new iRobotCreate.BumperChangedHandler(OnBumperChanged);
                robot.OnCliffDetectChanged += new iRobotCreate.CliffDetectChangedHandler(OnCliffDetectChanged);
                robot.OnWheelDropChanged += new iRobotCreate.WheelDropChangedHandler(OnWheelDropChanged);
            }

            public bool connectRobot(string thePort)
            {                
                if (!robot.connected)
                {
                    if (robot.Connect(thePort))
                    {
                        Console.Out.WriteLine("Connected to iRobot Create");
                    }
                    else
                    {
                        Console.Out.WriteLine("Could not Connect to iRobot Create on port:" + thePort);

                        Console.Out.WriteLine("Please enter another port: ");

                        thePort = Console.ReadLine();
                        connectRobot(thePort);
                    }
                }
                robot.StartInFullMode();
                robot.StartSensorStreaming();
                return robot.connected;
            }

            public bool handleRequest(HttpListener listener)
            {
                Console.WriteLine("Listening...");
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string requestedURL = request.RawUrl.ToString();

                switch (requestedURL)
                {
                    case "/forward":
                        System.Console.WriteLine("Going forward...");
                        robot.DriveDirect(100, 100);
                        Thread.Sleep(1000);
                        robot.DriveDirect(0, 0);
                        break;
                    case "/left":
                        System.Console.WriteLine("Turning left...");
                        robot.DriveDirect(50, -50);
                        Thread.Sleep(1000);
                        robot.DriveDirect(0, 0);
                        break;
                    case "/right":
                        System.Console.WriteLine("Turning right...");
                        robot.DriveDirect(-50, 50);
                        Thread.Sleep(1000);
                        robot.DriveDirect(0, 0);
                        break;
                    case "/back":
                        System.Console.WriteLine("Backing up...");
                        robot.DriveDirect(-100, -100);
                        Thread.Sleep(1000);
                        robot.DriveDirect(0, 0);
                        break;
                    default:
                        System.Console.WriteLine("Unrecognized command: " + requestedURL);
                        break;
                }

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                string responseString = "<HTML><BODY> ROBOT CONTROL INTERFACE: <br>" + sensorState + "</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();

                return true;
            }
        }
    }
}
