using Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryToolbox.Insights
{
    public class Insight
    {
        private Registry.RegistryHive ROOT;
        public Insight(Registry.RegistryHive pRegistry1)
        {
            ROOT = pRegistry1;
        }
        public string getValue(RegistryKey pEntry, string pKeyName)
        {
            foreach (KeyValue value in pEntry.Values)
            {
                if (value.ValueName == pKeyName)
                {
                    return value.ValueData;

                }
            }
            return null;
        }
        public void generateReport(string insights)
        {
            string html = "<html>" +
            @"<style>
            table {
              font-family: arial, sans-serif;
              border-collapse: collapse;
              width: 100%;
            }
            td, th {
              border: 1px solid #dddddd;
              text-align: left;
              padding: 8px;
            }
            tr:nth-child(even) {
              background-color: #dddddd;
            }
            </style>" +
            "<h1>Registry Toolbox Insights </h1>" + 
            "<h3>Insights Found</h3>" +
                    
            @"<table><tr> <th>Insight</th>
            <th>Finding</th></tr>"

            + insights +

             "</html>";
               
            
                System.IO.File.WriteAllText("report.html", html);
                System.Diagnostics.Process.Start(@"report.html");

        }

        // FIND DISK FILTERS

        public void GetDiskFilters()
        {
            RegistryKey key = ROOT.GetKey(@"ControlSet001\Control\Class\{4d36e967-e325-11ce-bfc1-08002be10318}");
            List<string> lowerfilters = getValue(key, "LowerFilters").Split(' ').ToList();
            List<string> upperfilters = getValue(key, "UpperFilters").Split(' ').ToList();
            
           string uheader = "Third Party Disk UpperFilters Found";
           string lheader = "Third Party Disk Lower Filters Found";
            lowerfilters.RemoveAll(x => x =="EhStorClass"); // REMOVE OFFICIAL FILTER

            string combinedString = string.Join(",", lowerfilters.ToArray());


            string output = String.Format(@"<tr>
            <td> {0} <br> Please check Registry Value HKLM\System\ControlSet001\Control\Class\{{4d36e967-e325-11ce-bfc1-08002be10318}} </td>
            <td>The following Third party filters were found <b>  {1} </b> </br>
            <hr>
            <i>Default values on Windows depending of the Version</i>
            </br>
            <img src='https://registrytoolbox.blob.core.windows.net/insights/diskfilters.png'>
            <br>
             If you are finding a NonBoot Innacessible Boot Device or 0x0000007B Bugcheck please remove this entry from the registry
             </td>
            </tr>", lheader, combinedString);
            generateReport(output);
        }

    }


}
