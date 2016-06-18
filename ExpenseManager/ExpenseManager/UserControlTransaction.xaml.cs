using ExpenseManager.Entities;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExpenseManager
{
    public sealed partial class UserControlTransaction : UserControl
    {
        private Transaction transaction;
        private PageMain mainPage;

        /// <summary>
        /// Initialize the user control
        /// </summary>
        public UserControlTransaction(PageMain mainPage, Transaction transaction)
        {
            this.InitializeComponent();

            this.mainPage = mainPage;
            this.transaction = transaction;

            if(transaction.amount > 0)
            {
                textBlockAmount.Text = "+" + transaction.amount.ToString("N2");
            }
            else
            {
                textBlockAmount.Text = transaction.amount.ToString("N2");
            }

            textBlockCategory.Text = transaction.category;
            textBlockDescription.Text = transaction.description;
            textBlockDate.Text = transaction.date.ToString("MMMM dd, yyyy");

            hideMenu();
        }

        /// <summary>
        /// Display the menu of the user control 
        /// </summary>
        public void showMenu()
        {
            buttonUpdate.Visibility = Visibility.Visible;
            buttonDelete.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide the menu of the user control
        /// </summary>
        public void hideMenu()
        {
            buttonUpdate.Visibility = Visibility.Collapsed;
            buttonDelete.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Delete the transaction
        /// </summary>
        private async void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            // Perform confirmation, and if confirmed then delete the transaction from the database
            MessageDialog confirmDialog = new MessageDialog("You sure you want to delete this transaction?", "Seriously...");
            confirmDialog.Commands.Add(new UICommand("Yup", new UICommandInvokedHandler(confirmDialogCommandHandlerToDeleteTransaction)));
            confirmDialog.Commands.Add(new UICommand("No, don't", new UICommandInvokedHandler(confirmDialogCommandHandlerToDeleteTransaction)));
            await confirmDialog.ShowAsync();
        }

        /// <summary>
        /// Handle confirmation of delete of transaction
        /// </summary>
        private void confirmDialogCommandHandlerToDeleteTransaction(IUICommand command)
        {
            // Stop if the user rejected
            if (!command.Label.Equals("Yup"))
            {
                return;
            }

            Database database = Database.getInstance();
            Account account = database.accounts[database.session["current_account"].ToString()];
            account.transactions.Remove(transaction);

            // Update the user interface
            mainPage.displayCurrentAccountBalanceAndTransactions();
        }

        /// <summary>
        /// Show the update transaction page
        /// </summary>
        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            mainPage.Frame.Navigate(typeof(PageTransactionUpdate), transaction);
        }
    }
}
