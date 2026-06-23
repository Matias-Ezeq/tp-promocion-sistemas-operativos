using System.Dynamic;
using System.Net.Sockets;

class Client
{
    static void Main()
    {
        Globals.loggedIn = false;
        //constantes de servidor, proximamente se pedirá al inicio de la ejecución
        const string serverIp = "127.0.0.1";
        const int serverPort = 5000;
        
        //inicialización de conección TCP y stream de datos
        using var client = new TcpClient(serverIp, serverPort);
        using NetworkStream stream = client.GetStream();

        //definición de lector y escritor del stream
        Globals.Reader = new(stream);
        Globals.Writer = new(stream) { AutoFlush = true };
        string message;

        //inicialización del proceso responsable de recibir los mensajes del server
        Thread hiloReader = new(() => serverResponse()){IsBackground = true};
        hiloReader.Start();

        //función auxiliar para definir el nick del usuario
        setNickname();

        //pausa de ejecución hasta que se inicie sesión en el server
        System.Threading.SpinWait.SpinUntil( () => Globals.loggedIn );

        //funcion de envío de mensaje hacia el servidor
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
        //recibe el nick por parte del usuario y lo envía al servidor, el cual se encarga de validarlo
        Console.Write("Ingrese su nickname: ");
        var nick = Console.ReadLine().Trim();
        Globals.Writer.WriteLine($"HELLO:{nick}");
    }

    private static void serverResponse()
    {
        //recibe el mensaje y lo envía a una funcion auxiliar que parsea la respuesta del servidor
        while(true){
            var response = Globals.Reader.ReadLine();

            responseHandler(response);
        }
    }

    private static void responseHandler(string response)
    {
        //separa la respuesta en un array para obtener su tipo y contenido(s)
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
        
        //cambia el formato del mensaje recibido en base al tipo
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

//variables globales
public static class Globals
{
    //flag de inicio de sesión
    public static bool loggedIn {get;set;}

    //lector y escritor de stream
    public static StreamReader Reader {get;set;}
    public static StreamWriter Writer {get;set;}
}
