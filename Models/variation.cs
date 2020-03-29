using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class variation
    {
        public int ProcedureID { get; set; }
        public int VariationID { get; set; }
        public string Header { get; set; }
        public string SubHeader { get; set; }

        public virtual procedure procedure { get; set; }
    }
}