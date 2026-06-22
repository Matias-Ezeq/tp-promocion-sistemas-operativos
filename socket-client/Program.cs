using System.Dynamic;
using System.Net.Sockets;

class Client
{
    static void Main()
    {
        Globals.loggedIn = false;
        const string serverIp = "127.0.0.1";
        const int serverPort = 5000;
        
        using var client = new TcpClient(serverIp, serverPort);
        using NetworkStream stream = client.GetStream();
        Globals.Reader = new(stream);
        Globals.Writer = new(stream) { AutoFlush = true };
        string message;

        Thread hiloReader = new(() => serverResponse()){IsBackground = true};
        hiloReader.Start();

        setNickname();

        System.Threading.SpinWait.SpinUntil( () => Globals.loggedIn );

        while(true){
            string output;
            message = Console.ReadLine().Trim();
            
            if(message == "EXIT") {break;}
            if (message.StartsWith("/")){
                output = message.TrimStart("/").ToString();
            }
            else
            {
                output = "MSG:" + message;
            }
            Globals.Writer.WriteLine(output);
        }
    }

    private static void setNickname()
    {
        Console.Write("Ingrese su nickname: ");
        var nick = Console.ReadLine().Trim();
        Globals.Writer.WriteLine($"HELLO:{nick}");
    }

    private static void serverResponse()
    {
        while(true){
            var response = Globals.Reader.ReadLine();

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
            Globals.loggedIn = true;
            break;
        case "ERROR":
            Console.WriteLine("[ERROR] " + response);
            setNickname();
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

public static class Globals
{
    public static bool loggedIn {get;set;}
    public static StreamReader Reader {get;set;}
    public static StreamWriter Writer {get;set;}
}
