using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

public class BigDataExchange<T>
{
    public static void SendBigMessage(T data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        NetworkStream stream = null;
        if (WifiServer_Host._opponent != null)
            stream = WifiServer_Host._opponent.Client.GetStream();
        else return;

        using (var ms = new MemoryStream())
        {
            formatter.Serialize(ms, data);
            byte[] bytedMaze = ms.ToArray();
            byte[] sizeInByte = BitConverter.GetBytes(bytedMaze.Length);
            stream.Write(sizeInByte, 0, sizeInByte.Length);
            stream.Write(bytedMaze, 0, bytedMaze.Length);
        }
    }

    public static T GetBigMessage()
    {
        var formatter = new BinaryFormatter();
        using (var ms = new MemoryStream(DataHolder.BigArray.ToArray()))
        {
            return (T)formatter.Deserialize(ms);
        }
    }
}
