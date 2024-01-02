using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

public class TCP_Manager : MonoBehaviour
{
    //public InputField ip_address;
    //public InputField port;
    [SerializeField] private Text status;
    //기본 적인 소켓 통신
    // .net -> packet: stream
    // thread를 이용해 data read
    StreamReader reader;
    StreamWriter writer;

    [SerializeField] private string ip;
    [SerializeField] private string port;

    public InputField message_box;
    //private MessegePooling message;

    private Queue<string> log_queue = new Queue<string>();
    private void status_message() {
        if (log_queue.Count > 0) {
            status.text = log_queue.Dequeue();
        }
    }

    #region Server
    public void server_open() {
        //message = FindObjectOfType<MessegePooling>();
        Thread thread = new Thread(server_connect);
        thread.IsBackground = true;
        thread.Start();
    }

    private void server_connect() {
        try
        {
            TcpListener tcp_listener = new TcpListener(IPAddress.Parse(ip), int.Parse(port));
            tcp_listener.Start();
            log_queue.Enqueue("Server Open");

            TcpClient client = tcp_listener.AcceptTcpClient();
            log_queue.Enqueue("Client Accept");
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;

            while (client.Connected) {
                string readData = reader.ReadLine();
                //message.Message(readData);
            }
        }
        catch (Exception e) {
            log_queue.Enqueue(e.Message);
        }
    }
    #endregion

    #region Client
    public void client_open() {
        //message = FindObjectOfType<MessegePooling>();
        log_queue.Enqueue("Client Open");
        Thread thread = new Thread(client_connect);
        thread.IsBackground = true;
        thread.Start();
    }

    private void client_connect()
    {
        try {
            TcpClient tcp_client = new TcpClient();
            IPEndPoint ip_end = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            tcp_client.Connect(ip_end);
            log_queue.Enqueue("Client Connect");

            reader = new StreamReader(tcp_client.GetStream());
            writer = new StreamWriter(tcp_client.GetStream());
            writer.AutoFlush = true;

            while (tcp_client.Connected)
            {
                string readData = reader.ReadLine();
                //message.Message(readData);
            }
        }
        catch (Exception e) { 
            log_queue.Enqueue(e.Message);
        }
    }

    public void sending_btn() {
        string msg = $"{SQL_Manager.instance.info.User_Name}: " + message_box.text;
        if (sending_message(msg)) {
            //message.Message(msg);
            message_box.text = string.Empty;
        }
    }
    private bool sending_message(string msg) {
        if (writer != null)
        {
            writer.WriteLine(msg);
            return true;
        }
        else {
            Debug.Log("Writer is Null");
            return false;
        }
    }
    #endregion

    private void Update()
    {
        status_message();
        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
            sending_btn();
        }
    }
}
