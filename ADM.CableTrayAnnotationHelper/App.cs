﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ADM.CableTrayAnnotationHelper
{
    public class App : IExternalApplication
    {
        public static App ThisApp;

        public static CTAHUi _mMyFormCTAH;

        public Result OnStartup(UIControlledApplication a)
        {
            _mMyFormCTAH = null;
            ThisApp = this;

            RibbonPanel panel = RibbonPanel(a);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            string[] cableTrayIconPath = { "ADM.CableTrayAnnotationHelper.cableTray.png", "ADM.CableTrayAnnotationHelper.cableTray_16.png" };

            PushButtonData CTAHButtonData = new PushButtonData(
                   "Лотки и короба",
                   "Лотки и\nкороба",
                   thisAssemblyPath,
                   "ADM.CableTrayAnnotationHelper.CTAH");

            string CTAHToolTip = "Расстановка аннотаций лотков и коробов";
            CreateNewPushButton(panel, CTAHButtonData, CTAHToolTip, cableTrayIconPath);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        public void ShowFormCTAH(UIApplication uiApp)
        {
            if (_mMyFormCTAH != null && _mMyFormCTAH == null) return;

            EventHandlerCTAHUiArg evUi = new EventHandlerCTAHUiArg();

            Application application = uiApp.Application;
            UIDocument uIDocument = uiApp.ActiveUIDocument;
            Document document = uIDocument.Document;

            Dictionary<Family, List<FamilySymbol>> families = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e.get_Parameter(BuiltInParameter.FAMILY_LINE_LENGTH_PARAM) != null)
                .Where(e => e != null)
                .Select(e => e as FamilySymbol)
                .GroupBy(e => e.Family, new FamilyComparer())
                .ToDictionary(e => e.Key, e => e.ToList());

            _mMyFormCTAH = new CTAHUi(uiApp, evUi, families) { Height = 240, Width = 800 };
            _mMyFormCTAH.Show();
        }

        #region Ribbon Panel

        public RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            string tab = "ADM"; // Tab name
            // Empty ribbon panel 
            RibbonPanel ribbonPanel = null;
            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch (Exception ex)
            {
                Utils.HandleError(ex);
            }

            // Try to create ribbon panel.
            try
            {
                RibbonPanel panel = a.CreateRibbonPanel(tab, "ЭОМ");
            }
            catch (Exception ex)
            {
                Utils.HandleError(ex);
            }

            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels.Where(p => p.Name == "ЭОМ"))
            {
                ribbonPanel = p;
            }

            //return panel 
            return ribbonPanel;
        }

        #endregion
        #region PushButton

        static void CreateNewPushButton(RibbonPanel ribbonPanel, PushButtonData pushButtonData, string toolTip, string[] iconPath)
        {
            BitmapSource bitmap_32 = Utils.GetEmbeddedImage(iconPath[0]);
            BitmapSource bitmap_16 = Utils.GetEmbeddedImage(iconPath[1]);
            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;
            pushButton.ToolTip = toolTip;
            pushButton.Image = bitmap_16;
            pushButton.LargeImage = bitmap_32;
        }
        #endregion
    }
}
