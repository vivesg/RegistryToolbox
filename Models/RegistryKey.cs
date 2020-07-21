using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegistryToolbox.Models
{
    public abstract class ModelRegistry {
       
        abstract public string Name { get; set; }
      

    }
    public class ModelRegistryKeyValues : ModelRegistry
    {

        private string _Name;
        private string _Type;
        private string _Value;
        private bool _Diff;

        public override string Name { get => _Name; set => _Name = value; }
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

    public class ModelRegistryKey : ModelRegistry, INotifyPropertyChanged
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

        public override string Name { get => _Name; set => _Name = value; }
        public bool Diff
        {
            get => _diff;
            set
            {
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
        public void SortValues()
        {
            var SubkeysValues = this.SubkeysValues.OrderBy(x => x.Name);
            this.SubkeysValues = new ObservableCollection<ModelRegistryKeyValues>(SubkeysValues);

        }
        public void SortKeys()
        {
            var Subkey = this.Subkeys.OrderBy(x => x.Name);
            this.Subkeys = new ObservableCollection<ModelRegistryKey>(Subkey);

        }
        public ModelRegistryKey(string name)
        {
            _Name = name;
            _diff = false;
            _SubKeys = new ObservableCollection<ModelRegistryKey>();
            _SubKeysValues = new ObservableCollection<ModelRegistryKeyValues>();

        }

        public ModelRegistryKey BinarySearch(ObservableCollection<ModelRegistryKey> list, string value)
        {
            if (list == null)
            {
                return null;
            }
            else
            {
                if (list.Count == 0)
                {
                    return null;
                }
                return BinarySearchAux(list, 0, list.Count - 1, value);
            }
        }
        private ModelRegistryKeyValues BinarySearchAux(ObservableCollection<ModelRegistryKeyValues> list, int infLim, int supLim, string value)
        {

            if (infLim < supLim)
            {
                int half = infLim + (supLim - infLim) / 2;
                if (String.Compare(list[half].Name, (value)) < 0) //Superior
                {
                    return BinarySearchAux(list, half + 1, supLim, value);
                }
                else //Inferior
                {
                    return BinarySearchAux(list, infLim, half, value);
                }
            }
            else
            {
                if (list[infLim].Name == value)
                {
                    return list[infLim];
                }

            }
            return null;
        } 
        private ModelRegistryKey BinarySearchAux(ObservableCollection<ModelRegistryKey> list, int infLim, int supLim, string value)
        {

            if (infLim < supLim)
            {
                int half = infLim + ((supLim - infLim) / 2);
                if (String.Compare(list[half].Name, (value)) < 0) //Superior
                {
                     return BinarySearchAux(list, half + 1, supLim, value);
                }
                else //Inferior
                {
                    return BinarySearchAux(list, infLim,  half, value);
                }
            }
            else
            {
                if (list[infLim].Name == value)
                {
                    return list[infLim];
                }

            }
            return null;
        }


        public ModelRegistryKeyValues BinarySearch(ObservableCollection<ModelRegistryKeyValues> list, string value)
        {
            if (list == null)
            {
                return null;
            }
            else
            {
                if (list.Count == 0)
                {
                    return null;
                }
                return BinarySearchAux(list, 0, list.Count - 1, value);
            }
          
        }
        public void Mark_Different(ModelRegistryKey key)
        {
            key.Diff = true;
            if (key.Subkeys.Count != 0){
               foreach(ModelRegistryKey skey in key.Subkeys)
                {
                    Mark_Different(skey);
                }
            }
        }
        public bool EqualsChilds(ModelRegistryKey NodeB)
        {

            bool cDiff = false;

            foreach (ModelRegistryKey key in this.Subkeys)
            {

               
                ModelRegistryKey keyb = this.BinarySearch(NodeB.Subkeys, key.Name);
               
                if (keyb != null){
                    if (key.EqualsChilds(keyb))
                    {
                        if (!key.EqualsValues(keyb))
                        {
                            cDiff = true;
                        }
                    }
                    else
                    {
                        cDiff = true;
                    }
                }
                else
                {
                    cDiff = true;
                    Mark_Different(key);  //as this does not exist on the other registry is different
                    this.Diff = true; //this key was not found so differences on childs
                }
            }
            this.Diff = cDiff;
            return !this.Diff;
        }

        public bool EqualsValues(ModelRegistryKey NodeB)
        {


            if (this._SubKeysValues.Count() != NodeB.SubkeysValues.Count())
            {
                this.Diff = true;
                return false;
            }
            foreach (ModelRegistryKeyValues KeyValA in this.SubkeysValues)
            {
                // bool found = false;

               
                
                ModelRegistryKeyValues KeyValB = this.BinarySearch(NodeB.SubkeysValues, KeyValA.Name);

                if (KeyValB != null)
                {
                    if (!KeyValA.Equals(KeyValB)) //There is  difference on the value
                    {
                        this.Diff = true;
                        return false;
                    }
                }
                else
                {
                    this.Diff = true;
                    return false;
                }
            }

            this.Diff = false;
            return true;
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
