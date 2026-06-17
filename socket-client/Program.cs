using System.Net.Sockets;

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
        string message;

        Thread hiloReader = new(() => serverResponse(reader)){IsBackground = true};
        hiloReader.Start();

        while(true){
            message = Console.ReadLine().Trim();
            if (message == "EXIT") break;
            writer.WriteLine(message);
        }
    }

    private static void serverResponse(StreamReader reader)
    {
        while(true){
            var response = reader.ReadLine();

            responseHandler(response);
        }
    }

    private static void responseHandler(string response)
    {
        var received = response.Split(":");
        string type = received[0];
        string content = received[1];
        string message;

        try
        {
            message = received[2];
        }
        catch (IndexOutOfRangeException)
        {
             message = "";
        }

        switch(type) 
        {
        case "OK":
            Console.WriteLine("[OK] " + content);
            break;
        case "ERROR":
            Console.WriteLine("[ERROR] " + response);
            //ToDo: crear funcion setNickname Y volver a llamarla en case de error
            break;
        case "JOIN":
            Console.WriteLine($"[+] {content} se conectó.");
            break;
        case "MSG":
            Console.WriteLine($"[{content}]: {message}");
            break;
        case "LEAVE":
            Console.WriteLine($"[-] {content} se desconectó.");
            break;
        default:
            Console.WriteLine(response);
            break;
        }
    }

}
