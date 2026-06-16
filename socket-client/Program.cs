using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        const string serverIp = "127.0.0.1";
        const int serverPort = 5000;
        
        using var client = new TcpClient(serverIp, serverPort);
        using NetworkStream stream = client.GetStream();
        StreamReader reader = new(stream);
        StreamWriter writer = new(stream) { AutoFlush = true };

        Console.Write("Ingrese su nickname:");
        string nickname = Console.ReadLine();

        while (nickname == null)
        {
            Console.Write("Error, usuario inválido.\n Ingrese su nickname:");
            nickname = Console.ReadLine();
        }

        writer.WriteLine("HELLO:" + nickname);
        Thread hiloReader = new(() => serverResponse(reader)){IsBackground = true};
        hiloReader.Start();

        while(true){
            Console.Write($"[{nickname}] > ");
            var message = Console.ReadLine();
            if (message == "exit") break;
            writer.WriteLine("MSG:" + message);
        }
    }

    private static void serverResponse(StreamReader reader)
    {
        while(true){
            var response = reader.ReadLine();
            Console.WriteLine(response);
        }
    }

}
