class Program
{
    static Server server;
    static Thread listenThread;

    static void Main(string[] args)
    {
        try
        {
            server = new Server();
            listenThread = new Thread(server.Listen);
            listenThread.Start();
        }
        catch (Exception ex)
        {
            server.Disconnect();
            Console.WriteLine(ex.Message);
        }
    }
}
