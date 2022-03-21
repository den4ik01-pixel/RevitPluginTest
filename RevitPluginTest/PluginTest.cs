using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
//using System.Windows;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Collections;
using Autodesk.Revit.ApplicationServices;
using System.IO;

namespace RevitPluginTest
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class PluginTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                List<Element> levels = getLevels(doc);
                int levelNum = levels.Count + 1;
                for(int i = 1; i <= levels.Count; i++)
                {
                    if (levels.FindIndex(level => level.Name == ("Level " + i)) == -1)
                    {
                        levelNum = i;
                        break;
                    }
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("test");

                    Level level = Level.Create(doc, levelNum * 5);
                    if (level == null)
                        throw new Exception("Level creation failed");
                    level.Name = "Level " + (levelNum);
                    Parameter param = level.LookupParameter("Comment");
                    if (param == null)
                        throw new Exception("You have to create a project parameter named \"Comment\" for levels to use this script");
                    param.Set("Created by plugin");

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }

            return Result.Succeeded;
        }

        public List<Element> getLevels(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementClassFilter levelsFilter = new ElementClassFilter(typeof(Level));
            List<Element> elems = collector.WherePasses(levelsFilter).ToElements() as List<Element>;

            for(int i = 0; i < elems.Count;)
            {
                Parameter param = elems[i].LookupParameter("Comment");
                if (param == null || param.AsString() != "Created by plugin")
                {
                    elems.RemoveAt(i);
                    continue;
                }

                i++;
            }

            return elems;
        }
    }
}

