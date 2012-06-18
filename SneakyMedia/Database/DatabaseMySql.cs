using System;
using System.Collections.Generic;
using System.Threading;

using System.Data.SqlClient;
using System.Data.SqlTypes;

using SneakyMedia.Scanner;

using MySql.Data.Types;
using MySql.Data.MySqlClient;

namespace SneakyMedia.Database
{
    public static class Extensions
    {
        public static IEnumerable<IMetadatum> Reversed(this IList<IMetadatum> aList)
        {
            int i = aList.Count;

            while (--i >= 0)
            {
                yield return aList[i];
            }
        }
    }

    public class DatabaseMySql : IDatabase
    {
        private const string kCreateMount = "CREATE TABLE Mount (" + 
        "`MountId` varchar(30) NOT NULL default ''," +
        "`MountUri` varchar(500) NOT NULL," +
        "`LastScanned` varchar(30) default NULL," +
        "PRIMARY KEY  (`MountId`) )" +
        "ENGINE=InnoDB DEFAULT CHARSET=ucs2;";

        private const string kCreateItem = "CREATE TABLE Item (" +
        "`ItemId` int(10) unsigned NOT NULL auto_increment," +
        "`MountId` varchar(30) NOT NULL," +
        "`ItemUri` varchar(300) NOT NULL," +
        "PRIMARY KEY  (`MountId`,`ItemUri`)," +
        "KEY `MOUNTID` (`MountId`)," +
        "KEY `ITEMID` (`ItemId`)," +
        "CONSTRAINT `FK_MOUNT` FOREIGN KEY (`MountId`) REFERENCES `Mount` (`MountId`)" +
        ") ENGINE=InnoDB DEFAULT CHARSET=ucs2;";

        private const string kCreateTag = "CREATE TABLE Tag (" +
        "`TagId` int(10) unsigned NOT NULL auto_increment," +
        "`TagNs` varchar(30) NOT NULL," +
        "`TagName` varchar(30) NOT NULL," +
        "PRIMARY KEY  USING BTREE (`TagNs`,`TagName`)," +
        "KEY `TAGID` (`TagId`)" +
        ") ENGINE=InnoDB DEFAULT CHARSET=ucs2;";

        private const string kCreateMetadatum = "CREATE TABLE Metadatum (" +
        "`MetadatumId` int(10) unsigned NOT NULL auto_increment," +
        "`ItemId` int(10) unsigned NOT NULL," +
        "`TagId` int(10) unsigned NOT NULL," +
        "`Value` varchar(300) NOT NULL," +
        "PRIMARY KEY  (`MetadatumId`)," +
        "KEY `ITEMID` (`ItemId`)," +
        "KEY `TAGID` (`TagId`)," +
        "KEY `VALUE` (`Value`)," +
        "CONSTRAINT `FK_ITEMID` FOREIGN KEY (`ItemId`) REFERENCES `Item` (`ItemId`)," +
        "CONSTRAINT `FK_TAGID` FOREIGN KEY (`TagId`) REFERENCES `Tag` (`TagId`)" +
        ") ENGINE=InnoDB DEFAULT CHARSET=ucs2;";

        private const string kCreateSearch = "CREATE TABLE Search (" +
        "`SearchId` int(10) unsigned NOT NULL auto_increment," +
        "`ItemId` int(10) unsigned NOT NULL," +
        "`TagId` int(10) unsigned NOT NULL," +
        "`Value` varchar(300) NOT NULL," +
        "PRIMARY KEY  (`SearchId`)," +
        "KEY `TAG` (`ItemId`,`TagId`)," +
        "FULLTEXT KEY `TEXT` (`Value`)" +
        ") ENGINE=MyISAM DEFAULT CHARSET=utf8;";

        public DatabaseMySql()
        {
            iConnection = new MySqlConnection("Server=eng.linn.co.uk;Database=sneakymedia;Uid=sneakymedia_user;Pwd=basket;");
            iConnection.Open();

            CreateTable(kCreateMount);
            CreateTable(kCreateItem);
            CreateTable(kCreateTag);
            CreateTable(kCreateMetadatum);
            CreateTable(kCreateSearch);
        }

