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

            socket.BeginReceive(readBuffer, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket connect failed:" + ex.ToString());
        }

    }
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            recvStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);

            Debug.Log("Receive sucsssue!:" + recvStr);
            socket.BeginReceive(readBuffer, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError("Receive error:" + ex.ToString());
        }
    }
    public void Send()
    {
        string sendStr = inputField.text;
        inputField.text = "";
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count= socket.EndSend(ar);
            Debug.Log("send ok!");
        }
        catch(SocketException ex)
        {
            Debug.LogError("send error:" + ex.ToString());
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
