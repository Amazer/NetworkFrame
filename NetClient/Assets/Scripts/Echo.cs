using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;
using System.Linq;

public class Echo : MonoBehaviour
{

    const int MaxCount = 1024;
    Socket socket;
    // 接收缓存区
    ByteArray readBuffer = new ByteArray();
    string recvStr = "";
    string sendStr = "";

    Queue<ByteArray> writeQueue = new Queue<ByteArray>();

    public InputField inputField;
    public Text text;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        text.text = recvStr;
    }
    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect("127.0.0.1", 8888, ConnectCallback, socket);
    }
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket connect Success!");
            socket.BeginReceive(readBuffer.bytes, readBuffer.writeIdx, readBuffer.remain, 0, RecvCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket connect failed:" + ex.ToString());

        }

    }
    private void RecvCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        int count = socket.EndReceive(ar);
        readBuffer.writeIdx += count;
        OnReceiveData();
        if(readBuffer.remain<8)
        {
            readBuffer.MoveBytes();
            readBuffer.ReSize(readBuffer.length*2);
        }
        socket.BeginReceive(readBuffer.bytes, readBuffer.writeIdx, readBuffer.remain, 0, RecvCallback, socket);
    }
    private void OnReceiveData()
    {
        Debug.Log("ReceiveOneData 1: bufferCount:" + readBuffer.length);
        Debug.Log("ReceiveOneData 2: readBuffer:" + readBuffer.Debug());
        if (readBuffer.length <= 2)
        {
            return;
        }

        Int16 bodyLength = readBuffer.PeekInt16();
        Debug.Log("ReceiveOneData: bodyLength:" + bodyLength);

        if (readBuffer.length < 2 + bodyLength)
        {
            return;
        }
        readBuffer.readIdx += 2;
        byte[] stringByte = new byte[bodyLength];
        readBuffer.Read(stringByte, 0, bodyLength);

        string str = System.Text.Encoding.UTF8.GetString(stringByte, 0, bodyLength);
        Debug.Log("Receive sucsess!:" + str);
        Debug.Log("Receive sucsess! readbuffer:" + readBuffer.ToString());

        Debug.Log("Receive sucsess!:" + str);
        recvStr = recvStr + "\n" + System.DateTime.Now.ToShortTimeString() + "\t" + str;
        if(readBuffer.length>2)
        {
            OnReceiveData();
        }

    }

    public void Send()
    {
        sendStr = inputField.text;
        inputField.text = "";
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
        byte[] bodyLen = BitConverter.GetBytes((Int16)sendBytes.Length);
        if (!BitConverter.IsLittleEndian)
        {
            bodyLen.Reverse();
        }
        sendBytes = bodyLen.Concat(sendBytes).ToArray();
        ByteArray ba = new ByteArray(sendBytes);
        int queueCount = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            queueCount = writeQueue.Count;
        }
        if (queueCount == 1)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }

    }
    private void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        int count = socket.EndSend(ar);
        ByteArray ba = null;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }
        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();
            }
        }
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }

    }
    private void OnApplicationQuit()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }
}
