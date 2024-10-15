using Autodesk.Revit.DB;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CableTrayAnnotationHelper.Events;

namespace CableTrayAnnotationHelper.MVVM
{
    public class ViewModelCTAH : INotifyPropertyChanged
    {
        private readonly EventHandlerCTAH _eventHandlerCTAH;
        private readonly Dictionary<Family, List<FamilySymbol>> _familyPairs;

        public ViewModelCTAH(EventHandlerCTAH eventHandler, Dictionary<Family, List<FamilySymbol>> familyPairs)
        {
            _eventHandlerCTAH = eventHandler;
            _familyPairs = familyPairs;
            _families = new(_familyPairs.Keys);
        }

        private const string V = "\n";
        private const string HELP_MESSAGE =
            "\tПлагин предназначен для расстановки аннотативных линий по ГОСТ на лотки и короба для текущего вида." + V +
            "\tПринцип работы с плагином достаточно прост. При его активации подгружаются наименования семейств аннотативных узлов." + V +
            "\tВам небходимо выбрать семейство \"Компонентов узла\" на основе линии. Далее поставьте галочки на те элементы, которые необходимо аннотировать," +
            " и выберите соответствующие типоразмеры узлов." + V +
            "\tВажный момент. Плагин ищет лотки и короба только на связанных видах в связанных файлах." +
            " То есть для текущего вида обязательно должен быть выбран связанный вид.";
        private RelayCommand _helpCommand;
        public virtual RelayCommand HelpCommand =>
            _helpCommand ??= new RelayCommand(obj => MessageBox.Show(HELP_MESSAGE, "Справка"));

        private RelayCommand _raiseEventCommand;
        public RelayCommand RaiseEventCommand =>
            _raiseEventCommand ??= new RelayCommand(obj => _eventHandlerCTAH.Raise(this));

        private bool _isViewEnabled = true;
        public bool IsViewEnabled
        {
            get => _isViewEnabled;
            set => SetProperty(ref _isViewEnabled, value);
        }

        private bool _includeCableTray = false;
        public bool IncludeCableTray
        {
            get => _includeCableTray;
            set => SetProperty(ref _includeCableTray, value);
        }

        private bool _includeConduit = false;
        public bool IncludeConduit
        {
            get => _includeConduit;
            set => SetProperty(ref _includeConduit, value);
        }

        private readonly ObservableCollection<Family> _families;
        public ObservableCollection<Family> Families => _families;

        private Family _selectedFamily;
        public Family SelectedFamily
        {
            get => _selectedFamily;
            set
            {
                SetProperty(ref _selectedFamily, value);
                UpdateFamilySymbols();
            }
        }

        private void UpdateFamilySymbols()
        {
            if (SelectedFamily is null) return;
            Symbols = new(_familyPairs.GetValueOrDefault(SelectedFamily));
            SelectedCableTray = null;
            SelectedConduit = null;
        }

        private ObservableCollection<FamilySymbol> _symbols;
        public ObservableCollection<FamilySymbol> Symbols
        {
            get => _symbols;
            set => SetProperty(ref _symbols, value);
        }

        private FamilySymbol _selectedCableTray;
        public FamilySymbol SelectedCableTray
        {
            get => _selectedCableTray;
            set => SetProperty(ref _selectedCableTray, value);
        }

        private FamilySymbol _selectedConduit;
        public FamilySymbol SelectedConduit
        {
            get => _selectedConduit;
            set => SetProperty(ref _selectedConduit, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}