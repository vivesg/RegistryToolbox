using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Registry;
using Registry.Abstractions;
using RegistryToolbox.Models;
using System.Threading;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using RegistryToolbox.Insights;
using System.Text;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        

        public bool is_samepath { get; set; }
  
        public bool VISIBLE;
        Registry.RegistryHive Registry1 = null;
        Registry.RegistryHive Registry2 = null;
        int ACTUAL;
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

            this.DataContext = this;
            _Hive1 = new ObservableCollection<ModelRegistryKey>();
            _Hive2 = new ObservableCollection<ModelRegistryKey>();

            Reg1Tree.Visibility = Visibility.Hidden;
            gridClientsContainer1.Visibility = Visibility.Hidden;
            gridClientsContainer2.Visibility = Visibility.Hidden;
            uispliter.Visibility = Visibility.Collapsed;
            btnalign.Visibility = Visibility.Hidden;
            btnCompareKeyandsub.Visibility = Visibility.Hidden;
            Hide2();

            Reg1Tree.DataContext = Hive1;
            Reg1Tree.ItemsSource = Hive1;
            Reg2Tree.DataContext = Hive2;
            Reg2Tree.ItemsSource = Hive2;

            lastselected1 = new List<ModelRegistryKey>();
            lastparentselected1 = new ObservableCollection<ModelRegistryKey>();
            lastselected2 = new List<ModelRegistryKey>();
            lastparentselected2 = new ObservableCollection<ModelRegistryKey>();
            string version = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + '.'
                       + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString()
                       + '.' + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();

            this.Title = "Registry Toolbox Version: " + version;
#if DEBUG
            this.Title ="BETA NON PROD Registry Toolbox Version: " + version;
