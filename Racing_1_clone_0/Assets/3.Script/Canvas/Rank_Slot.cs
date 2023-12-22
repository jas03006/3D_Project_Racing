using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rank_Slot : MonoBehaviour
{
    [SerializeField] private Text name_text;
    [SerializeField] private Text record_text;
    [SerializeField] private Image record_BG;
    public void update_name(string value) {
        name_text.text = value;
    }

    public void show_record(string value) {
        record_BG.enabled = true;
        record_text.gameObject.SetActive(true);
        record_text.text = value;
    }
    public void hide_record()
    {
        record_BG.enabled = false;
    }
}
