using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;//Encoding
using System;//Bitconverter
/*
    * Buffersize = 5
    * Byte 0 = Code
    * 
    * Byte 1 - 4 = Data (buf size & frame)
    * Received Codes:
    * 1 -> Client: Resize buffer
    * 2 -> Client: Received link, Server: Received link confirmation
    * 3 -> Server: Client loaded video
    * 4 -> Pause
    * 5 -> Resume
    * 6 -> Move to frame, ping
    * 7 -> Server: Client asked for link
    * 8 -> Checking offset
    */

public class Interpreter : MonoBehaviour
{
    public static Interpreter Instance;
    public enum DataKeys
    {
        LinkRequest, //Bool
        LinkString, //String
        Pause, //int
        Resume, //int
        MoveToFrame, //int
        OffsetPing //int
    }

    private static List<byte[]> _ReceivedData = new List<byte[]>();
    private static List<bool> _NewMessages = new List<bool>();

    public static void Send(DataKeys Key, int _val)
    {
        if(Instance!=null)
        {
            byte[] payload = new byte[12];
            
            byte[] code = new byte[4];
            code = BitConverter.GetBytes((uint)Key); //Always 4 byte size
            Array.Copy(code, payload, 4);
            code = BitConverter.GetBytes(_val); //Always 4 byte size
            Array.Copy(code, 0, payload, 4, 4);

            Connection.SendMessage(payload);
        }
    }
    public static void Send(DataKeys Key, float _val)
    {
        if (Instance != null)
        {
            byte[] payload = new byte[12];

            byte[] code = new byte[4];
            code = BitConverter.GetBytes((uint)Key); //Always 4 byte size
            Array.Copy(code, payload, 4);
            code = BitConverter.GetBytes(_val); //Always 4 byte size
            Array.Copy(code, 0, payload, 4, 4);

            Connection.SendMessage(payload);
        }
    }
    public static void Send(DataKeys Key, bool _val)
    {
        if (Instance != null)
        {
            byte[] payload = new byte[12];

            byte[] code = new byte[4];
            code = BitConverter.GetBytes((uint)Key); //Always 4 byte size
            Array.Copy(code, payload, 4);
            code = BitConverter.GetBytes(_val); //Always 1 byte size
            Array.Copy(code, 0, payload, 4, 1);

            Connection.SendMessage(payload);
        }
    }
    public static void Send(DataKeys Key, string _val)
    {
        if (Instance != null)
        {
            byte[] payload = new byte[4+_val.Length];

            byte[] code = new byte[4];
            code = BitConverter.GetBytes((uint)Key); //Always 4 byte size
            Array.Copy(code, payload, 4);

            Array.Copy(Encoding.ASCII.GetBytes(_val), 0, payload, 4, _val.Length);
            Connection.SendMessage(payload);
        }
    }

    public static int ReceiveInt(DataKeys Key)
    {
        if (_ReceivedData.Count > (int)Key && _ReceivedData[(int)Key] != null)
        {
            _NewMessages[(int)Key] = false;
            return BitConverter.ToInt32(_ReceivedData[(int)Key],0);
        }
        return 0;
    }
    public static float ReceiveFloat(DataKeys Key)
    {
        if (_ReceivedData.Count > (int)Key && _ReceivedData[(int)Key] != null)
        {
            _NewMessages[(int)Key] = false;
            return BitConverter.ToSingle(_ReceivedData[(int)Key], 0);
        }
        return float.NaN;
    }
    public static bool ReceiveBool(DataKeys Key)
    {
        if (_ReceivedData.Count > (int)Key && _ReceivedData[(int)Key] != null)
        {
            _NewMessages[(int)Key] = false;
            return BitConverter.ToBoolean(_ReceivedData[(int)Key], 0);
        }
        return false;
    }
    public static string ReceiveString(DataKeys Key)
    {
        if (_ReceivedData.Count > (int)Key && _ReceivedData[(int)Key] != null)
        {
            _NewMessages[(int)Key] = false;
            return Encoding.ASCII.GetString(_ReceivedData[(int)Key], 0, _ReceivedData[(int)Key].Length);
        }
        return "";
    }
    /// <summary> Returns true whether the value was changed </summary>
    public static bool IsNewData(DataKeys Key)
    {
        if (_NewMessages.Count > (int)Key)
            return _NewMessages[(int)Key];
        return false;
    }

    private void _CheckForNewMessages()
    {
        List<byte[]> Messages = Connection.ReceiveMessages();
        if(Messages!=null)
            foreach(byte[] mess in Messages)
            {
                _Save_Message(mess);
            }
    }
    private void _Save_Message(byte[] mess)
    {
        if(mess.Length>4)
        {
            byte[] code = new byte[4];
            Array.Copy(mess, 0, code, 0, 4);
            int codeNumb = BitConverter.ToInt32(code, 0);

            byte[] newData = new byte[mess.Length-4];
            Array.Copy(mess, 4, newData, 0, mess.Length - 4);

            _ReceivedData[codeNumb] = newData;
            _NewMessages[codeNumb] = true;
        }
    }
    private void FixedUpdate()
    {
        _CheckForNewMessages();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);

        //Assigning receiving data buffer
        _ReceivedData = new List<byte[]>();
        for (int i=0;i< Enum.GetNames(typeof(DataKeys)).Length;i++)
        {
            _ReceivedData.Add(null);
            _NewMessages.Add(false);
        }
    }
}
