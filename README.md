# RegistryToolbox
Windows Registry binary files comparison Tool.

[![image](https://user-images.githubusercontent.com/8367687/216160596-b89e711c-f493-40b8-af74-7c069a787304.png)](https://github.com/vivesg/RegistryToolbox/releases/download/v1.3.0/RegistryToolbox.exe)


![foo](https://user-images.githubusercontent.com/8367687/115762728-02428080-a361-11eb-9f59-b583e7581531.png)


## Functionality

On the Left panel we can find 4 main buttons that are 

### 1. Load Registry

What it does it is to open a Windows Registry Hive binary
Files located on  *C:\Windows\System32\Config*
i.e  *System*, *Software* from a Windows PC or VM

![image](https://user-images.githubusercontent.com/8367687/140194670-20f095da-1050-4cfb-ad87-ea8823cb0319.png)

You can find this files from differente sources *(IaaS Disk report)*
or tools like https://github.com/Azure/azure-diskinspect-service

When selected it's going to ask for a File path

![image](https://user-images.githubusercontent.com/8367687/140195431-52f4ef61-a4f2-45b8-ba4d-fbfb61c688ec.png)

### You can explore the Registry tree and move to check the values 

If selected for example in the previous image you can find the Key Entry call Control if you press B you are going to go to BackupRestore key so it's going to look in the subkeys

As well you can use the mouse to move in the Registry Tree explorer

![image](https://user-images.githubusercontent.com/8367687/140195906-cbf3d9e2-7f7c-47db-86b9-797585322d34.png)

As well you can go to an specific registry with the path 

Just type the Registry key path that you want to go and Press the Blue arrow button

![image](https://user-images.githubusercontent.com/8367687/140196988-05cb2c34-ec38-4a9f-aec8-a1790c143dac.png)

For example in Regedit on windows a path looks like this

*HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Terminal Server\WinStations\RDP-Tcp*

Registry Toolbox uses paths like this, please see that the HKLM\System it's not in the path as this File is a System hive so only contains the subcontents so the Paths needs to remove the HLM\System and HLM\Software if this files are loaded

Example of a Valid path 
*ControlSet001\Control\Terminal Server\WinStations\RDP-Tcp*



![image](https://user-images.githubusercontent.com/8367687/140196858-a9fe34c3-ea5e-4f6b-b546-9b2b7e5a23eb.png)


### 2. Load 2 Hives

Please Select the second option on the left panel call **Load hives**

![image](https://user-images.githubusercontent.com/8367687/140197647-e42a934f-e96e-4e74-b252-57ebb5f4926d.png)


When the 2 files are loaded you are going to see the tool like this

![image](https://user-images.githubusercontent.com/8367687/140198106-a2dad93a-ef3a-47af-96de-3698dba6c22c.png)


You can customize the name of the registry for example on the left i wrote Working and on the right Not Working. 

As well you can see the file path open 

Example **C:\temp\DemoRegToolbox\SystemBefore** 
Remember Windows Registries files have no extension on the names


![image](https://user-images.githubusercontent.com/8367687/140198243-ca41e9ac-6e38-4ce3-b23d-a337ca4886ec.png)


Press to customize the Title color of the panel example in this case was red selected to show Not working

![image](https://user-images.githubusercontent.com/8367687/140198395-21f098cd-4154-492b-b21a-2838212563b4.png)


# Comparaison
When two registries opened if you click on the Button 
COMPARE KEYS

![image](https://user-images.githubusercontent.com/8367687/140209655-44919f5d-9698-4bae-9e4b-66f341100689.png)


You are going to see that the Registry Toolbox it's going to show on the registry Tree on Red in bold text the keys that have differences

If you click in SETUP in this example you can see

- On the top you can see the registry tree differences
- If you open the Setup key you can see it contains differences on the key values **SystemPartition** and **Respecialize**

Red: Means that the value is different between registries
Light blue: means that the value it is on a registry but not in the other in this example 

***respecialize*** Its on the registry on the left but not in the right.

![image](https://user-images.githubusercontent.com/8367687/140205540-6fe22ff8-6828-485c-967f-8189e3ab1723.png)

***CONGRATULATIONS***
You can now find differences on your Windows Registry data.

***TIP***
If you are comparing a value on the left and you want to see the same value on the right please press 
MATCH PATH button this will set the values of the Registry Path equally to the same place


![image](https://user-images.githubusercontent.com/8367687/140209995-f048fb47-9841-4db9-9f5f-aa2bb0a038a5.png)




1. Export .REG (Experimental)

On the left registry or with one registry loaded click on the Registry tree and then select export, this is going to export the registry to .REG format

2. Compare .Reg (it requires VSCODE installed)
   
It going to ask to open 2 .reg files and will open VS Code in comparaison mode.
