using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

using Mirror;

public enum item_index
{
    empty = -1,
    smoke_screen = 0,
    slicer = 1,
    oil = 2,
    water_bomb = 3,
    temp = 4,
    item_box = 5
}

public class Item_Box_Manager : NetworkBehaviour
{
    public static Item_Box_Manager instance = null;
    [SerializeField] public Sprite[] item_sprite_arr;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else {
            Destroy(this);
            return;
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkRoomManager net_manager = FindObjectOfType<NetworkRoomManager>();
        GameObject[] pos = GameObject.FindGameObjectsWithTag("Item_Box_Pos");
        for (int i =0; i < pos.Length; i++) {
            GameObject go = Instantiate(net_manager.spawnPrefabs[(int)item_index.item_box], pos[i].transform.position, Quaternion.identity);
            NetworkServer.Spawn(go);
        }
    }


}
