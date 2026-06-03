using System;
using System.Collections.Generic;

#nullable disable

namespace BehindAGirl.Data
{
    public partial class Team
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid GroupId { get; set; }
        public int Points { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int For { get; set; }
        public int Against { get; set; }
        public int Goals { get; set; }
        public string LogoUrl { get; set; }

    }
}
