using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class keypoint
    {
        public int ProcedureID { get; set; }
        public int KeyPointID { get; set; }
        public Nullable<int> Importance { get; set; }
        public string Description { get; set; }
        public string DiagramURL { get; set; }

        public virtual procedure procedure { get; set; }
    }
}