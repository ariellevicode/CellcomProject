using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace CellcomServer
{
    public class ServerManager
    {
        private List<SerialPort> activePorts;

        public ServerManager()
        {
            activePorts = new List<SerialPort>();
        }

        public void Start()
        {
            Console.WriteLine("initializing server on COM10-COM19");

            for (int i = 10; i <= 19; i++)
            {
                string portName = "COM" + i;
                try
                {
                    SerialPort port = new SerialPort(portName, 9600);
                    port.NewLine = "\r";
                    port.Open();

                    activePorts.Add(port);
                    Console.WriteLine("successfully opened " + portName + " - starting Task");

                    Task.Run(() => Listen(port));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to open " + portName + ": " + ex.Message);
                }
            }

            Console.WriteLine("\nserver is fully operational. press Enter to terminate.");
            Console.ReadLine();
        }

        private void Listen(SerialPort port)
        {
            CancellationTokenSource currentCallToken = null;

            
            object portLock = new object();

            while (port.IsOpen)
            {
                try
                {
                    string incomingMessage = port.ReadLine();
                    Console.WriteLine("[received on " + port.PortName + "]: " + incomingMessage);

                    if (TryParseMessage(incomingMessage, out string clientId, out string command))
                    {
                        
                        HandleCommand(clientId, command, port, ref currentCallToken, portLock);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }


        private bool TryParseMessage(string message, out string clientId, out string command)
        {
            clientId = string.Empty;
            command = string.Empty;

            string[] parts = message.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                clientId = parts[0];
                command = parts[1];
                return true;
            }
            return false;
        }


        
        private void HandleCommand(string clientId, string command, SerialPort port, ref CancellationTokenSource currentCallToken, object portLock)
        {
            if (command == "JOIN")
            {
                for (int i = 1; i <= 10; i++)
                {
                    Console.WriteLine(i);
                }

                
                lock (portLock)
                {
                    port.WriteLine("<" + clientId + ">DONE");
                }
            }
            else if (command == "NEW")
            {
                if (currentCallToken != null)
                {
                    currentCallToken.Cancel();
                }

                currentCallToken = new CancellationTokenSource();
                var token = currentCallToken.Token;

                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        
                        lock (portLock)
                        {
                            port.WriteLine("<" + clientId + ">CELLCOM");
                        }
                        await Task.Delay(1000);
                    }
                });
            }
            else if (command == "STOP")
            {
                if (currentCallToken != null)
                {
                    currentCallToken.Cancel();
                    currentCallToken = null;

                    
                    lock (portLock)
                    {
                        port.WriteLine("<" + clientId + ">BYE");
                    }
                }
            }
        }
    }
}