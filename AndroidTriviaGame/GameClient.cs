using System;
using System.Net.Sockets;
using System.Threading;

namespace AndroidTriviaGame;

public class GameClient
{
    private string IpAddress {get;}
    private int Port {get;}

    public string Username = "";
    public string UId = "";
    
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    
    public event EventHandler<PacketReceivedEventArgs>? PacketReceived;
    public GameClient(string ipAddress, int port)
    {
        this.IpAddress = ipAddress;
        this.Port = port;
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
                Console.WriteLine("Server conn closed!");
                _stream?.Close();
                _stream?.Dispose();
                break;
            }
            Console.WriteLine($"Packet received: {packet.Type.ToString()}");
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet));
        }
       
    }

    public void ConnectToServer()
    {
        try
        {
            _tcpClient = new TcpClient(IpAddress, Port);
            _stream = _tcpClient.GetStream();
            
            Console.WriteLine("Connected to " + IpAddress + ":" + Port);
            
            Thread  recievePacketsThread = new Thread(ThreadedReceivePackets);
            recievePacketsThread.IsBackground = true;
            recievePacketsThread.Start();
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        // finally
        // {
        //     _tcpClient?.Close();
        // }
    }
    
}