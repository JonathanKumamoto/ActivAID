﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    // port: 49172
    // login user name: sa
    // password: activaid
    class ActivAIDDB
    {
        private SqlConnection conn;
        private string dblocation;
        
        // private int elementCounter;
        public ActivAIDDB()
        {
            string dbName = "ActivAID DB";
            //string serverName = Environment.GetEnvironmentVariable("SERVER");
            string serverName = "localhost\\SQLEXPRESS";
            dblocation = "Server="+serverName+";Database=" + dbName + ";Integrated Security=true";          
        }

        public void insertIntoFiles(string filepath)// string keywords)
        {
            using (conn = new SqlConnection(dblocation))
            {
                string fileQuery = "INSERT INTO Files (filePath, filename) VALUES (@file, @filename)";
                SqlCommand cmd = new SqlCommand(fileQuery, conn);
                cmd.Parameters.AddWithValue("@file", filepath);
                string filename = System.IO.Path.GetFileName(filepath);
                cmd.Parameters.AddWithValue("@filename", filename);
                //cmd.Parameters.AddWithValue("@key", keywords);
                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();

            }
        }

        public void insertIntoHyperlinks(string parentpath, string text, string filepath)
        {
            int parentId = GetFileId(parentpath);
            if (parentId > 0)
            {

                using (conn = new SqlConnection(dblocation))
                {
                    string hyperQuery = "INSERT INTO Hyperlinks (fileId, filePath, filename, text) VALUES (@id, @path, @fname, @text)";
                    SqlCommand cmd = new SqlCommand(hyperQuery, conn);
                    cmd.Parameters.AddWithValue("@id", parentId);
                    cmd.Parameters.AddWithValue("@path", filepath);
                    string fname = System.IO.Path.GetFileName(filepath);
                    cmd.Parameters.AddWithValue("@fname", fname);
                    cmd.Parameters.AddWithValue("@text", text);
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }

        public void insertIntoElements(string parentpath, int block, string data)
        {
            // bug here, check what the parentpath is and how a parentId is determined
            // incElementCounter();
            int parentId = GetFileId(parentpath);
            Console.WriteLine(parentId);
            if (parentId > 0)
            {
                using (conn = new SqlConnection(dblocation))
                {
                    string elementQuery = "INSERT INTO Elements (fileId, blockNumber, data) VALUES (@id, @block, @dat)";
                    SqlCommand cmd = new SqlCommand(elementQuery, conn);
                    cmd.Parameters.AddWithValue("@id", parentId);
                    cmd.Parameters.AddWithValue("@block", block);
                    cmd.Parameters.AddWithValue("@dat", data);
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }

        public void insertIntoImages(int elid, string elpath)
        {
            using (conn = new SqlConnection(dblocation))
            {
                string imageQuery = "INSERT INTO Images (elementId, elementImg) VALUES (@id, @path)";
                SqlCommand cmd = new SqlCommand(imageQuery, conn);
                cmd.Parameters.AddWithValue("@id", elid);
                cmd.Parameters.AddWithValue("@path", elpath);
                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();

            }
        }

        public int queryFileId(string filename)
        {
            int fileid;
            using (conn = new SqlConnection(dblocation))
            {
                string getid = "SELECT fileId FROM Files WHERE filename=@fname";
                SqlCommand cmd = new SqlCommand(getid, conn);
                cmd.Parameters.AddWithValue("@fname", filename);
                conn.Open();
                using (SqlDataReader freader = cmd.ExecuteReader())
                {
                    freader.Read();
                    fileid = freader.GetInt32(0);
                }
            }
            return fileid;
        }

        public int queryFileId(string[] keywords)
        {
            List<int> matchingIds = new List<int>();

            using (conn = new SqlConnection(dblocation))
            {
                conn.Open();
                string getid = "SELECT fileId FROM Files WHERE keyWords LIKE '%' + @key + '%'";
                foreach (string key in keywords)
                {
                    SqlCommand cmd = new SqlCommand(getid, conn);
                    cmd.Parameters.AddWithValue("@key", key);
                    using (SqlDataReader freader = cmd.ExecuteReader())
                    {
                        freader.Read();
                        matchingIds.Add(freader.GetInt32(0));
                    }
                }
            }
            return MostCommon(matchingIds);
        }

        public Dictionary<int, List<string>> getAllElements(string filepath)
        {
            Dictionary<int, List<string>> elementList = new Dictionary<int, List<string>>();

            int fileid = GetFileId(filepath);
            using (conn = new SqlConnection(dblocation))
            {
                string getElements = "SELECT blockNumber, data FROM Elements WHERE fileId=@id";
                SqlCommand cmd = new SqlCommand(getElements, conn);
                cmd.Parameters.AddWithValue("@id", fileid);
                conn.Open();
                using (SqlDataReader eReader = cmd.ExecuteReader())
                {
                    while (eReader.Read())
                    {
                        int blocknum = Convert.ToInt32(eReader["blockNumber"].ToString());
                        string data = eReader["data"].ToString();
                        if (elementList.ContainsKey(blocknum))
                        {
                            elementList[blocknum].Add(data);
                        }
                        else
                        {
                            List<string> block = new List<string>();
                            block.Add(data);
                            elementList.Add(blocknum, block);
                        }
                    }
                }
            }
            return elementList;
        }

        public Tuple<string,string>[] getHyperlinks(string filepath)
        {

            List<Tuple<string,string>> hyperlist = new List<Tuple<string,string>>();
            int fileid = GetFileId(filepath);
            using (conn = new SqlConnection(dblocation))
            {
                string getHyperlinks = "SELECT text, filePath FROM Hyperlinks WHERE fileId=@id";
                SqlCommand cmd = new SqlCommand(getHyperlinks, conn);
                cmd.Parameters.AddWithValue("@id", fileid);
                conn.Open();
                using (SqlDataReader hReader = cmd.ExecuteReader())
                {
                    while (hReader.Read())
                    {
                        string text = hReader["text"].ToString();
                        string link = hReader["filePath"].ToString();                        
                        hyperlist.Add(new Tuple<string, string>(text, link));
                    }
                }
            }
            Tuple<string,string>[] hrefs = hyperlist.ToArray() as Tuple<string,string>[];
            return hrefs;
        }

        // Utility Methods
        private int GetFileId(string filepath)
        {
            // SOMETHING WRONG WITH THE PARSER, ITS MAKING FILEPATHS OUT OF THE FILE DESCRIPTIONS
            int fileid;
            using (conn = new SqlConnection(dblocation))
            {
                //string getid = "SELECT fileId FROM Files WHERE filePath=@path";
                Console.WriteLine(filepath);
                string getid = "SELECT fileId FROM Files WHERE filename LIKE '%' + @fname + '%'";
                SqlCommand cmd = new SqlCommand(getid, conn);
                cmd.Parameters.AddWithValue("@path", "'" + filepath.Replace(@"\", @"\\") + "'");
                //int start = filepath.Length - 15;
                //int end = 8;
                string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
                cmd.Parameters.AddWithValue("@fname", filename);
                conn.Open();
                //Console.WriteLine(filepath.Substring(start, end));
                //Console.WriteLine(filepath);
                using (SqlDataReader fReader = cmd.ExecuteReader())
                {
                    /*
                    fReader.Read();
                    Console.WriteLine(fReader[0]);
                    fileid = fReader.GetInt32(0);
                    */

                    if (fReader.Read())
                    {
                        fileid = fReader.GetInt32(0);
                        return fileid;
                    }
                    else
                    {
                        fileid = -1;
                    }

                    //fileid = -1;
                }

            }
            return fileid;
        }

        public int MostCommon(List<int> list)
        {
            return (from i in list
                    group i by i into grp
                    orderby grp.Count() descending
                    select grp.Key).First();
        }

        /*
        private int getElementId()
        {
            returns the element counter once
            a boolean flag indicating it is an image element is 
            detected. Boolean flag would be passed from the Parser.
        }
        private void incElementCounter()
        {
            increments the element counter according to how
            each element id is incremented in the database table.
            Called everytime an element is added into the database.
        }
                
        */
    }
}