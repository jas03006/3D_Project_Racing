using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiManager : MonoBehaviour
{
    public static MultiManager instance = null;

    public uint current_player_cnt { get; private set; } = 0;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else {
            Destroy(this);
            return;
        }
    }

    public uint generate_player_index() {
        current_player_cnt++;
        return current_player_cnt-1;
    }
}
