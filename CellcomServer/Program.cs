namespace CellcomServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServerManager server = new ServerManager();
            server.Start();
        }
    }
}