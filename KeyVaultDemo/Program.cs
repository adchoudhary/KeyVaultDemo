using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault; 
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace KeyVaultDemo
{
	class Program
	{
		/// <summary>
		/// The Application ID for the app registered with the Azure Active Directory.
		/// </summary>
		/// <remarks>
		/// Register your application in https://portal.azure.com/ within the "App Registrations" blade.
		/// Be sure to grant your app permissions to "Azure Key Vault (AzureKeyVault)".
		/// </remarks>
		private static readonly string ADALClientId = ConfigurationManager.AppSettings.Get("ADALClientId");

		/// <summary>
		/// A URI recorded for the AAD registered app as a valid redirect URI.
		/// </summary>
		/// <remarks>
		/// For example: "https://myapp/finish". Literally, it could be that. You don't need to have a server responding to this URI.
		/// </remarks>
		private static readonly Uri ADALRedirectUri = new Uri(ConfigurationManager.AppSettings.Get("ADALRedirectUri"));

		/// <remarks>
		/// For example: https://yourCoolApp.vault.azure.net/
		/// </remarks>
		private static readonly string KeyVaultAddress = ConfigurationManager.AppSettings.Get("KeyVaultAddress");

		static void Main(string[] args)
		{
			var keyVault = new KeyVaultClient(
				new KeyVaultClient.AuthenticationCallback(GetAccessTokenAsync),
				new HttpClient());
			string secret = keyVault.GetSecretAsync(KeyVaultAddress, "vsazure").Result.Value;
			Console.WriteLine("vsazure secret: " + secret);
		}

		private static async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
		{
			var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
			AuthenticationResult result;
			try
			{
				// Try to get the token from Windows auth
				result = await context.AcquireTokenAsync(resource, ADALClientId, new UserCredential());
			}
			catch (AdalException)
			{
				try
				{
					// Try to get the token silently, either using the token cache or browser cookies.
					result = await context.AcquireTokenAsync(resource, ADALClientId, ADALRedirectUri, new PlatformParameters(PromptBehavior.Never));
				}
				catch (AdalException)
				{
					// OK, ultimately fail: ask the user to authenticate manually.
					result = await context.AcquireTokenAsync(resource, ADALClientId, ADALRedirectUri, new PlatformParameters(PromptBehavior.Always));
				}
			}

			return result.AccessToken;
		}
	}
}