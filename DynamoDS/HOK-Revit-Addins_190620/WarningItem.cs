﻿#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.MissionControl.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

#endregion

namespace HOK.MissionControl.Core.Schemas.Warnings
{
    public class WarningData
    {
        [JsonProperty("closedBy")] public string ClosedBy { get; set; }
        [JsonProperty("centralPath")] public string CentralPath { get; set; }
        [JsonProperty("newWarnings")] public IEnumerable<WarningItem> NewWarnings { get; set; }
        [JsonProperty("existingWarningIds")] public IEnumerable<string> ExistingWarningIds { get; set; }

        [JsonConstructor]
        public WarningData()
        {
        }

        public WarningData(string cb, string cp, IEnumerable<WarningItem> nw, IEnumerable<string> ew)
        {
            ClosedBy = cb.ToLower();
            CentralPath = cp.ToLower();
            NewWarnings = nw;
            ExistingWarningIds = ew;
        }
    }

    public class WarningItem : INotifyPropertyChanged
    {
        #region Properties

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        /// <summary>
        /// This is a special construct that helps compare warnings.
        /// UniqueId = FailureDefinitionId + string.Join(FailingElements)
        /// </summary>
        [JsonProperty("uniqueId")] public string UniqueId { get; set; }
        [JsonProperty("centralPath")]public string CentralPath { get; set; }
        [JsonProperty("failingElements")] public List<string> FailingElements { get; set; }
        [JsonProperty("descriptionText")] public string DescriptionText { get; set; }
        [JsonProperty("createdBy")] public string CreatedBy { get; set; } = "";
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonProperty("closedBy")] public string ClosedBy { get; set; } = "";
        [JsonProperty("closedAt")] public DateTime ClosedAt { get; set; }
        [JsonProperty("isOpen")] public bool IsOpen { get; set; } = true;
        [JsonProperty("updatedAt")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Constructors

        [JsonConstructor]
        public WarningItem()
        {
        }

        public WarningItem(FailureMessageAccessor msg, Document doc)
        {
            CentralPath = FileInfoUtil.GetCentralFilePath(doc).ToLower();
            FailingElements = msg.GetFailingElementIds().Select(x => doc.GetElement(x).UniqueId).ToList();
            DescriptionText = msg.GetDescriptionText();
            CreatedBy = Environment.UserName.ToLower();
            UniqueId = msg.GetFailureDefinitionId().Guid + string.Join("", FailingElements);
        }

        public WarningItem(FailureMessage msg, Document doc)
        {
            CentralPath = FileInfoUtil.GetCentralFilePath(doc).ToLower();
            FailingElements = msg.GetFailingElements().Select(x => doc.GetElement(x).UniqueId).ToList();
            DescriptionText = msg.GetDescriptionText();
            // (Konrad) I am not setting created by here on purpose. This will be set when warnings are reconciled.
            UniqueId = msg.GetFailureDefinitionId().Guid + string.Join("", FailingElements);
        }

        #endregion

        #region Utilities

        public override bool Equals(object obj)
        {
            var item = obj as WarningItem;
            return item != null && UniqueId.Equals(item.UniqueId);
        }

        public override int GetHashCode()
        {
            return UniqueId.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        #endregion
    }
}
