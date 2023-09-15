// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using System;
using System.Collections.Immutable;

namespace ManageSqlVirtualNetworkRules
{
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        /**
         * Azure SQL sample for managing SQL Virtual Network Rules
         *  - Create a Virtual Network with two subnets.
         *  - Create a SQL Server along with one virtual network rule.
         *  - Add another virtual network rule in the SQL Server
         *  - Get a virtual network rule.
         *  - Update a virtual network rule.
         *  - List all virtual network rules.
         *  - Delete a virtual network.
         *  - Delete Sql Server
         */
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                // ============================================================
                //Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                //Create a resource group in the EastUS region
                string rgName = Utilities.CreateRandomName("rgSQLServer");
                Utilities.Log("Creating resource group...");
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
                ResourceGroupResource resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log($"Created a resource group with name: {resourceGroup.Data.Name} ");

                // ============================================================
                // Create a virtual network with two subnets.
                Utilities.Log("Create a virtual network with two subnets: subnet1 and subnet2");

                var virtualNetworkName = Utilities.CreateRandomName("vnetsql");
                var subnet1Name = Utilities.CreateRandomName("testSubnet1-");
                var subnet2Name = Utilities.CreateRandomName("testSubnet2-");
                var subnet1Data = new SubnetData()
                {
                    Name = subnet1Name,
                    AddressPrefix = "192.168.1.0/24",
                    ServiceEndpoints =
                    {
                        new ServiceEndpointProperties()
                        {
                            Service = "Microsoft.Sql"
                        }
                    }
                };
                var subnet2Data = new SubnetData()
                {
                    Name = subnet2Name,
                    AddressPrefix = "192.168.2.0/24",
                    ServiceEndpoints =
                    {
                        new ServiceEndpointProperties()
                        {
                            Service = "Microsoft.Sql"
                        }
                    }
                };
                var list = new List<SubnetData>() { subnet1Data, subnet2Data };
                var virtualNetworkData = new VirtualNetworkData()
                {
                    Location = AzureLocation.SoutheastAsia,
                    AddressPrefixes = { "192.168.0.0/16" },
                    Subnets =
                    {
                        list[0],list[1]
                    }
                };
                var virtualNetwork = (await resourceGroup.GetVirtualNetworks().CreateOrUpdateAsync(WaitUntil.Completed, virtualNetworkName, virtualNetworkData)).Value;

                Utilities.Log($"Created a virtual network with name: {virtualNetwork.Data.Name}");
                // Print the virtual network details
                Utilities.Log($"Print the virtual network ip address: {virtualNetwork.Data.AddressPrefixes[0]}");
                Utilities.Log($"Print the virtual network Subnet: {virtualNetwork.Data.Subnets[0].Name}, {virtualNetwork.Data.Subnets[1].Name}");

                // ============================================================
                // Create a SQL Server, with one virtual network rule.
                Utilities.Log("Create a SQL server with one virtual network rule");

                Utilities.Log("Creating a SQL server...");
                var sqlServerName = Utilities.CreateRandomName("sqlserver-vntest");
                string sqlAdmin = "sqladmin1234";
                string sqlAdminPwd = Utilities.CreatePassword();
                SqlServerData sqlServerData = new SqlServerData(AzureLocation.SoutheastAsia)
                {
                    AdministratorLogin = sqlAdmin,
                    AdministratorLoginPassword = sqlAdminPwd
                };
                var sqlServer = (await resourceGroup.GetSqlServers().CreateOrUpdateAsync(WaitUntil.Completed, sqlServerName, sqlServerData)).Value;
                Utilities.Log($"Created a SQL server with name: {sqlServer.Data.Name}");

                Utilities.Log("Creating one virtual network rule in SQL Server...");
                string virtualNetworkRuleName = "virtualNetworkRule1";
                var subnet1Id = (await virtualNetwork.GetSubnetAsync(subnet1Name)).Value.Data.Id;
                var virtualNetworkRuleData = new SqlServerVirtualNetworkRuleData()
                {
                    VirtualNetworkSubnetId = new ResourceIdentifier(subnet1Id),
                    IgnoreMissingVnetServiceEndpoint = false
                };
                var virtualNetworkRule = (await sqlServer.GetSqlServerVirtualNetworkRules().CreateOrUpdateAsync(WaitUntil.Completed, virtualNetworkRuleName, virtualNetworkRuleData)).Value;
                Utilities.Log($"Created one virtual network rule in SQL Server with name: {virtualNetworkRule.Data.Name}");

                // ============================================================
                // Get the virtual network rule created above.
                virtualNetworkRule = (await sqlServer.GetSqlServerVirtualNetworkRuleAsync(virtualNetworkRule.Data.Name)).Value;

                Utilities.Log($"Get the virtual network rule created above with name: {virtualNetworkRule.Data.Name}");

                // ============================================================
                // Add new virtual network rules.
                Utilities.Log("Adding another virtual network rule in existing SQL Server...");
                string virtualNetworkRule2Name = "virtualNetworkRule2";
                var subnet2Id = (await virtualNetwork.GetSubnetAsync(subnet2Name)).Value.Data.Id;
                var virtualNetworkRule2Data = new SqlServerVirtualNetworkRuleData()
                {
                    VirtualNetworkSubnetId = new ResourceIdentifier(subnet2Id),
                    IgnoreMissingVnetServiceEndpoint = true
                };
                virtualNetworkRule = (await sqlServer.GetSqlServerVirtualNetworkRules().CreateOrUpdateAsync(WaitUntil.Completed,virtualNetworkRule2Name,virtualNetworkRule2Data)).Value;

                Utilities.Log($"Added another virtual network rule in existing SQL Server with SubnetId: {virtualNetworkRule.Data.Name}");

                // ============================================================
                // Update a virtual network rules.
                Utilities.Log("Updating an existing virtual network rules in SQL Server...");
                var updateVirtualNetworkRuleData = new SqlServerVirtualNetworkRuleData()
                {
                    VirtualNetworkSubnetId = new ResourceIdentifier(subnet1Id),
                };
                virtualNetworkRule = (await virtualNetworkRule.UpdateAsync(WaitUntil.Completed,updateVirtualNetworkRuleData)).Value;

                Utilities.Log($"Updated an existing virtual network rules with SubnetId: {virtualNetworkRule.Data.VirtualNetworkSubnetId}");

                // ============================================================
                // List and delete all virtual network rules.
                Utilities.Log("Listing all virtual network rules in SQL Server...");

                var virtualNetworkRules = sqlServer.GetSqlServerVirtualNetworkRules().ToList();
                foreach (var vnetRule in virtualNetworkRules)
                {
                    // Delete the virtual network rule.
                    Utilities.Log($"Deleting a virtual network rule with name: {vnetRule.Data.Name} ...");
                    await vnetRule.DeleteAsync(WaitUntil.Completed);
                }

                // Delete the SQL Server.
                Utilities.Log("Deleting a Sql Server...");
                await sqlServer.DeleteAsync(WaitUntil.Completed);
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group...");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId.Name}");
                    }
                }
                catch (Exception e)
                {
                    Utilities.Log(e);
                }
            }
        }
        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e.ToString());
            }
        }
    }
}