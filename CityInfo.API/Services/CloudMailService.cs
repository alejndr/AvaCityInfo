using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
	public class CloudMailService : IMailService
	{
		// Cogemos los mails del archivo de configuración appSettings.json
		private string _mailTo = Startup.Configuration["mailSettings:mailToAddress"];
		private string _mailFrom = Startup.Configuration["mailSettings:mailFromAddress"];

		public void Send(string subject, string message)
		{
			// Send mail
			Debug.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with CloudMainService.");
			Debug.WriteLine($"Subject: {subject}");
			Debug.WriteLine($"Message: {message}");
		}
	}
}
