using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace AndroidTriviaGame;

public enum PacketType
{
    LoginCredentials,
    LoginResponse
}

public record LoginCredentials(string Name, string Password);
public record LoginResponse(string UId);
public record Packet(PacketType Type, string Data);

public class PacketReceivedEventArgs : EventArgs
{
    public Packet Packet { get; }

    public PacketReceivedEventArgs(Packet packet)
    {
        Packet = packet;
    }
}

public class NetworkingAPI
{
    public static void SendPacket<T>(NetworkStream stream, PacketType type, T dataObject)
    {
        try
        {
            // 1. Serialize the payload data to a JSON string
            string jsonPayload = JsonSerializer.Serialize(dataObject);
            Packet packet = new Packet(type, jsonPayload);

            // 2. Serialize the entire Packet wrapper to raw UTF-8 bytes
            string entirePacketJson = JsonSerializer.Serialize(packet);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(entirePacketJson);

            // 3. Convert the length of the payload into a 4-byte integer header
            byte[] lengthHeader = BitConverter.GetBytes(payloadBytes.Length);

            // 4. Lock-free writing: Completely safe because this 'stream' is unique 
            // to the calling thread/socket.
            stream.Write(lengthHeader, 0, lengthHeader.Length);
            stream.Write(payloadBytes, 0, payloadBytes.Length);
            stream.Flush(); 
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error sending packet: {e.Message}");
            throw;
        }
    }
    
    private static byte[]? ReadExactBuffer(NetworkStream stream, int size)
    {
        byte[] buffer = new byte[size];
        int totalBytesRead = 0;

        while (totalBytesRead < size)
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, size - totalBytesRead);
            if (bytesRead == 0) return null; // Socket closed

            totalBytesRead += bytesRead;
        }
        return buffer;
    }
    
    public static Packet? ReceivePacket(NetworkStream stream)
    {
        try
        {
            // 1. Read the 4-byte length header
            byte[]? lengthBuffer = ReadExactBuffer(stream, 4);
            if (lengthBuffer == null)
            {
                return null; // Connection closed gracefully
            }

            int totalPayloadLength = BitConverter.ToInt32(lengthBuffer, 0);

            // 2. Read exactly the amount of bytes specified by the header
            byte[]? payloadBytes = ReadExactBuffer(stream, totalPayloadLength);
            if (payloadBytes == null) 
                throw new IOException("Connection lost mid-packet transmission.");

            // 3. Turn the bytes back into the outer Packet wrapper string
            string entirePacketJson = Encoding.UTF8.GetString(payloadBytes);
            
            // 4. Deserialize directly into your Packet record (extracts Type and Data text)
            return JsonSerializer.Deserialize<Packet>(entirePacketJson);
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error receiving packet: {e.Message}");
            return null;
        }
    }
    
}