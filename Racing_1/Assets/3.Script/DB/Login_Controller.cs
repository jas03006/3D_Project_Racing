using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login_Controller : MonoBehaviour
{
    public InputField id_i;
    public InputField pwd_i;

    [SerializeField] private Text Log;

    public void login_btn() {
        if (id_i.text.Equals(string.Empty) || pwd_i.text.Equals(string.Empty)) {
            Log.text = "���̵� ��й�ȣ�� �Է��ϼ���.";
            return;
        }

        if (SQL_Manager.instance.login(id_i.text, pwd_i.text))
        {
            User_Info info = SQL_Manager.instance.info;
            Debug.Log(info.User_Name + " | "+info.User_Password);
            gameObject.SetActive(false);
        }
        else {
            Log.text = "���̵� ��й�ȣ�� Ȯ�����ּ���.";
        }
    }
}
