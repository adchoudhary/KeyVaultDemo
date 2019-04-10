using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			var azureServiceTokenProvider = new AzureServiceTokenProvider();
			var keyVaultClient = new KeyVaultClient(
				 new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
			var secret =  keyVaultClient.GetSecretAsync(
				"https://{{my-vault-name}}.vault.azure.net/", "{{my-secret}}").Result;
		}
	}
}
