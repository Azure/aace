using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveSearch.CustomSkills.Models
{
    public class WebApiResponse
    {
        public WebApiResponse()
        {
            this.values = new List<OutputRecord>();
        }

        public List<OutputRecord> values { get; set; }
    }
}