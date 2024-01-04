using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_Info 
{
    public string name { get; private set; }
    public string host { get; private set; }
    public string ip { get; private set; }
    public int port { get; private set; }
    
    public Room_Info(string name_, string host_)
    {
        name = name_;
        host = host_;
        string[] arr = host_.Split(':');
        ip = arr[0];
        port = int.Parse(arr[1]);
    }
}
public class Room_Slot : MonoBehaviour
{
    public int index;
    public Room_Info info;
    [SerializeField]private Text text;

    public void show() {
        if (info == null) {
            text.text = "Room";
            return;
        }
        text.text = info.name;
    }

    public void enter_room() {
        if (info != null) {
            GameObject.FindObjectOfType<LoginNetworkHUD>().room_slot_button(info.ip, info.port);
        }
       
    }
}
