using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using View = Autodesk.Revit.DB.View;

namespace ADM.CableTrayAnnotationHelper
{
    public static class Utils
    {
        public static void PlaceNewDetailLine(Document document, View view, FamilySymbol familySymbol, Element cableTray, string paramIdName)
        {
            Line line = Utils.GetTheLine(cableTray);
            DetailCurve detailLine = document.Create.NewDetailCurve(view, line);
            Line baseLine = detailLine.GeometryCurve as Line;

            FamilyInstance insertNew = document.Create
            .NewFamilyInstance(baseLine, familySymbol, view);

            insertNew.LookupParameter(paramIdName).Set(cableTray.UniqueId);

            document.Delete(detailLine.Id);
        }
        public static Line GetTheLine(Element element)
        {
            Line line = (element.Location as LocationCurve).Curve as Line;
            return line;
        }

        public static bool AreAligned(Element cableTray, Element detailLineInstance)
        {
            Line cableTrayLine = GetTheLine(cableTray);
            Line detailLine = GetTheLine(detailLineInstance);

            SetComparisonResult result = cableTrayLine.Intersect(detailLine);

            if (result == SetComparisonResult.Overlap)
            {
                return true;
            }
            return false;
        }

        public static void LogThreadInfo(string name = "")
        {
            Thread th = Thread.CurrentThread;
            Debug.WriteLine($"Task Thread ID: {th.ManagedThreadId}, Thread Name: {th.Name}, Process Name: {name}");
        }

        public static void HandleError(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.Source);
            Debug.WriteLine(ex.StackTrace);
        }

        public static BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                Stream s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }
    }
}
