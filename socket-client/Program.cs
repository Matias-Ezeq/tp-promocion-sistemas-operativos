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

        Console.Write("Ingrese su nickname:");
        string nickname = Console.ReadLine();

        while (nickname == null)
        {
            Console.Write("Error, usuario inválido.\n Ingrese su nickname:");
            nickname = Console.ReadLine().Trim();
        }

        writer.WriteLine("HELLO:" + nickname);
        Thread hiloReader = new(() => serverResponse(reader)){IsBackground = true};
        hiloReader.Start();

        while(true){
            var message = Console.ReadLine().Trim();
            if (message == "EXIT") break;
            writer.WriteLine("MSG:" + message);
        }
    }

    private static void serverResponse(StreamReader reader)
    {
        while(true){
            var response = reader.ReadLine();

            string output = responseParser(response);
            
            Console.WriteLine(output);
        }
    }

    private static string responseParser(string response)
    {
        var received = response.Split(":");
        string type = received[0];
        string nick = received[1];
        string message;

        try
        {
            message = received[2];
        }
        catch (IndexOutOfRangeException)
        {
             message = "";
        }

        string output;

        switch(type) 
        {
        case "JOIN":
            output = $"[+] {nick} se conectó";
            break;
        case "MSG":
            output = $"[{nick}]: {message}";
            break;
        case "LEAVE":
            output = $"[-] {nick} se desconectó";
            break;
        default:
            output = response;
            break;
        }

        return output;
    }

}
