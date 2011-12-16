using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Simple.Data;

namespace SimpleData.Console
{
    class Program
    {
        protected const string connectionStringName = "SimpleDataSpike";
        protected static dynamic db;
        protected static List<User> users;
        
        static void Main(string[] args)
        {
            var bootStrapper = new SqlCeBootStrapper(connectionStringName);
            bootStrapper.CreateDBFile();
            bootStrapper.CreateSchema();
            
            db = Database.OpenNamedConnection(connectionStringName);

            db.Users.DeleteAll();

            //PrepareUsers(1000,true);
            PrepareUsersWithTransaction(10,true,false);

            FindOneUserWithoutPoco();

            FindAllUsersWithoutPoco();

            FindAllUsersWithPoco();

            bootStrapper.DeleteDBFile();

            System.Console.WriteLine();
            System.Console.WriteLine("Press the any key to continue");
            System.Console.ReadLine();
        }

        private static void PrepareUsers(int userCreationCount, bool isBulkInsert)
        {
            var usersList = Builder<User>.CreateListOfSize(userCreationCount)
                .All()
                    .With(x=>x.Id = Guid.NewGuid())
                .Build();
            
            users = usersList.ToList();

            if(isBulkInsert)
                db.Users.Insert(users);
            else
                users.ForEach(x=> db.Users.Insert(x));
        }

        private static void PrepareUsersWithTransaction(int userCreationCount, bool isBulkInsert, bool fail)
        {
            var usersList = Builder<User>.CreateListOfSize(userCreationCount)
                .All()
                    .With(x => x.Id = Guid.NewGuid())
                .Build();

            users = usersList.ToList();

            if (fail)
            {
                var failUser = users.Last();
                users.Add(failUser);
            }

            using (var tx = db.BeginTransaction())
            {
                try
                {
                    if(isBulkInsert)
                        tx.Users.Insert(users);
                    else
                    {
                        users.ForEach(x => tx.Users.Insert(x));        
                    }
                    
                    tx.Commit();
                }
                catch(Exception e)
                {
                    System.Console.WriteLine("Exception: {0}", e.Message);
                }
            }
        }
        
        private static void FindOneUserWithoutPoco()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("************************************************************");
            System.Console.WriteLine();
            
            var theUser = db.Users.FindById(users.First().Id);

            if (theUser == null)
            {
                System.Console.WriteLine("No Records Returned!");
            }
            else
            {
                System.Console.WriteLine("UserName: {0}, Email: {1}, FirstName: {2}, LastName: {3}",
                theUser.UserName, theUser.EmailAddress, theUser.FirstName, theUser.LastName);
            }

            System.Console.WriteLine();
            System.Console.WriteLine("************************************************************");
            System.Console.WriteLine();    
        }

        private static void FindAllUsersWithPoco()
        {
            IList<User> theUsers = db.Users.All().ToList<User>();

            if (theUsers == null)
            {
                System.Console.WriteLine("No Records Returned!");
            }
            else
            {
                theUsers.ToList().ForEach(user => System.Console.WriteLine("UserName: {0}, Email: {1}, FirstName: {2}, LastName: {3}",
                user.UserName, user.EmailAddress, user.FirstName, user.LastName));

            }
            System.Console.WriteLine();
            System.Console.WriteLine("************************************************************");
            System.Console.WriteLine();
        }

        private static void FindAllUsersWithoutPoco()
        {
            var theUsers = db.Users.All();

            if (theUsers == null)
            {
                System.Console.WriteLine("No Records Returned!");
            }
            else
            {

                foreach (var user in theUsers)
                {
                    System.Console.WriteLine("UserName: {0}, Email: {1}, FirstName: {2}, LastName: {3}",
                        user.UserName, user.EmailAddress, user.FirstName, user.LastName);
                }
            }
            System.Console.WriteLine();
            System.Console.WriteLine("************************************************************");
            System.Console.WriteLine();
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
