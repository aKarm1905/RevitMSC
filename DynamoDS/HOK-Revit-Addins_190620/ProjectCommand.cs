using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows;

namespace HOK.ModelManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class ProjectCommand:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                ManagerWindow managerWindow = new ManagerWindow(m_app, ModelManagerMode.ProjectReplication);
                Nullable<bool> dlgResult = managerWindow.ShowDialog();
              
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize Project Replication.\n"+ex.Message, "Project Replication",MessageBoxButton.OK, MessageBoxImage.Warning);
                return Result.Cancelled;
            }
            
        }
    }
}
