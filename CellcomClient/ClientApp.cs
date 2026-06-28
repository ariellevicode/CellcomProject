using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace CellcomClient
{
    public class ClientApp
    {
        private SerialPort clientPort;

        public void Start()
        {
            Console.WriteLine("--- Cellcom Client ---");

            while (true)
            {
                Console.Write("enter port to connect (example: COM20): ");
                string portName = Console.ReadLine();

                clientPort = new SerialPort(portName, 9600);
                clientPort.NewLine = "\r";

                try
                {
                    clientPort.Open();
                    Console.WriteLine("\n Successfully connected to " + portName);
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine(" port " + portName + " is currently occupied. Please try another.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection error: " + ex.Message + "\n");
                }
            }

            Console.WriteLine("commands: <ID>JOIN, <ID>NEW, <ID>STOP");

            Task.Run(() => ListenToServer(clientPort));

            while (true)
            {
                string command = Console.ReadLine();
                clientPort.WriteLine(command);
            }
        }

        private void ListenToServer(SerialPort port)
        {
            while (port.IsOpen)
            {
                try
                {
                    string incomingMessage = port.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Server]: " + incomingMessage);
                    Console.ResetColor();
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }
}