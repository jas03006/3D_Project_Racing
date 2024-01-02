using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using LitJson;

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

   private Room_Info[] rooms;
   [SerializeField] private GameObject[] rooms_go;

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

        db_path = Application.dataPath+"/Database";
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
        string json_string = File.ReadAllText(path+"/config.json");
        JsonData item_data = JsonMapper.ToObject(json_string);
        string server_info = 
            $"Server={item_data[0]["IP"]};" +
            $" Database={item_data[0]["TableName"]};"+
            $" Uid={item_data[0]["ID"]};"+
            $" Pwd={item_data[0]["PW"]};"+
            $" Port={item_data[0]["PORT"]};"+
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
                string.Format(@"SELECT User_Name, User_Password FROM user_info WHERE User_Name = '{0}' AND User_Password = '{1}';",id_,pwd_);
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);
            sql_reader = cmd.ExecuteReader();
            if (sql_reader.HasRows)
            {
                while (sql_reader.Read()) {
                    string name = (sql_reader.IsDBNull(0)?string.Empty: (string)sql_reader["User_Name"].ToString());
                    string password = (sql_reader.IsDBNull(0)?string.Empty: (string)sql_reader["User_Password"].ToString());
                    if (!name.Equals(string.Empty) && !password.Equals(string.Empty))
                    {
                        info = new User_Info(name,password);
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

    public bool create_room() {
        
         try
        {
            if (!connection_check(connection))
            {
                return false;
            }
            string sql_cmd =
               // string.Format(@"SELECT HOST FROM {0} WHERE ID = {1};", "information_schema.PROCESSLIST", "CONNECTION_ID()");
                 string.Format(@"INSERT INTO room_list VALUES ( '{0}', (SELECT HOST FROM {1} WHERE ID = {2} LIMIT 1) );", info.User_Name, "information_schema.PROCESSLIST", "CONNECTION_ID()" );
            MySqlCommand cmd = new MySqlCommand(sql_cmd, connection);
           
             sql_reader = cmd.ExecuteReader();
           
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

    public void show_room() {
        rooms = new Room_Info[rooms_go.Length];

    }
}
