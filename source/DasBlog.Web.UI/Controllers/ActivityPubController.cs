﻿using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using DasBlog.Services;
using DasBlog.Web.Models.ActivityPubModels;
using DasBlog.Web.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz.Util;

namespace DasBlog.Web.Controllers
{
	[Route(".wellknown")]
	public class ActivityPubController : DasBlogBaseController
	{
		private readonly IDasBlogSettings dasBlogSettings;

		public ActivityPubController(IDasBlogSettings settings) : base(settings) 
		{
			dasBlogSettings = settings;
		}

		[Produces("text/json")]
		[HttpGet("webfinger")]
		public ActionResult WebFinger(string resource)
		{
			string usersurl = new Uri(new Uri(dasBlogSettings.SiteConfiguration.MastodonServerUrl),
						string.Format("users/{0}", dasBlogSettings.SiteConfiguration.MastodonAccount)).AbsoluteUri;

			string accturl = new Uri(new Uri(dasBlogSettings.SiteConfiguration.MastodonServerUrl),
						string.Format("@{0}", dasBlogSettings.SiteConfiguration.MastodonAccount)).AbsoluteUri;

			string authurl = new Uri(new Uri(dasBlogSettings.SiteConfiguration.MastodonServerUrl), 
						"authorize_interaction").AbsoluteUri + "?uri={uri}";

			string email = string.Format("acct:{0}", dasBlogSettings.SiteConfiguration.MastodonEmail);

			if (dasBlogSettings.SiteConfiguration.MastodonServerUrl.IsNullOrWhiteSpace() || 
				dasBlogSettings.SiteConfiguration.MastodonAccount.IsNullOrWhiteSpace())
			{
				return NoContent();
			}

			if(string.Compare(email, resource, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				return NotFound();
			}

			var results = new Root
			{
				subject = email,
				aliases = new List<string> { accturl, usersurl },

				links = new List<Link>
				{
					new Link() { rel="http://webfinger.net/rel/profile-page", type="text/html", href=accturl },
					new Link() { rel="self", type=@"application/activity+json", href=usersurl},
					new Link() { rel="http://ostatus.org/schema/1.0/subscribe", template=authurl }
				}
			};

			var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

			return Json(results, options);
		}
	}
}
