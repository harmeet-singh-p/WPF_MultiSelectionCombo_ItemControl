using DemoApp.Messages;
using DemoApp.Helper;
using System.Threading;
using System.Collections.ObjectModel;
using DemoApp.Models;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace DemoApp
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }

    internal class DemoAppViewModel : ViewModelBase
    {
        private ObservableCollection<Person> persons;
        private List<Person> _allPersons;
        private int selectedItemId;
        private ObservableCollection<FilerByInfo> filerByInfos;
        private string selectedItems;

        public DemoAppViewModel()
        {
            LoadData();
            SortByInfos = new List<SortByInfo> { new SortByInfo { Id = 1, Name = "Name" }, new SortByInfo { Id = 2, Name = "Country" } };
        }

        public ObservableCollection<Person> Persons { get => persons; set => persons = value; }
        public ObservableCollection<FilerByInfo> FilerByInfos { get => filerByInfos; set => filerByInfos = value; }

        public List<SortByInfo> SortByInfos { get; set; }
        public string SelectedItems
        {
            get => selectedItems;
            set
            {
                selectedItems = value;
                OnPropertyRaised("SelectedItems");
            }
        }

        public int SelectedItemId
        {
            get => selectedItemId;
            set
            {
                selectedItemId = value;
                OnPropertyRaised("SelectedItemId");
                OnSelectionChanged();
            }
        }

        private void OnSelectionChanged()
        {
            LoadData();
        }

        private async void LoadData()
        {
            if (persons == null)
            {
                filerByInfos = new ObservableCollection<FilerByInfo>();
                persons = new ObservableCollection<Person>();
                var request = new CSVReadRequestMessage("Data/PersonsDemo.csv", ',', true);
                CSVReaderHalper helper = new CSVReaderHalper();
                _allPersons = await helper.ReadCSVFile(request, CancellationToken.None);
                _allPersons.Select(_ => _.Country).Distinct().OrderBy(_ => _).ToList().ForEach(_ =>
                {
                    var obj = new FilerByInfo { Name = _, IsChecked = true };
                    obj.PropertyChanged += FilerByInfo_PropertyChanged;
                    filerByInfos.Add(obj);
                });
            }
            else
            {
                persons.Clear();
            }
            CreateFilerText();
            if (SelectedItemId == 1)
            {
                _allPersons.OrderBy(_ => _.Name).Where(FilterData).ToList().ForEach(_ => persons.Add(_));
            }
            else if (SelectedItemId == 2)
            {
                _allPersons.OrderBy(_ => _.Country).Where(FilterData).ToList().ForEach(_ => persons.Add(_));
            }
            else
            {
                _allPersons.Where(FilterData).ToList().ForEach(_ => persons.Add(_));
            }
        }

        private void FilerByInfo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            LoadData();
        }

        private void CreateFilerText()
        {
            if (FilerByInfos.Any(_ => _.IsChecked))
            {
                SelectedItems = string.Join(", ", FilerByInfos.Where(_ => _.IsChecked).Select(_ => _.Name));
            }
            else
            {
                SelectedItems = string.Empty;
            }
        }

        private bool FilterData(Person person)
        {
            if (FilerByInfos.Any(_ => _.IsChecked && _.Name == person.Country))
            {
                return true;
            }
            return false;
        }
    }

    internal class SortByInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    internal class FilerByInfo : ViewModelBase
    {
        private bool isChecked;

        public string Name { get; set; } = string.Empty;
        public bool IsChecked 
        { 
            get => isChecked; 
            set 
            { 
                isChecked = value;
                OnPropertyRaised("IsChecked");
            }
        }
    }
}
