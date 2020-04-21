using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;

public class Echo : MonoBehaviour
{

    Socket socket;
    public InputField inputField;
    public Text text;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 8888);
    }
    public void Send()
    {
        string sendStr = inputField.text;
        inputField.text = "";
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);

        byte[] readBuffer = new byte[1024];
        int count = socket.Receive(readBuffer);
        string recvStr = System.Text.Encoding.Default.GetString(readBuffer, 0, count);
        text.text = recvStr;
        socket.Close();
    }
}
