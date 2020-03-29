using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class step
    {
        public int ProcedureID { get; set; }
        public int StepID { get; set; }
        public string Content { get; set; }
        public string DiagramURL { get; set; }

        public virtual procedure procedure { get; set; }
    }
}