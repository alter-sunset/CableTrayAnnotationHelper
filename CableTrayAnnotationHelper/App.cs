using Autodesk.Revit.UI;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CableTrayAnnotationHelper
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonPanel panel = RibbonPanel(a);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData ButtonDataCTAH = new
            (
                "CableTraysAndConduits",
                "Лотки и\nкороба",
                thisAssemblyPath,
                "CableTrayAnnotationHelper.MVVM.CTAH"
            );

            string[] cableTrayIconPath =
            [
                "CableTrayAnnotationHelper.Resources.cableTray16.png",
                "CableTrayAnnotationHelper.Resources.cableTray32.png",
            ];
            string ToolTipCTAH = "Расстановка аннотаций лотков и коробов";

            CreateNewPushButton(panel, ButtonDataCTAH, ToolTipCTAH, cableTrayIconPath);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a) => Result.Succeeded;

        private static RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            const string TAB = "AlterTools";
            const string PANEL_NAME = "ЭОМ";

            try { a.CreateRibbonTab(TAB); }
            catch { }
            try { a.CreateRibbonPanel(TAB, PANEL_NAME); }
            catch { }

            return a.GetRibbonPanels(TAB).FirstOrDefault(p => p.Name == PANEL_NAME);
        }

        private static void CreateNewPushButton(RibbonPanel ribbonPanel, PushButtonData pushButtonData, string toolTip, string[] iconPath)
        {
            pushButtonData.ToolTip = toolTip;
            pushButtonData.Image = GetEmbeddedImage(iconPath[0]);
            pushButtonData.LargeImage = GetEmbeddedImage(iconPath[1]);

            ribbonPanel.AddItem(pushButtonData);
        }
        private static BitmapFrame GetEmbeddedImage(string name)
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