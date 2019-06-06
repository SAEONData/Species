using portal;
using species;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace species
{
    public partial class ajax : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String mode = Request["mode"];

            switch (mode)
            {
                case "login":
                    login();
                    return;
                case "sign_out":
                    sign_out();
                    return;
                case "testmail":
                    testmail();
                    return;
                case "loadspecies":
                    loadspecies();
                    return;
                case "loadspeciesNames":
                    loadspeciesNames();
                    return;
                case "loadcruises":
                    loadcruises();
                    return;
                case "loadtransects":
                    loadtransects();
                    return;
                case "reqpassreset":
                    reqpassreset();
                    return;
                case "resetpsw":
                    resetpsw();
                    return;

            }

            Response.Write("Invalid mode: " + mode);

        }

        void login()
        {
            String query = "SELECT * FROM WebUser WHERE EmailAddress = '~usr~' AND Password = '~psw~'";
            query = query.Replace("~usr~", Request["usr"].Replace("'", "''"));
            query = query.Replace("~psw~", Request["psw"].Replace("'", "''"));
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            Session["userID"] = set["WebUserID"];
                            writeSuccess("ok");
                        }
                        else
                        {
                            writeError("Invalid user name or password");
                        }
                    }
                }
            }
        }

        void sign_out()
        {
            Session["userID"] = null;
            Session["ADUNumber"] = null;
            Session["Administrator"] = null;

        }

        void loadspecies(String mode = "list")
        {
            List<int> filteredSpecies = new List<int>();
            String filter = "";

            // surveys
            if (Request["items"] != "")
            {
                filter += "(1 = 0";
                String[] surveys = Request["items"].Split(',');
                foreach (String t in surveys)
                {
                    filter += " OR fSurveyID = " + int.Parse(t);
                }
                filter += ")";
            }

            // stations
            if (Request["stations"] != "")
            {
                if (filter != "")
                    filter += " AND ";
                filter += "(1 = 0";
                String[] stations = Request["stations"].Split(',');
                foreach (String t in stations)
                {
                    filter += " OR fStationID = " + int.Parse(t);
                }
                filter += ")";
            }

            if (filter != "")
            {
                String[] species = Request["species"].Split(',');
                foreach (String s in species)
                {
                    if (s.Trim() != "")
                    {
                        int fSpeciesID = int.Parse(s);
                        if (filteredSpecies.Contains(fSpeciesID) == false)
                            filteredSpecies.Add(fSpeciesID);
                    }
                }
            }
            else
            {
                filter = "1 = 0";
                String[] species = Request["species"].Split(',');
                foreach (String s in species)
                {
                    if (s.Trim() != "")
                    {
                        filter += " OR fSpeciesID = " + int.Parse(s);
                    }
                }
            }



            // List<DataPoint> points = new List<DataPoint>();
            Dictionary<int, DataPoint> data = new Dictionary<int, DataPoint>();

            String query = "SELECT * FROM vwSpeciesEvent WHERE fStartLat IS NOT NULL AND fStartLng IS NOT NULL";
            if (filter != "")
                query += " AND (" + filter + ")";

            query += filtertools.BuildFilter(Context);
            query += " ORDER BY fCommonName";



                


            Random rnd = new Random();

            if (mode == "list")
            {

                using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader set = command.ExecuteReader())
                        {
                            while (set.Read())
                            {
                                int species = (int)set["fSpeciesID"];

                                int fEventID = (int)set["fEventID"];
                                if (data.ContainsKey(fEventID) == false)
                                    data[fEventID] = new DataPoint();
                                DataPoint point = data[fEventID];
                                point.shape = "point";
                                point.id = fEventID;
                                point.transect = fEventID;
                                point.x = (double)set["fStartLng"];
                                point.y = (double)set["fStartLat"];

                                point.x1 = point.x;
                                point.y1 = point.y;

                                if (set["fEndLng"] != DBNull.Value)
                                    point.x2 = (double)set["fEndLng"];
                                if (set["fEndLat"] != DBNull.Value)
                                    point.y2 = (double)set["fEndLat"];

                                int fSpeciesID = (int)set["fSpeciesID"];
                                if (filteredSpecies.Count == 0 ||  filteredSpecies.Contains(fSpeciesID))
                                {
                                    DataObservation obs = new DataObservation();
                                    obs.sname = (string)set["fCommonName"].ToString();
                                    obs.id = (int)set["fSpeciesEventID"];
                                    obs.species = (int)set["fSpeciesID"];
                                    obs.sitting = (int)set["fOccurance"];
                                    obs.flying = (int)0;
                                    obs.color = (string)set["fBoxColor"];
                                    point.obs.Add(obs);
                                }
                            }
                        }
                    }
                }

                List<DataPoint> list = new List<DataPoint>();
                foreach (int k in data.Keys)
                    list.Add(data[k]);

                list.Sort();


                JavaScriptSerializer js = new JavaScriptSerializer();
                Response.Write(js.Serialize(list));
            }
        }

        void loadspeciesNames()
        {
            List<speciesItem> sitems = new List<speciesItem>();

            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                String[] species = Request["species"].Split(',');
                foreach (String s in species)
                {
                    if (s.Trim() != "")
                    {
                        String query = "SELECT * FROM Species WHERE fSpeciesID = " + int.Parse(s);
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader set = command.ExecuteReader())
                            {
                                if (set.Read())
                                {
                                    speciesItem item = new speciesItem();
                                    item.fSpeciesID = (int)set["fSpeciesID"];
                                    item.commonName = set["English_Species"].ToString() + " " + set["English_Genus"].ToString();
                                    item.scientName = set["Taxonomic_Species"].ToString() + " " + set["Taxonomic_Genus"].ToString();
                                    sitems.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            Response.Write(js.Serialize(sitems));
        }

        void loadcruises()
        {
            int vessel = int.Parse(Request["vessel"]);
            String query = "SELECT * FROM TblSurvey ORDER BY fSurveyLabel";
            List<comboItem> items = new List<comboItem>();
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            comboItem item = new comboItem();
                            item.id = (int)set["fSurveyID"];
                            item.name = (string)set["fSurveyLabel"];
                            items.Add(item);
                        }
                    }
                }
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            Response.Write(js.Serialize(items));
        }

        void loadtransects()
        {
            int cruise = int.Parse(Request["cruise"]);
            String query = "SELECT * FROM Transect WHERE CruiseID = " + cruise + "ORDER BY TransectRef";
            List<comboItem> items = new List<comboItem>();
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        while (set.Read())
                        {
                            comboItem item = new comboItem();
                            item.id = (int)set["fEventID"];
                            item.name = (string)set["TransectRef"];
                            items.Add(item);
                        }
                    }
                }
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            Response.Write(js.Serialize(items));
        }

        protected void writeError(String msg)
        {
            jsresponse resp = new jsresponse();
            resp.add("success", false);
            resp.add("message", msg);
            Response.Write(resp.write());
        }

        protected void writeSuccess(String msg)
        {
            jsresponse resp = new jsresponse();
            resp.add("success", true);
            resp.add("message", msg);
            Response.Write(resp.write());
        }

        protected void testmail()
        {
            var fromAddress = new MailAddress("seabirdsbla@gmail.com", "AS@S");
            var toAddress = new MailAddress("johan@softwave.co.za");
            const string fromPassword = "Genes11s";
            const string subject = "Test mail";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            var message = new MailMessage(fromAddress, toAddress);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = "Hello World!";
            smtp.Send(message);

            Response.End();
        }

        void sendPasswordResetMain(String mail, String guid)
        {
            String url = "http://app01.saeon.ac.za/dev/bla/pswreset.aspx?reqid=" + Server.UrlEncode(guid);

            String body = "Someone requested that the password to your account be reset.<br><br>";
            body += "If this was not you, you may safely ignore this mail. If you wish to <bt>";
            body += "proceed and reset your password, please visit this address:<br><br>";
            body += "<a href='" + url + "'>" + url + "</a><br><br>";
            body += "The AS@S Team";


            var fromAddress = new MailAddress("seabirdsbla@gmail.com", "AS@S");
            var toAddress = new MailAddress(mail);
            const string fromPassword = "Genes11s";
            const string subject = "Password reset request";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            var message = new MailMessage(fromAddress, toAddress);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;
            smtp.Send(message);

            Response.Write("Your password reset request has been mailed. It should arrive in your mailbox momentarily. When you receive the message, visit the address it contains to reset your password.");
        }

        void reqpassreset()
        {
            String email = Request["email"];
            String query = "SELECT guid FROM WebUser WHERE EmailAddress = '~usr~'";
            query = query.Replace("~usr~", Request["email"].Replace("'", "''"));
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            String guid = set["guid"].ToString();
                            sendPasswordResetMain(email, guid);
                        }
                        else
                        {
                            Response.Write("Your email address was not found");
                            return;
                        }
                    }
                }
            }
        }

        public string CalculateMD5Hash(string input)

        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString().ToLower();
        }


        void resetpsw()
        {
            String mail = Request["memberId"];
            String guid = Request["reqid"];
            String pasw = Request["passwd"];
            String query = "SELECT WebUserID FROM WebUser WHERE EmailAddress = '~mail~' AND guid = '~guid~'";
            query = query.Replace("~mail~", mail.Replace("'", "''"));
            query = query.Replace("~guid~", guid.Replace("'", "''"));
            using (SqlConnection connection = new SqlConnection(DataSources.dbConSpecies))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader set = command.ExecuteReader())
                    {
                        if (set.Read())
                        {
                            int userID = (int)set["WebUserID"];
                            set.Close();
                            String sql = "UPDATE WebUser SET Password = '~psw~' WHERE WebUserID = " + userID;
                            sql = sql.Replace("~psw~", CalculateMD5Hash(pasw));
                            using (SqlCommand cmd = new SqlCommand(sql, connection))
                            {
                                cmd.ExecuteNonQuery();
                                Response.Write("Your password have been successfully updated.");
                            }
                        }
                        else
                        {
                            Response.Write("Your email address or authentication token does not match");
                            return;
                        }
                    }
                }
            }


        }
    }

    class comboItem
    {
        public int id;
        public string name;
    }

    class speciesItem
    {
        public int fSpeciesID;
        public string commonName;
        public string scientName;
    }


}