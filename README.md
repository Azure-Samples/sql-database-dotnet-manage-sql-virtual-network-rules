---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
- services: Sql
- platforms: dotnet
---

# Getting started on managing SQL virtual network rules in C# #

 Azure SQL sample for managing SQL Virtual Network Rules
  - Create a Virtual Network with two subnets.
  - Create a SQL Server along with one virtual network rule.
  - Add another virtual network rule in the SQL Server
  - Get a virtual network rule.
  - Update a virtual network rule.
  - List all virtual network rules.
  - Delete a virtual network.
  - Delete Sql Server


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/sql-database-dotnet-manage-sql-virtual-network-rules.git

    cd sql-database-dotnet-manage-sql-virtual-network-rules

    dotnet build

    bin\Debug\net452\ManageSqlVirtualNetworkRules.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.