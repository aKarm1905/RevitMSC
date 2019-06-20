﻿using System;

namespace HOK.MissionControl.Core.Schemas
{
    public class RsFileInfo
    {
        public string Path { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string LastModifiedBy { get; set; }
        public Guid ModelGUID { get; set; }
        public int ModelSize { get; set; }
        public int SupportSize { get; set; }
    }
}
