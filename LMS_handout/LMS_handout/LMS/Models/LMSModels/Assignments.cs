using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignments
    {
        public Assignments()
        {
            Submission = new HashSet<Submission>();
        }

        public ulong AId { get; set; }
        public string Name { get; set; }
        public string Contents { get; set; }
        public DateTime DueDate { get; set; }
        public uint Points { get; set; }
        public uint Category { get; set; }

        public virtual AssignmentCategories CategoryNavigation { get; set; }
        public virtual ICollection<Submission> Submission { get; set; }
    }
}
