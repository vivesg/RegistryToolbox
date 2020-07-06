using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public bool Equals(ModelRegistryKeyValues Value)
        {
            bool a = this.Name == Value.Name;
            bool b = this.Type == Value.Type;
            bool c = this.Value == Value.Value;
            return (a & b & c);
        }
    }
    public class ModelRegistryKey : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        private string _Name;
        private bool _diff;  //This is True if differences has been found 
        private ObservableCollection<ModelRegistryKey> _SubKeys;
        private ObservableCollection<ModelRegistryKeyValues> _SubKeysValues;

        public string Name { get => _Name; set => _Name = value; }
        public bool Diff {
            get => _diff; 
            set  {
                _diff = value;
                OnPropertyChanged("Diff");
            } 
        }

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
        public bool EqualsChilds(ModelRegistryKey NodeB)
        {
            bool cDiff = false;
            foreach (ModelRegistryKey key in this.Subkeys)
            {
                bool found = false;
                foreach (ModelRegistryKey keyb in NodeB.Subkeys)
                {
                    if (key.Name == keyb.Name) //subkey has been found
                    {
                        found = true;
                        if (key.EqualsChilds(keyb))
                        {
                            if (key.EqualsValues(keyb))
                            {
                                cDiff = false;
                            }
                            else
                            {
                                cDiff = true;
                            }     
                        }
                        else
                        {
                            cDiff = true;

                        }
                    }
                }
                if (!found)
                {
                    this.Diff = true; //this key was not found so differences on childs
               
                }
            }
            this.Diff = cDiff;    
            return !this.Diff;
        }

        public bool EqualsValues(ModelRegistryKey NodeB)
        {
            bool diffs = false;
            foreach (ModelRegistryKeyValues kvaluea in this.SubkeysValues)
            {
                bool found = false;
                foreach (ModelRegistryKeyValues kvalueb in NodeB.SubkeysValues)
                {
                    if (kvaluea.Name == kvalueb.Name)
                    {
                        found = true;
                        if (!kvaluea.Equals(kvalueb)) //There is no difference on the value
                        {
                            this.Diff = true;
                            diffs = true;
                        }
                     
                        break; // the value has been found
                    }
                }
                if (!found)
                {
                    //Non existent value in the other registry
                    this.Diff = true;
                    diffs = true;
                }
            }
            this.Diff = diffs;
            return !diffs;
        }


        public void FindDifferences(ModelRegistryKey NodeB)
        {
            bool res1 = false;
            bool res2 = false;
            if (!this.EqualsValues(NodeB))
            {
                res1 = true;


            }
            if (!this.EqualsChilds(NodeB))
            {
                res2 = true;
            }
            this.Diff = (res1 | res2);

        }

    }
}
