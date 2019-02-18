using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

//we need these to talk to mysql
using MySql.Data;
using MySql.Data.MySqlClient;
//and we need this to manipulate data from a db
using System.Data;

namespace accountmanager
{
	/// <summary>
	/// Summary description for AccountServices
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class AccountServices : System.Web.Services.WebService
	{

		[WebMethod(EnableSession = true)]
		public int NumberOfAccounts()
		{
            if (Convert.ToInt32(Session["admin"]) == 1)
            {
                //here we are grabbing that connection string from our web.config file
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //here's our query.  A basic select with nothing fancy.
                string sqlSelect = "SELECT * from accounts";



                //set up our connection object to be ready to use our connection string
                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                //set up our command object to use our connection, and our query
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);


                //a data adapter acts like a bridge between our command object and 
                //the data we are trying to get back and put in a table object
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //here's the table we want to fill with the results from our query
                DataTable sqlDt = new DataTable();
                //here we go filling it!
                sqlDa.Fill(sqlDt);
                //return the number of rows we have, that's how many accounts are in the system!
                return sqlDt.Rows.Count;
            }
            else
            {
                // just returns -1 if the person is not an admin 
                return -1;
            }
            
		}
        [WebMethod(EnableSession = true)]
        public bool LogOn(string uid, string pass)
        {
            // Login: pass the parameters into the db to see if an account
            // with these credentials exists. If it does return true. If it
            // doesn't, then return false

            // We return this flag to tell them if they logged in or not
            bool success = false;

            // our connection string comes from our web.config file 
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            // here is our query. Basic select with nothing fancy
            string sqlSelect = "SELECT id FROM accounts WHERE userid=@idValue and pass=@passValue";

            // set up connection object
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            // set up out command object to use connection and query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            // tell our command to replace the @parameters with real values
            // we decode them because they came to us via the web so they were encoded
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            // a data adapter acts like a bridege between our command object and the
            // data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            // heres the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            // here we go filling it!
            sqlDa.Fill(sqlDt);
            // check to see if any rows were returned. If they were it means it's
            // a legit account
            if(sqlDt.Rows.Count > 0)
            {
                Session["id"] = sqlDt.Rows[0]["id"];
                Session["admin"] = sqlDt.Rows[0]["admin"];
                success = true; // user is in the db
            }
            // return result
            return success;
        }
        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            // if they log off we dont do much
            // later we can use this method to "forget"
            // this user at the server level
            Session.Abandon();
            return true;
        }
        [WebMethod(EnableSession = true)]
        public void RequestAccount(string uid, string pass, string firstName, string lastName, string email)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            // the only thing fancy about this query is SELECT LAST_INSERT_ID() 
            // at the end. All that does is tell mySql server to return the PK
            // of the last inserted row.
            string sqlSelect = "insert into accounts (userid, pass, firstname, lastname, email) " +
                "values(@idValue, @passValue, @fnameValue, @lnameValue, @emailValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
            sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));

            // this time were not using a data adapter to fill a data table. we're just
            // opening the connection, 'executescalar' getting back the ID and closing connection
            sqlConnection.Open();

            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
            }
            catch(Exception e)
            {
            }
            sqlConnection.Close();
        }
        [WebMethod(EnableSession = true)]
        public Account[] GetAccounts()
        {
            //check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
            //just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
            //sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
            //Keeps everything simple.

            //WE ONLY SHARE ACCOUNTS WITH LOGGED IN USERS!
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("accounts");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select id, userid, pass, firstname, lastname, email from accounts where active=1 order by lastname";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Account.  Fill each acciount with
                //data from the rows, then dump them in a list.
                List<Account> accounts = new List<Account>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    //only share user id and pass info with admins!
                    if (Convert.ToInt32(Session["admin"]) == 1)
                    {
                        accounts.Add(new Account
                        {
                            id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                            userId = sqlDt.Rows[i]["userid"].ToString(),
                            password = sqlDt.Rows[i]["pass"].ToString(),
                            firstName = sqlDt.Rows[i]["firstname"].ToString(),
                            lastName = sqlDt.Rows[i]["lastname"].ToString(),
                            email = sqlDt.Rows[i]["email"].ToString()
                        });
                    }
                    else
                    {
                        accounts.Add(new Account
                        {
                            id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                            firstName = sqlDt.Rows[i]["firstname"].ToString(),
                            lastName = sqlDt.Rows[i]["lastname"].ToString(),
                            email = sqlDt.Rows[i]["email"].ToString()
                        });
                    }
                }
                //convert the list of accounts to an array and return!
                return accounts.ToArray();
            }
            else
            {
                //if they're not logged in, return an empty array
                return new Account[0];
            }
        }
        [WebMethod(EnableSession = true)]
        public void UpdateAccount(string id, string uid, string pass, string firstName, string lastName, string email)
        {
            //WRAPPING THE WHOLE THING IN AN IF STATEMENT TO CHECK IF THEY ARE AN ADMIN!
            if (Convert.ToInt32(Session["admin"]) == 1)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //this is a simple update, with parameters to pass in values
                string sqlSelect = "update accounts set userid=@uidValue, pass=@passValue, firstname=@fnameValue, lastname=@lnameValue, " +
                    "email=@emailValue where id=@idValue";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@uidValue", HttpUtility.UrlDecode(uid));
                sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
                sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
                sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
                sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
                sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

                sqlConnection.Open();
                //we're using a try/catch so that if the query errors out we can handle it gracefully
                //by closing the connection and moving on
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
                sqlConnection.Close();
            }
        }
        [WebMethod]
        public Account[] GetAccountRequests()
        { // get all account requests and return them
            DataTable sqlDt = new DataTable("accountrequests");
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            // requests have active set to 0
            string sqlSelect = "select id, userid, pass, firstname, lastname, email from accounts where active=0 order by lastname";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Account> accountRequests = new List<Account>();
            for(int i = 0; i < sqlDt.Rows.Count; i++)
            {
                accountRequests.Add(new Account
                {
                    id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                    firstName = sqlDt.Rows[i]["firstname"].ToString(),
                    lastName = sqlDt.Rows[i]["lastname"].ToString(),
                    email = sqlDt.Rows[i]["email"].ToString()
                });
            }
            // convert to array
            return accountRequests.ToArray();
        }
        [WebMethod]
        public void DeleteAccount(string id)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "delete from accounts where id=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }
        [WebMethod]
        public void RejectAccount(string id)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            // sample update
            string sqlSelect = "delete from account where id=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }
        [WebMethod]
        public void ActivateAccount(string id)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "update account set active=1 where id=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }
    }
}
