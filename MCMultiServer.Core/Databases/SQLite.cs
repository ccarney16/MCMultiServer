using System;
using System.Data;
using System.Data.SQLite;

using MCMultiServer.Net;
using MCMultiServer.Srv;

using Newtonsoft.Json;

namespace MCMultiServer.Databases {
    /// <summary>
    /// SQLite Database Support. For usage under development or small production servers. (refer to MSSQL or MariaDB for bigger production systems) 
    /// </summary>
    public class SQLite : Database {
        private SQLiteConnection connection;

        //Settings, currently nothing
        private String dbFile = Paths.DatabaseDirectory + "/SQLite.db";
        private String connectionString = "Data Source=db/SQLite.db;Version=3;";

        //init system
        public override Boolean init(String configFile = null) {
            //Check to make sure the file exists in some form or another
            if (!System.IO.File.Exists(dbFile)) {
                try {
                    base.OnLog("database file being created");
                    SQLiteConnection.CreateFile(dbFile);
                } catch (System.IO.IOException ex) {
                    throw ex;
                }
            }

            connection = new SQLiteConnection(connectionString);
            connection.Open();

            //create the server list table. 
            executeQuery(@"CREATE TABLE IF NOT EXISTS `mcms_serverlist` (
	                `guid` TEXT NOT NULL,
	                `date_created` TEXT NULL,
	                `settings` LONGTEXT NULL,
                    PRIMARY KEY(`guid`));");

            //autoloading table. 
            executeQuery(@"CREATE TABLE IF NOT EXISTS 'mcms_autoload' (
                    'guid' TEXT NOT NULL,
                    'autoload' INTEGER NOT NULL,
                     PRIMARY KEY('guid'));");

            //ip tables 
            executeQuery(@"CREATE TABLE IF NOT EXISTS 'mcms_ipmaps' ('maps' TEXT NOT NULL, PRIMARY KEY('maps'));");

            //return to logger.
            return true;
        }

        //completely prone to sql injections.
        public override void executeQuery(String query, Boolean verbose = false) {
            if (connection.State != ConnectionState.Open) {
                base.OnLog("Connection state of database is closed, attempting to reopen");
                connection.Open();
            }
            SQLiteCommand cmd = new SQLiteCommand(query, connection);
            if (verbose) {
                OnLog(cmd.ExecuteNonQuery().ToString() + " rows affected");
            } else {
                cmd.ExecuteNonQuery();
            }
        }

        //Gets the data from the database
        public override DataTable getDataTable(String query) {
            if (connection.State != ConnectionState.Open) {
                base.OnLog("Connection state of database is closed, attempting to reopen");
                connection.Open();
            }

            SQLiteCommand cmd = new SQLiteCommand(connection);
            cmd.CommandText = query;

            SQLiteDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            return dt;
        }

        public override Boolean addServer(Guid serverID, String serverName, ServerProperties properties) {
            if (serverID == Guid.Empty) { OnLog("Server ID is empty"); return false; }
            if (serverName == null) { OnLog("Server Name is empty"); return false; }

            string current = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            executeQuery("INSERT INTO mcms_serverlist (guid, date_created, settings) VALUES ('" + serverID.ToString() + "','" + current.ToString() + "','" + Newtonsoft.Json.JsonConvert.SerializeObject(properties) + "');");
            return true;
        }

        //No longer in use, will be removed at a later date
        public override bool changeServerName(Guid serverID, String newName) {
            //executeQuery("UPDATE 'mcms_serverlist' SET server_name='" + newName + "' WHERE GUID");
            return true;
        }

        public override bool changeServerSettings(Guid serverID, ServerProperties properties) {
            executeQuery("UPDATE 'mcms_serverlist' SET settings='" + Newtonsoft.Json.JsonConvert.SerializeObject(properties) + "' WHERE guid='" + serverID.ToString() + "';");
            return true;
        }

        public override bool dropServer(Guid serverID) {
            //might be prone to sql injections, or maybe not.
            String query = "DELETE from 'mcms_serverlist' WHERE guid='{0}';";
            string final = String.Format(query, serverID);

            executeQuery(final);
            return true;
        }

