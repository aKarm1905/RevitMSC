﻿using System.Linq;
using System.Xml;
using Dynamo.Models;

namespace Dynamo.Migration
{
    public class MigrationNode
    {
        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string name, string funcName)
        {
            return MigrateToDsFunction(data, "", name, funcName);
        }

        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string assembly, string name, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", name);
            element.SetAttribute("function", funcName);

            var lacingAttribute = xmlNode.Attributes["lacing"];
            if (lacingAttribute != null)
            {
                element.SetAttribute("lacing", lacingAttribute.Value);
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        protected static NodeMigrationData MigrateToDsVarArgFunction(
            NodeMigrationData data, string assembly, string name, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateVarArgFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", name);
            element.SetAttribute("function", funcName);
            element.SetAttribute("lacing", xmlNode.Attributes["lacing"].Value);

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
