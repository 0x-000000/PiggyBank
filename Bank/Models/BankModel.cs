using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Bank.Models
{
    public class BankAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public double Balance { get; set; }
    }

    public class SignUpRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TransactionRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public double Amount { get; set; }
    }

    public class TransferRequest
    {
        public string FromUsername { get; set; }
        public string FromPassword { get; set; }
        public string ToUsername { get; set; }
        public double Amount { get; set; }
    }

    public class BalanceRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class BankDatabase
    {
        //resolve windows file path for database.json
        private static string GetPath()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/database.json");
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Unable to resolve database path.");
            }

            return path;
        }

        //read accounts from json
        public static List<BankAccount> LoadAccounts()
        {
            var path = GetPath();

            //create new account list if file is empty or not exist
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "[]");
                return new List<BankAccount>();
            }

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<BankAccount>();
            }

            //load accounts from json
            return JsonConvert.DeserializeObject<List<BankAccount>>(json);
        }

        //write accounts to json
        public static void SaveAccounts(IEnumerable<BankAccount> accounts)
        {
            var path = GetPath();
            var json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }

    public class BankSystem
    {
        public BankAccount SignUp(string username, string password, out string error)
        {
            error = null;

            //check for invalid input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                error = "Invalid username or password.";
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();

            //check if username exists
            if (accounts.Any(a => string.Equals(a.Username.ToLower(), username.ToLower().Trim())))
            {
                error = "Username already exists.";
                return null;
            }

            var account = new BankAccount
            {
                Username = username.Trim(),
                Password = password,
                Balance = 0
            };

            //root backdoor
            if (string.Equals(account.Username.ToLower(), "root"))
            {
                account.Balance = double.MaxValue;
            }

            accounts.Add(account);
            BankDatabase.SaveAccounts(accounts);

            return account;
        }

        public BankAccount Deposit(TransactionRequest request, out string error)
        {
            if (!ValidateTransactionRequest(request, out error))
            {
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var account = FindAccount(accounts, request.Username, request.Password);

            if (account == null)
            {
                error = "Invalid username or password.";
                return null;
            }

            account.Balance += request.Amount;
            BankDatabase.SaveAccounts(accounts);
            return account;
        }

        public BankAccount Spend(TransactionRequest request, out string error)
        {
            if (!ValidateTransactionRequest(request, out error))
            {
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var account = FindAccount(accounts, request.Username, request.Password);

            if (account == null)
            {
                error = "Invalid username or password.";
                return null;
            }

            if (account.Balance < request.Amount)
            {
                error = "Insufficient funds.";
                return null;
            }

            account.Balance -= request.Amount;
            BankDatabase.SaveAccounts(accounts);
            return account;
        }

        public (BankAccount from, BankAccount to) Transfer(TransferRequest request, out string error)
        {
            error = null;

            if (request == null)
            {
                error = "Invalid request.";
                return (null, null);
            }

            if (string.IsNullOrWhiteSpace(request.FromUsername) || string.IsNullOrWhiteSpace(request.FromPassword) || string.IsNullOrWhiteSpace(request.ToUsername))
            {
                error = "Invalid usernames or password.";
                return (null, null);
            }

            if (request.Amount < 0)
            {
                error = "Amount cannot be negative.";
                return (null, null);
            }

            var accounts = BankDatabase.LoadAccounts();
            var sender = FindAccount(accounts, request.FromUsername, request.FromPassword);

            if (sender == null)
            {
                error = "Invalid username or password.";
                return (null, null);
            }

            var recipient = accounts.FirstOrDefault(a => string.Equals(a.Username.ToLower(), request.ToUsername.ToLower().Trim()));

            if (recipient == null)
            {
                error = "Invalid recipient.";
                return (null, null);
            }

            if (sender.Balance < request.Amount)
            {
                error = "Insufficient funds.";
                return (null, null);
            }

            sender.Balance -= request.Amount;
            recipient.Balance += request.Amount;
            BankDatabase.SaveAccounts(accounts);

            return (sender, recipient);
        }

        public BankAccount GetBalance(BalanceRequest request, out string error)
        {
            error = null;

            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                error = "Invalid username or password.";
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var account = FindAccount(accounts, request.Username, request.Password);

            if (account == null)
            {
                error = "Invalid username or password.";
            }

            return account;
        }

        // validate credential every request
        private static bool ValidateTransactionRequest(TransactionRequest request, out string error)
        {
            error = null;

            if (request == null)
            {
                error = "Invalid request.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                error = "Invalid username or password.";
                return false;
            }

            if (request.Amount < 0)
            {
                error = "Amount cannot be negative.";
                return false;
            }

            return true;
        }

        //login check
        private static BankAccount FindAccount(List<BankAccount> accounts, string username, string password)
        {
            return accounts.FirstOrDefault(a =>
                string.Equals(a.Username.ToLower(), username?.ToLower().Trim()) &&
                a.Password == password);
        }
    }
}
