using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CableTrayAnnotationHelper
{
    public class App : IExternalApplication
    {
        public static App ThisApp;
        private static CTAHUi _mMyFormCTAH;

        public Result OnStartup(UIControlledApplication a)
        {
            ThisApp = this;
            _mMyFormCTAH = null;

            RibbonPanel panel = RibbonPanel(a);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string[] cableTrayIconPath =
            [
                "CableTrayAnnotationHelper.Resources.cableTray.png",
                "CableTrayAnnotationHelper.Resources.cableTray_16.png"
            ];

            PushButtonData CTAHButtonData = new
            (
                "Лотки и короба",
                "Лотки и\nкороба",
                thisAssemblyPath,
                "CableTrayAnnotationHelper.CTAH"
            );

            string CTAHToolTip = "Расстановка аннотаций лотков и коробов";
            CreateNewPushButton(panel, CTAHButtonData, CTAHToolTip, cableTrayIconPath);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a) => Result.Succeeded;

        public static void ShowFormCTAH(UIApplication uiApp)
        {
            CloseCurrentForm();
            EventHandlerCTAHUi evUi = new();

            Document document = uiApp.ActiveUIDocument.Document;

            Dictionary<Family, List<FamilySymbol>> families = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e.get_Parameter(BuiltInParameter.FAMILY_LINE_LENGTH_PARAM) is not null)
                .Cast<FamilySymbol>()
                .GroupBy(e => e.Family, new FamilyComparer())
                .ToDictionary(e => e.Key, e => e.ToList());

            _mMyFormCTAH = new CTAHUi(evUi, families) { Height = 240, Width = 800 };
            _mMyFormCTAH.Show();
        }
        private static void CloseCurrentForm()
        {
            if (_mMyFormCTAH is not null)
            {
                _mMyFormCTAH.Close();
                _mMyFormCTAH = null;
            }
        }

        public static RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            const string TAB = "AlterTools";
            const string PANEL_NAME = "ЭОМ";

            try
            {
                a.CreateRibbonTab(TAB);
            }
            catch { }

            // Try to create ribbon panel.
            try
            {
                a.CreateRibbonPanel(TAB, PANEL_NAME);
            }
            catch { }

            return a.GetRibbonPanels(TAB).FirstOrDefault(p => p.Name == PANEL_NAME);
        }

        static void CreateNewPushButton(RibbonPanel ribbonPanel, PushButtonData pushButtonData, string toolTip, string[] iconPath)
        {
            BitmapSource bitmap_32 = Utils.GetEmbeddedImage(iconPath[0]);
            BitmapSource bitmap_16 = Utils.GetEmbeddedImage(iconPath[1]);
            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;

            if (pushButton is not null)
            {
                pushButton.ToolTip = toolTip;
                pushButton.Image = bitmap_16;
                pushButton.LargeImage = bitmap_32;
            }
        }
    }
}