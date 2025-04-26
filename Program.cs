using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace MiniBankSystem_1
{
    internal class Program
    {
        // File path 
        const string filePath = "accountInfo.txt";


        // Queue to hold account requests
        static Queue<string> accountRequests = new Queue<string>();

        // List to hold all accounts
        static List<int> accountNumbers = new List<int>();
        static List<string> accountName = new List<string>();
        static List<double> accountBalance = new List<double>();


        static int lastAccountNumber; // To keep track of the last account number assigned
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
            bool inUserMenu = true; // to keep the user in the menu until they choose to exit
            while (inUserMenu)
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

                switch (userChoice)
                {
                    case "1": CreateAccount(); break;
                    case "2": Deposit(); break;
                    case "3": Withdraw(); break;
                    case "4": CheckBalance(); break;
                    //case "5": SubmitReview(); break;
                    case "0":
                        inUserMenu = false;
                        break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
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

            //create Queue
            string request = $"{name},{nationalID}";

            // add new entry to the queue
            accountRequests.Enqueue(request);

            Console.WriteLine("Account request submitted successfully.");




        }

        static void ProcessNextAccountRequest()
        {
            // Check if there are any pending requests

            if (accountRequests.Count == 0) //if the queue is empty
            {
                Console.WriteLine("No pending account requests.");
                return;
            }
            // Dequeue the next request from the queue(request)
            string request = accountRequests.Dequeue();
            string[] strings = request.Split(','); // split the string dpending on the ,
            string name = strings[0]; // save the name.
            string nationalID = strings[1]; // save the national ID

            // Process the request ( create an account)>>>>>>>

            //To ensure that every account created receives a unique and sequential number.

            int newAccountNumber = lastAccountNumber + 1;
            accountNumbers.Add(newAccountNumber); // add the new account number to the list of account numbers
            accountName.Add(name); // add the name to the list of account names
            accountBalance.Add(0.0); // add the new account number to the list of balances with a default balance of 0
            lastAccountNumber = newAccountNumber; // update the last account number

            Console.WriteLine("Account created for :" + name, "with Account Number :" + newAccountNumber);

        }

        static void Deposit()
        {
            int index = GetAccountIndex();
            if (index == -1) return; // if the account number is not found, return

            try
            {
                Console.WriteLine("\n====== Deposit ======");
                Console.Write("Enter amount to deposit: ");
                double amount = double.Parse(Console.ReadLine());
                if (amount <= 0)
                {
                    Console.WriteLine("Invalid amount. Please enter a positive number.");
                    return;
                }
                accountBalance[index] += amount; // add the amount to the account balance
                Console.WriteLine($"Deposited {amount} to account number {accountNumbers[index]}.");

            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }


        }
        static int GetAccountIndex()
        {
            Console.Write("Enter account number: ");
            try
            {
                int accNum = Convert.ToInt32(Console.ReadLine());
                int index = accountNumbers.IndexOf(accNum);

                if (index == -1)
                {
                    Console.WriteLine("Account not found.");
                    return -1;
                }

                return index;
            }
            catch
            {
                Console.WriteLine("Invalid input.");
                return -1;
            }
        }

        static void Withdraw()
        {
            int index = GetAccountIndex();
            if (index == -1) return; // if the account number is not found, return
            try
            {
                Console.WriteLine("\n====== Withdraw ======");
                Console.Write("Enter amount to withdraw: ");
                double amount = double.Parse(Console.ReadLine());
                if (amount <= 0)
                {
                    Console.WriteLine("Invalid amount. Please enter a positive number.");
                    return;
                }
                if (accountBalance[index] < amount)// check if the balance is Insufficient

                {
                    Console.WriteLine("Insufficient balance.");
                    return;
                }
                accountBalance[index] -= amount; // subtract the amount from the account balance
                Console.WriteLine($"Withdrew {amount} from account number {accountNumbers[index]}.");
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }

        static void SaveAccountInformationToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    for (int i = 0; i < accountNumbers.Count; i++)
                    {
                        string dataLine = ($"{accountNumbers[i]},{accountName[i]},{accountBalance[i]}");
                        writer.WriteLine(dataLine);
                    }
                }
                Console.WriteLine("Account information saved to file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving account information: ");

            }
        }

        static void LoadAccountInformationFromFile()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] data = line.Split(',');
                            accountNumbers.Add(int.Parse(data[0]));
                            accountName.Add(data[1]);
                            accountBalance.Add(double.Parse(data[2]));
                        }
                    }
                    Console.WriteLine("Account information loaded from file.");
                }
                else
                {
                    Console.WriteLine("No saved account information found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading account information: " + ex.Message);
            }
        }

        static void CheckBalance()
        {
            int index = GetAccountIndex();
            if (index == -1) return; // if the account number is not found, return
            Console.WriteLine($"Account Number: {accountNumbers[index]}");
            Console.WriteLine($"Account Name: {accountName[index]}");
            Console.WriteLine($"Account Balance: {accountBalance[index]}");
        }

        static void ViewAllAccounts()
        {
            Console.Clear();
            Console.WriteLine("\n====== All Accounts ======");
            for (int i = 0; i < accountNumbers.Count; i++)
            {
                Console.WriteLine($"Account Number: {accountNumbers[i]}, Name: {accountName[i]}, Balance: {accountBalance[i]}");
            }
        }

        static void ViewPendingRequests()
        {
            Console.Clear();
            Console.WriteLine("\n====== Pending Account Requests ======");
            if (accountRequests.Count == 0) //if the queue is empty
            {
                Console.WriteLine("No pending account requests.");
                return;
            }
            foreach (string request in accountRequests)
            {
                string[] strings = request.Split(",");

                Console.WriteLine($"Name: {strings[0]}, National ID: {strings[1]}");

            }
        }


    }

}
  

