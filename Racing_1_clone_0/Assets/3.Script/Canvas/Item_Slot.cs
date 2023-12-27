using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Slot : MonoBehaviour
{
    public item_index index { get; private set; } = item_index.empty;
    [SerializeField] Image image;

    public void set_img(item_index ind) {
        image.sprite = Item_Box_Manager.instance.item_sprite_arr[(int)ind];
        index = ind;
    }

    public void remove_img() {
        image.sprite = null;
        
    }

    public item_index use_item() {
        remove_img();
        item_index old_ind = index;
        index = item_index.empty;
        return old_ind;
    }
}
