using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
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
            Scroll1.Visibility = Visibility.Hidden;
            Reg2Tree.Visibility = Visibility.Hidden;
            Scroll2.Visibility = Visibility.Hidden;
            
        }

        private void btnOpenReg_Click(object sender, RoutedEventArgs e)
        {
            lbloutput.Content = ("UserName: {0}", Environment.UserName);
            Scroll1.Visibility = Visibility.Visible;
            Scroll1.SetValue(Grid.ColumnSpanProperty, (int)Scroll1.GetValue(Grid.ColumnSpanProperty) + 1);
            Reg1Tree.Visibility = Visibility.Visible;
            Reg1Tree.SetValue(Grid.ColumnSpanProperty, (int)Reg1Tree.GetValue(Grid.ColumnSpanProperty) + 1);

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
            File_Load(path,1);
        }
        private void File_Load(string path,int registryfile)
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
                catch (Exception e)
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
                catch (Exception e)
                {
                    MessageBox.Show("The file you selected for registry 2 it is not a registryfile", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            
           
           
        }
        private RegistryKey Subnode_Route(string route,int currentreg)
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
                    return MyTreeViewParent( ((TreeViewItem)child).Parent );
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
        private void loadtable(string ruta,int tabla)
        {
            string selectedNodeText = ruta;
            RegistryKey value = null;
            DataGrid currentTable = null;
            if (tabla == 1)
            {
                value = Registry1.Root;
                currentTable = Reg1Values;
            }
            else
            {
                value = Registry2.Root;
                currentTable = Reg2Values;
            }
            
            DataTable dt = new DataTable();
            DataRow dr;
            RegistryKey res = this.Subnode_Route(selectedNodeText,tabla);
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

        private RegistryKey ActiveRegistry(int value)
        {
            if (value == 1)
                return Registry1.Root;
            return Registry2.Root;
        }

        private RegistryKey Drawhive(RegistryKey pKey, TreeViewItem guiNode,int registryfile)
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
                            Drawhive(subkey, guiNodechild,registryfile);
                        }
                        else
                        {
                            this.Reg2Tree.Items.Add(guiNodechild);
                            Drawhive(subkey, guiNodechild,registryfile);
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

            lbloutput.Content = ("UserName: {0}", Environment.UserName);
            Scroll1.Visibility = Visibility.Visible;
            Scroll1.SetValue(Grid.ColumnSpanProperty, 3);
            Reg1Tree.Visibility = Visibility.Visible;
            Reg1Tree.SetValue(Grid.ColumnSpanProperty, 3);
            Scroll2.Visibility = Visibility.Visible;
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
            File_Load(path,1);
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
            File_Load(path,2);
        }
    }
}
