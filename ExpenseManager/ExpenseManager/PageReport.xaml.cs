using ExpenseManager.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class PageReport : Page
    {
        private List<Transaction> transactionsForTheYear = new List<Transaction>();
        private List<Transaction> transactionsForTheMonth = new List<Transaction>();
        private List<Transaction> transactionsForTheDay = new List<Transaction>();
        private string[] monthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        /// <summary>
        /// Initialize the report
        /// </summary>
        public PageReport()
        {
            this.InitializeComponent();

            Database database = Database.getInstance();
            Account account = database.accounts[database.session["current_account"].ToString()];

            // Add all the years for the account
            HashSet<int> years = new HashSet<int>();

            foreach (Transaction transaction in account.transactions)
            {
                years.Add(transaction.date.Year);
            }

            // Put the years on display
            foreach (int year in years)
            {
                comboBoxYear.Items.Add(year.ToString());
            }

            // Add the all years menu
            comboBoxYear.Items.Add("All Years");
        }

        /// <summary>
        /// Initialize the values of the combo box
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Show the latest year
            if(comboBoxYear.Items.Count >= 2)
            {
                comboBoxYear.SelectedIndex = comboBoxYear.Items.Count - 2;
            }

            // Load the latest month
            if(comboBoxMonth.Items.Count >= 2)
            {
                comboBoxMonth.SelectedIndex = comboBoxMonth.Items.Count - 2;
            }

            // Load the latest day
            if(comboBoxDay.Items.Count >= 2)
            {
                comboBoxDay.SelectedIndex = comboBoxDay.Items.Count - 2;
            }
        }

        /// <summary>
        /// Back to main page
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PageMain));
        }

        /// <summary>
        /// When a year is selected filter the month
        /// </summary>
        private void comboBoxYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxMonth.Items.Clear();
            comboBoxDay.Items.Clear();

            Database database = Database.getInstance();
            Account account = database.accounts[database.session["current_account"].ToString()];

            string targetYear = comboBoxYear.SelectedItem.ToString();

            // Filter all transactions on the given year
            transactionsForTheYear.Clear();
            
            foreach(Transaction transaction in account.transactions)
            {
                if(targetYear.Equals("All Years") || targetYear.Equals(transaction.date.Year.ToString()))
                {
                    transactionsForTheYear.Add(transaction);
                }
            }

            // Display the months of all transactions in the combo box
            List<int> months = new List<int>();

            foreach (Transaction transaction in transactionsForTheYear)
            {
                if(!months.Contains(transaction.date.Month))
                {
                    months.Add(transaction.date.Month);
                }
            }

            months.Sort();
            
            foreach(int month in months)
            {
                comboBoxMonth.Items.Add(monthNames[month - 1]);
            }

            comboBoxMonth.Items.Add("All Months");
        }

        /// <summary>
        /// When a month is selected filter the days
        /// </summary>
        private void comboBoxMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxDay.Items.Clear();

            if (comboBoxMonth.SelectedItem == null)
            {
                return;
            }

            string targetMonth = comboBoxMonth.SelectedItem.ToString();

            // Filter all transactions from the month to display only transactions for the day
            transactionsForTheMonth.Clear();

            foreach (Transaction transaction in transactionsForTheYear)
            {
                if (targetMonth.Equals("All Months") || monthNames[transaction.date.Month - 1].Equals(targetMonth))
                {
                    transactionsForTheMonth.Add(transaction);
                }
            }

            // Display the day of all transactions in the combo box
            List<int> days = new List<int>();

            foreach(Transaction transaction in transactionsForTheMonth)
            {
                if(!days.Contains(transaction.date.Day))
                {
                    days.Add(transaction.date.Day);
                }
            }

            days.Sort();

            foreach(int day in days)
            {
                comboBoxDay.Items.Add(day);
            }

            comboBoxDay.Items.Add("All Days");
        }

        /// <summary>
        /// Display all transactions in the list view by category and calculate the summary
        /// </summary>
        private void comboBoxDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxDay.SelectedItem == null)
            {
                return;
            }

            decimal income = 0;
            decimal expense = 0;

            transactionsForTheDay.Clear();
            string targetDay = comboBoxDay.SelectedItem.ToString();

            // Filter the days
            foreach(Transaction transaction in transactionsForTheMonth)
            {
                if(targetDay.Equals("All Days") || transaction.date.Day.ToString().Equals(targetDay))
                {
                    transactionsForTheDay.Add(transaction);
                }
            }

            // Display and calculate the transactions
            foreach(Transaction transaction in transactionsForTheDay)
            {
                if (transaction.amount > 0)
                {
                    income += transaction.amount;
                }

                if (transaction.amount < 0)
                {
                    expense += transaction.amount;
                }
            }

            decimal balance = income + expense;

            textBlockIncome.Text = income.ToString("N2");
            textBlockExpense.Text = expense.ToString("N2");
            textBlockBalance.Text = balance.ToString("N2");

            // For each transaction group them by categories to calculate the expense
            Dictionary<string, decimal> categories = new Dictionary<string, decimal>();

            foreach(Transaction transaction in transactionsForTheDay)
            {
                if(transaction.amount >= 0)
                {
                    // Don't report income
                    continue;
                }

                if(!categories.ContainsKey(transaction.category))
                {
                    categories.Add(transaction.category, 0);
                }

                // Make amounts positive for calculation of progress bar
                categories[transaction.category] += (transaction.amount * -1);
            }

            // Alright find the highest category spending
            decimal highestAmount = 0;

            foreach(KeyValuePair<string, decimal> entry in categories)
            {
                decimal amount = entry.Value;

                if(amount > highestAmount)
                {
                    highestAmount = amount;
                }
            }

            // Now we can display the report
            listViewSummary.Items.Clear();

            foreach(KeyValuePair<string, decimal> entry in categories)
            {
                string categoryName = entry.Key;
                decimal amount = entry.Value;                
                double amountProgressValue = (double)(amount / highestAmount) * 100;

                UserControlTransactionCategoryReport categoryReport = new UserControlTransactionCategoryReport(categoryName, amount, amountProgressValue);
                listViewSummary.Items.Add(categoryReport);
            }
        }
    }
}
