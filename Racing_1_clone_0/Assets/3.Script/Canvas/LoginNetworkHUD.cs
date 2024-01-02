using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[AddComponentMenu("Network/Login Network HUD")]
[RequireComponent(typeof(NetworkManager))]
public class LoginNetworkHUD : MonoBehaviour
{
    [SerializeField] private Text Log;
    public InputField id_i;
    public InputField pwd_i;

    NetworkRoomManager manager;
    SceneManager scene_manager;
    [SerializeField] private SQL_Manager sql_manager;

    public int offsetX;
    public int offsetY;

    private bool is_login = false;

    void Awake()
    {
        manager = GetComponent<NetworkRoomManager>();
    }

    void OnGUI()
    {

        if (!is_login) {
            GUILayout.BeginArea(new Rect( offsetX, 40 + offsetY, 250, 9999));
            if (GUILayout.Button("Login"))
            {
                if (login_btn()) {
                    sql_manager.show_room();
                    is_login = true;
                }               
                
            }
            GUILayout.EndArea();
            return;
        }
       
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            GUILayout.BeginArea(new Rect(offsetX, 40 + offsetY, 250, 9999));
            
                StartButtons();
           
        }
        else
        {
            GUILayout.BeginArea(new Rect(10 , 40 , 250, 9999));
            if (!Utils.IsSceneActive(manager.GameplayScene))
            {
                StatusLabels();
            }
        }

        // client ready
        if (NetworkClient.isConnected && !NetworkClient.ready)
        {
            if (GUILayout.Button("Client Ready"))
            {
                NetworkClient.Ready();
                if (NetworkClient.localPlayer == null)
                {
                    NetworkClient.AddPlayer();
                }
            }
        }

        if (!Utils.IsSceneActive(manager.GameplayScene))
        {
            StopButtons();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (!NetworkClient.active)
        {
            // Server + Client
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Host (Server + Client)"))
                {
                    sql_manager.create_room();
                    manager.StartHost();
                    kcp2k.KcpTransport kcp = (manager.transport as kcp2k.KcpTransport);
                    Debug.Log("IP: "+manager.networkAddress);
                    Debug.Log("port: "+kcp.Port);
                }
            }

            // Client + IP
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Client"))
            {
              
                kcp2k.KcpTransport kcp = (manager.transport as kcp2k.KcpTransport);
                kcp.Port = 6666;
                manager.StartClient();
            }
            // This updates networkAddress every frame from the TextField
            manager.networkAddress = GUILayout.TextField(manager.networkAddress);
            
            GUILayout.EndHorizontal();

            // Server Only
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // cant be a server in webgl build
                GUILayout.Box("(  WebGL cannot be server  )");
            }
            else
            {
                if (GUILayout.Button("Server Only")) manager.StartServer();
            }
        }
        else
        {
            // Connecting
            GUILayout.Label($"Connecting to {manager.networkAddress}..");
            if (GUILayout.Button("Cancel Connection Attempt"))
            {
                manager.StopClient();
            }
        }
    }

    void StatusLabels()
    {
        // host mode
        // display separately because this always confused people:
        //   Server: ...
        //   Client: ...
        if (NetworkServer.active && NetworkClient.active)
        {
            GUILayout.Label($"<b>Host</b>: running via {Transport.active}");
        }
        // server only
        else if (NetworkServer.active)
        {
            GUILayout.Label($"<b>Server</b>: running via {Transport.active}");
        }
        // client only
        else if (NetworkClient.isConnected)
        {
            GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress} via {Transport.active}");
        }
    }

    void StopButtons()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop Host"))
            {
                manager.StopHost();
            }
            if (GUILayout.Button("Stop Client"))
            {
                manager.StopClient();
            }
            GUILayout.EndHorizontal();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Client"))
            {
                manager.StopClient();
            }
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            if (GUILayout.Button("Stop Server"))
            {
                manager.StopServer();
            }
        }
    }
    

    public bool login_btn()
    {
        if (id_i.text.Equals(string.Empty) || pwd_i.text.Equals(string.Empty))
        {
            Log.text = "아이디 비밀번호를 입력하세요.";
            return false;
        }

        if (SQL_Manager.instance.login(id_i.text, pwd_i.text))
        {
            User_Info info = SQL_Manager.instance.info;
            Debug.Log(info.User_Name + " | " + info.User_Password);
            id_i.gameObject.SetActive(false);
            pwd_i.gameObject.SetActive(false);
            Log.gameObject.SetActive(false);
            //gameObject.SetActive(false);
            return true;
        }
        else
        {
            Log.text = "아이디 비밀번호를 확인해주세요.";
            return false;
        }
    }
}
