using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts
{
    public class OfferWarning
    {
        public string OfferName { get; set; }

        public string WarningMessage { get; set; }

        public string Details { get; set; }

        public OfferWarning(string offerName, string warningMessage, string details)
        {
            this.OfferName = offerName;
            this.WarningMessage = warningMessage;
            this.Details = details;
        }
    }
}
