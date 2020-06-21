using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
using Registry;
using Registry.Abstractions;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Registry.RegistryHive Registry = null;
        string actualpath = "";

        public MainWindow()
        {
            InitializeComponent();
            
            //   Reg2Values.SetValue(Grid.ColumnProperty, 3);
        }

        private void btnOpenReg_Click(object sender, RoutedEventArgs e)
        {
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
            }
            File_Load(path);
        }
        private void File_Load(string path)
        {
          
            Reg1Tree.Items.Clear();
            string testFile = path;
            try
            {
               
                var registryHive = new RegistryHive(testFile);
                Registry = registryHive;
                registryHive.ParseHive();
                RegistryKey value = Registry.Root;
                Drawhive(value, null);
               
            }
            catch (Exception e) {
                MessageBox.Show("The file you selected it is not a registry binary file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }
        private RegistryKey Subnode_Route(string route)
        {

            string[] routes = route.Split('\\');
            RegistryKey actual = this.Registry.Root;
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

        void treeItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is TreeView)
                actualpath = "";
            TreeViewItem item = sender as TreeViewItem;
            string path = GetFullPath(item);
            if(!this.actualpath.Contains(path))
            loadtable(GetFullPath(item));
            this.actualpath = path;
        }
        ItemsControl GetParentItem(ItemsControl nodo)
        {
            return ItemsControl.ItemsControlFromItemContainer(nodo);
           
        }
        private void loadtable(string ruta)
        {
            string selectedNodeText = ruta;
            RegistryKey value = Registry.Root;
            DataTable dt = new DataTable();
            DataRow dr;
            RegistryKey res = this.Subnode_Route(selectedNodeText);
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
           
            Reg1Values.DataContext = dt.DefaultView;
           

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

       

        private RegistryKey Drawhive(RegistryKey pKey, TreeViewItem guiNode)
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
                    if (subkey.SubKeys.Count != 0)
                    {
                       
                    }
                    if (subkey.Parent == this.Registry.Root)
                    {
                        this.Reg1Tree.Items.Add(guiNodechild);
                        Drawhive(subkey, guiNodechild);
                    }
                    else
                    {
                        guiNode.Items.Add(guiNodechild);

                        Drawhive(subkey, guiNodechild);
                    }

                }
                return null;
            }

        }

     
    }
}
