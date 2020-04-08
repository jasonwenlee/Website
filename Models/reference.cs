﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class reference
    {
        public reference()
        {
            this.histories = new HashSet<history>();
        }

        public int ProcedureID { get; set; }
        public int ReferenceID { get; set; }
        public string Content { get; set; }
        public Nullable<int> Number { get; set; }

        public virtual procedure procedure { get; set; }
        public virtual ICollection<history> histories { get; set; }
    }
}