using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using CableTrayAnnotationHelper.MVVM;
using CableTrayAnnotationHelper.Events;
using System.IO;

namespace CableTrayAnnotationHelper
{
    public class App : IExternalApplication
    {
        public static App ThisApp;
        private static ViewCTAH _mMyFormCTAH;

        public Result OnStartup(UIControlledApplication a)
        {
            ThisApp = this;
            _mMyFormCTAH = null;

            RibbonPanel panel = RibbonPanel(a);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string[] cableTrayIconPath =
            [
                "CableTrayAnnotationHelper.Resources.cableTray32.png",
                "CableTrayAnnotationHelper.Resources.cableTray16.png"
            ];

            PushButtonData ButtonDataCTAH = new
            (
                "CableTraysAndConduits",
                "Лотки и\nкороба",
                thisAssemblyPath,
                "CableTrayAnnotationHelper.MVVM.CTAH"
            );

            string ToolTipCTAH = "Расстановка аннотаций лотков и коробов";
            CreateNewPushButton(panel, ButtonDataCTAH, ToolTipCTAH, cableTrayIconPath);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a) => Result.Succeeded;

        private static RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            const string TAB = "AlterTools";
            const string PANEL_NAME = "ЭОМ";

            a.CreateRibbonTab(TAB);
            a.CreateRibbonPanel(TAB, PANEL_NAME);

            return a.GetRibbonPanels(TAB).FirstOrDefault(p => p.Name == PANEL_NAME);
        }

        private static void CreateNewPushButton(RibbonPanel ribbonPanel, PushButtonData pushButtonData, string toolTip, string[] iconPath)
        {
            BitmapFrame bitmap_32 = GetEmbeddedImage(iconPath[0]);
            BitmapFrame bitmap_16 = GetEmbeddedImage(iconPath[1]);
            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;

            if (pushButton is not null)
            {
                pushButton.ToolTip = toolTip;
                pushButton.Image = bitmap_16;
                pushButton.LargeImage = bitmap_32;
            }
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