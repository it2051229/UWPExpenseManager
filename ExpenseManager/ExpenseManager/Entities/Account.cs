using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseManager.Entities
{
    /// <summary>
    /// Holds multiple transactions
    /// </summary>
    [DataContract]
    public class Account : IComparable<Account>
    {
        [DataMember]
        public string name;

        [DataMember]
        public List<Transaction> transactions = new List<Transaction>();

        /// <summary>
        /// Compare by name for sorting
        /// </summary>
        public int CompareTo(Account other)
        {
            return name.CompareTo(other.name);
        }
    }
}
