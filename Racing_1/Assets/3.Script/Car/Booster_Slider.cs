using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Booster_Slider : MonoBehaviour
{
    public static Booster_Slider instance = null;

    [SerializeField] Slider booster_slider_ui;
    [SerializeField] GameObject[] booster_image_arr;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else {
            Destroy(this);
        }
    }
    public void get_gage(float value)
    {
        if (is_booster_full()) {            
            return;
        }
        float new_value = booster_slider_ui.value + value;
        if (new_value >= booster_slider_ui.maxValue) {
            get_booster();
            new_value -= booster_slider_ui.maxValue;
        }
        if (is_booster_full())
        {
            booster_slider_ui.value = 0;
            return;
        }
        booster_slider_ui.value = new_value;
    }

    private bool is_booster_full() {
        for (int i =0; i< booster_image_arr.Length; i++) {
            if (!booster_image_arr[i].activeSelf) {
                return false;
            }
        }
        return true;
    }
    public void get_booster() {
        for (int i = 0; i < booster_image_arr.Length; i++)
        {
            if (!booster_image_arr[i].activeSelf)
            {
                booster_image_arr[i].SetActive(true);
                return;
            }
        }
    }
    public bool use_booster() {
        for (int i = booster_image_arr.Length-1; i >= 0 ; i--)
        {
            if (booster_image_arr[i].activeSelf)
            {
                booster_image_arr[i].SetActive(false);
                return true;
            }
        }
        return false;
    }   
}
