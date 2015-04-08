using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpPhoneNumberParser
{
    public class CallerIdDict : Dictionary<CallerId, int>
    {
        public void Add(string phoneNumber, string name, int levenshtein)
        {
            CallerId val;
            val.name = name;
            val.phoneNumber = phoneNumber;

            //add to dict
            this.Add(val, levenshtein);
        }
    }
}
