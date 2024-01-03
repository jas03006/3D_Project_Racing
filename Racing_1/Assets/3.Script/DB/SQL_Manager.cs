using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using LitJson;
using System.Net;
using System.Net.Sockets;
public class User_Info {
    public string User_Name { get; private set; }
    public string User_Password { get; private set; }

    public User_Info(string name_, string pwd_) {
        User_Name = name_;
        User_Password = pwd_;
    }
}
public class SQL_Manager : MonoBehaviour
{
    public User_Info info;

    public MySqlConnection connection;
    public MySqlDataReader sql_reader;

    public string db_path = string.Empty;

    public static SQL_Manager instance = null;

    private List<Room_Info> room_list;
    [SerializeField] private Room_Slot[] room_go_arr;
    private int page_num = 0;

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

        db_path = Application.dataPath + "/Database";
        string server_info = server_set(db_path);
        Debug.Log(server_info);
        try {
            if (server_info.Equals(string.Empty)) {
                Debug.Log("SQL Server Json error");
                return;
            }
            connection = new MySqlConnection(server_info);
            connection.Open();
            Debug.Log("SQL Server Open Complete");

        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    private string server_set(string path) {
        if (!File.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        string json_string = File.ReadAllText(path + "/config.json");
        JsonData item_data = JsonMapper.ToObject(json_string);
        string server_info =
            $"Server={item_data[0]["IP"]};" +
            $" Database={item_data[0]["TableName"]};" +
            $" Uid={item_data[0]["ID"]};" +
            $" Pwd={item_data[0]["PW"]};" +
            $" Port={item_data[0]["PORT"]};" +
            $" CharSet=utf8;";
        return server_info;
    }

    private bool connection_check(MySqlConnection con) {
        if (con.State != System.Data.ConnectionState.Open)
        {
            con.Open();
            if (con.State != System.Data.ConnectionState.Open)
            {
                return false;
            }
        }
        return true;
    }
    public bool login(string id_, string pwd_) {

        try
        {
            if (!connection_check(connection))
            {
                return false;
            }
            string sql_cmd =
                string.Format(@"SELECT User_Name, User_Password FROM user_info WHERE User_Name = '{0}' AND User_Password = '{1}';", id_, pwd_);
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);
            sql_reader = cmd.ExecuteReader();
            if (sql_reader.HasRows)
            {
                while (sql_reader.Read()) {
                    string name = (sql_reader.IsDBNull(0) ? string.Empty : (string)sql_reader["User_Name"].ToString());
                    string password = (sql_reader.IsDBNull(0) ? string.Empty : (string)sql_reader["User_Password"].ToString());
                    if (!name.Equals(string.Empty) && !password.Equals(string.Empty))
                    {
                        info = new User_Info(name, password);
                        if (!sql_reader.IsClosed) {
                            sql_reader.Close();
                        }
                        return true;
                    }
                    else {
                        break;
                    }
                }

            }
            if (!sql_reader.IsClosed)
            {
                sql_reader.Close();
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }
    #region room page
    public string get_address(int port = 7777) {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        string ippaddress = "";

        for (int i = 0; i < host.AddressList.Length; i++)
        {
            if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                ippaddress = host.AddressList[i].ToString() + ":"+ port;
                break;
            }
        }
        return ippaddress;
    }
    public ushort create_room(bool is_LAN = false) {

        try
        {
            if (!connection_check(connection))
            {
                return 0;
            }
            string sql_cmd="";
            if (!is_LAN)
            {

                sql_cmd =
                     string.Format(@"INSERT INTO room_list VALUES ( '{0}', (SELECT HOST FROM {1} WHERE ID = {2} LIMIT 1), (SELECT HOST FROM {1} WHERE ID = {2} LIMIT 1) );", info.User_Name, "information_schema.PROCESSLIST", "CONNECTION_ID()");
            }
            else {
                string ippaddress = get_address(7777);
                sql_cmd =
                         string.Format(@"INSERT INTO room_list VALUES ( '{0}', '{1}', (SELECT HOST FROM {2} WHERE ID = {3} LIMIT 1));", info.User_Name, ippaddress, "information_schema.PROCESSLIST", "CONNECTION_ID()");
            }
            
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);

            sql_reader = cmd.ExecuteReader();
            if (!sql_reader.IsClosed)
            {
                sql_reader.Close();
            }

            if (!is_LAN)
            {
                sql_cmd =
                     string.Format(@"SELECT HOST FROM {0} WHERE ID = {1} LIMIT 1;", "information_schema.PROCESSLIST", "CONNECTION_ID()");
                cmd = new MySqlCommand(sql_cmd, connection);

                sql_reader = cmd.ExecuteReader();
                Room_Info room_info;
                if (sql_reader.HasRows)
                {
                    sql_reader.Read();
                    room_info = new Room_Info("", (string)sql_reader["host"].ToString());
                    if (!sql_reader.IsClosed)
                    {
                        sql_reader.Close();
                    }
                    return (ushort)room_info.port;
                }
                else
                {
                    return 0;
                }
            }
            
            if (!sql_reader.IsClosed)
            {
                sql_reader.Close();
            }
            return 7777;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return 0;
        }
    }

    public bool get_room_data() {
        room_list = new List<Room_Info>();

        try
        {
            if (!connection_check(connection))
            {
                return false;
            }
            string sql_cmd =
                 string.Format(@"SELECT * FROM {0};", "room_list");
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);
            sql_reader = cmd.ExecuteReader();

            if (sql_reader.HasRows)
            {
                while (sql_reader.Read())
                {
                    string name = (sql_reader.IsDBNull(0) ? string.Empty : (string)sql_reader["name"].ToString());
                    string host = (sql_reader.IsDBNull(0) ? string.Empty : (string)sql_reader["host"].ToString());
                    Room_Info room = new Room_Info(name, host);
                    room_list.Add(room);
                }
            }

            if (!sql_reader.IsClosed)
            {
                sql_reader.Close();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    public void show_room() {
        if (room_list == null) {
            return;
        }
        for (int i = 0; i < room_go_arr.Length; i++) {
            int ind = page_num * room_go_arr.Length + i;
            if (ind < room_list.Count)
            {
                Room_Info room = room_list[ind];
                room_go_arr[i].info = room;
            }
            else {
                room_go_arr[i].info = null;                
            }
            room_go_arr[i].show();
        }
    }

    public void turn_page(bool is_right) {
        if (is_right)
        {
            if (Mathf.CeilToInt(room_list.Count / room_go_arr.Length) >= page_num + 1)
            {
                page_num++;
            }
            return;
        }
        else {
            if (page_num-1 >= 0) {
                page_num--;
                return;
            }
        }
    }

    public bool delete_room(string host) {
        try
        {
            if (!connection_check(connection))
            {
                return false;
            }
            string sql_cmd =
                 string.Format(@"DELETE FROM room_list WHERE host = '{0}';", host);
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);

            sql_reader = cmd.ExecuteReader();

            if (!sql_reader.IsClosed)
            {
                sql_reader.Close();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }


    public bool update_DB() {
        try
        {
            if (!connection_check(connection))
            {
                return false;
            }
            string ip = get_address(7777);
            //Debug.Log(ip);
            string sql_cmd =
                   //string.Format(@"DELETE FROM {0} WHERE public_host NOT IN  (SELECT HOST FROM {1}) OR host = {2} OR host = '{3}';", "room_list", "information_schema.PROCESSLIST", "CONNECTION_ID()", get_address(7777));
                   string.Format(@"DELETE FROM {0} WHERE( public_host NOT IN  (SELECT HOST FROM {1})) OR (host = '{2}');", "room_list", "information_schema.PROCESSLIST", ip);
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);

            sql_reader = cmd.ExecuteReader();        

            if (!sql_reader.IsClosed)
            {
                sql_reader.Close();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }
    #endregion
}
