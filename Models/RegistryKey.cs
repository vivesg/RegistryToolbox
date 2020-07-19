using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class ObservableDictionary<T> : INotifyPropertyChanged
    {

        Dictionary<String, T> dict;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public ObservableDictionary()
        {
            dict = new Dictionary<String, T>();
        }
        public T Get(string Index)
        {
            return dict[Index];
        }
        public void Clear()
        {
            this.dict.Clear();
        }
        public void Add(string index, T obj)
        {
            this.dict.Add(index, obj);
            OnPropertyChanged("dict");

        }
        public void Remove(string index)
        {
            this.dict.Remove(index);
            OnPropertyChanged("dict");
        }
        public List<String> Keys()
        {
            List<String> myKeys = dict.Keys.ToList();
            return myKeys;
        }
        public int Count()
        {
            return dict.Count();
        }




    }
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
        private ObservableDictionary<ModelRegistryKeyValues> _SubKeysValues;

        public string Name { get => _Name; set => _Name = value; }
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
        public ObservableDictionary<ModelRegistryKeyValues> SubkeysValues
        {
            get { return _SubKeysValues; }
            set { _SubKeysValues = value; }
        }

        public ModelRegistryKey(string name)
        {
            _Name = name;
            _diff = false;
            _SubKeys = new ObservableCollection<ModelRegistryKey>();
            _SubKeysValues = new ObservableDictionary<ModelRegistryKeyValues>();

        }
        public ModelRegistryKey BinarySearch(ObservableCollection<ModelRegistryKey> list, string value)
        {
            if (list == null)
            {
                return null;
            }
            else
            {
                if (list.Count <= 2)
                {
                    int half = (list.Count / 2) - 1;
                    ObservableCollection<ModelRegistryKey> chunklist = new ObservableCollection<ModelRegistryKey>();
                    if (list[half].Name[0] < value[0])
                    { //the superior half 
                        for (int i = half + 1; i < list.Count; i++)
                        {

                            chunklist.Add(list[i]);  //O(log n)
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= half; i++)
                        {
                            chunklist.Add(list[i]); // O(log n)
                        }

                    }
                    return BinarySearch(chunklist, value);
                }
                else
                {
                    if (list[0].Name == value)
                    {
                        return list[0];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
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


            if (this._SubKeysValues.Count() != NodeB.SubkeysValues.Count())
            {
                this.Diff = true;
                return false;
            }
            foreach (string indexA in this.SubkeysValues.Keys())
            {
                // bool found = false;
                ModelRegistryKeyValues KeyValA = this.SubkeysValues.Get(indexA);
                ModelRegistryKeyValues KeyValB = NodeB.SubkeysValues.Get(indexA);
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
