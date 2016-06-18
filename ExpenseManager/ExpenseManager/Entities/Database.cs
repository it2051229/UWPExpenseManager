using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ExpenseManager.Entities
{
    public class Database
    {
        private static Database database = new Database();

        public Dictionary<string, Account> accounts = new Dictionary<string, Account>();
        public Dictionary<string, object> session = new Dictionary<string, object>();
        
        /// <summary>
        /// Initialize the database from file
        /// </summary>
        private Database()
        {
            Task loadAccountsTask = Task.Run(async () => { await loadAccounts(); });
            loadAccountsTask.Wait();

            Task loadSessionTask = Task.Run(async () => { await loadSession(); });
            loadSessionTask.Wait();

            // Build the categoies based on the account transactions
            // Build the transaction descriptions based on the account transactions
            HashSet<string> transactionCategories = new HashSet<string>();
            HashSet<string> transactionDescriptions = new HashSet<string>();
            
            foreach(KeyValuePair<string, Account> entry in accounts)
            {
                foreach(Transaction transaction in entry.Value.transactions)
                {
                    transactionDescriptions.Add(transaction.description);
                    transactionCategories.Add(transaction.category);
                }
            }

            session.Add("transaction_categories", transactionCategories);
            session.Add("transaction_descriptions", transactionDescriptions);
        }

        /// <summary>
        /// Load the accounts from a JSON file
        /// </summary>
        private async Task loadAccounts()
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Dictionary<string, Account>));
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                // Open the json file and load the json string
                StorageFile accountsFile = await storageFolder.GetFileAsync("accounts.json");
                string jsonData = await FileIO.ReadTextAsync(accountsFile);
                MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));
                
                // Parse the json data back to acounts
                accounts = json.ReadObject(memoryStream) as Dictionary<string, Account>;
            }
            catch (Exception e)
            {
                // If parsing fails create a new fresh database
                Account account = new Account();
                account.name = "My Account";
                accounts.Add(account.name, account);

                // Create the file
                await storageFolder.CreateFileAsync("accounts.json", CreationCollisionOption.ReplaceExisting);

                string error = e.Message;
            }
        }

        /// <summary>
        /// Load session data
        /// </summary>
        private async Task loadSession()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            // Load the current account related sessions
            try
            {
                // Open the json file and load the json string
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(string));

                StorageFile sessionFile = await storageFolder.GetFileAsync("current_account.json");
                string jsonData = await FileIO.ReadTextAsync(sessionFile);
                MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));

                // Parse the json data back to session
                session["current_account"] = json.ReadObject(memoryStream);
            }
            catch (Exception e)
            {
                // Create the file
                await storageFolder.CreateFileAsync("current_account.json", CreationCollisionOption.ReplaceExisting);

                string error = e.Message;
            }
        }
        
        /// <summary>
        /// Save the accounts to the JSON file
        /// </summary>
        public async Task saveAccounts()
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(Dictionary<string, Account>));
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                // Convert object to json string
                MemoryStream memoryStream = new MemoryStream();
                json.WriteObject(memoryStream, accounts);
                string jsonData = Encoding.UTF8.GetString(memoryStream.ToArray());

                // Overwrite the file and write the json data
                StorageFile accountsFile = await storageFolder.GetFileAsync("accounts.json");
                await FileIO.WriteTextAsync(accountsFile, jsonData);
            }
            catch (Exception e)
            {
                string error = e.Message;
            }
        }

        /// <summary>
        /// Save the user session
        /// </summary>
        public async Task saveSession()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            // Save account related sessions
            try
            {
                // Convert object to json string
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(string));

                MemoryStream memoryStream = new MemoryStream();
                json.WriteObject(memoryStream, session["current_account"]);
                string jsonData = Encoding.UTF8.GetString(memoryStream.ToArray());

                // Overwrite the file and write the json data
                StorageFile sessionFile = await storageFolder.GetFileAsync("current_account.json");
                await FileIO.WriteTextAsync(sessionFile, jsonData);
            }
            catch (Exception e)
            {
                string error = e.Message;
            }
        }
        
        /// <summary>
        /// Get the one and only one instance of the database
        /// </summary>
        public static Database getInstance()
        {
            return database;
        }
    }
}
