using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

public class Serializer<T>
{
    public static void SendMessage(T data, DataHolder.ConnectType type)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        NetworkStream stream = null;
        if (WifiServer_Host.Opponent != null)
        {
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, data);
                byte[] bytedMessage = ms.ToArray();
                byte[] messageSize = BitConverter.GetBytes(bytedMessage.Length);

                if (type == DataHolder.ConnectType.TCP)
                {
                    stream = WifiServer_Host.Opponent.Client.GetStream();
                    stream.Write(messageSize, 0, messageSize.Length);
                    stream.Write(bytedMessage, 0, bytedMessage.Length);
                }
                else if (type == DataHolder.ConnectType.UDP)
                {
                    Network.ClientUDP.SendMessage(bytedMessage);
                }                
            }
        }           
        else return;       
    }

    public static T GetMessage()
    {
        var formatter = new BinaryFormatter();
        using (var ms = new MemoryStream(Network.ClientTCP.BigArray.ToArray()))
        {
            return (T)formatter.Deserialize(ms);
        }
    }

    public static T GetMessage(byte[] data)
    {
        var formatter = new BinaryFormatter();
        using (var ms = new MemoryStream(data))
        {
            return (T)formatter.Deserialize(ms);
        }
    }
}
