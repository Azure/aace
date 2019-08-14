using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveSearch.CustomSkills.Models
{
    public class OutputRecord
    {
        public class OutputRecordData
        {
            public string MyOutputField { get; set; }
        }

        public class OutputRecordMessage
        {
            public string Message { get; set; }
        }

        public string RecordId { get; set; }
        public OutputRecordData Data { get; set; }
        public List<OutputRecordMessage> Errors { get; set; }
        public List<OutputRecordMessage> Warnings { get; set; }
    }
}