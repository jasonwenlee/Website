﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class keypoint
    {
        public int ProcedureID { get; set; }
        public int KeyPointID { get; set; }
        public string Description { get; set; }
        public string DiagramURL { get; set; }
        public Nullable<int> Number { get; set; }
        public string Header { get; set; }


        public virtual procedure procedure { get; set; }
    }
}