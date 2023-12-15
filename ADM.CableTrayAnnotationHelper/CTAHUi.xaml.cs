using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ADM.CableTrayAnnotationHelper
{
    public partial class CTAHUi : Window
    {
        private readonly EventHandlerCTAHUiArg _eventHandlerCTAHUiArg;

        private Dictionary<Family, List<FamilySymbol>> _families;

        public CTAHUi(UIApplication uiApp, EventHandlerCTAHUiArg eventHandlerCTAHUiArg, Dictionary<Family, List<FamilySymbol>> families)
        {
            InitializeComponent();

            _eventHandlerCTAHUiArg = eventHandlerCTAHUiArg;
            _families = families;

            ComboBoxFamilyName.ItemsSource = _families.Select(e => e.Key.Name);
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            string msg = "\tПлагин предназначен для расстановки аннотативных линий по ГОСТ на лотки и короба для текущего вида." +
                "\n" +
                "\tПринцип работы с плагином достаточно прост. При его активации подгружаются названия семейств аннотативных узлов." +
                "\n" +
                "\tВам небходимо выбрать семейство \"Компонентов узла\" на основе линии. Далее поставьте галочки на те элементы, которые необходимо аннотировать," +
                " и выберите соответствующие типоразмеры узлов.";//дописать
            MessageBox.Show(msg, "Справка");

        }

        private void ComboBoxFamilyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //don't look into this method, i'm myself ashamed for what i've done here
            ComboBoxCableTray.ItemsSource = null;
            ComboBoxConduit.ItemsSource = null;

            ComboBoxCableTray.Items.Clear();
            ComboBoxConduit.Items.Clear();

            ComboBoxCableTray.ItemsSource = _families
                .First(f => f.Key.Name == (string)ComboBoxFamilyName.SelectedValue)
                .Value
                .Select(f => f.Name);

            ComboBoxConduit.ItemsSource = _families
                .First(f => f.Key.Name == (string)ComboBoxFamilyName.SelectedValue)
                .Value
                .Select(f => f.Name);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            _eventHandlerCTAHUiArg.Raise(this);
        }
    }
}
