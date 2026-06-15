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
        NetworkStream stream = cliente.GetStream();
        StreamReader reader = new(stream);
        StreamWriter writer = new(stream) { AutoFlush = true };

        string nombreUsuario = "";

        try
        {
            bool registrado = false;

            while (!registrado)
            {
                string mensaje = reader.ReadLine();

                if (mensaje == null) return;

                if (mensaje.StartsWith("HELLO:")) // El cliente DEBE mandar HELLO:NombreUsuario como primer mensaje. Mientras no lo haga, el servidor no lo registra y no puede participar del chat.
                {
                    string nombre = mensaje.Substring(6).Trim();

                    if (string.IsNullOrEmpty(nombre))
                    {
                        writer.WriteLine("ERROR:El nombre no puede estar vacío");
                    }
                    else if (clientes.ContainsKey(nombre))
                    {
                        writer.WriteLine("ERROR:Nombre ya en uso, elegí otro");
                    }
                    else
                    {
                        clientes[nombre] = cliente;
                        nombreUsuario = nombre;
                        registrado = true;

                        writer.WriteLine($"OK:Bienvenido al chat, {nombre}!");
                        Broadcast($"JOIN:{nombre}", nombreUsuario);

                        Console.WriteLine($"[+] {nombre} se conectó. Usuarios: {clientes.Count}");
                    }
                }
                else
                {
                    writer.WriteLine("ERROR:Debés identificarte primero con HELLO:TuNombre");
                }
            }

            string linea;
            while ((linea = reader.ReadLine()) != null)
            {
                if (linea.StartsWith("MSG:"))
                {
                    string texto = linea.Substring(4);

                    Console.WriteLine($"[{nombreUsuario}]: {texto}");

                    Broadcast($"MSG:{nombreUsuario}:{texto}", nombreUsuario); //permite que el cliente sepa quién envió cada mensaje
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error con {nombreUsuario}: {ex.Message}");
        }

        finally // Esto garantiza que ningún cliente quede "fantasma" en el diccionario después de desconectarse.
        {
            if (!string.IsNullOrEmpty(nombreUsuario))
            {
                // Sacar al cliente del diccionario
                clientes.TryRemove(nombreUsuario, out _);

                // Avisar a todos que se fue
                Broadcast($"LEAVE:{nombreUsuario}", nombreUsuario);

                Console.WriteLine($"[-] {nombreUsuario} se desconectó. Usuarios: {clientes.Count}");
            }

            // Cerrar la conexión TCP limpiamente
            cliente.Close();
        }
    }
    
    static void Broadcast(string mensaje, string exceptoUsuario) // BROADCAST: envía un mensaje a todos menos al emisor
    {
        foreach (var par in clientes)
        {
            if (par.Key == exceptoUsuario) continue;

            try
            {
                NetworkStream stream = par.Value.GetStream();
                StreamWriter writer = new(stream) { AutoFlush = true };
                writer.WriteLine(mensaje);
            }
            catch (Exception)
            {
                // Cliente caído, se ignora
            }
        }
    }
}