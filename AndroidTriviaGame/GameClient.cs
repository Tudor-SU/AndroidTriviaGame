using System;
using System.Net.Sockets;
using System.Threading;

namespace AndroidTriviaGame;

public enum Platform
{
    Android, Windows
}

public class GameClient
{
    private string IpAddress {get;}
    private int Port {get;}

    public string Username = "";
    
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private Platform _platform;
    
    public event EventHandler<PacketReceivedEventArgs>? PacketReceived;
    public GameClient(string ipAddress, int port, Platform platform)
    {
        this.IpAddress = ipAddress;
        this.Port = port;
        this._platform = platform;
    }

    public void Log(string message)
    {
        if (_platform == Platform.Windows)
        {
            Console.WriteLine($"[LOG] {message}");
            return;
        }
        if (_platform == Platform.Android)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG] {message}");
            return;
        }
    }
    
    public NetworkStream? GetNetworkStream()
    {
        return _stream;
    }

    public bool IsConnected()
    {
        return _tcpClient?.Connected ?? false;
    }
    
    public void ThreadedReceivePackets()
    {
        if (_stream == null) return;

        while (_tcpClient?.Connected ?? false)
        {
            Packet? packet = NetworkingAPI.ReceivePacket(_stream);
            if (packet == null)
            {
                Log("Server conn closed!");
                _stream?.Close();
                _stream?.Dispose();
                break;
            }
            Log($"Packet received: {packet.Type.ToString()}");
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet));
        }
       
    }

    public void ConnectToServer()
    {
        try
        {
            _tcpClient = new TcpClient(IpAddress, Port);
            _stream = _tcpClient.GetStream();
            
            Log("Connected to " + IpAddress + ":" + Port);
            
            Thread  recievePacketsThread = new Thread(ThreadedReceivePackets);
            recievePacketsThread.IsBackground = true;
            recievePacketsThread.Start();
            
        }
        catch (Exception e)
        {
            Log(e.Message);
        }
    }
    
}