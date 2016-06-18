using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseManager.Entities
{
    /// <summary>
    /// Holds transaction info
    /// </summary>
    [DataContract]
    public class Transaction : IComparable<Transaction>
    {
        [DataMember]
        public decimal amount = 0;

        [DataMember]
        public string category = "Others";

        [DataMember]
        public string description = "";

        [DataMember]
        public DateTime date = DateTime.Now;

        /// <summary>
        /// Compare transactions by date
        /// </summary>
        public int CompareTo(Transaction other)
        {
            return date.CompareTo(other.date);
        }
    }
}
