using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;

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

        //EXAMPLE OF A SIMPLE SELECT QUERY (PARAMETERS PASSED IN FROM CLIENT)
        [WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
        public Account LogOn(string uid, string pass)
        {
            // we want to return some account info if they can login
            Account loginInfo = new Account();
            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            //NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
            string sqlSelect = "SELECT * FROM accounts WHERE userid=@idValue and pass=@passValue";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //if we found an account, store the id and admin status in the session
                //so we can check those values later on other method calls to see if they 
                //are 1) logged in at all, and 2) and admin or not
                Session["id"] = sqlDt.Rows[0]["id"];
                Session["admin"] = sqlDt.Rows[0]["admin"];
                // make the object we want to return
                loginInfo.id = Convert.ToInt32(sqlDt.Rows[0]["id"]);
                loginInfo.admin = Convert.ToInt32(sqlDt.Rows[0]["admin"]);
                loginInfo.loggedIn = true;
                loginInfo.userId = sqlDt.Rows[0]["userid"].ToString(); // aka username
                loginInfo.firstName = sqlDt.Rows[0]["firstname"].ToString();
                loginInfo.lastName = sqlDt.Rows[0]["lastname"].ToString();
                loginInfo.email = sqlDt.Rows[0]["email"].ToString();
                
            }
            return loginInfo;
            //return the result!
            //var serializer = new JavaScriptSerializer();
            //var serializedResult = serializer.Serialize(loginInfo);
            //return serializedResult;

        }

        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            //if they log off, then we remove the session.  That way, if they access
            //again later they have to log back on in order for their ID to be back
            //in the session!
            Session.Abandon();
            return true;
        }

        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        [WebMethod(EnableSession = true)]
        public void RequestAccount(string uid, string pass, string firstName, string lastName, string email)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into accounts (userid, pass, firstname, lastname, email) " +
                "values(@idValue, @passValue, @fnameValue, @lnameValue, @emailValue); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
            sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }

        //EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
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

        //EXAMPLE OF AN UPDATE QUERY WITH PARAMS PASSED IN
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
        //EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
        [WebMethod(EnableSession = true)]
        public Account[] GetAccountRequests()
        {//LOGIC: get all account requests and return them!
            if (Convert.ToInt32(Session["admin"]) == 1)
            {
                DataTable sqlDt = new DataTable("accountrequests");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //requests just have active set to 0
                string sqlSelect = "select id, userid, pass, firstname, lastname, email from accounts where active=0 order by lastname";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                sqlDa.Fill(sqlDt);

                List<Account> accountRequests = new List<Account>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    accountRequests.Add(new Account
                    {
                        id = Convert.ToInt32(sqlDt.Rows[i]["id"]),
                        firstName = sqlDt.Rows[i]["firstname"].ToString(),
                        lastName = sqlDt.Rows[i]["lastname"].ToString(),
                        email = sqlDt.Rows[i]["email"].ToString()
                    });
                }
                //convert the list of accounts to an array and return!
                return accountRequests.ToArray();
            }
            else
            {
                return new Account[0];
            }
        }

        //EXAMPLE OF A DELETE QUERY
        [WebMethod(EnableSession = true)]
        public void DeleteAccount(string id)
        {
            if (Convert.ToInt32(Session["admin"]) == 1)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //this is a simple update, with parameters to pass in values
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
        }

        //EXAMPLE OF AN UPDATE QUERY
        [WebMethod(EnableSession = true)]
        public void ActivateAccount(string id)
        {
            if (Convert.ToInt32(Session["admin"]) == 1)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //this is a simple update, with parameters to pass in values
                string sqlSelect = "update accounts set active=1 where id=@idValue";

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

        //EXAMPLE OF A DELETE QUERY
        [WebMethod(EnableSession = true)]
        public void RejectAccount(string id)
        {
            if (Convert.ToInt32(Session["admin"]) == 1)
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
        }

        //Our main service to get our questions out of the db
        [WebMethod(EnableSession = true)]
        public Question[] GetAllQuestions()
        {
            //LOGIC: get all questions without their answers first
            DataTable sqlDt = new DataTable("questions");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //select all the questions *this wont iclude wrong answers*
            string sqlSelect = "select * from questions";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            DataTable sqlDtWrong = new DataTable("wrongAnswers");
            // Now add the wrong answers
            string sqlSelectWrong = "select q.questionId, w.wrongAnswerText from questions q, wrong_answers w where q.questionId = w.questionId";
            MySqlCommand sqlCommandWrong = new MySqlCommand(sqlSelectWrong, sqlConnection);
            MySqlDataAdapter sqlDaWrong = new MySqlDataAdapter(sqlCommandWrong);
            sqlDaWrong.Fill(sqlDtWrong);

            List<Question> tmpQuestions = new List<Question>();

            // get all the questions without the wrong answers
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                tmpQuestions.Add(new Question
                {
                    questionId = Convert.ToInt32(sqlDt.Rows[i]["questionId"]),
                    creatorId = Convert.ToInt32(sqlDt.Rows[i]["creatorId"]),
                    questionText = sqlDt.Rows[i]["questionText"].ToString(),
                    videoId = sqlDt.Rows[i]["videoId"].ToString(),
                    correctAnswerText = sqlDt.Rows[i]["correctAnswerText"].ToString()
                });
            }
            // get all the wrong answers
            List<WrongAnswer> wrongAnswersList = new List<WrongAnswer>();
            for (int i = 0; i < sqlDtWrong.Rows.Count; i++)
            {
                wrongAnswersList.Add(new WrongAnswer
                {
                    questionId = Convert.ToInt32(sqlDtWrong.Rows[i]["questionId"]),
                    wrongAnswerText = sqlDtWrong.Rows[i]["wrongAnswerText"].ToString()
                });
            }
            // call the helper method to combine
            List<Question> questionsWithWrongAnswers = CombineQuestions(tmpQuestions, wrongAnswersList);

            //convert the list of accounts to an array and return!
            return questionsWithWrongAnswers.ToArray();
        }   

        // combine questions with wrong answers helper function
        public List<Question> CombineQuestions(List<Question> questionList,  List<WrongAnswer> wrongAnswersList)
        {
            for(int i = 0; i < questionList.Count; i++) 
            {
                int tempId = questionList[i].questionId;
                Question tmpQuestion = new Question();
                List<WrongAnswer> tmpList = new List<WrongAnswer>();

                for(int j = 0; j < wrongAnswersList.Count; j++)
                {
                    if(tempId == wrongAnswersList[j].questionId)
                    {
                        tmpList.Add(new WrongAnswer {
                            questionId = wrongAnswersList[j].questionId,
                            wrongAnswerText = wrongAnswersList[j].wrongAnswerText
                        });
                    }
                }
                questionList[i].wrongAnswers = tmpList;
            }
            return questionList;
        }
        //Service to get the questions a user created by user id
        [WebMethod(EnableSession = true)]
        public Question[] GetUserCreatedQuestions(string id)
        {
            //LOGIC: get all questions without their answers first
            DataTable sqlDt = new DataTable("questions");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //select all the questions *this wont iclude wrong answers*
            string sqlSelect = "select * from questions where creatorId=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            DataTable sqlDtWrong = new DataTable("wrongAnswers");
            // Now add the wrong answers
            string sqlSelectWrong =
                "select q.questionId, w.wrongAnswerText from questions q, wrong_answers w where q.questionId = w.questionId";

            MySqlCommand sqlCommandWrong = new MySqlCommand(sqlSelectWrong, sqlConnection);
            sqlCommandWrong.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));
            MySqlDataAdapter sqlDaWrong = new MySqlDataAdapter(sqlCommandWrong);
            sqlDaWrong.Fill(sqlDtWrong);

            List<Question> tmpQuestions = new List<Question>();

            // get all the questions without the wrong answers
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                tmpQuestions.Add(new Question
                {
                    questionId = Convert.ToInt32(sqlDt.Rows[i]["questionId"]),
                    creatorId = Convert.ToInt32(sqlDt.Rows[i]["creatorId"]),
                    questionText = sqlDt.Rows[i]["questionText"].ToString(),
                    videoId = sqlDt.Rows[i]["videoId"].ToString(),
                    correctAnswerText = sqlDt.Rows[i]["correctAnswerText"].ToString()
                });
            }
            // get all the wrong answers
            List<WrongAnswer> wrongAnswersList = new List<WrongAnswer>();
            for (int i = 0; i < sqlDtWrong.Rows.Count; i++)
            {
                wrongAnswersList.Add(new WrongAnswer
                {
                    questionId = Convert.ToInt32(sqlDtWrong.Rows[i]["questionId"]),
                    wrongAnswerText = sqlDtWrong.Rows[i]["wrongAnswerText"].ToString()
                });
            }
            // call the helper method to combine
            List<Question> questionsWithWrongAnswers = CombineQuestions(tmpQuestions, wrongAnswersList);

            // convert do I have to convert these to array or anything?
            // it seems to be okay with me just returning a C# object
            return questionsWithWrongAnswers.ToArray();
        }
        [WebMethod(EnableSession = true)]
        public bool DeleteQuestion(int questionid) 
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //this is a simple update, with parameters to pass in values
            string sqlSelect = "DELETE FROM questions WHERE questionid = @questionid;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@questionid", questionid);

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
            }
            sqlConnection.Close();
            return true;
        }
        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        [WebMethod(EnableSession = true)]
        public int CreateQuestion(int creatorId, string questionText, string sampleTrackName, string sampleArtistName, string sampleYouTubeLink, string songArtistName, string songTitle, string wrongAnswer1, string wrongAnswer2, string wrongAnswer3)
        {
            int questionId = -1;
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string correctAnswerText = $"{songTitle} by {songArtistName}";
            string sqlSelect = $"insert into questions (creatorId, questionText, videoId, correctAnswerText) values({creatorId}, @questionText, @videoId, @correctAnswerText); SELECT LAST_INSERT_ID();";
            Console.WriteLine(sqlSelect);
            // add creatorId
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@creatorId", creatorId);
            sqlCommand.Parameters.AddWithValue("@questionText", HttpUtility.UrlDecode(questionText));
            sqlCommand.Parameters.AddWithValue("@videoId", HttpUtility.UrlDecode(sampleYouTubeLink));
            sqlCommand.Parameters.AddWithValue("@correctAnswerText", HttpUtility.UrlDecode(correctAnswerText));
             
            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                questionId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {
                return questionId; // will return -1
            }

            // add the wrong answers to their table
            if (questionId != -1)
            {
                //string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
                //does is tell mySql server to return the primary key of the last inserted row.
                string sqlSelectWrongAnswers = $"START TRANSACTION;" +
                    $"INSERT into wrong_answers (wrongAnswerText, questionId ) values(@wrongAnswer1, {questionId});" +
                    $"INSERT into wrong_answers (wrongAnswerText, questionId ) values(@wrongAnswer2, {questionId});" +
                    $"INSERT into wrong_answers (wrongAnswerText, questionId ) values(@wrongAnswer3, {questionId});" +
                    $"COMMIT;);" +
                    $"SELECT LAST_INSERT_ID();";
                // add creatorId
                //MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommandWrongAnswers = new MySqlCommand(sqlSelectWrongAnswers, sqlConnection);

                //sqlCommand.Parameters.AddWithValue("@creatorId", creatorId);
                sqlCommandWrongAnswers.Parameters.AddWithValue("@wrongAnswer1", HttpUtility.UrlDecode(wrongAnswer1));
                sqlCommandWrongAnswers.Parameters.AddWithValue("@wrongAnswer2", HttpUtility.UrlDecode(wrongAnswer2));
                sqlCommandWrongAnswers.Parameters.AddWithValue("@wrongAnswer3", HttpUtility.UrlDecode(wrongAnswer3));

                //this time, we're not using a data adapter to fill a data table.  We're just
                //opening the connection, telling our command to "executescalar" which says basically
                //execute the query and just hand me back the number the query returns (the ID, remember?).
                //don't forget to close the connection!
                //sqlConnection.Open();
                //we're using a try/catch so that if the query errors out we can handle it gracefully
                //by closing the connection and moving on
                try
                {
                    questionId = Convert.ToInt32(sqlCommandWrongAnswers.ExecuteScalar());
                    //here, you could use this accountID for additional queries regarding
                    //the requested account.  Really this is just an example to show you
                    //a query where you get the primary key of the inserted row back from
                    //the database!
                    sqlConnection.Close();
                    return questionId;
                }
                catch (Exception e)
                {
                    sqlConnection.Close();
                    return questionId; // will return -1
                }
            }
            return questionId;
        }
        [WebMethod(EnableSession = true)]

        public string EditQuestion(int questionId, string questionText, string sampleYouTubeLink, string correctAnswerText,  string wrongAnswer1, string wrongAnswer2, string wrongAnswer3)
        {
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            
            string sqlSelect = $"update questions set questionText=@questionText, videoId=@videoId, correctAnswerText=@correctAnswerText where questionId=@questionId";
            
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@questionId", questionId);
            sqlCommand.Parameters.AddWithValue("@questionText", HttpUtility.UrlDecode(questionText));
            sqlCommand.Parameters.AddWithValue("@videoId", HttpUtility.UrlDecode(sampleYouTubeLink));
            sqlCommand.Parameters.AddWithValue("@correctAnswerText", HttpUtility.UrlDecode(correctAnswerText));
            
            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "failed at first try catch";
            }

            // We have to delete the wrong answers and start over
            // then insert the changed ones
            string sqlSelectWrongAnswers = "START TRANSACTION; DELETE from wrong_answers where questionId = @questionId; INSERT into wrong_answers(wrongAnswerText, questionId) values(@wrongAnswer1, @questionId); INSERT into wrong_answers(wrongAnswerText, questionId) values(@wrongAnswer2, @questionId); INSERT into wrong_answers(wrongAnswerText, questionId) values(@wrongAnswer3, @questionId); COMMIT;";

            //MySqlConnection sqlConnectionWrong = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommandWrongAnswers = new MySqlCommand(sqlSelectWrongAnswers, sqlConnection);

            sqlCommandWrongAnswers.Parameters.AddWithValue("@questionId", questionId);
            sqlCommandWrongAnswers.Parameters.AddWithValue("@wrongAnswer1", HttpUtility.UrlDecode(wrongAnswer1));
            sqlCommandWrongAnswers.Parameters.AddWithValue("@wrongAnswer2", HttpUtility.UrlDecode(wrongAnswer2));
            sqlCommandWrongAnswers.Parameters.AddWithValue("@wrongAnswer3", HttpUtility.UrlDecode(wrongAnswer3));

            try
            {
                sqlCommandWrongAnswers.ExecuteScalar();

                sqlConnection.Close();
                return "I returned from the sweet spot";
            }
            catch (Exception e)
            {
                sqlConnection.Close();
                return "I failed at the second try catch";
            }

        }
    }
}

