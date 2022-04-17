using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PodcastsWebapi.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using MongoDB.Driver;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;

namespace PodcastsWebapi.Controllers
{
    [Route("api/podcasts")]
    public class PodcastController : Controller
    {
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> GetByIdAsync(string id)
        { 
            byte[]  bytes_url = WebEncoders.Base64UrlDecode(id);
            string url = System.Text.Encoding.UTF8.GetString(bytes_url, 0, bytes_url.Length);
            Uri rssFeedUri = new Uri(url);
            HttpClient httpClient = new HttpClient();
            Stream xmlStream = httpClient.GetStreamAsync(rssFeedUri).Result;
            Podcast podcast = await Podcast.parsePodcastAsync(id, xmlStream);
            podcast.PodcastUrl = rssFeedUri;
            return new ObjectResult(podcast);
        }
           
    }


}