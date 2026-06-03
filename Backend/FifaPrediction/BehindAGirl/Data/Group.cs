using System;
using System.Collections.Generic;

#nullable disable

namespace BehindAGirl.Data
{
    public partial class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Team> Teams { get; internal set; }
    }
}
