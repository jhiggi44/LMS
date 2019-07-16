using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Classes
    {
        public Classes()
        {
            AssignmentCategories = new HashSet<AssignmentCategories>();
            EnrollmentGrade = new HashSet<EnrollmentGrade>();
        }

        public uint ClassId { get; set; }
        public string Semester { get; set; }
        public string CatalogId { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Professor { get; set; }

        public virtual Courses Catalog { get; set; }
        public virtual Professors ProfessorNavigation { get; set; }
        public virtual ICollection<AssignmentCategories> AssignmentCategories { get; set; }
        public virtual ICollection<EnrollmentGrade> EnrollmentGrade { get; set; }
    }
}
