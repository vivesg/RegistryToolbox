using RegistryToolbox.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RegistryToolbox
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class ValueInspector : Window
    {
        public ValueInspector()
        {
            InitializeComponent();
        }
        public struct REG
        {
            public string Name { set; get; }
            public string Type { set; get; }
            public string Hex { set; get; }
            public string Dec { set; get; }
         
        }
        public ValueInspector(ModelRegistryKeyValues prow) 
        {
          

            InitializeComponent();
            
            string value = prow.Value;

            string[] vals = value.Split(' ');
            vals[1] = vals[1].Trim('(');
            vals[1] = vals[1].Trim(')');

            valuesgrid.Items.Add(new REG { Name = prow.Name, Type = prow.Type,Hex = vals[0],Dec = vals[1]});

            foreach (DataGridColumn column in valuesgrid.Columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);
            }

            valuesgrid.Items.Refresh();
          
        }
    }
}
