using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class complication
    {
        public int ProcedureID { get; set; }
        public int ComplicationID { get; set; }
        public string Name { get; set; }
        public string DiagramURL { get; set; }

        public virtual procedure procedure { get; set; }
    }
}