using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-player
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomPlayer.html
*/

/// <summary>
/// This component works in conjunction with the NetworkRoomManager to make up the multiplayer room system.
/// The RoomPrefab object of the NetworkRoomManager must have this component on it.
/// This component holds basic room player data required for the room to function.
/// Game specific data for room players can be put in other components on the RoomPrefab or in scripts derived from NetworkRoomPlayer.
/// </summary>
public class CarNetworkRoomPlayer : NetworkRoomPlayer
{

    [Header("Network")]
    [SyncVar]
    public int player_index = 0;
    [SyncVar]
    public string user_name = "";
    public int name_tag_index;
    [SyncVar]
    public color_index material_index = 0;

    [SerializeField] private MeshRenderer car_body;
    private Position_Setter ps;
    public int connection_ID;
    private void Awake()
    {
        ps = FindObjectOfType<Position_Setter>();
    }
    public void set_body_material(color_index ind)
    {
        material_index = ind;
        car_body.material = Color_Manager.instance.car_material_arr[(int)material_index];
    }
    [Command]
    public void set_body_material_CMD(color_index ind) {
        set_body_material(ind);
        set_body_material_RPC(ind);
    }
    [ClientRpc]
    public void set_body_material_RPC(color_index ind)
    {
        set_body_material(ind);
    }

    /*   [Command]
       public void deactive_CMD() {
           deactive_RPC();
           gameObject.SetActive(false);
       }*/
    /*[ClientRpc]
    public void deactive_RPC()
    {
        gameObject.SetActive(false);
        Debug.Log("deactive RPC!!!!!!");
    }*/
    [TargetRpc]
    public void on_game_scene_load_RPC()
    {
        room_player_deactive();
    }
    public void room_player_deactive()
    {
        NetworkRoomPlayer[] nrp_arr = FindObjectsOfType<NetworkRoomPlayer>();
        foreach (NetworkRoomPlayer nrp in nrp_arr)
        {
            nrp.gameObject.SetActive(false);
        }
    }
    #region Start & Stop Callbacks

    public override void Start()
    {
        base.Start();
        /*if (isServer) {
            Position_Setter ps = FindObjectOfType<Position_Setter>();
            ps.locate_car(this.gameObject);
            set_car_room_pos(transform.position , transform.rotation);
        }*/
        
    }

    
    public void Update()
    {
        transform.Rotate(0f,Time.deltaTime*10f, 0f);
        //set_car_room_pos(transform.position, transform.rotation);
    }

    [ClientRpc]
    public void set_car_room_pos(Vector3 position, Quaternion rotation) {
        transform.position = position;
        transform.rotation = rotation;
    }
    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() {
        set_body_material(material_index);
        set_body_material_CMD(material_index);
    }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() {
        //Debug.Log("set local room car");
        Color_Manager.instance.local_car_room = this;
        set_name_CMD(SQL_Manager.instance.info.User_Name);
    }

    [Command]
    public void set_name_CMD(string name_) {
        user_name = name_;
        set_name_tag(name_);
        set_name_tag_RPC(name_tag_index, name_);

        for (int i = 0; i < name_tag_index; i++)
        {
            set_name_tag_TRPC(i, ps.car_arr[i].transform.position, ps.car_arr[i].GetComponentInChildren<CarNetworkRoomPlayer>().user_name);
        }
    }
    public void set_name_tag( string name_) {
        ps.locate_car(this.gameObject);
        set_car_room_pos(transform.position, transform.rotation);
        ps.name_tag_arr[name_tag_index].position = Camera.main.WorldToScreenPoint(transform.position) + Vector3.up * 215f;
        ps.name_tag_arr[name_tag_index].GetComponentInChildren<Text>().text = name_;
    }
    [ClientRpc]
    public void set_name_tag_RPC(int index_, string name_) {     
        ps.name_tag_arr[index_].position = Camera.main.WorldToScreenPoint(transform.position) + Vector3.up * 215f;
        ps.name_tag_arr[index_].GetComponentInChildren<Text>().text = name_;
    }
    [TargetRpc]
    public void set_name_tag_TRPC(int index_, Vector3 pos_, string name_)
    {
        ps.name_tag_arr[index_].position = Camera.main.WorldToScreenPoint(pos_) + Vector3.up * 210f;
        ps.name_tag_arr[index_].GetComponentInChildren<Text>().text = name_;
    }


    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion

    #region Room Client Callbacks

    /// <summary>
    /// This is a hook that is invoked on all player objects when entering the room.
    /// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
    /// </summary>
    public override void OnClientEnterRoom() {
    }

    /// <summary>
    /// This is a hook that is invoked on all player objects when exiting the room.
    /// </summary>
    public override void OnClientExitRoom() {
    }

    
    #endregion

    #region SyncVar Hooks

    /// <summary>
    /// This is a hook that is invoked on clients when the index changes.
    /// </summary>
    /// <param name="oldIndex">The old index value</param>
    /// <param name="newIndex">The new index value</param>
    public override void IndexChanged(int oldIndex, int newIndex) {
        
    }

    /// <summary>
    /// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
    /// <para>This function is called when the a client player calls SendReadyToBeginMessage() or SendNotReadyToBeginMessage().</para>
    /// </summary>
    /// <param name="oldReadyState">The old readyState value</param>
    /// <param name="newReadyState">The new readyState value</param>
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) { }

    #endregion

    #region Optional UI

    public override void OnGUI()
    {
        base.OnGUI();
    }

    #endregion
}
