using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using Microsoft.Rest;

namespace TestingWindowsAzure
{
    class Program
    {
        static void Main(string[] args)
        {
            VMManager vm = new VMManager();
            vm.m_cientID = "{{}}";
            vm.m_clientSecret = "{{}}";
            vm.m_subscriptionID = "{{}}";
            vm.m_tenandID = "{{}}";
            vm.m_location = "eastasia";
            vm.m_groupname = "NewTestGroup";
            vm.m_stroagename = "newstroageaccount";
            vm.m_ipname = "newipname";
            vm.vnetName = "newtestvname";
            vm.m_ipname = "newipname";
            vm.nicName = "newnicname";
            vm.subnetName = "default";
            vm.avsetName = "favset";
            vm.vmName = "vmnet";
            vm.adminName = "{{}}";
            vm.adminPassword = "{{}}";
            
            try
            {
                var token = vm.GetAccessTokenAsync();

                var credential = new TokenCredentials(token.Result.AccessToken);

                var rgResult = vm.CreateResourceGroupAsync(credential);
                Console.WriteLine(rgResult.Result.Properties.ProvisioningState);


                var stResult = vm.CreateStorageAccountAsync(credential);
                Console.WriteLine(stResult.Result.ProvisioningState);

                var ipResult = vm.CreatePublicIPAddressAsync(credential);
                Console.WriteLine(ipResult.Result.ProvisioningState);

                var vnResult = vm.CreateVirtualNetworkAsync(credential);
                Console.WriteLine(vnResult.Result.ProvisioningState);

                var ncResult = vm.CreateNetworkInterfaceAsync(credential);
                Console.WriteLine(ncResult.Result.ProvisioningState);

                var avResult = vm.CreateAvailabilitySetAsync(credential);
                Console.WriteLine(avResult.Result.ToString());

                var vmResult = vm.CreateVirtualMachineAsync(credential);
                Console.WriteLine(vmResult.Result.ProvisioningState);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.ToString());
            }
        }
    }
}
