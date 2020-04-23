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
    byte[] readBuffer = new byte[MaxCount];
    int bufferCount = 0;        // 接收的数据的count
    string recvStr = "";
    string sendStr = "";

    public InputField inputField;
    public Text text;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (socket == null)
        {
            return;
        }
        if (sendStr != "")
        {
            if (socket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
                byte[] bodyLen = BitConverter.GetBytes((Int16)sendBytes.Length);
                sendBytes = bodyLen.Concat(sendBytes).ToArray();

                socket.Send(sendBytes);
                sendStr = "";
            }
        }
        if (socket.Poll(0, SelectMode.SelectRead))
        {
            Debug.Log("before Receive bufferCount:"+bufferCount);
            int count = socket.Receive(readBuffer, bufferCount, MaxCount - bufferCount, 0);

            Debug.Log("Receive count:"+count);
            bufferCount += count;
            ReceiveOneData();
        }
    }
    private void ReceiveOneData()
    {
        Debug.Log("ReceiveOneData 1: bufferCount:"+bufferCount);
        Debug.Log("ReceiveOneData 2: readBuffer:"+System.Text.Encoding.Default.GetString(readBuffer,0,bufferCount));
        if(bufferCount<=2)
        {
            return;
        }

        Int16 bodyLength = BitConverter.ToInt16(readBuffer, 0);
        Debug.Log("ReceiveOneData: bodyLength:"+bodyLength);

        if(bufferCount<2+bodyLength)
        {
            return;
        }

        string str= System.Text.Encoding.Default.GetString(readBuffer, 2, bodyLength);
        int start = 2 + bodyLength;
        int count = bufferCount - start;
        Array.Copy(readBuffer, start, readBuffer, 0,count);
        bufferCount -= start;



        Debug.Log("Receive sucsess!:" + str);
        recvStr =recvStr+"\n"+System.DateTime.Now.ToShortTimeString()+"\t" + str;
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
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket connect failed:" + ex.ToString());

        }

    }
    public void Send()
    {
        sendStr = inputField.text;
        inputField.text = "";
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
