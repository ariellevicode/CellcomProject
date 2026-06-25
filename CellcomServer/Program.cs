using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace CellcomServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("initializing server on COM10-COM19");//com0com פתחתי והשתמשתי בפורטים האלו ב
            
            List<SerialPort> activePorts = new List<SerialPort>();

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

        public static void Listen(SerialPort port)
        {
            CancellationTokenSource currentCallToken = null;

            while (port.IsOpen)
            {
                try
                {
                    string incomingMessage = port.ReadLine();
                    Console.WriteLine("[received on " + port.PortName + "]: " + incomingMessage);

                    string[] parts = incomingMessage.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        string clientId = parts[0];
                        string command = parts[1];

                        if (command == "JOIN")
                        {
                            for (int i = 1; i <= 10; i++)
                            {
                                Console.WriteLine(i);
                            }
                            port.WriteLine("<" + clientId + ">DONE");
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
                                    port.WriteLine("<" + clientId + ">CELLCOM");
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
                                port.WriteLine("<" + clientId + ">BYE");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }
}