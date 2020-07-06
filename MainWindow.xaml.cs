using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Converters;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using Registry;
using Registry.Abstractions;
using RegistryToolbox.Models;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Registry.RegistryHive Registry1 = null;
        Registry.RegistryHive Registry2 = null;

        string actualpath1 = "";
        string actualpath2 = "";
        private ObservableCollection<ModelRegistryKey> _Hive1;
        private ObservableCollection<ModelRegistryKey> _Hive2;
        public ObservableCollection<ModelRegistryKey> Hive1
        {
            get { return _Hive1; }
            set { _Hive1 = value; }
        }
        public ObservableCollection<ModelRegistryKey> Hive2
        {
            get { return _Hive2; }
            set { _Hive2 = value; }
        }
        public void CleanMemory()
        {
            this.Registry1 = null;
            this.Registry2 = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public MainWindow()
        {
            InitializeComponent();
            _Hive1 = new ObservableCollection<ModelRegistryKey>();
            _Hive2 = new ObservableCollection<ModelRegistryKey>();


            Reg1Tree.Visibility = Visibility.Hidden;
            gridClientsContainer1.Visibility = Visibility.Hidden;
            Reg2Tree.Visibility = Visibility.Hidden;
            Reg2Values.Visibility = Visibility.Hidden;
            btnalign.Visibility = Visibility.Hidden;
            btnCompareKeyandsub.Visibility = Visibility.Hidden;
            btnload2.Visibility = Visibility.Hidden;

            Reg1Tree.DataContext = Hive1;
            Reg1Tree.ItemsSource = Hive1;
            Reg2Tree.DataContext = Hive2;
            Reg2Tree.ItemsSource = Hive2;

            lastselected1 = new List<ModelRegistryKey>();
            lastparentselected1 = new ObservableCollection<ModelRegistryKey>();
            lastselected2 = new List<ModelRegistryKey>();
            lastparentselected2 = new ObservableCollection<ModelRegistryKey>();
        }
        private void btnOpenReg_Click(object sender, RoutedEventArgs e)
        {
            _Hive1.Clear();
            _Hive2.Clear();
           // Reg1Values.Items.Clear();
            Reg1Values.DataContext = null; 
            //  lbloutput.Content = ("UserName: {0}", Environment.UserName);
            gridClientsContainer1.Visibility = Visibility.Visible;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 2);
            Reg1Tree.Visibility = Visibility.Visible;
            Reg1Tree.SetValue(Grid.ColumnSpanProperty, 2);


            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "Registry2 Binary File";
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
                File_Load(path, 1);
            }
          
          
        }
        private void File_Load(string path, int registryfile)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (registryfile == 1)
            {

                string testFile = path;
                try
                {
                    var registryHive = new RegistryHive(testFile);
                    Registry1 = registryHive;
                    registryHive.ParseHive();

                    Drawhive(Registry1.Root, _Hive1);

                }
                catch (Exception)
                {
                    MessageBox.Show("The file you selected for registry 1 it is not a registry file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string testFile = path;
                try
                {
                    var registryHive = new RegistryHive(testFile);
                    Registry2 = registryHive;
                    registryHive.ParseHive();
                    Drawhive(Registry2.Root, _Hive2);
                }
                catch (Exception)
                {
                    MessageBox.Show("The file you selected for registry 2 it is not a registryfile", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            Mouse.OverrideCursor = null;
            this.CleanMemory();
            if (this.Registry1 == null)
            {
                MessageBox.Show("memoria limpia");
            }
        }
        private RegistryKey Subnode_Route(string route, int currentreg)
        {

            string[] routes = route.Split('\\');
            RegistryKey actual = null;
            if (currentreg == 1)
            {
                actual = this.Registry1.Root;
            }
            else
            {
                actual = this.Registry2.Root;
            }

            foreach (string vroute in routes)
            {
                actual = this.Look_Leaf(vroute, actual);
            }
            return actual;
        }
        private RegistryKey Look_Leaf(string ruta, RegistryKey regKey)
        {

            foreach (RegistryKey subkey in regKey.SubKeys)
            {
                if (subkey.KeyName == ruta)
                {
                    regKey = subkey;
                    break;
                }
            }
            return regKey;
        }
        private TreeView MyTreeViewParent(object child)
        {
            if (child == null)
            {
                return null;
            }
            else
            {
                if (child is TreeView)
                    return (TreeView)child;
                else
                {
                    return MyTreeViewParent(((TreeViewItem)child).Parent);
                }
            }
        }
        // THIS NEEDS TO BE OPTIMIZED
        private long millisec = 0;

        ItemsControl GetParentItem(ItemsControl nodo)
        {
            return ItemsControl.ItemsControlFromItemContainer(nodo);

        }
        private void loadtable(ModelRegistryKey key, int tabla)
        {
            DataTable dt = new DataTable();
            DataRow dr;
            DataColumn dc;
            dc = new DataColumn("Name", typeof(String));
            dt.Columns.Add(dc);

            dc = new DataColumn("Type", typeof(String));
            dt.Columns.Add(dc);

            dc = new DataColumn("Value", typeof(String));
            dt.Columns.Add(dc);
            
            DataGrid currentTable = Reg1Values;
            if (tabla == 2)
                currentTable = Reg2Values;


            ModelRegistryKey Selected = key;
            if (Selected == null)
                return;
            foreach (ModelRegistryKeyValues KeyValue in Selected.SubkeysValues)
            {
                dr = dt.NewRow();
                dr[0] = KeyValue.Name;
                dr[1] = KeyValue.Type;
                dr[2] = KeyValue.Value;
                dt.Rows.Add(dr);
            }
            currentTable.AutoGenerateColumns = false;
            currentTable.DataContext = dt;

            foreach (DataGridColumn column in currentTable.Columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);
            }

        }
        public string GetFullPath(TreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException();
            var result = Convert.ToString(node.Header);
            for (var i = GetParentItem(node); i != null; i = GetParentItem(i))
            {
                if (i is TreeViewItem)
                {
                    result = ((TreeViewItem)i).Header + "\\" + result;
                }
            }
            return result;
        }

        public TreeViewItem Select_FullPath(string path, TreeViewItem pleaf, TreeView ptree)
        {
            if (path == "")
            {

                return null;
            }
            string[] routes;
            routes = path.Split('\\');
            ItemCollection subleafs;
            string actualp = routes[0];
            if (pleaf == null)
            {
                subleafs = ptree.Items;
            }
            else
            {
                if (pleaf.Header.ToString() == routes[0])
                    return pleaf;
                subleafs = pleaf.Items;

            }


            foreach (TreeViewItem leaf in subleafs)
            {
                if (leaf.Header.ToString() == actualp.ToString())
                {
                    if (routes.Length > 1)
                    {
                        leaf.IsExpanded = true;
                        leaf.IsSelected = true;
                        leaf.Focus();
                        return Select_FullPath(path.Substring(path.IndexOf(@"\") + 1), leaf, null);
                    }
                    else
                    {
                        leaf.IsExpanded = true;
                        leaf.IsSelected = true;
                        leaf.Focus();
                        return leaf;
                    }
                }
            }

            return null;
        }

        private RegistryKey ActiveRegistry(int value)
        {
            if (value == 1)
                return Registry1.Root;
            return Registry2.Root;

        }

        private void Drawhive(RegistryKey bKey, ObservableCollection<ModelRegistryKey> mKey)
        {
           
            foreach (RegistryKey rk in bKey.SubKeys)
            {
                ModelRegistryKey current = new ModelRegistryKey(rk.KeyName);
                foreach (KeyValue value in rk.Values)
                {
                    ModelRegistryKeyValues currentvalue = new ModelRegistryKeyValues(value.ValueName, value.ValueType, value.ValueData);
                    current.SubkeysValues.Add(currentvalue);
                }
                Drawhive(rk, current.Subkeys);
                mKey.Add(current);
            }
          
        }

        private void btnCMPReg_Click(object sender, RoutedEventArgs e)
        {
          

            _Hive1.Clear();
            _Hive2.Clear();
            
            Reg1Values.DataContext = null;
            Reg2Values.DataContext = null;
            // lbloutput.Content = ("UserName: {0}", Environment.UserName);
            gridClientsContainer1.Visibility = Visibility.Visible;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 1);
            Reg1Tree.Visibility = Visibility.Visible;
            Reg1Tree.SetValue(Grid.ColumnSpanProperty, 1);
            Reg2Values.Visibility = Visibility.Visible;
            Reg2Tree.Visibility = Visibility.Visible;
            btnalign.Visibility = Visibility.Visible;
            btnCompareKeyandsub.Visibility = Visibility.Visible;
            btnload2.Visibility = Visibility.Visible;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "Registry2 Binary File";
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
                File_Load(path, 1);
                MessageBox.Show("Select Second File to analyze", "Please select a second registry file to analyze", MessageBoxButton.OK, MessageBoxImage.Information);
                Microsoft.Win32.OpenFileDialog openFileDialog2 = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Title = "Registry2 Binary File";
                path = "";
                if (openFileDialog2.ShowDialog() == true)
                {
                    try
                    {

                        path = openFileDialog2.FileName;
                        File_Load(path, 2);
                        this.CleanMemory();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }
            }
        }
        private Boolean CompareRow(ItemsControl row1, ItemsControl row2)
        {
            return false;
        }
        private void Paint_Differences(DataGrid values1, DataGrid values2)
        {
            int j = 0;
            int i = 0;
            foreach (DataRowView row2 in values2.Items)
            {
                j = 0;
                Boolean found2 = false;
                foreach (DataRowView row1 in values1.Items)
                {
                    //KEY IS THE SAME CHECK VALUE
                    if (row2.Row.ItemArray[0].ToString() == row1.Row.ItemArray[0].ToString())
                    {
                        //KEY IS DIFFERENT VALUE ON REG2
                        if (row2.Row.ItemArray[2].ToString() != row1.Row.ItemArray[2].ToString())
                        {
                            DataGridRow row = (DataGridRow)values2.ItemContainerGenerator.ContainerFromIndex(i);
                            row.Background = new SolidColorBrush(Color.FromRgb(255, 204, 203));
                        }
                        else
                        {
                            DataGridRow rowclean = (DataGridRow)values2.ItemContainerGenerator.ContainerFromIndex(i);
                            rowclean.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        }
                        found2 = true;
                    }

                    j++;
                }
                //This value does not exists on the registry 1
                if (!found2)
                {
                    DataGridRow row = (DataGridRow)values2.ItemContainerGenerator.ContainerFromIndex(i);
                    row.Background = Brushes.LightCyan;
                }
                i++;
            }
        }
        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {

            Reg1Values.UpdateLayout();
            Reg2Values.UpdateLayout();
            Paint_Differences(Reg2Values, Reg1Values);
            Paint_Differences(Reg1Values, Reg2Values);

        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {



            About cw = new About();
            cw.ShowInTaskbar = false;
            cw.Owner = Application.Current.MainWindow;
            cw.Show();
        }

        private void btnload1_Click(object sender, RoutedEventArgs e)
        {

            TreeViewItem item = Select_FullPath(txtpath1.Text, null, Reg1Tree);

            ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
            if (item == null)
            {
                MessageBox.Show("Invalid path", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
                item.IsSelected = true;
                item.Focus();
                item.IsExpanded = true;
                ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
            }

        }

        private void btnload2_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = Select_FullPath(txtpath2.Text, null, Reg2Tree);

            ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
            if (item == null)
            {
                MessageBox.Show("Invalid path", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
                item.IsSelected = true;
                item.Focus();
                item.IsExpanded = true;
                ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
            }
        }

        private void btnalign_Click(object sender, RoutedEventArgs e)
        {
            txtpath2.Text = txtpath1.Text;
            btnload1_Click(null, null);
            btnload2_Click(null, null);

        }
        private void PaintDifferencesTree(TreeViewItem pNode1, TreeViewItem pNode2)
        {
            if (pNode1 == null)
            {
                if (pNode2 != null)
                {
                    pNode2.Background = new SolidColorBrush(Color.FromRgb(224, 255, 255)); //CYAN
                }
            }
            else
            {
                if (pNode2 == null)
                {
                    pNode1.Background = new SolidColorBrush(Color.FromRgb(224, 255, 255)); //CYAN
                }
                else // BOTH ARE NOT NULL THEY HAVE VALID
                {
                    string path1 = GetFullPath(pNode1);
                    string path2 = GetFullPath(pNode2);
                    RegistryKey keyA = Registry1.GetKey(path1);
                    RegistryKey keyB = Registry2.GetKey(path2);
                    if (!Compare_KeyEq(keyA, keyB))
                    {
                        {
                            pNode1.Background = new SolidColorBrush(Color.FromRgb(255, 204, 203)); //LIGHT RED
                            pNode2.Background = new SolidColorBrush(Color.FromRgb(255, 204, 203)); //LIGHT RED

                        }
                    }
                    foreach (TreeViewItem item1 in pNode1.Items)
                    {
                        foreach (TreeViewItem item2 in pNode2.Items)
                        {
                            if (item1.Header.ToString() == item2.Header.ToString())
                            {

                                PaintDifferencesTree(item1, item2);

                            }

                        }
                    }
                }


            }
        }
        private void btnCompareKeyandsub_Click(object sender, RoutedEventArgs e)
        {

            //Reg1Values.UpdateLayout();
            //Reg2Values.UpdateLayout();
            //Paint_Differences(Reg2Values, Reg1Values);
            //Paint_Differences(Reg1Values, Reg2Values);

            //TreeViewItem node1 = (TreeViewItem)Reg1Tree.SelectedItem;
            //TreeViewItem node2 = (TreeViewItem)Reg2Tree.SelectedItem;
            //PaintDifferencesTree(node1, node2);
           // string path = this.Reg2Tree.SelectedValuePath;
             ((ModelRegistryKey)this.Reg2Tree.SelectedItem).FindDifferences((ModelRegistryKey)this.Reg1Tree.SelectedItem);
            //Reg2Tree.Items.Refresh();
           // Reg2Tree.SelectedValuePath = path;
           // Reg2Tree.UpdateLayout();

        }
        private bool Compare_RegkeyEq(KeyValue pvalues1, KeyValue pvalues2)
        {
            bool nameeq = pvalues1.ValueName == pvalues2.ValueName;
            bool typeeq = pvalues1.ValueType == pvalues2.ValueType;
            bool valueeq = pvalues1.ValueData == pvalues2.ValueData;

            return (nameeq & typeeq & valueeq);
        }
        private bool Compare_KeyAndSubkeys(RegistryKey pValues1, RegistryKey pValues2)
        {
            if (pValues1.SubKeys.Count == 0)
            {
                if (pValues2.SubKeys.Count == 0)
                {
                    return Compare_KeyEq(pValues1, pValues2);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (pValues2.SubKeys.Count == 0)
                {
                    return false;
                }
                {
                    //this case is both trees have leafs
                    Boolean equals = true;
                    foreach (RegistryKey subkey1 in pValues1.SubKeys)
                    {
                        Boolean found = false;
                        foreach (RegistryKey subkey2 in pValues2.SubKeys)
                        {
                            if (subkey1.KeyName == subkey2.KeyName)
                            {
                                equals = equals & Compare_KeyAndSubkeys(subkey1, subkey2);
                                found = true;
                            }
                        }
                        if (found == false) //This is differente because we found a key with different name
                        {
                            return false;
                        }
                        if (!equals) // If equals is false we found a difference
                        {
                            return false;
                        }

                    }
                }
                return true;
            }
        }

        private bool Compare_KeyEq(RegistryKey pValues1, RegistryKey pValues2)
        {
            if (pValues1.Values.Count != pValues2.Values.Count) //If different number of values we found differences
            {
                return false;
            }
            else
            {
                foreach (KeyValue key1 in pValues1.Values)
                {
                    foreach (KeyValue key2 in pValues2.Values)
                    {
                        if (key1.ValueName == key2.ValueName)
                        {
                            if (!Compare_RegkeyEq(key1, key2))
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        private void btnExportReg_Click(object sender, RoutedEventArgs e)
        {
            if (Registry1 == null)
            {
                MessageBox.Show("Please Load a registry hive first before exporting", "Registry not loaded", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog1.Filter = ".Reg File|*.reg";
            saveFileDialog1.Title = "Save an .Reg File";
            saveFileDialog1.ShowDialog();
            string path = "";
            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.

                fs.Close();
                path = saveFileDialog1.FileName;
            }

            ExportReg export;
            export = new ExportReg(Registry1, txtpath1.Text, "HKEY_LOCAL_MACHINE\\SYSTEM");

            export.ExpToReg(path);
        }

        private void btnCMPRegF_Click(object sender, RoutedEventArgs e)
        {

            string texto = Environment.GetEnvironmentVariable("path");
            if (texto.Contains("Microsoft VS Code"))

            {
                Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
                Microsoft.Win32.OpenFileDialog openFileDialog2 = new Microsoft.Win32.OpenFileDialog();
                openFileDialog1.Title = "Registry .reg File";
                openFileDialog1.Filter = ".Reg Files|*.reg";
                openFileDialog2.Title = "Registry .reg File";
                openFileDialog2.Filter = ".Reg Files|*.reg";
                string path1 = "";
                string path2 = "";
                if (openFileDialog1.ShowDialog() == true)
                {
                    try
                    {

                        path1 = openFileDialog1.FileName;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }
                if (openFileDialog2.ShowDialog() == true)
                {
                    try
                    {

                        path2 = openFileDialog2.FileName;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }


                string param = "--diff \"" + path1 + "\" \"" + path2 + "\"";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/c code " + param;
                process.StartInfo = startInfo;
                process.Start();
            }
            else
            {
                MessageBox.Show("This functionality Needs VS Code Installed, please install it on the PC", "VSCODE Not Installed", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult a = MessageBox.Show("Do you want to provide some Feedback?", "Feedback", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (a == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start("https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR4t8jW939JRFosWxHm34rHRUNVEzVjhaUk4zQVQxM1hSNU82SkgxS1JFQS4u");

        }

        private void Reg1Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModelRegistryKey Selected = (ModelRegistryKey)((TreeView)sender).SelectedItem;
            loadtable(Selected, 1);
        }
        private List<ModelRegistryKey> lastselected1;
        private ObservableCollection<ModelRegistryKey> lastparentselected1;
        private List<ModelRegistryKey> lastselected2;
        private ObservableCollection<ModelRegistryKey> lastparentselected2;
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
                    tvi = FindTviFromObjectRecursive(tvi2, o);
                if (tvi != null)
                    return tvi;
            }
            return null;
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
            if (tree == 1)
            {
                clastselected = ref lastselected1;
                clastparentselected = ref lastparentselected1;
                Hive = ref _Hive1;
                RegTree = ref Reg1Tree;
            }
            else
            {
                clastselected = ref lastselected2;
                clastparentselected = ref lastparentselected2;
                Hive = ref _Hive2;
                RegTree = ref Reg2Tree;
            }

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
        private void Reg1Tree_KeyDown(object sender, KeyEventArgs e)
        {

            navigation_on_tree(1, e.Key.ToString());
        }

        private void Reg2Tree_KeyDown(object sender, KeyEventArgs e)
        {
            navigation_on_tree(2, e.Key.ToString());
        }

        private void Reg2Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModelRegistryKey Selected = (ModelRegistryKey)((TreeView)sender).SelectedItem;
            loadtable(Selected, 2);
        }
    }
}
