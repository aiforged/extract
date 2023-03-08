using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public class Enums
    {
        public enum ValueType : int
        {
            None = 0,
            String = 1,
            Number = 2,
            Currency = 3,
            Boolean = 4,
            DateTime = 5,
            TimeSpan = 6,
            Email = 7,
            Enum = 8,
            Picture = 9,
            Password = 10,
            SecureString = 11,
            Address = 12,
            IDNo = 13,
            Country = 14,
            Code = 15,
            TelephoneNo = 17,
            GPS = 18,
            Table = 19,
            Column = 20,
        }


    }
}
