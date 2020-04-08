using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class history
    {
        public history()
        {
            this.references = new HashSet<reference>();
        }
        public int ProcedureID { get; set; }
        public int HistoryID { get; set; }
        public string Content { get; set; }
        public Nullable<int> Number { get; set; }
        public virtual procedure procedure { get; set; }
        public virtual ICollection<reference> references { get; set; }
    }
}