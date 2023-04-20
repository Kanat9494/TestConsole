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

    public async Task Process()
    {
        try
        {
            Stream = tcpClient.GetStream();
            string jsonMessage = await GetMessageAsync();
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
                    jsonMessage = await GetMessageAsync();
                    message = JsonConvert.DeserializeObject<Message>(jsonMessage);
                    server.BroadcastMessage(jsonMessage, this.Id);
                }
                catch
                {
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
            builder.Length = 0;
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
        }
        while (Stream.DataAvailable);

        return builder.ToString();
    }

    private async Task<string> GetMessageAsync()
    {
        //byte[] data = new byte[64];
        //StringBuilder builder = new StringBuilder();
        //int bytes = 0;
        //do
        //{
        //    builder.Length = 0;
        //    bytes = await Stream.ReadAsync(data, 0, data.Length);
        //    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
        //}
        //while (Stream.DataAvailable);
        byte[] data = new byte[1024];
        StringBuilder builder = new StringBuilder();
        int bytesRead = 0;
        do
        {
            builder.Length = 0;
            bytesRead = await Stream.ReadAsync(data, 0, data.Length);
            builder.Append(Encoding.UTF8.GetString(data, 0, bytesRead));

            if (bytesRead == data.Length)
                Array.Resize(ref data, data.Length * 2);

        } while (Stream.DataAvailable);

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
