using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;   // needed for .Any()

//-----Seconde part 
using System.Security.Cryptography;   //  hashing
using System.Text;                    //  string > bytes


namespace MiniBankSystem_1
{

    struct Tx          // simple record
    {
        public int Acc;
        public DateTime When;
        public string Kind;   // DEPOSIT / WITHDRAW / TRANSFER-IN / TRANSFER-OUT
        public double Amount;
        public double Balance;
    }
    internal class Program
    {
        // File path 
        const string filePath = "accountInfo.txt";
        const string transFile = "transactions.txt";


        // --- Admin credentials -------------------------------------------------
        const string ADMIN_ID = "admin";
        const string ADMIN_PASS_HASH = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9"; // ← HashPassword("admin123")



        // Queue to hold account requests
        static Queue<string> accountRequests = new Queue<string>();
        static Stack<string> Reviews = new Stack<string>();  // Stack to hold reviews/complaints

        // List to hold all accounts
        static List<int> accountNumbers = new List<int>();
        static List<string> accountName = new List<string>();
        static List<double> accountBalance = new List<double>();
        static List<string> accountNationalIDs = new List<string>();   // NEW
        static List<Tx> txLog = new List<Tx>();
        // List for loan
        static List<bool> hasActiveLoan = new List<bool>();  // track if user has a loan
        static List<(double Amount, double Interest)> loanRequests = new List<(double, double)>(); // one per account

        //------Part 2
        static List<string> accountPasswordHashes = new List<string>();   //  stores SHA-256 hex


        static int lastAccountNumber; // To keep track of the last account number assigned
        static int currentAccountIndex = -1;   // −1 means “not logged in”

