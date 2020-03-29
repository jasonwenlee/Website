using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class history
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public history()
        {
            this.references = new HashSet<reference>();
        }

        public int ProcedureID { get; set; }
        public int HistoryID { get; set; }
        public string Content { get; set; }

        public virtual procedure procedure { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<reference> references { get; set; }

    }
}