using Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RegistryToolbox.Insights
{
    public class Insight
    {
        private Registry.RegistryHive ROOT;
        public Insight(Registry.RegistryHive pRegistry1)
        {
            ROOT = pRegistry1;
            // GetNICFilters();
            generateReport("");
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
            return "";
        }

        public struct NIC
        {
            public string Name { set; get; }
            public string GUID { set; get; }
            public string LastSeen { set; get; }
            public List<Filter> Filters { set; get; }
        }
        public struct Filter
        {
            public string ComponentId { set; get; }
            public string ComponentDescription { set; get; }
            public Service service { set; get; }
        }
        public struct Service
        {
            public string Name { get; set; }
            public string Start { get; set; }
            public string ImagePath { set; get; }
            public string HelpText { get; set; }

        }


        public string getVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + '.'
                      + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString()
                      + '.' + Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            return version;
        }
        public void generateReport(string insights)
        {


            RegistryKey key = ROOT.GetKey(@"ControlSet001\Control\ComputerName\ComputerName");

            string PCNAME = getValue(key, "ComputerName");

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
            "<h2>Registry Toolbox Insights </h2>" +
            "<h3>" + getVersion() + "</h3>" +
            "<h4>Computer Name:" + PCNAME + "</h4>" +
            @"<table><tr> <th>Insight</th>
            <th>Finding</th></tr>"

            + insights +

             "</html>";

            List<NIC> nics = GetNICFilters();
            string dsk = @"<div><table><tr><th>Disk Filters</th>
            <th>Finding</th></tr>" + GetDiskFilters() + "</table></div>";

            html = Report(nics);
            html = html.Replace("{COMPUTERNAME}", PCNAME);
            html = html.Replace("{DISKFILTER}", dsk);
            System.IO.File.WriteAllText("report.html", html);
            System.Diagnostics.Process.Start(@"report.html");

        }


        // FIND DISK FILTERS

        public string StartUpType(int valor)
        {
            // ref https://docs.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicestartmode?view=dotnet-plat-ext-6.0
            string output = "Start value not in range 0-4";
            switch (valor)
            {
                case 0:
                    output = "Boot";
                    break;
                case 1:
                    output = "System";
                    break;
                case 2:
                    output = "Automatic";
                    break;
                case 3:
                    output = "Manual";
                    break;
                case 4:
                    output = "Disabled";
                    break;
            }
            return output;

        }

        public string GetDiskFilters()
        {
            RegistryKey key = ROOT.GetKey(@"ControlSet001\Control\Class\{4d36e967-e325-11ce-bfc1-08002be10318}");
            List<string> lowerfilters = getValue(key, "LowerFilters").Split(' ').ToList();
            List<string> upperfilters = getValue(key, "UpperFilters").Split(' ').ToList();

            string uheader = "Third Party Disk UpperFilters Found";
            string lheader = "Third Party Disk Lower Filters Found";
            lowerfilters.RemoveAll(x => x == "EhStorClass" | x == "storflt"); // REMOVE OFFICIAL Lower FILTER
            upperfilters.RemoveAll(x => x == "partmgr"); // REMOVE OFFICIAL Upper FILTER


            string combinedString = string.Join(" and ", lowerfilters.ToArray());

            // INSIGHT LowerFilder

            string output = "";
            if (combinedString != "")
            {
                output = String.Format(@"<tr>
            <td> {0} <br> Please check Registry Value HKLM\System\ControlSet001\Control\Class\{{4d36e967-e325-11ce-bfc1-08002be10318}} </td>
            <td>The following Entry <b>LowerFilters</b> has third party filters <b>{1}</b> </br>
            <hr>
            <i>Default values on Windows depending of the Version</i>
            </br>
            <img src='https://registrytoolbox.blob.core.windows.net/insights/diskfilters.png'>
            <br>
             If you are finding a NonBoot Innacessible Boot Device or 0x0000007B Bugcheck please remove this entry from the registry
             </td>
            </tr>", lheader, combinedString);
            }

            // INSIGHT UPPERFILDER

            combinedString = string.Join(" and ", upperfilters.ToArray());
            if (combinedString != "")
            {
                output = output + String.Format(@"<tr>
            <td> {0} <br> Please check Registry Value HKLM\System\ControlSet001\Control\Class\{{4d36e967-e325-11ce-bfc1-08002be10318}} </td>
            <td>The following Entry <b>UpperFilter</b> has third party filters <b> {1} </b> </br>
            <hr>
            <i>Default values on Windows depending of the Version</i>
            </br>
            <img src='https://registrytoolbox.blob.core.windows.net/insights/diskfilters.png'>
            <br>
             If you are finding a NonBoot Innacessible Boot Device or 0x0000007B Bugcheck please remove this entry from the registry
             </td>
            </tr>", uheader, combinedString);
            }

            return output;
        }
        public string Report(List<NIC> nics){


            int i = 0;
            string nicstabs = "";


            string html = @" <!DOCTYPE html>
<html>

<head>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {
            font-family: Arial;
        }

        /* Style the tab */
        .tab {
            overflow: hidden;
            border: 1px solid #ccc;
            background-color: #f1f1f1;
        }

        .tab2 {
            float: left;
            border: 1px solid #ccc;
            background-color: #f1f1f1;
            width: 20%;
            height: 300px;
            margin-right: 2%;
        }

        /* Style the buttons inside the tab */
        .tab button {
            background-color: inherit;
            float: left;
            border: none;
            outline: none;
            cursor: pointer;
            padding: 14px 16px;
            transition: 0.3s;
            font-size: 17px;
        }

        .tab2 button {
            display: block;
            background-color: inherit;
            color: black;
            padding: 22px 16px;
            width: 100%;
            outline: none;
            text-align: left;
            cursor: pointer;
            transition: 0.3s;
            font-size: 17px;

        }


        /* Change background color of buttons on hover */
        .tab button:hover {
            background-color: #ddd;
        }

        /* Create an active/current tablink class */
        .tab button.active {
            background-color: #ccc;
        }

        /* Style the tab content */
        .Insight {
            display: none;
            padding: 6px 12px;
            border: 1px solid #ccc;
            border-top: none;
             height: 50000px;
        }

        .ERROR {

            background-color: lightcoral !important;

        }

        .WARNING {

            background-color: orange !important;

        }

        .selected {
            font-weight: 600;
            border: 8px solid black;
        }


        table,
        td {
            border: 1px solid #333;
        }

       thead,
        tfoot {
            background-color: black;
            color: #fff;
        }

        .head{

    
    justify-content: left;
    padding: 5px;
    background-color: darkblue;
    color: #fff;

        }
  .tabcontent{
            margin-left: 30%;
            position: sticky;
        }

    </style>
</head>

<body>
    <div class='head'>
        <h2>Registry Toolbox Insights</h2>
        <h3>Filters Inspector</h3>
        <p>Version 1.0</p>
        <h4>COMPUTER NAME: {COMPUTERNAME}</h4>
        <h4 style='text-align:right'><a href='https://forms.office.com/r/pUhx2F1Ths'>Feedback</a></h4>
    </div>
   

    <div class='tab'>
        <button class='tablinks' onclick='openInsight(event, ""Disk"")'>Disk Filters</button>
        <button class='tablinks' onclick='openInsight(event, ""Network"")'>Network Filters</button>

    </div>

    <div id = 'Disk' class='Insight'>
        <DIV>
           {DISKFILTER}
        </DIV>
    </div>

    <div id = 'Network' class='Insight'>

        <div class='tab2'> 
{CONTENTTAB}
            </div>
                 {CONTENT}
         
             </div>
         
             <script>
                 function openInsight(evt, nic) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName('Insight');
    // HIDE ALL 
    for (i = 0; i <tabcontent.length; i++)
    {
        tabcontent[i].style.display = 'none';
    }
    // GET THE LEVEL 1 TABS  AKA GET THE NICS
    tablinks = document.getElementsByClassName('tablinks');
    for (i = 0; i <tablinks.length; i++)
    {
        tablinks[i].className = tablinks[i].className.replace(' active', '');
    }
    document.getElementById(nic).style.display = 'block'; // SHOW THE ID
    evt.currentTarget.className += ' active';
}

function openDetails(evt, cityName)
{
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName('tabcontent');
    for (i = 0; i <tabcontent.length; i++)
    {
        tabcontent[i].style.display = 'none';
    }
    tablinks = document.getElementsByClassName('nic');
    for (i = 0; i <tablinks.length; i++)
    {
        temp = tablinks[i].className;
        temp = temp.replaceAll(' active', '')
                temp = temp.replaceAll(' selected', '')
                tablinks[i].className = temp
            }
    document.getElementById(cityName).style.display = 'block';
    evt.currentTarget.className += ' selected active ';
}
document.getElementById('defaultOpen').click();
    </script>
</body>
</html>";




            string cont = "";
            foreach (NIC nic in nics)
            {
                if (nic.Filters == null)
                {
                    
                   continue;
                }
                string auxl = "";
                if( i == 0){
                    auxl = "";
                }
                else
                {
                    auxl = i.ToString();
                }
                nicstabs = nicstabs + @"<button class='nic' onclick='openDetails(event, """ + "nic" + i.ToString() + '"' + ")' id='defaultOpen"+ auxl +"'>" +
                nic.Name 
                + @"<br>"+
                nic.GUID +
                "</button>";


                // GENERATE CONTENT

                string service = @"<table>
                                  <thead>
                                      <tr>
                                          <th> Property</th>
                                          <th> Value</th>
                                      </tr>
                                  </thead>
                                  <tbody>
                                      <tr>
                                          <td> Name</td>
                                          <td> {NAME}</td>
                                      </tr>
                                      <tr>
                                          <td> Start</td>
                                          <td> {START} </td>
                                      </tr>
                                      <tr>
                                          <td> ImagePath</td>
                                          <td> {IMAGEPATH}</td>
                                         </tr>
                                      <tr>
                                             <td> HelpText</td>
                                             <td>{HELPTEXT}</td>
                                             </tr>
                                         </tbody>
                                     </table>";

                string filters = "";

                if (nic.Filters != null) {
                    foreach (Filter filter in nic.Filters)
                    {
                        service = service.Replace("{NAME}", filter.service.Name);
                        service = service.Replace("{START}", filter.service.Start);
                        service = service.Replace("{IMAGEPATH}", filter.service.ImagePath);
                        service = service.Replace("{HELPTEXT}", filter.service.HelpText);

                        filters =  filters + "<tr><td>" + filter.ComponentId + "</td>" + "<td>" + filter.ComponentDescription + "</td>" + "<td>" + service + "</td></tr>";
                    } }
                else
                {
                    filters = "<tr>" + "NO FILTERS ASSOCIATED" + "</tr>";
                }
               
             string htmlnic = @" <div id = {NICVALUE} class= 'tabcontent'>
                          <h3> {NICNAME} </h3>
                          <h4> {NICGUID} </h3>
                              <h4> Last Seen {DATE}</h3>
                                 <table>
                                     <thead>
                                         <tr>
                                             <th> ComponentId</th>
                                             <th> ComponentDescription</th>
                                             <th> service</th>
                                         </tr>
                                     </thead>
                                     <tbody>" +
                                      filters
                         + @"</tbody>   
                     </table>
                 </div>";
               
                string nicid = "nic" + i.ToString();
                htmlnic = htmlnic.Replace("{NICNAME}", nic.Name);
                htmlnic = htmlnic.Replace("{NICGUID}", nic.GUID);
                htmlnic = htmlnic.Replace("{DATE}", nic.LastSeen);
                htmlnic = htmlnic.Replace("{NICVALUE}", nicid);
                cont =   cont+ htmlnic;
                i++;
            }

            html = html.Replace("{CONTENTTAB}", nicstabs);
            html = html.Replace("{CONTENT}", cont);

            return html;



        }


        public List<NIC> GetNICFilters()
        {
            string output = "";


            RegistryKey key = ROOT.GetKey(@"ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}");

           
            List<string> RootDevice = new List<string>();
            List<NIC> nics = new List<NIC>();


            List<Filter> FilterList = new List<Filter>();
            foreach (RegistryKey k in key.SubKeys)
            {
                FilterList = new List<Filter>();
                string nicname = getValue(k, "DriverDesc");
                string nicGUID = getValue(k, "NetCfgInstanceId");
               
              

                string keyname = k.KeyName.ToString();


                RegistryKey aux = ROOT.GetKey(@"ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\" + keyname + "\\Linkage");
                if (aux == null)
                {
                    continue;
                }


                string FL = getValue(aux, "FilterList");
                if (FL != "")
                {
                    List<string> lines = FL.Split(' ').ToList();
                    List<string> GUIDS = new List<string>();
                    foreach (string line in lines)
                    {
                        int start = line.IndexOf('{', line.IndexOf('{') + 1);
                        int end = line.IndexOf('}', line.IndexOf('}') + 1);

                        string GUID = line.Substring(start, end - start + 1);
                        GUIDS.Add(GUID);
                    }
                    GUIDS = GUIDS.Distinct().ToList(); // REMOVE DUPLICATE GUIDS
                    foreach (string GUID in GUIDS)
                    {
                        string Path = @"ControlSet001\Control\Network\{4d36e975-e325-11ce-bfc1-08002be10318}\" + GUID;
                        RegistryKey translate = ROOT.GetKey(Path);

                        if (translate == null)
                        {
                            Path = @"ControlSet001\Control\Network\{4d36e974-e325-11ce-bfc1-08002be10318}\" + GUID;
                            translate = ROOT.GetKey(Path);
                        }

                        string cid = getValue(translate, "ComponentId");
                        string Desc = getValue(translate, "Description");

                        RegistryKey ServiceRef = ROOT.GetKey(Path +"\\Ndi");
                        string Service = getValue(ServiceRef, "Service");
                        string helptext = getValue(ServiceRef, "HelpText");

                        //get services
                        Path = @"ControlSet001\Services\" + Service;
                                                        
                        RegistryKey serviceInfo = ROOT.GetKey(Path);

  
                        string imagepath = getValue(serviceInfo, "ImagePath");
                        string Start = getValue(serviceInfo, "Start");


                        Start = Start + " (" + StartUpType(Int32.Parse(Start)) + ")";

                        if (cid != "" & Desc != "")
                        {
                            FilterList.Add(new Filter { ComponentId = cid, ComponentDescription = Desc, service = new Service { Name = Service, HelpText = helptext, ImagePath = imagepath, Start = Start } }) ;
                        }
                    }
                    nics.Add(new NIC { Name = getValue(k, "DriverDesc") , GUID = nicGUID, Filters = FilterList });
                }
                else
                {
                    nics.Add(new NIC { Name = getValue(k, "DriverDesc"), GUID = nicGUID, Filters = null  });
                }
              
            }
            var serializer = new JavaScriptSerializer();

          //  var json = serializer.Serialize(nics);
           // generateReport(json.ToString());
            return nics;
        }


    }
}
