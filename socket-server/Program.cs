class Server
{
    static ConcurrentDictionary<string, TcpClient> clientes =
        new ConcurrentDictionary<string, TcpClient>();

    static void Main()
    {
        TcpListener servidor = new TcpListener(IPAddress.Any, 5000);
        servidor.Start();

        Console.WriteLine("Servidor iniciado. Esperando conexiones...");

        while (true)
        {
            TcpClient cliente = servidor.AcceptTcpClient();
            Console.WriteLine("Nueva conexión entrante...");

            Thread hilo = new(() => ManejarCliente(cliente)) { IsBackground = true };
            hilo.Start();
        }
    }

    static void ManejarCliente(TcpClient cliente)
    {
        Console.WriteLine("Hilo iniciado para un cliente nuevo.");
    }
}