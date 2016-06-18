using ExpenseManager.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExpenseManager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageTransactionUpdate : Page
    {
        private Transaction transaction;

        /// <summary>
        /// Show the update transaction page
        /// </summary>
        public PageTransactionUpdate()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initialize the transaction that needs to be modified
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            transaction = e.Parameter as Transaction;
            
            decimal amount = transaction.amount;
            comboBoxTransactionType.SelectedIndex = 1;

            if(transaction.amount < 0)
            {
                amount = transaction.amount * -1;
                comboBoxTransactionType.SelectedIndex = 0;
            }

            textBoxTransactionAmount.Text = amount.ToString();
            autoSuggestBoxDescription.Text = transaction.description;
            autoSuggestBoxCategory.Text = transaction.category;
            datePickerDate.Date = transaction.date;
        }

        /// <summary>
        /// Update the transaction information
        /// </summary>
        private async void buttonUpdateTransaction_Click(object sender, RoutedEventArgs e)
        {
            string amountTemp = textBoxTransactionAmount.Text.Trim();
            string type = (comboBoxTransactionType.SelectedItem as ComboBoxItem).Content.ToString();
            string description = autoSuggestBoxDescription.Text.Trim();

            if(description == string.Empty)
            {
                description = "...";
            }

            // Make sure all fields are provided
            if (amountTemp == String.Empty || type == String.Empty)
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

            // Initialize the category
            string category = autoSuggestBoxCategory.Text.Trim();

            if (category == string.Empty)
            {
                category = "Others";
            }

            // Update the transaction
            if (type.Equals("Expense"))
            {
                amount = -amount;
            }

            transaction.amount = amount;
            transaction.category = category;
            transaction.description = description;
            transaction.date = datePickerDate.Date.DateTime;

            // Navidate back to main page
            Frame.Navigate(typeof(PageMain));
        }

        /// <summary>
        /// Back to main page
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageMain));
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

            foreach (string description in transactionDescriptions)
            {
                if (description.ToLower().StartsWith(sender.Text.ToLower()))
                {
                    suggestions.Add(description);
                }
            }

            sender.ItemsSource = suggestions.ToArray();
        }

        /// <summary>
        /// Handle showing suggestion for the category text box
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
    }
}
