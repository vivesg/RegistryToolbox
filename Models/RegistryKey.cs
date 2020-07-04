using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RegistryToolbox.Models
{
    public class ModelRegistryKeyValues
    {
        private string _Name;
        private string _Type;
        private string _Value;
        private bool _Diff;

        public string Name { get => _Name; set => _Name = value; }
        public string Type { get => _Type; set => _Type = value; }
        public string Value { get => _Value; set => _Value = value; }
        public bool Diff { get => _Diff; set => _Diff = value; }

        public ModelRegistryKeyValues(string name, string type, string value)
        {
            _Name = name;
            _Type = type;
            _Value = value;
            _Diff = false;
        }
    }
    public class ModelRegistryKey
    {
        private string _Name;
        private bool _diff;  //This is True if differences has been found 
        private ObservableCollection<ModelRegistryKey> _SubKeys;
        private ObservableCollection<ModelRegistryKeyValues> _SubKeysValues;
        
        public string Name { get => _Name; set => _Name = value; }
        public bool Diff { get => _diff; set => _diff = value; }

        public ObservableCollection<ModelRegistryKey> Subkeys
        {
            get { return _SubKeys; }
            set { _SubKeys = value; }
        }
        public ObservableCollection<ModelRegistryKeyValues> SubkeysValues
        {
            get { return _SubKeysValues; }
            set { _SubKeysValues = value; }
        }

        public ModelRegistryKey(string name)
        {
            _Name = name;
            _diff = false;
            _SubKeys = new ObservableCollection<ModelRegistryKey>();
            _SubKeysValues = new ObservableCollection<ModelRegistryKeyValues>(); 
        }
    }
}
