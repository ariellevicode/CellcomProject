using System;
using System.IO.Ports;
using System.Threading;
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

            
            try
            {
                Console.WriteLine("commands: <ID>JOIN, <ID>NEW, <ID>STOP");

                Thread listenerThread = new Thread(() => ListenToServer(clientPort));
                listenerThread.IsBackground = true;
                listenerThread.Start();

                while (true)
                {
                    string command = Console.ReadLine();
                    clientPort.WriteLine(command);
                }
            }
            finally
            {
                
                if (clientPort != null && clientPort.IsOpen)
                {
                    clientPort.Close();
                    Console.WriteLine("\nPort safely closed. Goodbye!");
                }
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