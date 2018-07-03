using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiteTest
{
    class TimeEntry
    {
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }

        public int Minutes { get; set; }
    }

    class TimeEntryWrapper
    {
        [JsonProperty("time_entry")]
        public TimeEntry TimeEntry { get; set; }
    }
}
