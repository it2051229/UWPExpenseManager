using ExpenseManager.Entities;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ExpenseManager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageMain : Page
    {
        private UserControlTransaction clickedTransactionUserControl = null;

        /// <summary>
        /// Initialize our data
        /// </summary>
        public PageMain()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handle showing suggestion for the description text box
        /// </summary>
        private void autoSuggestBoxDescription_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || sender.Text.Length < 3)
            {
                return;
            }

            HashSet<string> transactionDescriptions = Database.getInstance().session["transaction_descriptions"] as HashSet<string>;
            List<string> suggestions = new List<string>();
                    
            foreach(string description in transactionDescriptions)
            {
                if(description.ToLower().StartsWith(sender.Text.ToLower()))
                {
                    suggestions.Add(description);
                }
            }

            sender.ItemsSource = suggestions.ToArray();
        }

        /// <summary>
        /// Handle showing suggestion for the categories text box
        /// </summary>
        private void autoSuggestBoxCategory_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput || sender.Text.Length < 3)
            {
                return;
            }

            HashSet<string> transactionCategories = Database.getInstance().session["transaction_categories"] as HashSet<string>;
            List<string> suggestions = new List<string>();

            foreach (string category in transactionCategories)
            {
                if (category.ToLower().StartsWith(sender.Text.ToLower()))
                {
                    suggestions.Add(category);
                }
            }

            sender.ItemsSource = suggestions.ToArray();
        }

        /// <summary>
        /// Show the menu of the pressed item, hide the menu of the previous item
        /// </summary>
        private void listViewTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clickedTransactionUserControl != null)
            {
                clickedTransactionUserControl.hideMenu();
            }

            if (listViewTransactions.SelectedIndex >= 0)
            {
                clickedTransactionUserControl = listViewTransactions.SelectedItem as UserControlTransaction;
                clickedTransactionUserControl.showMenu();
            }
        }

        /// <summary>
        /// Load a different account
        /// </summary>
        private void comboBoxAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxAccount.SelectedItem == null)
            {
                return;
            }

            string accountName = comboBoxAccount.SelectedItem.ToString();

            if(accountName.Equals("New Account..."))
            {
                // Show the frame for creating a new account
                Frame.Navigate(typeof(PageAccountCreate));
            }
            else
            {
                Database database = Database.getInstance();
                database.session["current_account"] = accountName;

                displayCurrentAccountBalanceAndTransactions();
                comboBoxTransactionType.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// When the page is loaded we have to redisplay accounts
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Display the accounts
            Database database = Database.getInstance();

            foreach (KeyValuePair<string, Account> entry in database.accounts)
            {
                Account account = entry.Value;
                comboBoxAccount.Items.Add(account.name);
            }

            comboBoxAccount.Items.Add("New Account...");

            // Display the viewed current account
            if (!database.session.ContainsKey("current_account"))
            {
                comboBoxAccount.SelectedIndex = 0;
            }
            else
            {
                comboBoxAccount.SelectedItem = database.session["current_account"];
            }
        }

        /// <summary>
        /// Delete the currently selected account
        /// </summary>
        private async void menuFlyoutItemDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            // Do not delete if there is only one account left
            Database database = Database.getInstance();

            if(database.accounts.Count == 1)
            {
                MessageDialog messageDialog = new MessageDialog("Sorry but I won't let you delete your last account.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            // Confirm for deletion, get dialog ready
            MessageDialog confirmDialog = new MessageDialog("You sure you want to delete this account?", "Seriously...");
            confirmDialog.Commands.Add(new UICommand("Yup", new UICommandInvokedHandler(confirmDialogCommandHandlerToDeleteAccount)));
            confirmDialog.Commands.Add(new UICommand("No, don't", new UICommandInvokedHandler(confirmDialogCommandHandlerToDeleteAccount)));
            await confirmDialog.ShowAsync();
        }

        /// <summary>
        /// Handle the confirm dialog's responses
        /// </summary>
        private void confirmDialogCommandHandlerToDeleteAccount(IUICommand command)
        {
            // Stop if the user rejected
            if(!command.Label.Equals("Yup"))
            {
                return;
            }

            // Delete account
            Database database = Database.getInstance();

            string currentAccountName = comboBoxAccount.SelectedItem.ToString();
            database.accounts.Remove(currentAccountName);

            // Point to the first account as the current account
            comboBoxAccount.Items.RemoveAt(comboBoxAccount.SelectedIndex);
            comboBoxAccount.SelectedIndex = 0;
        }

        /// <summary>
        /// Record a new transaction
        /// </summary>
        private async void buttonAddTransaction_Click(object sender, RoutedEventArgs e)
        {
            string amountTemp = textBoxTransactionAmount.Text.Trim();
            string type = (comboBoxTransactionType.SelectedItem as ComboBoxItem).Content.ToString();
            string description = autoSuggestBoxDescription.Text.Trim();

            if(description == string.Empty)
            {
                description = "...";
            }

            // Make sure all fields are provided
            if(amountTemp == String.Empty || type == String.Empty)
            {
                MessageDialog messageDialog = new MessageDialog("I'm gonna need you to complete all the fields if you want me to add this transaction.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            // Perform validation on the amount
            decimal amount = 0;

            if (!decimal.TryParse(amountTemp, out amount) || amount <= 0)
            {
                MessageDialog messageDialog = new MessageDialog("Come on now, enter a valid amount.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            if (type.Equals("Expense") || type.Equals("Income"))
            {
                // Initialize the category
                string category = autoSuggestBoxCategory.Text.Trim();

                if(category == string.Empty)
                {
                    category = "Others";
                }
                
                // Create the transaction
                if (type.Equals("Expense"))
                {
                    amount = -amount;
                }

                Transaction transaction = new Transaction();
                transaction.amount = amount;
                transaction.date = DateTime.Now;
                transaction.category = category;
                transaction.description = description;

                Database database = Database.getInstance();
                Account account = database.accounts[database.session["current_account"].ToString()];
                account.transactions.Add(transaction);

                // Cache the description for usability
                (database.session["transaction_categories"] as HashSet<string>).Add(category);
                (database.session["transaction_descriptions"] as HashSet<string>).Add(description);

                // Redisplay output
                displayCurrentAccountBalanceAndTransactions();
            }
            else if(type.Equals("Fund Transfer"))
            {
                if(comboBoxToAccount.SelectedIndex == -1)
                {
                    MessageDialog messageDialog = new MessageDialog("I'm gonna need the account where to transfer funds.", "Oh Snap!");
                    await messageDialog.ShowAsync();
                    return;
                }

                // Perform fund transfer
                Database database = Database.getInstance();

                string toAccountName = comboBoxToAccount.SelectedItem.ToString();
                Account toAccount = database.accounts[toAccountName];

                // Remove the amount from the current account
                Transaction transaction = new Transaction();
                transaction.amount = -amount;
                transaction.date = DateTime.Now;
                transaction.category = "Fund transfer to " + toAccount.name;
                transaction.description = description;

                Account currentAccount = database.accounts[comboBoxAccount.SelectedItem.ToString()];
                currentAccount.transactions.Add(transaction);

                // Add the amount to the other account
                transaction = new Transaction();
                transaction.amount = amount;
                transaction.date = DateTime.Now;
                transaction.category = "Fund transfer from " + currentAccount.name;
                transaction.description = description;
                toAccount.transactions.Add(transaction);

                // Refresh the display of balance of current account
                displayCurrentAccountBalanceAndTransactions();
            }

            // Clear the fields
            autoSuggestBoxCategory.Text = "";
            textBoxTransactionAmount.Text = "";
            autoSuggestBoxDescription.Text = "";
        }

        /// <summary>
        /// Display the account transactions and balance
        /// </summary>
        public void displayCurrentAccountBalanceAndTransactions()
        {
            textBoxFilter.Text = "";
            listViewTransactions.Items.Clear();

            Database database = Database.getInstance();
            string accountName = database.session["current_account"].ToString();

            Account account = database.accounts[accountName];
            decimal balance = 0;

            account.transactions.Sort();

            // Display each transaction in a transaction user control
            for(int i = account.transactions.Count - 1; i >= 0; i--)
            {
                Transaction transaction = account.transactions[i];
                balance += transaction.amount;

                UserControlTransaction transactionUserControl = new UserControlTransaction(this, transaction);
                listViewTransactions.Items.Add(transactionUserControl);
            }

            
            textBlockBalance.Text = balance.ToString("N2");    
        }

        /// <summary>
        /// If the type is account transfer, we will show the option for fund transfer
        /// </summary>
        private void comboBoxTransactionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(textBlockCategory == null)
            {
                // don't do anything if the other controls has not yet been fully loaded
                return;
            }

            string transactionType = (comboBoxTransactionType.SelectedItem as ComboBoxItem).Content.ToString();

            textBlockCategory.Visibility = Visibility.Collapsed;
            autoSuggestBoxCategory.Visibility = Visibility.Collapsed;

            textBlockToAccount.Visibility = Visibility.Collapsed;
            comboBoxToAccount.Visibility = Visibility.Collapsed;

            if(transactionType.Equals("Income") || transactionType.Equals("Expense"))
            {
                textBlockCategory.Visibility = Visibility.Visible;
                autoSuggestBoxCategory.Visibility = Visibility.Visible;
            }
            else
            {
                
                Database database = Database.getInstance();
                string currentAccountName = comboBoxAccount.SelectedItem.ToString();

                // Populate the to account excluding the currently selected account
                comboBoxToAccount.Items.Clear();

                foreach (KeyValuePair<string, Account> entry in database.accounts)
                {
                    string accountName = entry.Key;

                    if(!accountName.Equals(currentAccountName))
                    {
                        comboBoxToAccount.Items.Add(accountName);
                    }
                }

                textBlockToAccount.Visibility = Visibility.Visible;
                comboBoxToAccount.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Show the page for updating account information
        /// </summary>
        private void menuFlyoutItemUpdateAccount_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageAccountUpdate));
        }

        /// <summary>
        /// Display the transactions of the current account that is related to the filter search
        /// </summary>
        private void textBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            listViewTransactions.Items.Clear();

            Database database = Database.getInstance();
            Account account = database.accounts[database.session["current_account"].ToString()];

            string keyword = textBoxFilter.Text.Trim().ToLower();
            
            if(keyword == string.Empty)
            {
                // Empty keyword simply display everything
                for(int i = account.transactions.Count - 1; i >= 0; i--)
                {
                    Transaction transaction = account.transactions[i];
                    UserControlTransaction transactionUserControl = new UserControlTransaction(this, transaction);
                    listViewTransactions.Items.Add(transactionUserControl);
                }

                return;
            }

            // Break the keyword into tokens and perform an "AND" search
            HashSet<string> keys = new HashSet<string>();

            foreach(string key in keyword.Split(' '))
            {
                if(key != string.Empty)
                {
                    keys.Add(key.ToLower());
                }
            }
            
            // Add only transaction that has all the keys
            for(int i = account.transactions.Count - 1; i >= 0; i--)
            {
                Transaction transaction = account.transactions[i];
                bool hasAllKeys = true;

                foreach(string key in keys)
                {
                    if(transaction.date.ToString("MMMM dd, yyyy").ToLower().Contains(key)
                        || transaction.description.ToLower().Contains(keyword) 
                        || transaction.category.ToLower().Contains(keyword))
                    {
                        continue;
                    }

                    hasAllKeys = false;
                    break;
                }

                if (hasAllKeys)
                {
                    UserControlTransaction transactionUserControl = new UserControlTransaction(this, transaction);
                    listViewTransactions.Items.Add(transactionUserControl);
                }
            }
        }

        /// <summary>
        /// Show the report page
        /// </summary>
        private void buttonReport_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageReport));
        }
    }
}
