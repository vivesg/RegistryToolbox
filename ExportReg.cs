using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Registry;
using Registry.Abstractions;
using RegistryToolbox.Models;

namespace RegistryToolbox
{

    class ExportReg
    {
        private ModelRegistryKey myHive;
        private string Path;
        private string root;

        public ExportReg(ModelRegistryKey myHive, string path, string proot)
        {
            this.myHive = myHive;
            Path = path;
            
        }
        public void ExpToReg(string fPath)
        {
            List<string> lines = new List<string>();
            lines.Add("Windows Registry Editor Version 5.00");
            lines.Add("");
            lines.AddRange(this.Export_Key(myHive));

           using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(fPath))
            {
                foreach (string line in lines)
                {
                   file.WriteLine(line);
                }
                
            }

        }
        private string get_Path(ModelRegistryKey key)
        {
            return Path;
        }
        private string ByteArrayToString_LittleEndian(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            for(int i = ba.Length-1;  i >= 0 ; i--)  // ENDIAN CONFIGURATION
                hex.AppendFormat("{0:x2}", ba[i]);   
            return hex.ToString();
        }
        private string ByteArrayToString_BigEndian(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            for (int i = 0 ; i < ba.Length; i++)  // ENDIAN CONFIGURATION
                hex.AppendFormat("{0:x2}", ba[i]);
            return hex.ToString();
        }
        static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
        private List<string> Export_Key(ModelRegistryKey key)
        {
            List<string> output = new List<string>();

            output.Add("[" + root + "\\" + get_Path(key) + "]");
            foreach (ModelRegistryKeyValues value in key.SubkeysValues)
            {
                string a = "\"" + value.Name.ToString() + "\""; 
                string type = "";
                if (value.Type.ToString() == "RegDword")
                {
                    type = "dword";
                    string b = "=" + type + ":" + ByteArrayToString_LittleEndian(value.ValueRaw); //THIS ASSUMES dword
                    a = a + b;
                    output.Add(a);
                }
                else if (value.Type.ToString() == "RegSz")
                {

                    string b = "=" + "\"" + value.Value + "\""; //THIS ASSUMES dword
                    a = a + b;
                    output.Add(a);
                }
                else if (value.Type.ToString() == "RegQword")
                {

                    string b = "=" + "\"" + value.Value + "\""; //THIS ASSUMES dword
                    a = a + b;
                    output.Add(a);
                }
                else if (value.Type.ToString() == "RegBinary")
                {
                    List<string> lineas = new List<string>();
                    string linea = "";
                    linea = a + "=hex:";
                    int primera = 0;
                    int contador = 0;
                    IEnumerable<string> lista = Split(ByteArrayToString_BigEndian(value.ValueRaw), 2);
                    contador = 1;
                    for (int i = 0; i < lista.Count<string>(); i++)
                    {
                        
                        if (primera == 0 & (i+1)%20==0)
                        {
                            primera = 1;
                            linea = linea + "\\";
                            lineas.Add(linea);
                            linea = "";
                            contador = 1;
                        }
                        if (i == lista.Count<string>() - 1)
                        {
                            linea = linea + lista.ElementAt(i);
                        }
                        else
                        {
                            linea = linea + lista.ElementAt(i) + ",";
                        }
                       
                        if (contador == 25 & primera !=0)
                        {
                            linea = linea + "\\";
                            lineas.Add(linea);
                            linea = "";
                            contador = 0;
                        }
                        contador++;
                    }
                    if (linea != "")
                        lineas.Add(linea);
                    output.AddRange(lineas);
                }
               
            }
            output.Add("");

            if (key.Subkeys.Count != 0)
            {
                foreach(ModelRegistryKey skey in key.Subkeys)
                {
                    this.Path = this.Path + "\\" + skey.Name;
                    output.AddRange(Export_Key(skey));
                    this.Path = this.Path.Replace( "\\" + skey.Name.ToString(),""); 
                }
            }
            return output;
        }


    }
}
