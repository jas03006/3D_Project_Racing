using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room_Info : MonoBehaviour
{
    public string room_name { get; private set; }
    public string ip { get; private set; }
    public int port { get; private set; }

    public Room_Info(string name_, string host)
    {
        room_name = name_;
        string[] arr = host.Split(':');
        ip = arr[0];
        port = int.Parse(arr[1]);
    }
}
