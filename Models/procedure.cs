using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class procedure
    {
        public procedure()
        {
            this.complications = new HashSet<complication>();
            this.histories = new HashSet<history>();
            this.keypoints = new HashSet<keypoint>();
            this.procedures = new HashSet<procedure_has_relatedprocedure>();
            this.relatedprocedures = new HashSet<procedure_has_relatedprocedure>();
            this.references = new HashSet<reference>();
            this.steps = new HashSet<step>();
            this.variations = new HashSet<variation>();
        }

        public int ProcedureID { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string VideoSource { get; set; }

        public virtual ICollection<complication> complications { get; set; }
        public virtual ICollection<history> histories { get; set; }
        public virtual ICollection<keypoint> keypoints { get; set; }
        public virtual ICollection<procedure_has_relatedprocedure> procedures { get; set; }
        public virtual ICollection<procedure_has_relatedprocedure> relatedprocedures { get; set; }
        public virtual ICollection<reference> references { get; set; }
        public virtual ICollection<step> steps { get; set; }
        public virtual ICollection<variation> variations { get; set; }
    }
}