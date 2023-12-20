using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum color_index { 
    mint = 0,
    yellow,
    blue,
    red,
    gray,
    purple
}
public class Color_Manager : MonoBehaviour
{
    public static Color_Manager instance = null;
    [SerializeField] public Material[] car_material_arr;
    [SerializeField] public Material[] minimap_material_arr;

    [SerializeField] public CarNetworkRoomPlayer local_car_room;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else {
            Destroy(this.gameObject);
            return;
        }
    }

    public void click_color_button(int ind) {
        if (local_car_room != null) {
            local_car_room.set_body_material((color_index)ind);
            local_car_room.set_body_material_CMD((color_index)ind);
        }        
    }

}
