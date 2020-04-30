using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts
{
    public class APISubscriptionKeyName
    {
        public string KeyName { get; set; }

        public APISubscriptionKeyName(string keyName)
        {
            KeyName = keyName;
        }
    }
}
