using System;
using System.Collections.Generic;

namespace mongo.Local
{
    public class ScheduledEvent
    {
        /// <summary>
        /// The ID of the AWS account that owns the rule.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The AwsRegion in which the schedule was inovked on
        /// </summary>
        public string Region { get; set; }
        
        public Detail Detail { get; set; }
        
        /// <summary>
        /// Static string of "Scheduled Event"
        /// </summary>
        public string DetailType { get; set; }
        
        public string Source { get; set; }
        
        public DateTime Time { get; set; }
        
        public string Id { get; set; }
        
        /// <summary>
        /// The resource of the invoking schedule 
        /// </summary>
        public List<string> Resources { get; set; }
    }
    
    /// <summary>
    /// The class representing the information for a Detail
    /// </summary>
    public class Detail
    {
    }
}