using System.Text;
using System.Threading.Channels;

namespace TestConsole;

public class Client
{
    public Client(TcpClient tcpClient, Server server)
    {
        Id = Guid.NewGuid().ToString();
        this.tcpClient = tcpClient;
        this.server = server;
        server.AddConnection(this);
    }
    protected internal string Id { get; private set; }
    protected internal NetworkStream Stream { get; private set; }
    string userName;
    protected internal string UserName { get; set; }
    protected internal string ReceiverName { get; set; }
    TcpClient tcpClient;
    Server server;

    public void Process()
    {
        try
        {
            Stream = tcpClient.GetStream();
            string jsonMessage = GetMessage();
            var message = JsonConvert.DeserializeObject<Message>(jsonMessage);
            userName = message.SenderName;
            UserName = message.SenderName;
            ReceiverName = message.ReceiverName;

            message = new Message()
            {
                SenderName = userName,
                Content = userName + " вошел в чат",
                ReceiverName = ReceiverName
            };
            var sendMessage = JsonConvert.SerializeObject(message);
            server.BroadcastMessage(sendMessage, this.Id);
            Console.WriteLine(message.Content + "\nА получатель должен быть " + ReceiverName);

            while (true)
            {
                try
                {
                    jsonMessage = GetMessage();
                    message = JsonConvert.DeserializeObject<Message>(jsonMessage);
                    server.BroadcastMessage(jsonMessage, this.Id);
                }
                catch
                {
                    //message = String.Format($"{userName}: покинул чат");
                    //Console.WriteLine(message);
                    //server.BroadcastMessage(message, this.Id);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            server.RemoveConnection(this.Id);
            Close();
        }
    }

    private string GetMessage()
    {
        byte[] data = new byte[64];
        StringBuilder builder = new StringBuilder();
        int bytes = 0;
        do
        {
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (Stream.DataAvailable);

        return builder.ToString();
    }

    protected internal void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (tcpClient != null)
            tcpClient.Close();
    }
}
