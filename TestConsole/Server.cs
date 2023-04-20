
using System.Text;

namespace TestConsole;

public class Server
{
    static TcpListener tcpListener;
    List<Client> clients = new List<Client>();

    protected internal void AddConnection(Client client)
        => clients.Add(client);

    protected internal void RemoveConnection(string id)
    {
        Client client = clients.FirstOrDefault(c => c.Id == id);
        if (client != null)
            clients.Remove(client);
    }

    protected internal void Listen()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 8888);
            tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                Client client = new Client(tcpClient, this);
                //Thread clientThread = new Thread(client.Process);
                //clientThread.Start();
                Task.Run(async () =>
                {
                    await client.Process();
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }

    protected internal void BroadcastMessage(string jsonMessage, string id)
    {
        var message = JsonConvert.DeserializeObject<Message>(jsonMessage);
        byte[] data = Encoding.UTF8.GetBytes(message?.Content ?? "0");
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].Id != id)
            {
                if (clients[i].UserName == message.ReceiverName)
                    clients[i].Stream.Write(data, 0, data.Length);

            }
        }
    }

    protected internal void Disconnect()
    {
        tcpListener.Stop();
        for (int i = 0; i < clients.Count; i++)
            clients[i].Close();

        Environment.Exit(0);
    }
}