        private void CreateTable(string aSqlCommand)
        {
            MySqlCommand cmd = new MySqlCommand(aSqlCommand, iConnection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
            }
        }

        public void AddMount(string aMountId, string aMountUri)
        {
            string sql = String.Format("INSERT INTO Mount (MountId, MountUri) VALUES ('{0}', '{1}');", aMountId, aMountUri);

            MySqlCommand cmd = new MySqlCommand(sql, iConnection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                throw (new MountAlreadyExistsError());
            }
        }

        public void UpdateMountUri(string aMountId, string aMountUri)
        {
            string sql = String.Format("UPDATE Mount SET MountUri = '{1}' WHERE MountId = '{0}';", aMountId, aMountUri);

            MySqlCommand cmd = new MySqlCommand(sql, iConnection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        public void UpdateMountScanned(string aMountId, DateTime aLastScanned)
        {
            string sql = String.Format("UPDATE Mount SET LastScanned = '{1}' WHERE MountId = '{0}';", aMountId, aLastScanned);

            MySqlCommand cmd = new MySqlCommand(sql, iConnection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        public string QueryMountUri(string aMountId)
        {
            string sql = String.Format("SELECT MountUri FROM Mount WHERE MountId = '{0}';", aMountId);

            MySqlCommand cmd = new MySqlCommand(sql, iConnection);

            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string mounturi = reader.GetString(0);
                    reader.Close();
                    return (mounturi);
                }

                throw (new MountNotFoundError());
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        public Nullable<DateTime> QueryLastScanned(string aMountId)
        {
            string sql = String.Format("SELECT LastScanned FROM Mount WHERE MountId = '{0}';", aMountId);

            MySqlCommand cmd = new MySqlCommand(sql, iConnection);

            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string lastscanned;

                    try
                    {
                        lastscanned = reader.GetString(0);
                        reader.Close();
                        return (DateTime.Parse(lastscanned));
                    }
                    catch (SqlNullValueException)
                    {
                        reader.Close();
                        return (null);
                    }
                }

                throw (new MountNotFoundError());
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        public IList<string> QueryMounts()
        {
            string sql = String.Format("SELECT MountId FROM Mount;");

            MySqlCommand cmd = new MySqlCommand(sql, iConnection);

            List<string> list = new List<string>();

            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }

                reader.Close();

                return (list);
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        public void Add(IItem aItemId, IList<IMetadatum> aMetadata)
        {
            foreach (IMetadatum metadatum in aMetadata)
            {
                Add(aItemId, metadatum);
            }
        }

        public void Remove(IItem aItemId)
        {
        }

        public void Add(IItem aItemId, IMetadatum aMetadatum)
        {
            ITag tag = aMetadatum.Tag;
            string value = aMetadatum.Value;

            string mountid = aItemId.MountId;
            string itemuri = aItemId.ItemUri;
            string tagns = tag.Ns;
            string tagname = tag.Name;
            bool searchable = Tag.IsSearchable(tag);

            try
            {
                InsertMetadatum(itemuri, mountid, tagname, tagns, value);
            }
            catch (MySqlException)
            {
                try
                {
                    InsertItem(mountid, itemuri);
                }
                catch (MySqlException)
                {
                    try
                    {
                        InsertTag(tagns, tagname);
                    }
                    catch (MySqlException)
                    {
                        throw (new ApplicationException());
                    }

                    try
                    {
                        InsertMetadatum(itemuri, mountid, tagname, tagns, value);
                    }
                    catch (MySqlException)
                    {
                        throw (new ApplicationException());
                    }
                }

                try
                {
                    InsertMetadatum(itemuri, mountid, tagname, tagns, value);
                }
                catch (MySqlException)
                {
                    try
                    {
                        InsertTag(tagns, tagname);
                    }
                    catch (MySqlException)
                    {
                        throw (new ApplicationException());
                    }

                    try
                    {
                        InsertMetadatum(itemuri, mountid, tagname, tagns, value);
                    }
                    catch (MySqlException)
                    {
                        throw (new ApplicationException());
                    }
                }
            }

            if (searchable)
            {
                try
                {
                    InsertSearch(itemuri, mountid, tagname, tagns, value);
                }
                catch (MySqlException)
                {
                    throw (new ApplicationException());
                }
            }
        }

        private void InsertMetadatum(string aItemUri, string aMountId, string aTagName, string aTagNs, string aValue)
        {
            string sql = "INSERT INTO Metadatum (ItemId, TagId, Value) VALUES((SELECT ItemId FROM Item WHERE MountId = @MountId AND ItemUri = @ItemUri), (SELECT TagId FROM Tag T WHERE T.TagNs = @TagNs AND T.TagName = @TagName), @Value);";
            MySqlCommand cmd = new MySqlCommand(sql, iConnection);
            cmd.Parameters.Add(new MySqlParameter("ItemUri", aItemUri));
            cmd.Parameters.Add(new MySqlParameter("MountId", aMountId));
            cmd.Parameters.Add(new MySqlParameter("TagNs", aTagNs));
            cmd.Parameters.Add(new MySqlParameter("TagName", aTagName));
            cmd.Parameters.Add(new MySqlParameter("Value", aValue));
            cmd.ExecuteNonQuery();
        }

        private void InsertItem(string aMountId, string aItemUri)
        {
            string sql = "INSERT INTO Item (MountId, ItemUri) VALUES (@MountId, @ItemUri);";
            MySqlCommand cmd = new MySqlCommand(sql, iConnection);
            cmd.Parameters.Add(new MySqlParameter("ItemUri", aItemUri));
            cmd.Parameters.Add(new MySqlParameter("MountId", aMountId));
            cmd.ExecuteNonQuery();
        }

        private void InsertTag(string aTagNs, string aTagName)
        {
            string sql = "INSERT INTO Tag (TagNs, TagName) VALUES (@TagNs, @TagName);";
            MySqlCommand cmd = new MySqlCommand(sql, iConnection);
            cmd.Parameters.Add(new MySqlParameter("TagNs", aTagNs));
            cmd.Parameters.Add(new MySqlParameter("TagName", aTagName));
            cmd.ExecuteNonQuery();
        }

        private void InsertSearch(string aItemUri, string aMountId, string aTagName, string aTagNs, string aValue)
        {
            string sql = "INSERT INTO Search (ItemId, TagId, Value) VALUES((SELECT ItemId FROM Item WHERE MountId = @MountId AND ItemUri = @ItemUri), (SELECT TagId FROM Tag T WHERE T.TagNs = @TagNs AND T.TagName = @TagName), @Value);";
            MySqlCommand cmd = new MySqlCommand(sql, iConnection);
            cmd.Parameters.Add(new MySqlParameter("ItemUri", aItemUri));
            cmd.Parameters.Add(new MySqlParameter("MountId", aMountId));
            cmd.Parameters.Add(new MySqlParameter("TagNs", aTagNs));
            cmd.Parameters.Add(new MySqlParameter("TagName", aTagName));
            cmd.Parameters.Add(new MySqlParameter("Value", aValue));
            cmd.ExecuteNonQuery();
        }

        public void Remove(IItem aItemId, IMetadatum aMetadatum)
        {
        }

        public IList<IMetadatum> QueryItem(IItem aItemId)
        {
            return (new List<IMetadatum>());
        }

        public IList<IItem> QueryItems(string aMountId)
        {
            return (new List<IItem>());
        }

        private void AddRandom(MySqlCommand aCmd, IEnumerator<IMetadatum> aWheres, int aLevel, uint aRandom)
        {
            string level = aLevel.ToString();

            if (aWheres.MoveNext())
            {
                aCmd.CommandText += "SELECT DISTINCT X" + level;
                aCmd.CommandText += ".ItemId FROM (";
                AddWhere(aCmd, aWheres, ++aLevel);
                aCmd.CommandText += ") X" + level;
                aCmd.CommandText += " ORDER BY RAND() LIMIT " + aRandom.ToString();
            }
            else
            {
                aCmd.CommandText += "SELECT DISTINCT ItemId FROM Item ORDER BY RAND() LIMIT " + aRandom.ToString();
            }
        }

        private void AddWhere(MySqlCommand aCmd, IEnumerator<IMetadatum> aWheres, int aLevel)
        {
            string level = aLevel.ToString();
            IMetadatum current = aWheres.Current;

            aCmd.CommandText += "SELECT DISTINCT M" + level + ".ItemId FROM ";

            if (aWheres.MoveNext())
            {
                aCmd.CommandText += "(";
                AddWhere(aCmd, aWheres, ++aLevel);
                aCmd.CommandText += ") X" + level;
                aCmd.CommandText += " INNER JOIN Metadatum M" + level;
                aCmd.CommandText += " ON X" + level;
                aCmd.CommandText += ".ItemId = M" + level;
                aCmd.CommandText += ".ItemId";
            }
            else
            {
                aCmd.CommandText += "Metadatum M" + level;
            }

            aCmd.CommandText += " INNER JOIN Tag T" + level;
            aCmd.CommandText += " ON M" + level;
            aCmd.CommandText += ".TagId = T" + level;
            aCmd.CommandText += ".Tagid WHERE T" + level;
            aCmd.CommandText += ".TagNs = @TagNs" + level;
            aCmd.CommandText += " AND T" + level;
            aCmd.CommandText += ".TagName = @TagName" + level;
            aCmd.CommandText += " AND M" + level;
            aCmd.CommandText += ".Value = @Value" + level;

            aCmd.Parameters.Add(new MySqlParameter("TagNs" + level, current.Tag.Ns));
            aCmd.Parameters.Add(new MySqlParameter("TagName" + level, current.Tag.Name));
            aCmd.Parameters.Add(new MySqlParameter("Value" + level, current.Value));
        }

        public IList<IList<string>> SearchItems(string aSearch, uint aRandom, IList<ITag> aShow)
        {
            MySqlCommand cmd = new MySqlCommand();

            cmd.Connection = iConnection;

            cmd.Parameters.Add(new MySqlParameter("Search", aSearch));

            cmd.CommandText += "SELECT DISTINCT ";

            int l = 0;

            foreach (ITag show in aShow)
            {
                string level = l.ToString();

                if (l != 0)
                {
                    cmd.CommandText += ", ";
                }

                cmd.CommandText += "(SELECT M" + level;
                cmd.CommandText += ".Value FROM Metadatum M" + level;
                cmd.CommandText += " INNER JOIN Tag T" + level;
                cmd.CommandText += " ON M" + level;
                cmd.CommandText += ".TagId = T" + level;
                cmd.CommandText += ".TagId AND T" + level;
                cmd.CommandText += ".TagNs = @TagNs" + level;
                cmd.CommandText += " AND T" + level;
                cmd.CommandText += ".TagName = @TagName" + level;
                cmd.CommandText += " WHERE Y.ItemId = M" + level;
                cmd.CommandText += ".ItemId LIMIT 1) S" + level;

                cmd.Parameters.Add(new MySqlParameter("TagNs" + level, show.Ns));
                cmd.Parameters.Add(new MySqlParameter("TagName" + level, show.Name));

                l++;
            }

            cmd.CommandText += " FROM (";

            cmd.CommandText += "SELECT ItemId FROM Search S WHERE MATCH(S.Value) AGAINST(@Search IN BOOLEAN MODE)";

            if (aRandom != 0)
            {
                cmd.CommandText += " ORDER BY RAND() LIMIT " + aRandom.ToString();
            }

            cmd.CommandText += ") Y ORDER BY ";

            l = 0;

            foreach (ITag show in aShow)
            {
                string level = l.ToString();

                if (l != 0)
                {
                    cmd.CommandText += ", ";
                }

                cmd.CommandText += "S" + level;

                l++;
            }

            cmd.CommandText += ";";

            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();

                List<IList<string>> result = new List<IList<string>>();

                while (reader.Read())
                {
                    List<string> row = new List<string>();

                    l = 0;

                    foreach (ITag show in aShow)
                    {
                        if (reader.IsDBNull(l))
                        {
                            row.Add(String.Empty);
                        }
                        else
                        {
                            row.Add(reader.GetString(l));
                        }

                        l++;
                    }

                    result.Add(row);
                }

                reader.Close();
                return (result);
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        public IList<IList<string>> QueryItems(IList<IMetadatum> aWhere, uint aRandom, IList<ITag> aShow)
        {
            MySqlCommand cmd = new MySqlCommand();

            cmd.Connection = iConnection;

            IEnumerator<IMetadatum> wheres = aWhere.Reversed().GetEnumerator();

            cmd.CommandText += "SELECT DISTINCT ";

            int l = 0;

            foreach (ITag show in aShow)
            {
                string level = l.ToString();

                if (l != 0)
                {
                    cmd.CommandText += ", ";
                }

                if (show.Ns == "db")
                {
                    if (show.Name == "ItemUri" || show.Name == "MountId")
                    {
                        cmd.CommandText += "(SELECT I" + level;
                        cmd.CommandText += "." + show.Name;
                        cmd.CommandText += " FROM Item I" + level;
                        cmd.CommandText += " WHERE Y.ItemId = I" + level;
                        cmd.CommandText += ".ItemId) S" + level;
                    }
                    else
                    {
                        cmd.CommandText += "'' S" + level;
                    }
                }
                else
                {
                    cmd.CommandText += "(SELECT M" + level;
                    cmd.CommandText += ".Value FROM Metadatum M" + level;
                    cmd.CommandText += " INNER JOIN Tag T" + level;
                    cmd.CommandText += " ON M" + level;
                    cmd.CommandText += ".TagId = T" + level;
                    cmd.CommandText += ".TagId AND T" + level;
                    cmd.CommandText += ".TagNs = @TagNs" + level;
                    cmd.CommandText += " AND T" + level;
                    cmd.CommandText += ".TagName = @TagName" + level;
                    cmd.CommandText += " WHERE Y.ItemId = M" + level;
                    cmd.CommandText += ".ItemId LIMIT 1) S" + level;

                    cmd.Parameters.Add(new MySqlParameter("TagNs" + level, show.Ns));
                    cmd.Parameters.Add(new MySqlParameter("TagName" + level, show.Name));
                }

                l++;
            }

            cmd.CommandText += " FROM (";

            if (aRandom != 0)
            {
                AddRandom(cmd, wheres, l, aRandom);
            }
            else if (wheres.MoveNext())
            {
                AddWhere(cmd, wheres, l);
            }
            else
            {
                cmd.CommandText += "SELECT ItemId FROM Item";
            }

            cmd.CommandText += ") Y ORDER BY ";

            l = 0;
            
            foreach (ITag show in aShow)
            {
                string level = l.ToString();

                if (l != 0)
                {
                    cmd.CommandText += ", ";
                }

                cmd.CommandText += "S" + level;
                cmd.CommandText += " + 0, S" + level;

                l++;
            }

            cmd.CommandText += ";";

            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();

                List<IList<string>> result = new List<IList<string>>();

                while (reader.Read())
                {
                    List<string> row = new List<string>();

                    l = 0;

                    foreach (ITag show in aShow)
                    {
                        if (reader.IsDBNull(l))
                        {
                            row.Add(String.Empty);
                        }
                        else
                        {
                            row.Add(reader.GetString(l));
                        }

                        l++;
                    }

                    result.Add(row);
                }

                reader.Close();
                return (result);
            }
            catch (MySqlException)
            {
                throw (new MountNotFoundError());
            }
        }

        private MySqlConnection iConnection;
    }
}
