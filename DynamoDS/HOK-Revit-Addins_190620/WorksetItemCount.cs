﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Worksets;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class WorksetItemCount
    {
        /// <summary>
        /// Publishes Workset Item Count data when Document is closed.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="filePath">Revit Document central file path.</param>
        public void PublishData(Document doc, string filePath)
        {
            try
            {
                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .ToWorksets();

                var worksetInfo = new List<WorksetItem>();
                foreach (var w in worksets)
                {
                    // (Konrad) It turns out that for some reason Linked worksets
                    // contain things like CopyMonitor options and Sketch Planes. 
                    var worksetFilter = new ElementWorksetFilter(w.Id, false);
                    var planeFilter = new ElementClassFilter(typeof(SketchPlane), true);
                    var filter = new LogicalAndFilter(worksetFilter, planeFilter);
                    var count = new FilteredElementCollector(doc)
                        .WherePasses(filter)
                        .WhereElementIsNotElementType()
                        .GetElementCount();

                    worksetInfo.Add(new WorksetItem
                    {
                        Name = w.Name,
                        Count = count
                    });
                }

                var wItemData = new WorksetItemData
                {
                    CentralPath = filePath.ToLower(),
                    User = Environment.UserName.ToLower(),
                    Worksets = worksetInfo
                };
                if (!ServerUtilities.Post(wItemData, "worksets/itemcount", out WorksetItemData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Worksets Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}