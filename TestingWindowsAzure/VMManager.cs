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
    public class VMManager
    {
        public string m_cientID { set; get; }
        public string m_clientSecret { set; get; }
        public string m_tenandID { set; get; }
        public string m_subscriptionID { set; get;}

        public string m_location { set; get; }
        public string m_groupname { set; get; }
        public string m_stroagename { set; get;}

        public string vnetName { set; get; }
        public string subnetName { set; get; }


        public string m_ipname { set; get;}
        public string nicName { get;  set; }
        public string avsetName { get; set; }
        public string adminName { get;  set; }
        public string adminPassword { get;  set; }
        public string vmName { get;  set; }

        public async Task<AuthenticationResult> GetAccessTokenAsync()
        {
            var cc = new ClientCredential(m_cientID, m_clientSecret);
            var context = new AuthenticationContext(string.Format("https://login.windows.net/{0}", m_tenandID));
            AuthenticationResult token = null;
            try
            {

                token = await context.AcquireTokenAsync("https://management.azure.com/", cc);
                if (token == null)
                {
                    throw new InvalidOperationException("Could not get the token");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.InnerException.ToString());
            }
            return token;
        }

        public  async Task<ResourceGroup> CreateResourceGroupAsync(
        TokenCredentials credential)
        {
            var resourceManagementClient = new ResourceManagementClient(credential)
            { SubscriptionId = m_subscriptionID };

            Console.WriteLine("Registering the providers...");
            var rpResult = resourceManagementClient.Providers.Register("Microsoft.Storage");
            Console.WriteLine(rpResult.RegistrationState);
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Network");
            Console.WriteLine(rpResult.RegistrationState);
            rpResult = resourceManagementClient.Providers.Register("Microsoft.Compute");
            Console.WriteLine(rpResult.RegistrationState);

            Console.WriteLine("Creating the resource group...");
            var resourceGroup = new ResourceGroup { Location = m_location };
            return await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(m_groupname, resourceGroup);
        }

        public async Task<StorageAccount> CreateStorageAccountAsync(
        TokenCredentials credential)
        {
            Console.WriteLine("Creating the storage account...");
            var storageManagementClient = new StorageManagementClient(credential)
            { SubscriptionId = m_subscriptionID };
            return await storageManagementClient.StorageAccounts.CreateAsync(
              m_groupname,
              m_stroagename,
              new StorageAccountCreateParameters()
              {
                  Sku = new Microsoft.Azure.Management.Storage.Models.Sku()
                  { Name = SkuName.StandardLRS },
                  Kind = Kind.Storage,
                  Location = m_location
              }
            );
        }

        public async Task<PublicIPAddress> CreatePublicIPAddressAsync(TokenCredentials credential)
        {
            Console.WriteLine("Creating the public ip...");
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = m_subscriptionID };
            return await networkManagementClient.PublicIPAddresses.CreateOrUpdateAsync(
              m_groupname,
              m_ipname,
              new PublicIPAddress
              {
                  Location = m_location,
                  PublicIPAllocationMethod = "Dynamic"
              }
            );
        }

        public async Task<VirtualNetwork> CreateVirtualNetworkAsync(
        TokenCredentials credential)
        {
            Console.WriteLine("Creating the virtual network...");
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = m_subscriptionID };

            var subnet = new Subnet
            {
                Name = subnetName,
                AddressPrefix = "10.0.0.0/24"
            };

            var address = new AddressSpace
            {
                AddressPrefixes = new List<string> { "10.0.0.0/16" }
            };

            return await networkManagementClient.VirtualNetworks.CreateOrUpdateAsync(
              m_groupname,
              vnetName,
              new VirtualNetwork
              {
                  Location = m_location,
                  AddressSpace = address,
                  Subnets = new List<Subnet> { subnet }
              }
            );
        }


        public async Task<NetworkInterface> CreateNetworkInterfaceAsync(
        TokenCredentials credential)
        {
            Console.WriteLine("Creating the network interface...");
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = m_subscriptionID };
            var subnetResponse = await networkManagementClient.Subnets.GetAsync(
              m_groupname,
              vnetName,
              subnetName
            );
            var pubipResponse = await networkManagementClient.PublicIPAddresses.GetAsync(m_groupname, m_ipname);

            return await networkManagementClient.NetworkInterfaces.CreateOrUpdateAsync(
              m_groupname,
              nicName,
              new NetworkInterface
              {
                  Location = m_location,
                  IpConfigurations = new List<NetworkInterfaceIPConfiguration>
                {
         new NetworkInterfaceIPConfiguration
         {
           Name = nicName,
           PublicIPAddress = pubipResponse,
           Subnet = subnetResponse
         }
                }
              }
            );
        }


        public async Task<AvailabilitySet> CreateAvailabilitySetAsy\nc(
        TokenCredentials credential)
        {
            Console.WriteLine("Creating the availability set...");
            var computeManagementClient = new ComputeManagementClient(credential)
            { SubscriptionId = m_subscriptionID };
            return await computeManagementClient.AvailabilitySets.CreateOrUpdateAsync(
              m_groupname,
              avsetName,
              new AvailabilitySet()
              {
                  Location = m_location
              }
            );
        }

        public async Task<VirtualMachine> CreateVirtualMachineAsync(
        TokenCredentials credential)
        {
            var networkManagementClient = new NetworkManagementClient(credential)
            { SubscriptionId = m_subscriptionID };
            var nic = networkManagementClient.NetworkInterfaces.Get(m_groupname, nicName);

            var computeManagementClient = new ComputeManagementClient(credential);
            computeManagementClient.SubscriptionId = m_subscriptionID;
            var avSet = computeManagementClient.AvailabilitySets.Get(m_groupname, avsetName);

            Console.WriteLine("Creating the virtual machine...");
            return await computeManagementClient.VirtualMachines.CreateOrUpdateAsync(
              m_groupname,vmName
              ,
              new VirtualMachine
              {
                  Location = m_location,
                  AvailabilitySet = new Microsoft.Azure.Management.Compute.Models.SubResource
                  {
                      Id = avSet.Id
                  },
                  HardwareProfile = new HardwareProfile
                  {
                      VmSize = "Standard_A0"
                  },
                  OsProfile = new OSProfile
                  {
                      AdminUsername = adminName,
                      AdminPassword = adminPassword,
                      ComputerName = vmName,
                      WindowsConfiguration = new WindowsConfiguration
                      {
                          ProvisionVMAgent = true
                      }
                  },
                  NetworkProfile = new NetworkProfile
                  {
                      NetworkInterfaces = new List<NetworkInterfaceReference>
                  {
           new NetworkInterfaceReference { Id = nic.Id }
                  }
                  },
                  StorageProfile = new StorageProfile
                  {
                      ImageReference = new ImageReference
                      {
                          Publisher = "MicrosoftWindowsServer",
                          Offer = "WindowsServer",
                          Sku = "2012-R2-Datacenter",
                          Version = "latest"
                      },
                      OsDisk = new OSDisk
                      {
                          Name = "mytestod1",
                          CreateOption = DiskCreateOptionTypes.FromImage,
                          Vhd = new VirtualHardDisk
                          {
                            Uri = "http://" + m_stroagename + ".blob.core.windows.net/vhds/mytestod1.vhd"
                          }
                      }
                  }
              }
            );
        }

    }
}
