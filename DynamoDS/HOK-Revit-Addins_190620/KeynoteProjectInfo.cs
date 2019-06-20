﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.Keynote.ClassModels
{
    public class KeynoteProjectInfo
    {
        public string _id { get; set; }
        public string projectNumber { get; set; }
        public string projectName { get; set; }
        public string office { get; set; }
        public string keynoteSet_id { get; set; }

        public KeynoteProjectInfo()
        {

        }

        public KeynoteProjectInfo(string idVal, string pNumber, string pName, string officeName, string keynoteSetId)
        {
            _id = idVal;
            projectNumber = pNumber;
            projectName = pName;
            office = officeName;
            keynoteSet_id = keynoteSetId;
        }
    }
}
