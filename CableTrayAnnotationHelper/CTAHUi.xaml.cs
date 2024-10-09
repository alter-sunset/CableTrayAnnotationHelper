using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CableTrayAnnotationHelper
{
    public partial class CTAHUi : Window
    {
        private readonly EventHandlerCTAHUiArg _eventHandlerCTAHUiArg;
        private readonly Dictionary<Family, List<FamilySymbol>> _families;
        public Family Family;
        public FamilySymbol SymbolConduit;
        public FamilySymbol SymbolCableTray;

        public CTAHUi(EventHandlerCTAHUiArg eventHandlerCTAHUiArg, Dictionary<Family, List<FamilySymbol>> families)
        {
            InitializeComponent();

            _eventHandlerCTAHUiArg = eventHandlerCTAHUiArg;
            _families = families;

            ComboBoxFamilyName.ItemsSource = _families.Select(e => e.Key.Name);
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            const string msg = "\tПлагин предназначен для расстановки аннотативных линий по ГОСТ на лотки и короба для текущего вида." +
                "\n" +
                "\tПринцип работы с плагином достаточно прост. При его активации подгружаются названия семейств аннотативных узлов." +
                "\n" +
                "\tВам небходимо выбрать семейство \"Компонентов узла\" на основе линии. Далее поставьте галочки на те элементы, которые необходимо аннотировать," +
                " и выберите соответствующие типоразмеры узлов.";//дописать
            MessageBox.Show(msg, "Справка");
        }

        private void ComboBoxFamilyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Family = _families
                .FirstOrDefault(f => f.Key.Name == (string)ComboBoxFamilyName.SelectedItem)
                .Key;

            //don't look into this method, i'm myself ashamed for what i've done here
            ComboBoxCableTray.Items.Clear();
            ComboBoxConduit.Items.Clear();

            _families.FirstOrDefault(f => f.Key.Name == (string)ComboBoxFamilyName.SelectedValue)
                .Value
                .Select(f => ComboBoxCableTray.Items.Add(f.Name))
                .ToList();
            ComboBoxCableTray.SelectedIndex = 0;

            _families.FirstOrDefault(f => f.Key.Name == (string)ComboBoxFamilyName.SelectedValue)
                .Value
                .Select(f => ComboBoxConduit.Items.Add(f.Name))
                .ToList();
            ComboBoxConduit.SelectedIndex = 0;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (IsEverythingFilled())
            {
                _eventHandlerCTAHUiArg.Raise(this);
            }
            else
            {
                MessageBox.Show("Заполните необходимые поля!");
            }
        }

        private void ComboBoxCableTray_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SymbolCableTray = _families
                .First(f => f.Key == Family)
                .Value
                .FirstOrDefault(s => s.Name == (string)ComboBoxCableTray.SelectedItem);
        }

        private void ComboBoxConduit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SymbolConduit = _families
                .First(f => f.Key == Family)
                .Value
                .FirstOrDefault(s => s.Name == (string)ComboBoxConduit.SelectedItem);
        }

        private bool IsEverythingFilled()
        {
            if (ComboBoxFamilyName.SelectedItem == null
                || (CheckBoxConduit.IsChecked == false && CheckBoxCableTray.IsChecked == false)
                || (CheckBoxConduit.IsChecked == true && ComboBoxConduit.SelectedItem == null)
                || (CheckBoxCableTray.IsChecked == true && ComboBoxCableTray.SelectedItem == null))
            {
                return false;
            }
            return true;
        }
    }
}
