using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LionTourBot.Model.ViewModel
{
    [DataContract]
    public class Utterance
    {
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public List<Intent> intents { get; set; }
        [DataMember]
        public List<Entity> entities { get; set; }
    }
}
