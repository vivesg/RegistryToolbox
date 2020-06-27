using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Registry;
using Registry.Abstractions;

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

        public MainWindow()
        {
            InitializeComponent();
            Reg1Tree.Visibility = Visibility.Hidden;
            gridClientsContainer1.Visibility = Visibility.Hidden;
            Reg2Tree.Visibility = Visibility.Hidden;
            Reg2Values.Visibility = Visibility.Hidden;

        }

        private void btnOpenReg_Click(object sender, RoutedEventArgs e)
        {
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
            }
            File_Load(path, 1);
        }
        private void File_Load(string path, int registryfile)
        {
            if (registryfile == 1)
            {
                Reg1Tree.Items.Clear();
                string testFile = path;
                try
                {
                    var registryHive = new RegistryHive(testFile);
                    Registry1 = registryHive;
                    registryHive.ParseHive();
                    Drawhive(Registry1.Root, null, registryfile);
                }
                catch (Exception)
                {
                    MessageBox.Show("The file you selected for registry 1 it is not a registry file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                Reg2Tree.Items.Clear();
                string testFile = path;
                try
                {
                    var registryHive = new RegistryHive(testFile);
                    Registry2 = registryHive;
                    registryHive.ParseHive();
                    Drawhive(Registry2.Root, null, registryfile);
                }
                catch (Exception)
                {
                    MessageBox.Show("The file you selected for registry 2 it is not a registryfile", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

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

        void treeItem_Selected(object sender, RoutedEventArgs e)
        {
            if (MyTreeViewParent(sender) == Reg1Tree)
            {
                if (sender is TreeView)
                    actualpath1 = "";
                TreeViewItem item = sender as TreeViewItem;
                
                string path = GetFullPath(item);
                if (!this.actualpath1.Contains(path))
                    loadtable(GetFullPath(item), 1);
                this.actualpath1 = path;
            }
            else
            {
                if (sender is TreeView)
                    actualpath2 = "";
                TreeViewItem item = sender as TreeViewItem;
                string path = GetFullPath(item);
                if (!this.actualpath2.Contains(path))
                    loadtable(GetFullPath(item), 2);
                this.actualpath2 = path;

            }
        }

        ItemsControl GetParentItem(ItemsControl nodo)
        {
            return ItemsControl.ItemsControlFromItemContainer(nodo);

        }
        private void loadtable(string ruta, int tabla)
        {
            string selectedNodeText = ruta;
            RegistryKey value = null;
            DataGrid currentTable = null;
            if (tabla == 1)
            {
                value = Registry1.Root;
                currentTable = Reg1Values;
                txtpath1.Text = ruta;
            }
            else
            {
                value = Registry2.Root;
                currentTable = Reg2Values;
                txtpath2.Text = ruta;
            }
            currentTable.DataContext = null;
                 

            DataTable dt = new DataTable();
            DataRow dr;
            RegistryKey res = this.Subnode_Route(selectedNodeText, tabla);
            DataColumn dc;
            dc = new DataColumn("Name", typeof(String));
            dt.Columns.Add(dc);

            dc = new DataColumn("Type", typeof(String));
            dt.Columns.Add(dc);

            dc = new DataColumn("Value", typeof(String));
            dt.Columns.Add(dc);
            foreach (KeyValue obj in res.Values)
            {
                dr = dt.NewRow();
                dr[0] = obj.ValueName;
                dr[1] = obj.ValueType;
                dr[2] = obj.ValueData;
                dt.Rows.Add(dr);
            }
            
            currentTable.AutoGenerateColumns = false;
            currentTable.DataContext = dt;
            foreach (DataGridColumn column in currentTable.Columns)
            {
                //if you want to size your column as per the cell content
                //column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
                //if you want to size your column as per the column header
                //column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToHeader);
                ////if you want to size your column as per both header and cell content
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

        public TreeViewItem Select_FullPath(string path,TreeViewItem pleaf,TreeView ptree)
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
            

            foreach(TreeViewItem leaf in subleafs)
            {
                if  (leaf.Header.ToString() == actualp.ToString())
                {
                    if (routes.Length > 1)
                    {
                        leaf.IsExpanded = true;
                        leaf.IsSelected = true;
                        leaf.Focus();
                        return Select_FullPath(path.Substring(path.IndexOf(@"\")+1), leaf, null);
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

        private RegistryKey Drawhive(RegistryKey pKey, TreeViewItem guiNode, int registryfile)
        {
            if (pKey.SubKeys.Count == 0)
                return null;
            else
            {
                foreach (RegistryKey subkey in pKey.SubKeys)
                {
                    TreeViewItem guiNodechild = new TreeViewItem();
                    guiNodechild.Header = subkey.KeyName;
                    guiNodechild.Selected += treeItem_Selected;

                    if (subkey.Parent == ActiveRegistry(registryfile))
                    {
                        if (registryfile == 1)
                        {
                            this.Reg1Tree.Items.Add(guiNodechild);
                            Drawhive(subkey, guiNodechild, registryfile);
                        }
                        else
                        {
                            this.Reg2Tree.Items.Add(guiNodechild);
                            Drawhive(subkey, guiNodechild, registryfile);
                        }
                    }
                    else
                    {
                        guiNode.Items.Add(guiNodechild);
                        Drawhive(subkey, guiNodechild, registryfile);
                    }

                }
                return null;
            }

        }

        private void btnCMPReg_Click(object sender, RoutedEventArgs e)
        {

            // lbloutput.Content = ("UserName: {0}", Environment.UserName);
            gridClientsContainer1.Visibility = Visibility.Visible;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 1);
            Reg1Tree.Visibility = Visibility.Visible;
            Reg1Tree.SetValue(Grid.ColumnSpanProperty, 1);
            Reg2Values.Visibility = Visibility.Visible;
            Reg2Tree.Visibility = Visibility.Visible;
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

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
            File_Load(path, 2);
            
        }
        private Boolean CompareRow(ItemsControl row1,ItemsControl row2)
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
            About win2 = new About();
            win2.Show();
        }

        private void btnload1_Click(object sender, RoutedEventArgs e)
        {
            
            TreeViewItem item =  Select_FullPath(txtpath1.Text, null, Reg1Tree);
            
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
    }
}
