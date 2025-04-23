using System;
using System.Collections.Generic;
using System.IO;

namespace MiniBankSystem_1
{
    internal class Program
    {
        // Queue to hold account requests
        static Queue<string> accountRequests = new Queue<string>();
        static void Main()
        {

            // main menu
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("\n====== SafeBank System Main Menu ======");
                Console.WriteLine("1. User Menu");
                Console.WriteLine("2. Admin Menu");
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");
                string mainChoice = Console.ReadLine();

                switch (mainChoice)
                {
                    case "1": UserMenu(); break;
                    case "2": AdminMenu(); break;
                    case "0":

                        running = false;
                        break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }

            Console.WriteLine("Thank you for using SafeBank!");
        }

        // ===========User Menu===========
        static void UserMenu()
        {
            Console.Clear();
            Console.WriteLine("\n====== User Menu ======");
            Console.WriteLine("1. Create Account");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4. Check Balance");
            Console.WriteLine("5. Submit Review/Complaint");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Select option: ");
            string userChoice = Console.ReadLine();

            //switch (userChoice)
            //{
            //    case "1": CreateAccount(); break;
            //    case "2": Deposit(); break;
            //    case "3": Withdraw(); break;
            //    case "4": CheckBalance(); break;
            //    case "5": SubmitReview(); break;
            //    case "0": break;
            //    default: Console.WriteLine("Invalid choice."); break;
            //}
        }



        // ===========Admin Menu============

        static void AdminMenu()
        {
            Console.Clear();
            Console.WriteLine("\n====== Admin Menu ======");
            Console.WriteLine("1. Process Next Account Request");
            Console.WriteLine("2. View Submitted Reviews");
            Console.WriteLine("3. View All Accounts");
            Console.WriteLine("4. View Pending Account Requests");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Select option: ");
            string adminChoice = Console.ReadLine();

            //switch (adminChoice)
            //{
            //    case "1": ProcessNextAccountRequest(); break;
            //    case "2": ViewReviews(); break;
            //    case "3": ViewAllAccounts(); break;
            //    case "4": ViewPendingRequests(); break;
            //    case "0": inAdminMenu = false; break;
            //    default: Console.WriteLine("Invalid choice."); break;
            //}
        }

        ////////////////////////////////////////////////////////////////////

        static void CreateAccount()
        {
            Console.Clear();
            Console.WriteLine("\n====== Create Account ======");
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();
            Console.Write("Enter your National ID: ");
            string nationalID = (Console.ReadLine());

            //To save name & ID in Queue
            string request = $"{name},{nationalID}";

            // add new entry to the queue
            accountRequests.Enqueue(request);

            Console.WriteLine("Account request submitted successfully.");




        }

    }

}
