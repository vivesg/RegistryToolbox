using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Registry;
using Registry.Abstractions;

namespace RegistryToolbox
{

    class ExportReg
    {
        private Registry.RegistryHive myHive;
        private string Path;
        private string root;

        public ExportReg(RegistryHive myHive, string path, string root)
        {
            this.myHive = myHive;
            Path = path;
            this.root = "HKEY_LOCAL_MACHINE\\SYSTEM";
        }
        public void ExpToReg(string fPath)
        {
            RegistryKey key = myHive.GetKey(this.Path);
            List<string> lines = new List<string>();
            lines.Add("Windows Registry Editor Version 5.00");
            lines.Add("");
            lines.AddRange(this.Export_Key(key));

           using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(fPath))
            {
                foreach (string line in lines)
                {
                   file.WriteLine(line);
                }
                
            }

        }
        private string get_Path(RegistryKey key)
        {
            string ok = key.KeyPath.ToString().Replace("ROOT\\", "");
            return ok;
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
        private List<string> Export_Key(RegistryKey key)
        {
            List<string> output = new List<string>();

            output.Add("[" + root + "\\" + get_Path(key) + "]");
            foreach (KeyValue value in key.Values)
            {
                string a = "\"" + value.ValueName.ToString() + "\""; 
                string type = "";
                if (value.ValueType.ToString() == "RegDword")
                {
                    type = "dword";
                    string b = "=" + type + ":" + ByteArrayToString_LittleEndian(value.ValueDataRaw); //THIS ASSUMES dword
                    a = a + b;
                    output.Add(a);
                }
                else if (value.ValueType.ToString() == "RegSz")
                {

                    string b = "=" + "\"" + value.ValueData + "\""; //THIS ASSUMES dword
                    a = a + b;
                    output.Add(a);
                }
                else if (value.ValueType.ToString() == "RegBinary")
                {
                    List<string> lineas = new List<string>();
                    string linea = "";
                    linea = a + "=hex:";
                    int primera = 0;
                    int contador = 0;
                    IEnumerable<string> lista = Split(ByteArrayToString_BigEndian(value.ValueDataRaw), 2);
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

            if (key.SubKeys.Count != 0)
            {
                foreach(RegistryKey skey in key.SubKeys)
                {
                    output.AddRange(Export_Key(skey));
                }
            }
            return output;
        }


    }
}