#endif
            checkupdate();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;//put here your logic
            e.Handled = true;
        }

        private void checkupdate()
        {
            try
            {
                using (System.Net.WebClient w = new WebClient())
                {
                    var json = w.DownloadString("https://registrytoolbox.blob.core.windows.net/releases/release.json");

                    string version = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + '.'
                        + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString()
                        + '.' + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();

                    ReleaseInfo result = JsonConvert.DeserializeObject<ReleaseInfo>(json);

                    if (result.Version != version)
                    {
                        UpdateDialog upd = new UpdateDialog("Version: " + result.Version);
                        upd.ShowDialog();

                    }
                }
            }
            catch (Exception E)
            {
                MessageBox.Show("Getting Updating Information Failed please check manually on github", "Updates", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        private void Hide2()
        {

            txtfilepath2.Visibility = Visibility.Hidden;
            color2.Visibility = Visibility.Hidden;
            txtTag2.Visibility = Visibility.Hidden;
            txtpath2.Visibility = Visibility.Hidden;
            goto2.Visibility = Visibility.Hidden;
            lblPath2.Visibility = Visibility.Hidden;
            chk_showonlydiff.Visibility = Visibility.Hidden;
            uispliter.Visibility = Visibility.Hidden;
            gridClientsContainer2.Visibility = Visibility.Hidden;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 2);
            btnCompareKeyandsub.Visibility = Visibility.Hidden;
            btnalign.Visibility = Visibility.Hidden;
           

        }
        private void Show2()
        {
            txtfilepath2.Visibility = Visibility.Visible;
            color2.Visibility = Visibility.Visible;
            txtTag2.Visibility = Visibility.Visible;
            txtpath2.Visibility = Visibility.Visible;
            goto2.Visibility = Visibility.Visible;
            lblPath2.Visibility = Visibility.Visible;
            chk_showonlydiff.Visibility = Visibility.Visible;
            uispliter.Visibility = Visibility.Visible;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 1);
            btnCompareKeyandsub.Visibility = Visibility.Visible;
            btnalign.Visibility = Visibility.Visible;
            btnInsights.Visibility = Visibility.Hidden;
        }
        private void btnOpenReg_Click(object sender, RoutedEventArgs e)
        {

            _Hive1.Clear();
            _Hive2.Clear();

            Reg1Values.DataContext = null;

            gridClientsContainer1.Visibility = Visibility.Visible;
            btnInsights.Visibility = Visibility.Visible;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 2);
            Reg1Tree.Visibility = Visibility.Visible;

            Hide2();
            txtpath1.Text = "";
            txtpath2.Text = "";
            OpenFile();

        }
        private void OpenFile()
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
                MessageBox.Show("Please wait", "Processing your file", MessageBoxButton.OK, MessageBoxImage.Information);
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                File_Load(path, 1);
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;

            }
        }
        private void File_Load(string path, int registryfile)
        {

            var registryHive = new RegistryHive(path);
            try
            {
                if (registryfile == 1)
                {
                    txtfilepath1.Text = path;
                    Registry1 = registryHive;
                    registryHive.ParseHive();
                    Drawhive(Registry1.Root, _Hive1);
                }
                else
                {
                    txtfilepath2.Text = path;
                    Registry2 = registryHive;
                    registryHive.ParseHive();
                    Drawhive(Registry2.Root, _Hive2);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The file you selected for registry it is not a registry binary file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lookLeaf(string path, int registry)
        {
            path = path.ToUpper();
            string[] routes = path.Split('\\');
            List<TreeViewItem> itemroute = new List<TreeViewItem>();
            ObservableCollection<ModelRegistryKey> reg = null;
            TreeView cTree = null;
            if (registry == 1)
            {
                reg = this.Hive1;
                cTree = Reg1Tree;
            }
            else
            {
                reg = this.Hive2;
                cTree = Reg2Tree;
            }
            ModelRegistryKey cur = null;
            foreach (ModelRegistryKey skey in reg)
            {
                if (skey.Name.ToUpper() == routes[0])
                {
                    cur = skey;
                    var tva = FindTviFromObjectRecursive(cTree, cur);
                    if (tva != null)
                    {
                        tva.IsExpanded = true;
                        UpdateLayout();
                        itemroute.Add(tva);

                    }
                }
            }

            for (int i = 1; i < routes.Length; i++)
            {
                bool found = false;
                if (cur == null)
                    break;
                foreach (ModelRegistryKey skey in cur.Subkeys)
                {
                    if (skey.Name.ToUpper() == routes[i])
                    {
                        found = true;
                        cur = skey;
                        var tva = FindTviFromObjectRecursive(cTree, cur);
                        if (tva != null)
                        {
                            tva.IsExpanded = true;
                            UpdateLayout();
                            itemroute.Add(tva);
                        }

                    }
                }
                if (!found)
                {
                    cur = null;
                }
            }

            if (cur == null)
            {
                foreach (TreeViewItem item in itemroute)
                {
                    item.IsExpanded = false;
                }
                MessageBox.Show("Path not found", "Incorrect Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;

            }

            var tva2 = FindTviFromObjectRecursive(cTree, cur);
            if (tva2 != null)
            {

                tva2.IsExpanded = true;
                tva2.IsSelected = true;
                tva2.Focus();

            }
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

       
        ItemsControl GetParentItem(ItemsControl nodo)
        {
            return ItemsControl.ItemsControlFromItemContainer(nodo);

        }
        private void loadtable(ModelRegistryKey key, int tabla)
        {

            DataGrid currentTable = Reg1Values;
            if (tabla == 2)
                currentTable = Reg2Values;

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

        private RegistryKey ActiveRegistry(int value)
        {
            if (value == 1)
                return Registry1.Root;
            return Registry2.Root;

        }
        private  string ByteArrayToString(byte[] ba)
        {
         
            Array.Reverse(ba, 0, ba.Length);
           
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
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

        private void btnCMPReg_Click(object sender, RoutedEventArgs e)
        {

            txtpath1.Text = "";
            txtpath2.Text = "";
            _Hive1.Clear();
            _Hive2.Clear();

            Reg1Values.DataContext = null;
            Reg2Values.DataContext = null;

            gridClientsContainer1.Visibility = Visibility.Visible;
            gridClientsContainer1.SetValue(Grid.ColumnSpanProperty, 1);
            gridClientsContainer2.Visibility = Visibility.Visible;
            gridClientsContainer2.SetValue(Grid.ColumnSpanProperty, 1);
            Reg1Tree.Visibility = Visibility.Visible;
            Reg1Tree.SetValue(Grid.ColumnSpanProperty, 1);

            btnalign.Visibility = Visibility.Visible;
            btnCompareKeyandsub.Visibility = Visibility.Visible;
            Show2();


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
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                File_Load(path, 1);
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;

                MessageBox.Show("Select Second File to analyze", "Please select a second registry file to analyze", MessageBoxButton.OK, MessageBoxImage.Information);
                Microsoft.Win32.OpenFileDialog openFileDialog2 = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Title = "Registry2 Binary File";
                path = "";
                if (openFileDialog2.ShowDialog() == true)
                {
                    try
                    {

                        path = openFileDialog2.FileName;
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                        File_Load(path, 2);
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;


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

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {

            About cw = new About();
            cw.ShowInTaskbar = false;
            cw.Owner = Application.Current.MainWindow;
            cw.Show();
        }


        private void btnalign_Click(object sender, RoutedEventArgs e)
        {
            if (ACTUAL == 1)
            {
                txtpath2.Text = txtpath1.Text;
                goto2_Click(sender, e);
            }
            else
            {
                txtpath1.Text = txtpath2.Text;
                goto1_Click(sender, e);
            }


        }

        private void btnCompareKeyandsub_Click(object sender, RoutedEventArgs e)
        {
            bool showonlydiff = chk_showonlydiff.Visibility == Visibility.Visible;
            Process.GetCurrentProcess().ProcessorAffinity =
            new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();

            stopwatch.Start();
            UpdateLayout();

            ModelRegistryKey NodoA = new ModelRegistryKey("ROOT");
            ModelRegistryKey NodoB = new ModelRegistryKey("ROOT");
            NodoA.Subkeys = this.Hive1;
            NodoB.Subkeys = this.Hive2;

            Collection<String> lista = new Collection<string>();
            NodoA.FindDifferences(NodoB, lista);
            NodoB.FindDifferences(NodoA, lista);

            //((ModelRegistryKey)this.Reg2Tree.SelectedItem).FindDifferences((ModelRegistryKey)this.Reg1Tree.SelectedItem);
            //((ModelRegistryKey)this.Reg1Tree.SelectedItem).FindDifferences((ModelRegistryKey)this.Reg2Tree.SelectedItem);
            stopwatch.Stop();
            Debug.WriteLine("Ticks: " + stopwatch.ElapsedTicks + " mS: " + stopwatch.ElapsedMilliseconds);

            paintdifferences(Reg2Values, Reg1Values);
            paintdifferences(Reg1Values, Reg2Values);
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


                fs.Close();
                path = saveFileDialog1.FileName;
            }

            MessageBoxResult result = MessageBox.Show("This is a very experimental functionality please be aware that the application can crash or take a lot of time to process the export do you want to generate the file", "Experimental", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {

                ExportReg export;
                if (ACTUAL == 2)
                {

                    export = new ExportReg((ModelRegistryKey)Reg2Tree.SelectedItem, txtpath1.Text, "HKEY_LOCAL_MACHINE\\SYSTEM");
                    export.ExpToReg(path);
                    MessageBox.Show("Registry 2 selected node Exported succesfully", "Export completed", MessageBoxButton.OK);
                }
                else
                {
                    export = new ExportReg((ModelRegistryKey)Reg1Tree.SelectedItem, txtpath1.Text, "HKEY_LOCAL_MACHINE\\SYSTEM");
                    export.ExpToReg(path);
                    MessageBox.Show("Registry 1 selected node Exported succesfully", "Export completed", MessageBoxButton.OK);
                }
            }
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
            if (Selected == null)
                return;

            var tva = FindTviFromObjectRecursive(Reg1Tree, Selected);
            txtpath1.Text = GetFullPath(tva);
         
            loadtable(Selected, 1);
            UpdateLayout();
            onlydiff_whensamepath();
            if (Selected.Diff & txtpath1.Text == txtpath2.Text)
            {
                paintdifferences(Reg1Values, Reg2Values);
                paintdifferences(Reg2Values, Reg1Values);
            }
            this.ACTUAL = 1;
        }

        private void onlydiff_whensamepath()
        {


            if (txtpath1.Text == txtpath2.Text)
            {
                is_samepath = true;
                
            }
            else
            {
                is_samepath = false;
            }
        }
        private void Reg2Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModelRegistryKey Selected = (ModelRegistryKey)((TreeView)sender).SelectedItem;
            if (Selected == null)
                return;
            var tva = FindTviFromObjectRecursive(Reg2Tree, Selected);
            txtpath2.Text = GetFullPath(tva);
            loadtable(Selected, 2);
            onlydiff_whensamepath();
            if (Selected.Diff & txtpath1.Text == txtpath2.Text)
            {
                paintdifferences(Reg1Values, Reg2Values);
                paintdifferences(Reg2Values, Reg1Values);
            }
            this.ACTUAL = 2;
        }
        private void paintdifferences(DataGrid treevalues1, DataGrid treevalues2)
        {
            
            treevalues1.UpdateLayout();
            treevalues2.UpdateLayout();

            
            for (int i = 0; i < treevalues1.Items.Count; i++)
            {
                DataGridRow rowA = treevalues1.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                Boolean found = false;
                for (int j = 0; j < treevalues2.Items.Count; j++)
                {
                    DataGridRow rowB = treevalues2.ItemContainerGenerator.ContainerFromIndex(j) as DataGridRow;

                    ModelRegistryKeyValues ValueA = rowA.Item as ModelRegistryKeyValues;
                    ModelRegistryKeyValues ValueB = rowB.Item as ModelRegistryKeyValues;

                    if (ValueA.Name == ValueB.Name) //I have found the Key
                    {
                        found = true;
                        if (!ValueA.Equals(ValueB))
                        {
                            rowA.Background = new SolidColorBrush(Color.FromRgb(255, 204, 203));
                        }
                        else
                        {
                            rowA.Background = Brushes.White;
                            if (chk_showonlydiff.IsChecked.Value & is_samepath)
                            {
                                rowA.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
                if (!found)
                {
                    rowA.Background = Brushes.LightCyan;
                }
            }
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
                {
                    tvi = FindTviFromObjectRecursive(tvi2, o);
                }
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


        private void TreeViewItem_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    return;
                }
            }
            Debug.WriteLine(sender.ToString());

        }

        private void color1_Click(object sender, RoutedEventArgs e)
        {
            TagColor(1);
        }
        private void TagColor(int table)
        {

            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            System.Windows.Forms.DialogResult result = colorDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {

                var colores = colorDialog.Color;

                System.Windows.Media.Color mediaColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                System.Windows.Media.Brush brush = new SolidColorBrush(mediaColor);

                if (table == 1)
                    txtTag1.Background = brush;
                else
                    txtTag2.Background = brush;

            }

        }

        private void color2_Click(object sender, RoutedEventArgs e)
        {
            TagColor(2);
        }

        private void goto2_Click(object sender, RoutedEventArgs e)
        {
            lookLeaf(txtpath2.Text, 2);
        }

        private void goto1_Click(object sender, RoutedEventArgs e)
        {
            lookLeaf(txtpath1.Text, 1);
        }

        private void GridSplitter_MouseEnter(object sender, MouseEventArgs e)
        {

            Mouse.OverrideCursor = Cursors.SizeNS;
        }

        private void GridSplitter_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void uispliter_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void chk_showonlydiff_Checked(object sender, RoutedEventArgs e)
        {
            paintdifferences(Reg2Values, Reg1Values);
            paintdifferences(Reg1Values, Reg2Values);
           
        }

        private void Insights_Click(object sender, RoutedEventArgs e)
        {
            Insight ins = new Insight(this.Registry1);
            ins.GetDiskFilters();
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

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/vivesg/RegistryToolbox/blob/master/README.md");

        }

        private void Reg2Values_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ModelRegistryKeyValues values = (ModelRegistryKeyValues)Reg2Values.SelectedItem;
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
    }
}