        public override ServerProperties getServerProperties(Guid serverID) {
            SQLiteCommand cmd = new SQLiteCommand(connection);

            String query = "SELECT * FROM 'mcms_serverlist' WHERE guid='" + serverID.ToString() + "';";

            //Get table
            DataTable srvdate = getDataTable(query);

            ServerProperties prop;
            try {
                prop = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerProperties>(srvdate.Rows[0]["settings"].ToString());
                //setup id
                prop.ServerID = Guid.Parse(srvdate.Rows[0]["guid"].ToString());
                //setup name
                //prop.DisplayName = srvdate.Rows[0]["server_name"].ToString();
            } catch { return null; }

            return prop;
        }

        //While this has no major problems, having many servers may cause this to hang
        public override ServerProperties[] getAllServers() {
            DataTable srvdata = getDataTable("SELECT * FROM 'mcms_serverlist';");

            int cnt = 0;
            ServerProperties[] dat = new ServerProperties[srvdata.Rows.Count];
            foreach (System.Data.DataRow row in srvdata.Rows) {
                ServerProperties prop = new ServerProperties();
                prop = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerProperties>(row["settings"].ToString());

                prop.ServerID = Guid.Parse(row["guid"].ToString());

                //Since SQLite does not have any date functions, we have to convert it.
                prop.DateCreated = Convert.ToDateTime(row["date_created"].ToString());

                dat[cnt] = prop;
                cnt++;
            }
            return dat;
        }

        //first row and last row, built into SQLite perfectly, MySQL, not so much
        public override ServerProperties[] getServerList(int limit, int offset) {
            string query = "SELECT * FROM 'mcms_serverlist' LIMIT " + limit + " OFFSET " + offset + ";";
            DataTable dt = getDataTable(query);

            //we have seen this before, right?
            int cnt = 0;
            ServerProperties[] dat = new ServerProperties[dt.Rows.Count];
            foreach (System.Data.DataRow row in dt.Rows) {
                ServerProperties prop = new ServerProperties();
                prop = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerProperties>(row["settings"].ToString());

                prop.ServerID = Guid.Parse(row["guid"].ToString());

                prop.DateCreated = Convert.ToDateTime(row["date_created"].ToString());
                dat[cnt] = prop;
                cnt++;
            }
            return dat;
        }

        //public override String[] getAutoLoadList() {
        //    string query = "SELECT * FROM 'mcms_autoload';";
        //    DataTable dt = getDataTable(query);

        //    string[] list = new string[0];
        //    int cnt = 0;
        //    foreach (DataRow row in dt.Rows) {
        //        if (row["autoload"].ToString() == "1") {
        //            cnt++;
        //            Array.Resize(ref list, cnt);
        //            list[cnt - 1] = row["guid"].ToString();
        //        }
        //    }
        //    return list;
        //}

        ////Get autoload
        //public override Boolean isAutoload(Guid serverID) {
        //    string query = "SELECT * FROM 'mcms_autoload' WHERE guid='" + serverID.ToString() + "';";
        //    DataTable tbl = getDataTable(query);

        //    //Do stuff if server is found
        //    if (tbl.Rows.Count >= 1) {
        //        if (tbl.Rows[0]["guid"].ToString() == serverID.ToString()) {
        //            if (tbl.Rows[0]["autoload"].ToString() == "1") {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        //public override Boolean setAutoLoad(Guid serverID, Boolean enabled = false) {
        //    string query = "INSET INTO 'mcms_autoload' (guid, autoload) VALUES ('" + serverID.ToString() + "', 1);";
        //    executeQuery(query);
        //    return true;
        //}

        public override bool addIPMap(string map)
        {
            throw new NotImplementedException();
        }

        public override string[] getIPMap()
        {
            throw new NotImplementedException();
        }

        public override bool removeIPMap(string ip)
        {
            throw new NotImplementedException();
        }
    }
}
