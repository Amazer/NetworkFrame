using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;

public class Echo : MonoBehaviour
{

    Socket socket;
    byte[] readBuffer = new byte[1024];
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
                socket.Send(sendBytes);
                sendStr = "";
            }
        }
        if (socket.Poll(0, SelectMode.SelectRead))
        {
            int count = socket.Receive(readBuffer);
            recvStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);
            Debug.Log("Receive sucsssue!:" + recvStr);
            text.text = recvStr;
        }
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
