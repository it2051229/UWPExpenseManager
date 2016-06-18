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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExpenseManager
{
    public sealed partial class UserControlTransactionCategoryReport : UserControl
    {
        /// <summary>
        /// Initialize the values of the progress bars
        /// </summary>
        public UserControlTransactionCategoryReport(string categoryName, decimal amount, double amountProgressValue)
        {
            this.InitializeComponent();

            textBlockCategory.Text = categoryName;
            textBlockAmount.Text = amount.ToString("N2");
            progressBarAmount.Value = amountProgressValue;            
        }
    }
}
