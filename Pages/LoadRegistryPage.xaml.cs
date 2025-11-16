using Registry;
using Registry.Abstractions;
using RegistryToolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegistryToolbox.Pages
{
    /// <summary>
    /// Interaction logic for LoadRegistryPage.xaml
    /// </summary>
    public partial class LoadRegistryPage : Page
    {

        public bool VISIBLE;
        Registry.RegistryHive Registry1;
        private List<ModelRegistryKey> lastselected1;
        private ObservableCollection<ModelRegistryKey> _Hive1;
        private ObservableCollection<ModelRegistryKey> lastparentselected1;


        public LoadRegistryPage()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                var window = Window.GetWindow(this); // Get the parent Window
                if (window != null)
                {
                    Wpf.Ui.Appearance.SystemThemeWatcher.Watch(
                        window,                                 // Pass Window instance
                        Wpf.Ui.Controls.WindowBackdropType.Acrylic,
                        true
                    );
                }
            };

            
            _Hive1 = new ObservableCollection<ModelRegistryKey>();
            Reg1Tree.DataContext = Hive1;
            Reg1Tree.ItemsSource = Hive1;

          
            OpenFile();
        }

        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent == null)
                {
                    return null;
                }
            }

            return parent as ItemsControl;
        }

        private void navigation_on_tree(int tree, string Key)
        {
            ObservableCollection<ModelRegistryKey> pHive1;
            ref TreeView RegTree = ref Reg1Tree;
            ref ObservableCollection<ModelRegistryKey> Hive = ref _Hive1;
            ref List<ModelRegistryKey> clastselected = ref lastselected1;
            ref ObservableCollection<ModelRegistryKey> clastparentselected = ref lastparentselected1;
           
         
                clastselected = ref lastselected1;
                clastparentselected = ref lastparentselected1;
                Hive = ref _Hive1;
                RegTree = ref Reg1Tree;
         

            ModelRegistryKey selected = (ModelRegistryKey)RegTree.SelectedItem;
            var tva = FindTviFromObjectRecursive(RegTree, selected);
            int i = 0;
            if (!tva.IsExpanded)
            {
                ItemsControl parent = GetSelectedTreeViewItemParent(tva);
                if (parent != null)
                {
                    TreeViewItem treeitem = parent as TreeViewItem;
                    ModelRegistryKey ok = treeitem.Header as ModelRegistryKey;
                    pHive1 = ok.Subkeys;
                }
                else
                {
                    pHive1 = Hive;
                }
            }
            else
            {
                ItemsControl parent = GetSelectedTreeViewItemParent(tva);
                ModelRegistryKey ok = tva.Header as ModelRegistryKey;
                pHive1 = ok.Subkeys;
            }
            foreach (ModelRegistryKey subkey in pHive1)
                if (subkey.Name[0].ToString().ToUpper() == Key)
                    i++;
            if (clastselected.Count == i)
            {
                clastselected.Clear();
            }
            if (clastparentselected != pHive1)
            {
                clastselected.Clear();
            }

            clastparentselected = pHive1;
            foreach (ModelRegistryKey subkey in pHive1)
            {
                if (subkey.Name[0].ToString().ToUpper() == Key & !clastselected.Contains(subkey))
                {
                    var tvi = FindTviFromObjectRecursive(RegTree, subkey);
                    if (tvi != null)
                    {
                        tvi.IsSelected = true;

                    }
                    clastselected.Add(subkey);
                    break;
                }
            }

        }

        private void uispliter_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeWE;
        }


        private void GridSplitter_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        public ObservableCollection<ModelRegistryKey> Hive1
        {
            get { return _Hive1; }
            set { _Hive1 = value; }
        }
       

        public void CleanMemory()
        {
            this.Registry1 = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;//put here your logic
            e.Handled = true;
        }

        private void Reg1Tree_KeyDown(object sender, KeyEventArgs e)
        {

            navigation_on_tree(1, e.Key.ToString());
        }


        private void OpenFile()
        {
            

            _Hive1.Clear();
          

            Reg1Values.DataContext = null;

            gridClientsContainer1.Visibility = Visibility.Visible;
         //  gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 2);
   
            Reg1Tree.Visibility = Visibility.Visible;

           
            txtpath1.Text = "";
   


            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "Registry Binary File";
            string path = "";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    path = openFileDialog.FileName;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
                MessageBox.Show("Please wait", "Processing your file", MessageBoxButton.OK, MessageBoxImage.Information);
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                File_Load(path, 1);
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;

            }
        }

        public static TreeViewItem FindTviFromObjectRecursive(ItemsControl ic, object o)
        {
            //Search for the object model in first level children (recursively)
            TreeViewItem tvi = ic.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
            if (tvi != null) return tvi;
            //Loop through user object models
            foreach (object i in ic.Items)
            {
                //Get the TreeViewItem associated with the iterated object model
                TreeViewItem tvi2 = ic.ItemContainerGenerator.ContainerFromItem(i) as TreeViewItem;
                if (tvi2 != null)
                {
                    tvi = FindTviFromObjectRecursive(tvi2, o);
                }
                if (tvi != null)
                    return tvi;
            }
            return null;
        }
        private void loadtable(ModelRegistryKey key, int tabla)
        {

            DataGrid currentTable = Reg1Values;


            ModelRegistryKey Selected = key;
            if (Selected == null)
                return;

            currentTable.DataContext = null;
            currentTable.DataContext = Selected.SubkeysValues;
            currentTable.Columns.RemoveAt(3);
            currentTable.Columns.RemoveAt(3);
            foreach (DataGridColumn column in currentTable.Columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);
            }

            currentTable.Items.Refresh();

        }
        private void Reg1Values_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ModelRegistryKeyValues values = (ModelRegistryKeyValues)Reg1Values.SelectedItem;
            if (values == null)
            {
                return;
            }
            if (values.Type == "RegDword")
            {
                ValueInspector vi = new ValueInspector(values);
                vi.ShowDialog();
            }
        }

        ItemsControl GetParentItem(ItemsControl nodo)
        {
            return ItemsControl.ItemsControlFromItemContainer(nodo);

        }

        private void File_Load(string path, int registryfile)
        {

            var registryHive = new RegistryHive(path);
            try
            {
                
                    Registry1 = registryHive;
                    registryHive.ParseHive();
                    Drawhive(Registry1.Root, _Hive1);
                
            }
            catch (Exception)
            {
                MessageBox.Show("The file you selected for registry it is not a registry binary file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reg1Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
       
            ModelRegistryKey Selected = (ModelRegistryKey)((TreeView)sender).SelectedItem;
            if (Selected == null)
                return;

            var tva = FindTviFromObjectRecursive(Reg1Tree, Selected);
            txtpath1.Text = GetFullPath(tva);

            loadtable(Selected, 1);
            UpdateLayout();
         
           
        }
        public string GetFullPath(TreeViewItem node)
        {
            if (node == null)
                return "";
            var result = ((ModelRegistryKey)(node.Header)).Name;
            for (var i = GetParentItem(node); i != null; i = GetParentItem(i))
            {
                if (i is TreeViewItem)
                {
                    result = ((ModelRegistryKey)(((TreeViewItem)i).Header)).Name + "\\" + result;
                }
            }
            return result;
        }

        private ObservableCollection<ModelRegistryKey> Drawhive(RegistryKey bKey, ObservableCollection<ModelRegistryKey> mKey)
        {

            foreach (RegistryKey rk in bKey.SubKeys)
            {
                ModelRegistryKey current = new ModelRegistryKey(rk.KeyName);
                foreach (KeyValue value in rk.Values)
                {
                    ModelRegistryKeyValues currentvalue = null;
                    if (value.ValueType == "RegDword")
                    {
                        string hexvalue = ByteArrayToString(value.ValueDataRaw);
                        string val = "0x" + hexvalue + " (" + value.ValueData + ")";
                        currentvalue = new ModelRegistryKeyValues(value.ValueName, value.ValueType, val, value.ValueDataRaw);
                    }
                    else
                    {
                        currentvalue = new ModelRegistryKeyValues(value.ValueName, value.ValueType, value.ValueData, value.ValueDataRaw);
                    }
                    current.SubkeysValues.Add(currentvalue);
                }
                Drawhive(rk, current.Subkeys);
                current.SortValues();
                current.SortKeys();
                mKey.Add(current);
            }
            return mKey;

        }
        private string ByteArrayToString(byte[] ba)
        {

            Array.Reverse(ba, 0, ba.Length);

            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }


    }
}
