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
    public sealed partial class PageAccountUpdate : Page
    {
        /// <summary>
        /// Initialize the account page
        /// </summary>
        public PageAccountUpdate()
        {
            this.InitializeComponent();

            Database database = Database.getInstance();
            textBoxAccountName.Text = database.accounts[database.session["current_account"].ToString()].name;
        }

        /// <summary>
        /// Perform an update to the name of the account
        /// </summary>
        private async void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            string accountName = textBoxAccountName.Text.Trim();

            // Validate all fields are provided
            if (accountName == string.Empty)
            {
                MessageDialog messageDialog = new MessageDialog("I'm gonna need you to enter an account name.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            // Validate the the name is unique
            Database database = Database.getInstance();
            Account account = database.accounts[database.session["current_account"].ToString()];

            if(accountName.Equals(account.name))
            {
                MessageDialog messageDialog = new MessageDialog("I'm not sure but you aren't really updating anything.", "Oh Snap!");
                await messageDialog.ShowAsync();
            }
            
            if (database.accounts.ContainsKey(accountName))
            {
                MessageDialog messageDialog = new MessageDialog("You currently have this account.", "Oh Snap!");
                await messageDialog.ShowAsync();
                return;
            }

            // Update the name and go back to main page
            database.accounts.Remove(account.name);
            account.name = accountName;
            database.accounts.Add(accountName, account);
            database.session["current_account"] = accountName;
            Frame.Navigate(typeof(PageMain));
        }

        /// <summary>
        /// Back to main page
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageMain));
        }
    }
}
