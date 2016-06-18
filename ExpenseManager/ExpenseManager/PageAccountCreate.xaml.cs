using ExpenseManager.Entities;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExpenseManager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageAccountCreate : Page
    {
        /// <summary>
        /// Initialize the user interface
        /// </summary>
        public PageAccountCreate()
        {
            this.InitializeComponent();
            
        }

        /// <summary>
        /// Go back to the main page
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageMain));
        }

        /// <summary>
        /// Attempt to create the account
        /// </summary>
        private async void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            string accountName = textBoxAccountName.Text.Trim();
            string initialBalanceString = textBoxInitialBalance.Text.Trim();

            // Validate all fields are provided
            if(accountName == string.Empty || initialBalanceString == string.Empty)
            {
                MessageDialog messageDialog = new MessageDialog("I'm gonna need you to enter an account name and an initial balance.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }
            
            // Validate the the name is unique
            Database database = Database.getInstance();

            if(database.accounts.ContainsKey(accountName))
            {
                MessageDialog messageDialog = new MessageDialog("You currently have this account.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            // Validate the balance is a number
            decimal initialBalance = 0;

            if(!decimal.TryParse(initialBalanceString, out initialBalance) || initialBalance < 0)
            {
                MessageDialog messageDialog = new MessageDialog("Come on now, enter a valid amount.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            // Create the account and navigate back to main page
            Account account = new Account();
            account.name = accountName;

            Transaction transaction = new Transaction();
            transaction.amount = initialBalance;
            transaction.date = DateTime.Now;
            transaction.description = "Beginning balance";

            account.transactions.Add(transaction);
            database.accounts.Add(accountName, account);

            // Set the new account as the new selection when going back to main page
            database.session["current_account"] = accountName;
            Frame.Navigate(typeof(PageMain));
        }
    }
}
