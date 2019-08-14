using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveSearch.CustomSkills.Models
{
    public class InputRecord
    {
        public class InputRecordData
        {
            public string MyInputField;
        }

        public string RecordId { get; set; }
        public InputRecordData Data { get; set; }
    }
}