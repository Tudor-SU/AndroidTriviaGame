using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AndroidTriviaGame;

public enum PacketType
{
    LoginRequest,
    RegisterRequest,
    LoginResponse,
    RegisterResponse,
    CreateLobbyRequest,
    CreateLobbyResponse,
    JoinLobbyRequest,
    JoinLobbyResponse,
    LeaveLobbyUpdate,
    PlayerLeftUpdate,
    HostLeftUpdate,
    PlayerJoinedUpdate,
    SubmitAnswerUpdate,
    StartGameRequest,
    GameStateUpdate,
    ShowStatsUpdate
}

public record Credentials(string Name, string Password);
public record ResponseStatus(bool IsSuccess, string Message);
public record Packet(PacketType Type, string Data);
public record JoinLobbyInfo(string PlayerName, string LobbyCode);
public record JoinLobbyResponse(
    ResponseStatus Status, List<string>? PlayerNames,
    string? LobbyCode
);

public record QuizQuestion(
    string QuestionText, 
    List<string> Answers, 
    int CorrectIndex
);

public record GameStateUpdate(
    Dictionary<string, int> StatusTable, 
    QuizQuestion CurrentQuestion,        
    int CurrentQuestionIndex,            
    int TotalQuestions
);

public record AnswerUpdate(string PlayerName, int AnswerIndex);

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
            string jsonPayload = JsonSerializer.Serialize(dataObject);
            Packet packet = new Packet(type, jsonPayload);

            string entirePacketJson = JsonSerializer.Serialize(packet);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(entirePacketJson);

            byte[] lengthHeader = BitConverter.GetBytes(payloadBytes.Length);
            
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
            if (bytesRead == 0) return null;

            totalBytesRead += bytesRead;
        }
        return buffer;
    }
    
    public static Packet? ReceivePacket(NetworkStream stream)
    {
        try
        {
            byte[]? lengthBuffer = ReadExactBuffer(stream, 4);
            if (lengthBuffer == null)
            {
                return null; 
            }

            int totalPayloadLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[]? payloadBytes = ReadExactBuffer(stream, totalPayloadLength);
            if (payloadBytes == null) 
                throw new IOException("Connection lost mid-packet transmission.");

            string entirePacketJson = Encoding.UTF8.GetString(payloadBytes);
            
            return JsonSerializer.Deserialize<Packet>(entirePacketJson);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
}