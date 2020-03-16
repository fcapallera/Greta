using System;
using System.Collections.Generic;

namespace CoreBot.Models
{
    public partial class Naquestions
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public int UserId { get; set; }
        public DateTime CreationDate { get; set; }

        public UserProfile User { get; set; }
    }
}
