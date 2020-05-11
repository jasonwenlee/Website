using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class relatedprocedure
    {
        public int Procedure_has_RelatedProcedurecol { get; set; }
        public int ProcedureID { get; set; }
        public int RelatedProcedureID { get; set; }

        public virtual procedure procedures { get; set; }
        public virtual procedure relatedprocedures { get; set; }
    }
}