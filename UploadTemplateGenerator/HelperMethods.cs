//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HelperMethods.cs" company="LeaseQuery LLC">
//    Copyright (c)2020, LeaseQuery, LLC All Rights Reserved.
//  </copyright>
//  <summary>
//    The HelperMethods.cs in Project PlaywrightRestSharp in solution "PlaywrightRestSharp"
//     Created by login: Mark Judson on 2022-10-05, 09:29
//  </summary>
//   --------------------------------------------------------------------------------------------------------------------
namespace UploadTemplateGenerator;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using RestSharp;

public class HelperMethods 
{
	/// <summary>Create configuration reference</summary>
	/// <param name="settingsFile"></param>
	/// <returns>Configuration area</returns>
	public static IConfiguration CreateConfig(string? settingsFile = null)
	{
		if (string.IsNullOrEmpty(settingsFile))
		{
			settingsFile = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
		}

		var config = new ConfigurationBuilder()
			.AddJsonFile(settingsFile, true, true)
			.Build();

		return config;
	}

	public static List<Cookie>? BuildAuthCookies(RestResponse response)
	{
		if (response.Cookies == null)
		{
			return null;
		}

		var responseCookie = response.Cookies["APIAuth"];
		var cookies = new List<Cookie>();
		if (responseCookie != null)
		{
			var expires = (responseCookie.Expires - new DateTime(1970, 1, 1)).TotalSeconds;
			cookies = new List<Cookie>
			{
				new Cookie
				{
					Name = responseCookie.Name,
					Value = responseCookie.Value,
					Domain = ".leasequery.com",
					Expires = (float)expires,
					Path = responseCookie.Path
				},
				new Cookie
				{
					Name = "IsSSOLogin",
					Value = "False",
					Domain = "inittest.leasequery.com",
					Path = "/"
				},
			};
		}

		return cookies;
	}
	public static class StringFormatting 
	{
		public static string AddSpaceToCamelCase(string value)
		{
			var camelCase = new Regex(@"(?<name>[A-Z][a-z]+)");
			var items = camelCase.Matches(value);
			var newValue = string.Empty;
			foreach (Match item in items)
			{
				newValue += $"{item.Groups["name"].Value} ";
			}

			return newValue;
		}
	}
}
