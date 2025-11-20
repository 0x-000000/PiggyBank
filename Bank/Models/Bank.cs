using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;

namespace Bank.Models
{
    public class BankAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public double Balance { get; set; }
        public string AccountType { get; set; }
    }

    public class SignUpRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccountType { get; set; }
    }

    public class TransactionRequest
    {
        public string TargetUsername { get; set; }
        public double Amount { get; set; }
    }

    public class TransferRequest
    {
        public string FromUsername { get; set; }
        public string ToUsername { get; set; }
        public double Amount { get; set; }
    }

    public class BalanceRequest
    {
        public string TargetUsername { get; set; }
    }

    public class BankDatabase
    {
        // resolve windows file path for database.xml
        private static string GetPath(string virtualPath)
        {
            var path = HostingEnvironment.MapPath(virtualPath);

            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Unable to resolve database path.");
            }

            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return path;
        }

        // rug pull is crazy
        private static string GetMemberPath()
        {
            return GetPath("~/App_Data/Member.xml");
        }

        private static string GetStaffPath()
        {
            return GetPath("~/App_Data/Staff.xml");
        }

        private static List<BankAccount> LoadFromFile(string path, string role)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "<Accounts></Accounts>");
                return new List<BankAccount>();
            }

            try
            {
                var doc = XDocument.Load(path);
                if (doc.Root == null)
                {
                    return new List<BankAccount>();
                }

                var list = new List<BankAccount>();
                foreach (var node in doc.Root.Elements("BankAccount"))
                {
                    var username = (string)node.Element("Username");
                    var password = (string)node.Element("Password");
                    var balanceText = (string)node.Element("Balance");

                    double bal = 0;
                    double.TryParse(balanceText, out bal);

                    list.Add(new BankAccount
                    {
                        Username = username,
                        Password = password,
                        Balance = bal,
                        AccountType = role
                    });
                }

                return list;
            }
            catch
            {
                return new List<BankAccount>();
            }
        }

        //read accounts
        public static List<BankAccount> LoadAccounts()
        {
            var accounts = new List<BankAccount>();
            accounts.AddRange(LoadFromFile(GetMemberPath(), "member"));
            accounts.AddRange(LoadFromFile(GetStaffPath(), "admin"));
            return accounts;
        }

        private static void SaveList(IEnumerable<BankAccount> accounts, string path)
        {
            var doc = new XDocument();
            var root = new XElement("Accounts");
            doc.Add(root);

            if (accounts != null)
            {
                foreach (var account in accounts)
                {
                    root.Add(new XElement("BankAccount",
                        new XElement("Username", account?.Username),
                        new XElement("Password", account?.Password),
                        new XElement("Balance", account?.Balance ?? 0)
                    ));
                }
            }

            doc.Save(path);
        }

        //write accounts
        public static void SaveAccounts(IEnumerable<BankAccount> accounts)
        {
            var members = new List<BankAccount>();
            var staff = new List<BankAccount>();

            if (accounts != null)
            {
                foreach (var account in accounts)
                {
                    if (account != null && string.Equals(account.AccountType, "admin", StringComparison.OrdinalIgnoreCase))
                    {
                        staff.Add(account);
                    }
                    else
                    {
                        members.Add(account);
                    }
                }
            }

            SaveList(members, GetMemberPath());
            SaveList(staff, GetStaffPath());
        }
    }

    public class BankSystem
    {
        private readonly Credentials credentials = new Credentials();

        public BankAccount SignUp(string username, string password, string accountType, out string error)
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

            var role = string.Equals(accountType, "admin") ? "admin" : "member";

            var account = new BankAccount
            {
                Username = username.Trim(),
                Password = credentials.HashPassword(password),
                Balance = 0,
                AccountType = role
            };

            accounts.Add(account);
            BankDatabase.SaveAccounts(accounts);

            return account;
        }

        public BankAccount Authenticate(string username, string password, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                error = "Invalid username or password.";
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var account = FindAccount(accounts, username);

            if (account == null)
            {
                error = "Invalid username or password.";
                return null;
            }

            if (!credentials.VerifyPassword(password, account.Password))
            {
                error = "Invalid username or password.";
                return null;
            }

            return account;
        }

        public BankAccount GetAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            return FindAccount(accounts, username);
        }

        public void CreateBuiltInAccounts()
        {
            var accounts = BankDatabase.LoadAccounts();
            var hasAnyAccount = accounts.Count > 0;

            if (hasAnyAccount)
            {
                return;
            }

            SignUp("TA", "Cse445!", "admin", out _);
        }

        public BankAccount Deposit(BankAccount actor, TransactionRequest request, out string error)
        {
            if (!ValidateTransactionRequest(request, out error))
            {
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var target = PickTarget(accounts, actor, request.TargetUsername, out error);
            if (target == null)
            {
                return null;
            }

            target.Balance += request.Amount;
            BankDatabase.SaveAccounts(accounts);
            return target;
        }

        public BankAccount Spend(BankAccount actor, TransactionRequest request, out string error)
        {
            if (!ValidateTransactionRequest(request, out error))
            {
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var target = PickTarget(accounts, actor, request.TargetUsername, out error);
            if (target == null)
            {
                return null;
            }

            if (target.Balance < request.Amount)
            {
                error = "Insufficient funds.";
                return null;
            }

            target.Balance -= request.Amount;
            BankDatabase.SaveAccounts(accounts);
            return target;
        }

        public BankAccount GetBalance(BankAccount actor, BalanceRequest request, out string error)
        {
            error = null;
            var accounts = BankDatabase.LoadAccounts();
            var target = PickTarget(accounts, actor, request?.TargetUsername, out error);
            return target;
        }

        public (BankAccount from, BankAccount to) Transfer(BankAccount actor, TransferRequest request, out string error)
        {
            error = null;

            if (actor == null)
            {
                error = "Unknown account.";
                return (null, null);
            }

            if (request == null || string.IsNullOrWhiteSpace(request.ToUsername))
            {
                error = "Invalid request.";
                return (null, null);
            }

            if (request.Amount <= 0)
            {
                error = "Amount cannot be negative.";
                return (null, null);
            }

            var accounts = BankDatabase.LoadAccounts();
            var fromName = string.IsNullOrWhiteSpace(request.FromUsername)
                ? actor.Username
                : request.FromUsername.Trim();

            if (!IsAdmin(actor) && !string.Equals(fromName, actor.Username, StringComparison.OrdinalIgnoreCase))
            {
                error = "Members can only transfer their own money.";
                return (null, null);
            }

            var sender = FindAccount(accounts, fromName);

            if (sender == null)
            {
                error = "Invalid sender.";
                return (null, null);
            }

            var recipient = FindAccount(accounts, request.ToUsername);

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

        public BankAccount Promote(BankAccount actor, string targetUsername, out string error)
        {
            error = null;

            if (actor == null || !IsAdmin(actor))
            {
                error = "Only admins can do that.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(targetUsername))
            {
                error = "Need a username to promote.";
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var target = FindAccount(accounts, targetUsername);

            if (target == null)
            {
                error = "User not found.";
                return null;
            }

            target.AccountType = "admin";
            BankDatabase.SaveAccounts(accounts);
            return target;
        }

        public BankAccount Demote(BankAccount actor, string targetUsername, out string error)
        {
            error = null;

            if (actor == null || !IsAdmin(actor))
            {
                error = "Only admins can do that.";
                return null;
            }

            var accounts = BankDatabase.LoadAccounts();
            var name = string.IsNullOrWhiteSpace(targetUsername) ? actor.Username : targetUsername.Trim();
            var target = FindAccount(accounts, name);

            if (target == null)
            {
                error = "User not found.";
                return null;
            }

            if (!IsAdmin(target))
            {
                error = "User is not an admin.";
                return null;
            }

            var adminCount = accounts.Count(a => IsAdmin(a));
            if (adminCount <= 1)
            {
                error = "Need at least one admin.";
                return null;
            }

            target.AccountType = "member";
            BankDatabase.SaveAccounts(accounts);
            return target;
        }

        private static BankAccount PickTarget(List<BankAccount> accounts, BankAccount actor, string targetUsername, out string error)
        {
            error = null;

            if (actor == null)
            {
                error = "Unknown account.";
                return null;
            }

            var name = actor.Username;
            if (!string.IsNullOrWhiteSpace(targetUsername))
            {
                name = targetUsername.Trim();
            }

            if (!IsAdmin(actor) && !string.Equals(name, actor.Username, StringComparison.OrdinalIgnoreCase))
            {
                error = "Members can only use their own account.";
                return null;
            }

            var target = FindAccount(accounts, name);
            if (target == null)
            {
                error = "Account not found.";
            }

            return target;
        }

        private static bool ValidateTransactionRequest(TransactionRequest request, out string error)
        {
            error = null;

            // validate input every request
            if (request == null)
            {
                error = "Invalid request.";
                return false;
            }

            if (request.Amount <= 0)
            {
                error = "Amount must be positive.";
                return false;
            }

            return true;
        }

        private static bool IsAdmin(BankAccount account)
        {
            return account != null && string.Equals(account.AccountType, "admin", StringComparison.OrdinalIgnoreCase);
        }

        //login check
        private static BankAccount FindAccount(List<BankAccount> accounts, string username)
        {
            return accounts.FirstOrDefault(a =>
                string.Equals(a.Username.ToLower(), username?.ToLower().Trim()));
        }
    }
}
