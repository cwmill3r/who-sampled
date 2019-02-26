using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace accountmanager
{
    public class Account
    {
        // just a container for info related to account
        public int id;
        public bool loggedIn = false;
        public int admin = 0;
        public string userId;
        public string password;
        public string firstName;
        public string lastName;
        public string email;
    }
}