        static void Main()
        {
            LoadAccountInformationFromFile();
            LoadReviewsFromFile();// Load reviews from file at the start of the program
            lastAccountNumber = GetTheLastAccountNumberFromAccountFile(); // get the last account number from the file
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
                        SaveAccountInformationToFile();
                        SaveReviewsToFile();// Save reviews to file before exiting


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
                Console.WriteLine("2. Login");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select option: ");
                string userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1": CreateAccount(); Console.ReadLine(); break;
                    case "2":                           // Admin Menu
                        if (AdminLogin())               // ← ask for credentials first
                            AdminMenu();                // open menu only on success
                        break;

                    //case "2":
                    //    Login();
                    //    // Require login first
                    //    if (currentAccountIndex == -1 && !Login())
                    //    { return; }                // user failed / aborted
                    //    else
                    //    {
                    //        // If login is successful, show user operations menu
                    //        UserOperations(); // Call the user operations method to show the user menu
                    //    }
                    //    break;
                    case "0":
                        inUserMenu = false;
                        Logout();               // end session
                        break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        // User operation 
        static void UserOperations()
        {
            // Require login first
            if (currentAccountIndex == -1 && !Login())
                return;                     // user failed / aborted
            bool inUserMenu = true; // to keep the user in the menu until they choose to exit
            while (inUserMenu)
            {

                Console.Clear();
                Console.WriteLine("\n====== User Menu ======");
                Console.WriteLine("1. Deposit");
                Console.WriteLine("2. Withdraw");
                Console.WriteLine("3. Check Balance");
                Console.WriteLine("4. Submit Review/Complaint");
                Console.WriteLine("5. Transfer Funds");
                Console.WriteLine("6. Undo Last Complaint");
                Console.WriteLine("7. Request Monthly Statement");
                Console.WriteLine("8. Request Loan");
                Console.WriteLine("9. Show Last N Transactions");
                Console.WriteLine("10. Show Transactions After Date");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select option: ");
                string userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1": Deposit(); Console.ReadLine(); break;
                    case "2": Withdraw(); Console.ReadLine(); break;
                    case "3": CheckBalance(); Console.ReadLine(); break;
                    case "4": submitReview(); Console.ReadLine(); break;
                    case "5": TransferFunds(); Console.ReadLine(); break;
                    case "6": UndoLastComplaint(); Console.ReadLine(); break;
                    case "7": MonthlyStatement(); Console.ReadLine(); break;
                    case "8": RequestLoan(); Console.ReadLine(); break;
                    case "9": ShowLastNTransactions(); Console.ReadLine(); break;
                    case "10": ShowTransactionsAfterDate(); Console.ReadLine(); break;


                    case "0":
                        inUserMenu = false;
                        Logout();               // end session
                        break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        // ===========Admin Menu============

        static void AdminMenu()
        {
            bool inAdminMenu = true;
            while (inAdminMenu)
            {
                Console.Clear();
                Console.WriteLine("\n====== Admin Menu ======");
                Console.WriteLine("1. Process Next Account Request");
                Console.WriteLine("2. View Submitted Reviews");
                Console.WriteLine("3. View All Accounts");
                Console.WriteLine("4. View Pending Account Requests");
                Console.WriteLine("5. Search by Name or National ID");
                Console.WriteLine("6. Delete Account by Number");
                Console.WriteLine("7. Show Top 3 Richest Customers");
                Console.WriteLine("8. Show Total Bank Balance");
                Console.WriteLine("9. Export All Account Info to CSV");
                Console.WriteLine("10. Approve/Reject Loan Requests");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select option: ");
                string adminChoice = Console.ReadLine();

                switch (adminChoice)
                {
                    case "1": ProcessNextAccountRequest(); Console.ReadLine(); break;
                    case "2": ViewReviews(); Console.ReadLine(); break;
                    case "3": ViewAllAccounts(); Console.ReadLine(); break;
                    case "4": ViewPendingRequests(); break;
                    case "5": SearchAccountByNameOrID(); break;
                    case "6": DeleteAccountByNumber(); Console.ReadLine(); break;
                    case "7": ShowTopRichestCustomers(); Console.ReadLine(); break;
                    case "8": ShowTotalBankBalance(); Console.ReadLine(); break;
                    case "9": ExportAccountsToCsv(); Console.ReadLine(); break;
                    case "10":ApproveRejectLoans(); Console.ReadLine(); break;


                        Console.ReadLine();
                        break;
                    case "0": inAdminMenu = false; break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////

        static void CreateAccount()

        {
           
            
                Console.Clear();
                Console.WriteLine("\n====== Create Account ======");

                // ► 1. Name input & validation
                Console.Write("Enter your name: ");
                string name = Console.ReadLine();
                while (string.IsNullOrWhiteSpace(name) || name.Any(char.IsDigit))
                {
                    Console.Write("Invalid name. Please re-enter: ");
                    name = Console.ReadLine();
                }

                // ► 2. National-ID input & validation
                Console.Write("Enter your National ID (8 digits): ");
                string nationalID = Console.ReadLine();
                while (string.IsNullOrWhiteSpace(nationalID) ||
                       nationalID.Length != 8 ||
                       !nationalID.All(char.IsDigit))
                {
                    Console.Write("Invalid ID. It must be exactly 8 digits. Re-enter: ");
                    nationalID = Console.ReadLine();
                }

                // ► 3. DUPLICATE-CHECK  
                // 3a) already approved?
                if (accountNationalIDs.Contains(nationalID))
                {
                    Console.WriteLine(" An account with this National ID already exists.");
                    Console.ReadKey();
                    return;
                }

                // 3b) already pending in the queue?
                bool pending = accountRequests.Any(r => r.Split(',')[1] == nationalID);
                if (pending)
                {
                    Console.WriteLine("  A request with this National ID is already in the queue.");
                    Console.ReadKey();
                    return;
                }

            // ► 4. Password input
            Console.Write("Set account password: ");
            string pw1 = ReadPasswordMasked();
            Console.Write("Confirm password      : ");
            string pw2 = ReadPasswordMasked();

            if (pw1 != pw2 || pw1.Length < 4)
            {
                Console.WriteLine(" Passwords don’t match or too short (min 4).");
                Console.ReadKey();
                return;
            }

            string hash = HashPassword(pw1);


            // ► 5. Enqueue request (include hash)
            accountRequests.Enqueue($"{name},{nationalID},{hash}");
            Console.WriteLine(" Account request submitted successfully.");
            Console.ReadKey();



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
            string[] parts = request.Split(','); // split the string 
            string name = parts[0]; // save the name.
            string nationalID = parts[1]; // save the national ID
            string hash = parts[2];     // hash 

            accountPasswordHashes.Add(hash);  
 

            // Process the request ( create an account)>>>>>>>

            //To ensure that every account created receives a unique and sequential number.


            int newAccountNumber = lastAccountNumber + 1;
            accountNationalIDs.Add(nationalID);   // NEW: track the ID
            accountNumbers.Add(newAccountNumber); // add the new account number to the list of account numbers
            accountName.Add(name); // add the name to the list of account names
            accountBalance.Add(0.0); // add the new account number to the list of balances with a default balance of 0
            hasActiveLoan.Add(false); // no loan at creation
            loanRequests.Add((0, 0)); // empty request
            lastAccountNumber = newAccountNumber; // update the last account number
            
           
            Console.WriteLine($"Account created for :  {name}  with Account Number :  {newAccountNumber}  and national ID : {nationalID}");

            
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
                

                accountBalance[index] += amount;      // update
                LogTx(accountNumbers[index], "DEPOSIT", amount, accountBalance[index]);   // for statement

                PrintReceipt("DEPOSIT", amount, accountNumbers[index], accountBalance[index]);


            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }


        }
        static int GetAccountIndex()
        {
            Console.Write("Enter Account number: ");
            try
            {
                int accNum = Convert.ToInt32(Console.ReadLine());
                int index = accountNumbers.IndexOf(accNum);

                if (index == -1)
                {
                    Console.WriteLine("Account not found.");
                    return index;
                }

                return index;
            }
            catch
            {
                Console.WriteLine("Invalid input.");
               
            }
            return -1; // return -1 if the account number is not found
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
                
                accountBalance[index] -= amount;      // update
                LogTx(accountNumbers[index], "WITHDRAW", amount, accountBalance[index]);  // for Statemen

                PrintReceipt("WITHDRAW", amount, accountNumbers[index], accountBalance[index]);

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
                        string dataLine = $"{accountNumbers[i]},{accountName[i]},{accountBalance[i]}," + $"{accountNationalIDs[i]},{accountPasswordHashes[i]}";    // secure


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
                            int accountNumber = Convert.ToInt32(data[0]);
                            accountNumbers.Add(int.Parse(data[0]));
                            accountName.Add(data[1]);
                            accountBalance.Add(double.Parse(data[2]));
                            accountNationalIDs.Add(data[3]);   // NEW: load the national ID
                            accountPasswordHashes.Add(data[4]);          //To secure

                            
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
            foreach (string request in accountRequests) //starts looking for each request one by one in accountRequests, line by line
            {
                string[] strings = request.Split(","); //To split the string

                Console.WriteLine($"Name: {strings[0]}, National ID: {strings[1]}");

            }
        }

        //=============================


        static void submitReview()
        {
            Console.Clear();
            Console.WriteLine("\n====== Submit Review/Complaint ======");
            Console.Write("Enter your review/complaint: ");
            string review = Console.ReadLine();
            Reviews.Push(review);
            // Save the review to a file or process it as needed
            // For simplicity, we'll just print it to the console
            Console.WriteLine("Review submitted successfully.");

        }

        static void ViewReviews()
        {
            Console.Clear();

            if (Reviews.Count == 0)
            {
                Console.WriteLine("No reviews submitted yet.");
                return;
            }

            Console.WriteLine("\n====== Submitted Reviews ======");
            foreach (string request in Reviews)
            {
                Console.WriteLine("-" + request);
            }


        }

        static void SaveReviewsToFile()
        // Save the reviews to a file
        // For simplicity, we'll just print it to the console
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("reviews.txt"))
                {
                    foreach (string review in Reviews)
                    {
                        writer.WriteLine(review);
                    }
                }
                Console.WriteLine("Reviews saved to file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving reviews: " + ex.Message);
            }
        }

        static void LoadReviewsFromFile()
        {
            try
            {
                if (File.Exists("reviews.txt"))  //if the file exists
                {
                    using (StreamReader reader = new StreamReader("reviews.txt"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null) 
                        {
                            Reviews.Push(line);
                        }
                    }
                    Console.WriteLine("Reviews loaded from file.");
                    return;
                }

                else
                {   
                        Console.WriteLine("No saved reviews found.");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading reviews: " + ex.Message);
            }



        }

        static int GetTheLastAccountNumberFromAccountFile()
        {
            if (!File.Exists(filePath))
                return 0;

            string lastLine = File.ReadLines(filePath).LastOrDefault();

            if (string.IsNullOrEmpty(lastLine))
                return 0;

            string[] parts = lastLine.Split(',');

            if (parts.Length > 0 && int.TryParse(parts[0], out int lastAcc))
                return lastAcc;

            return 0;
        }

        static bool Login()
        {
            Console.Clear();
            Console.WriteLine("====== Login ======");
            Console.Write("National ID (8 digits): ");
            string id = Console.ReadLine()?.Trim();

            int idx = accountNationalIDs.IndexOf(id);
            if (idx == -1)
            {
                Console.WriteLine(" ID not found or not yet approved.");
                Console.ReadKey();
                return false;
            }

            Console.Write("Password: ");
            string pw = ReadPasswordMasked();
            if (accountPasswordHashes[idx] != HashPassword(pw))
            {
                Console.WriteLine(" Incorrect password.");
                Console.ReadKey();
                return false;
            }

            currentAccountIndex = idx;
            Console.WriteLine($"Welcome, {accountName[idx]}!");
            Console.ReadKey();
            return true;

        }

        static void Logout()
        {
            currentAccountIndex = -1;
        }

        static void SearchAccountByNameOrID()
        {
            Console.Clear();
            Console.WriteLine("\n====== Search Account ======");
            Console.Write("Search by (1) Name or (2) National ID: ");
            string choice = Console.ReadLine();
            bool found = false;

            if (choice == "1")
            {
                Console.Write("Enter Name: ");
                string name = Console.ReadLine();
                for (int i = 0; i < accountName.Count; i++)
                {
                    if (accountName[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Account Number: {accountNumbers[i]}");
                        Console.WriteLine($"Balance: {accountBalance[i]}");
                        Console.WriteLine($"National ID: {accountNationalIDs[i]}"); // Display the National ID

                        found = true;
                        break; // Exit after finding the first match
                        
                    }
                   
                }
                if (!found)
                {
                    Console.WriteLine("Account not found with the given name.");
                }
                Console.ReadLine();


            }
            else if (choice == "2")
            {
                Console.Write("Enter National ID (8 digits): ");
                string id = Console.ReadLine();

                for (int i = 0; i < accountNationalIDs.Count; i++)
                {
                    if (accountNationalIDs[i].Equals(id, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Account Number: {accountNumbers[i]}");
                        Console.WriteLine($"Name: {accountName[i]}");
                        Console.WriteLine($"Balance: {accountBalance[i]}");
                        found = true;
                        break; // Exit after finding the first match

                    }

                }
                if (!found)
                {
                    Console.WriteLine("Account not found with the given name.");
                }
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        static void DeleteAccountByNumber()
        {
            Console.Clear();
            Console.WriteLine("\n====== Delete Account ======");
            Console.Write("Enter the account number to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int accNo))
            {
                Console.WriteLine("Invalid number.");
                return;
            }

            int index = accountNumbers.IndexOf(accNo);
            if (index == -1)
            {
                Console.WriteLine("Account not found.");
                return;
            }

            // Safety prompt
            Console.Write($"Are you sure you want to delete account {accNo}? (y/n): ");
            string confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm != "y") { Console.WriteLine("Deletion cancelled."); return; }

            // Remove from every parallel list
            accountNumbers.RemoveAt(index);
            accountName.RemoveAt(index);
            accountBalance.RemoveAt(index);
            accountNationalIDs.RemoveAt(index);
            accountPasswordHashes.RemoveAt(index);   // Secure


            // Adjust currentAccountIndex if needed
            if (currentAccountIndex == index) currentAccountIndex = -1;
            else if (currentAccountIndex > index) currentAccountIndex--;

            Console.WriteLine($"Account {accNo} deleted successfully.");
        }

        static void TransferFunds()
        {
            // Sender must already be logged-in
            if (currentAccountIndex == -1 && !Login())
                return;

            int senderIndex = currentAccountIndex;
            int senderAcc = accountNumbers[senderIndex];

            Console.Clear();
            Console.WriteLine("\n====== Transfer Funds ======");
            Console.WriteLine($"Sender Account: {senderAcc}  |  Balance: {accountBalance[senderIndex]:C}");
            Console.Write("Enter recipient account number: ");

            if (!int.TryParse(Console.ReadLine(), out int recipientAcc))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }

            int recipIndex = accountNumbers.IndexOf(recipientAcc);
            if (recipIndex == -1)
            {
                Console.WriteLine("Recipient account not found.");
                return;
            }

            if (recipIndex == senderIndex)
            {
                Console.WriteLine("You cannot transfer to the same account.");
                return;
            }

            Console.Write("Enter amount to transfer: ");
            if (!double.TryParse(Console.ReadLine(), out double amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                return;
            }

            if (accountBalance[senderIndex] < amount)
            {
                Console.WriteLine("Insufficient balance. Transfer aborted.");
                return;
            }

            // --- Perform transfer ---
            accountBalance[senderIndex] -= amount;
            accountBalance[recipIndex] += amount;

            LogTx(senderAcc, "TRANSFER-OUT", amount, accountBalance[senderIndex]);
            LogTx(recipientAcc, "TRANSFER-IN", amount, accountBalance[recipIndex]);

            Console.WriteLine($"\n  {amount:C} transferred from {senderAcc} to {recipientAcc}.");
            Console.WriteLine($"New Sender Balance: {accountBalance[senderIndex]:C}");
        }

        static void ShowTopRichestCustomers()
        {
            Console.Clear();
            Console.WriteLine("\n====== Top 3 Richest Customers ======");

            if (accountBalance.Count == 0)
            {
                Console.WriteLine("No accounts available.");
                return;
            }

            // Build a list of anonymous objects (Balance + Index) and sort descending
            var top = accountBalance
                        .Select((bal, idx) => new { Balance = bal, Index = idx })
                        .OrderByDescending(x => x.Balance)
                        .Take(3)
                        .ToList();

            int rank = 1;
            foreach (var item in top)
            {
                int i = item.Index;
                Console.WriteLine($"{rank}. {accountName[i]}  |  Acc #{accountNumbers[i]}  |  Balance: {accountBalance[i]:C}");
                rank++;
            }
        }

        static void ShowTotalBankBalance()
        {
            Console.Clear();
            Console.WriteLine("\n====== Total Bank Balance ======");

            if (accountBalance.Count == 0)
            {
                Console.WriteLine("No accounts available.");
                return;
            }

            double total = accountBalance.Sum();   // using System.Linq;
            Console.WriteLine($"Total Holdings Across All Accounts: {total:C}");
        }

        static void ExportAccountsToCsv()
        {
            const string exportPath = "SafeBank_Accounts_Export.csv";
            Console.Clear();
            Console.WriteLine("\n====== Export Accounts to CSV ======");

            if (accountNumbers.Count == 0)
            {
                Console.WriteLine("No accounts to export.");
                return;
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(exportPath, false)) // overwrite each time
                {
                    // Header row
                    writer.WriteLine("AccountNumber,Name,Balance,NationalID,PasswordHash");

                    // Data rows
                    for (int i = 0; i < accountNumbers.Count; i++)
                    {
                        // Escape commas in names if needed by wrapping with quotes
                        string safeName = accountName[i].Contains(',')
                                          ? $"\"{accountName[i]}\""
                                          : accountName[i];

                        writer.WriteLine($"{accountNumbers[i]},{safeName},{accountBalance[i]},{accountNationalIDs[i]}");
                    }
                }

                Console.WriteLine($" Accounts exported to \"{exportPath}\".");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Error exporting accounts: " + ex.Message);
            }
        }

        static void UndoLastComplaint()
        {
            Console.Clear();
            Console.WriteLine("\n====== Undo Last Complaint ======");

            if (Reviews.Count == 0)
            {
                Console.WriteLine("There are no complaints to undo.");
                return;
            }

            Console.WriteLine("Most recent complaint:");
            Console.WriteLine($"\"{Reviews.Peek()}\"");
            Console.Write("Remove it? (y/n): ");
            string confirm = Console.ReadLine()?.Trim().ToLower();

            if (confirm == "y")
            {
                Reviews.Pop();
                Console.WriteLine(" Last complaint removed.");
            }
            else
            {
                Console.WriteLine(" Operation cancelled. Complaint kept.");
            }
        }


        static void PrintReceipt(string txType, double amount, int accNum, double newBalance)
        {
            // Timestamp (local time)
            DateTime now = DateTime.Now;
            string stamp = now.ToString("yyyy-MM-dd HH:mm:ss");
            string fileTag = now.ToString("yyyyMMdd_HHmmss");          // for filename

            // Pretty-printed receipt text
            string receipt =
              $@"----------------------------------------
               SAFE-BANK OFFICIAL RECEIPT
               ----------------------------------------
               Date / Time : {stamp}
               Transaction : {txType}
               Account No. : {accNum}
               Amount      : {amount:C}
               New Balance : {newBalance:C}
               ----------------------------------------";

            // -- Display on screen
            Console.WriteLine(receipt);

            // -- Persist to file
            string fileName = $"Receipt_{accNum}_{fileTag}.txt";
            try
            {
                File.WriteAllText(fileName, receipt);
                Console.WriteLine($"(Saved to {fileName})");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Could not write receipt file: " + ex.Message);
            }
        }




        //-------------- ((Mini Bank System Project ( Version 2.0 ) ))-----------------------

        
        //   SHA-256 hashing > 64-char lowercase hex string
        static string HashPassword(string plain)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        //   Read password masked with '*'

        static string ReadPasswordMasked()
        {
            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;                         // remove last
                    Console.Write("\b \b");              // erase star
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write('*');
                }
            }
            Console.WriteLine();
            return sb.ToString();
        }

        // ─── helper to append a log entry + file line ───
        static void LogTx(int acc, string kind, double amt, double bal)
        {
            var tx = new Tx { Acc = acc, When = DateTime.Now, Kind = kind, Amount = amt, Balance = bal };
            txLog.Add(tx);

            // CSV: acc,iso-datetime,kind,amount,balance
            string line = $"{acc},{tx.When:O},{kind},{amt},{bal}";
            File.AppendAllText(transFile, line + Environment.NewLine);
        }

        // ─── load the file on startup ───
        static void LoadTransactions()
        {
            if (!File.Exists(transFile)) return;
            foreach (var line in File.ReadAllLines(transFile))
            {
                var p = line.Split(',');
                if (p.Length != 5) continue;   

                txLog.Add(new Tx
                {
                    Acc = int.Parse(p[0]),
                    When = DateTime.Parse(p[1], null, System.Globalization.DateTimeStyles.RoundtripKind),
                    Kind = p[2],
                    Amount = double.Parse(p[3]),
                    Balance = double.Parse(p[4])
                });
            }
        }

        
        static void MonthlyStatement()
        {
            if (currentAccountIndex == -1) { Console.WriteLine("Please login first."); return; }

            int acc = accountNumbers[currentAccountIndex];

            Console.Write("\nEnter statement month (1-12): ");
            if (!int.TryParse(Console.ReadLine(), out int m) || m < 1 || m > 12)
            {
                Console.WriteLine("Bad month."); return;
            }
            Console.Write("Enter statement year (e.g. 2025): ");
            if (!int.TryParse(Console.ReadLine(), out int y) || y < 2000 || y > 2100)
            {
                Console.WriteLine("Bad year."); return;
            }

            // ― filter log
            var list = txLog.Where(t => t.Acc == acc && t.When.Month == m && t.When.Year == y)
                            .OrderBy(t => t.When)
                            .ToList();

            if (list.Count == 0)
            {
                Console.WriteLine("No transactions for that period.");
                return;
            }

            string heading = $"Statement for Account #{acc} — {y}-{m:00}";
            Console.WriteLine("\n" + heading);
            Console.WriteLine(new string('-', heading.Length));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(heading);
            sb.AppendLine("Date & Time          Type          Amount       Balance");
            sb.AppendLine("--------------------------------------------------------");

            foreach (var t in list)
            {
                string line = $"{t.When:yyyy-MM-dd HH:mm}  {t.Kind,-12}  {t.Amount,10:C}   {t.Balance,10:C}";
                Console.WriteLine(line);
                sb.AppendLine(line);
            }

            // ― save file
            string file = $"Statement_Acc{acc}_{y}-{m:00}.txt";
            File.WriteAllText(file, sb.ToString());
            Console.WriteLine($"\nStatement saved to \"{file}\"");
        }

        static void RequestLoan()
        {
            if (currentAccountIndex == -1) { Console.WriteLine("Login required."); return; }

            if (hasActiveLoan[currentAccountIndex])
            {
                Console.WriteLine("You already have an active loan.");
                return;
            }

            if (accountBalance[currentAccountIndex] < 5000)
            {
                Console.WriteLine("You must have at least 5000 balance to request a loan.");
                return;
            }

            Console.Write("Enter loan amount: ");
            if (!double.TryParse(Console.ReadLine(), out double amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                return;
            }

            Console.Write("Enter interest rate (e.g. 5 for 5%): ");
            if (!double.TryParse(Console.ReadLine(), out double rate) || rate <= 0)
            {
                Console.WriteLine("Invalid interest rate.");
                return;
            }

            loanRequests[currentAccountIndex] = (amount, rate);
            Console.WriteLine("Loan request submitted.");
        }

        static void ApproveRejectLoans()
        {
            Console.Clear();
            Console.WriteLine("\n====== Loan Approval ======");

            bool found = false;
            for (int i = 0; i < loanRequests.Count; i++)
            {
                if (loanRequests[i].Amount > 0 && !hasActiveLoan[i])
                {
                    found = true;
                    Console.WriteLine($"Account #{accountNumbers[i]} - Name: {accountName[i]}");
                    Console.WriteLine($"Requested: {loanRequests[i].Amount:C} at {loanRequests[i].Interest}%");

                    Console.Write("Approve (a) / Reject (r): ");
                    string choice = Console.ReadLine().ToLower();

                    if (choice == "a")
                    {
                        accountBalance[i] += loanRequests[i].Amount;
                        LogTx(accountNumbers[i], "LOAN-DISBURSE", loanRequests[i].Amount, accountBalance[i]);
                        hasActiveLoan[i] = true;
                        Console.WriteLine("Loan approved and amount added to balance.");
                    }
                    else
                    {
                        Console.WriteLine("Loan request rejected.");
                    }

                    loanRequests[i] = (0, 0); // reset
                   

                }
            }

            if (!found)
            {
                Console.WriteLine("No pending loan requests.");
            }
        }

        // ——— show the last N transactions for the logged-in user ———
        static void ShowLastNTransactions()
        {
            if (currentAccountIndex == -1) { Console.WriteLine("Login first."); return; }

            Console.Write("How many recent transactions? ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
            {
                Console.WriteLine("Invalid number."); return;
            }

            int acc = accountNumbers[currentAccountIndex];

            var slice = txLog
                        .Where(t => t.Acc == acc)
                        .OrderByDescending(t => t.When)
                        .Take(n)
                        .ToList();

            if (slice.Count == 0) { Console.WriteLine("No transactions found."); return; }

            Console.WriteLine("\nDate & Time              Type            Amount        Balance");
            Console.WriteLine("----------------------------------------------------------------");
            foreach (var t in slice.OrderBy(t => t.When))                    // print oldest→newest
                Console.WriteLine($"{t.When:yyyy-MM-dd HH:mm}  {t.Kind,-14}  {t.Amount,10:C}   {t.Balance,10:C}");
        }


        // ——— show all transactions after a given date ———
        static void ShowTransactionsAfterDate()
        {
            if (currentAccountIndex == -1) { Console.WriteLine("Login first."); return; }

            Console.Write("Enter date (yyyy-mm-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dt))
            {
                Console.WriteLine("Bad date."); return;
            }

            int acc = accountNumbers[currentAccountIndex];

            var list = txLog
                       .Where(t => t.Acc == acc && t.When >= dt)
                       .OrderBy(t => t.When)
                       .ToList();

            if (list.Count == 0) { Console.WriteLine("No transactions since that date."); return; }

            Console.WriteLine("\nDate & Time              Type            Amount        Balance");
            Console.WriteLine("----------------------------------------------------------------");
            foreach (var t in list)
                Console.WriteLine($"{t.When:yyyy-MM-dd HH:mm}  {t.Kind,-14}  {t.Amount,10:C}   {t.Balance,10:C}");
        }


        static bool AdminLogin()
        {
            Console.Clear();
            Console.WriteLine("====== Admin Login ======");
            Console.Write("Admin ID      : ");
            string id = Console.ReadLine()?.Trim();

            Console.Write("Admin Password: ");
            string pw = ReadPasswordMasked();

            bool ok = id == ADMIN_ID && HashPassword(pw) == ADMIN_PASS_HASH;
            Console.WriteLine(ok ? "Access granted." : "Access denied.");
            if (!ok) Console.ReadKey();
            return ok;
        }









    }

}