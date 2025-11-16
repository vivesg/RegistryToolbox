

$WindowsServer = Get-AzVMImageSku -Location "EastUs2" -PublisherName "MicrosoftWindowsServer" -Offer "WindowsServer" 

$WindowsServer | ForEach-Object {
    Write-Output "Getting SKU: $($_.Skus) "
    $LatestImage = Get-AzVMImage -Location "EastUs2" -PublisherName "MicrosoftWindowsServer" -Offer "WindowsServer" -Skus $_.Skus -Version latest
    Write-Output "    Latest Image Version: $($LatestImage.Version)"
    if ($latestImage.Version -ne $null) {
        $urn = "$($LatestImage.PublisherName):$($LatestImage.Offer):$($LatestImage.Skus):$($LatestImage.Version)"
        $RegistryName = "$($LatestImage.Offer)-$($LatestImage.Skus)-$($LatestImage.Version)"
        Write-Output "Generating $RegistryName"
    
 
        $resourceGroupName = "TempVMs"
        $location = "EastUs2"
        $vmName = "tempvm"    

        $username = "azureuser"
        $password = ConvertTo-SecureString "VMadmin.VMadmin." -AsPlainText -Force
        $credential = New-Object System.Management.Automation.PSCredential ($username, $password)

        $vm = New-AzVm -ResourceGroupName $resourceGroupName -Name $vmName -Size "Standard_B2ms" -Location $location -Image $urn -VirtualNetworkName "myVnet" -SubnetName "mySubnet" -Credential $credential

        # Remove VM without deleting the OS Disk
        Remove-AzVm -ResourceGroupName $resourceGroupName -Name $vmName -Force 
        Start-Sleep 60
        # Get the OS Disk
        $osDisk = Get-AzDisk -ResourceGroupName $resourceGroupName -DiskName $vm.StorageProfile.OsDisk.Name
        #Delete Nic 
        Remove-AzNetworkInterface -ResourceGroupName $resourceGroupName -Name $vm.NetworkInterfaceIDs.Split('/')[-1] -Force

        # HOST VM 
        #attach disk to HostVM
        $hostVm = Get-AzVM -ResourceGroupName $resourceGroupName -Name "MyVM"
        $hostVm = Add-AzVMDataDisk -VM $hostVm -Name $osDisk.Name -CreateOption Attach -ManagedDiskId $osDisk.Id -Lun 1
        Update-AzVM -ResourceGroupName $resourceGroupName -VM $hostVm

        #WAIT FOR HOST VM TO RECOGNIZE DISK
        Get-Disk | Where-Object IsOffline -eq $true | Set-Disk -IsOffline $false
        Start-Sleep -Seconds 30

        mkdir "C:\Registry" -ErrorAction SilentlyContinue
        # COPY SYSTEM HIVE
        Copy-Item -Path "F:\Windows\System32\Config\System" -Destination "C:\Registry\$RegistryName" -Force
        Start-Sleep -Seconds 10
        # DETACH DISK FROM HOST VM
        $hostVm = Get-AzVM -ResourceGroupName $resourceGroupName -Name "MyVM"
        $hostVm = Remove-AzVMDataDisk -VM $hostVm -Name $osDisk.Name
        Update-AzVM -ResourceGroupName $resourceGroupName -VM $hostVm
        Remove-AzDisk -ResourceGroupName $resourceGroupName -DiskName $osDisk.Name -Force
    }

}

