using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PodcastsWebapi.Models;
using System.Linq;
using MongoDB.Driver;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using Microsoft.IdentityModel.Tokens;

namespace PodcastsWebapi.Controllers
{
    [Route("api/channels")]
    public class ChannelController : Controller
    {
        private Channel GetChannelById(string id) {
            MongoDBContext dbContext = new MongoDBContext();

            var item = dbContext.Channels.Find(t => t.ChannelId == id);
            if (item == null || item.Count() == 0)
            {
                return null;
            }
            if (item.Count() > 1) {
                return null; // TODO: should be "server error" but how to do that ?
            }
            Channel channel = Channel.Hydrate(item.First());

            return channel;
        }
        
        [HttpGet]
        public IEnumerable<Channel> GetAll()
        {
            //return _context.Channels.ToList();
            MongoDBContext dbContext = new MongoDBContext();

            return dbContext.Channels.Find(m => true).ToList().Select(Channel.Hydrate);
        }

        [HttpGet(template: "{id}", Name = "GetByIdRoute")]
        public IActionResult GetById(string id)
        { 
            Channel channel = GetChannelById(id);
            if (channel == null) {
                return NotFound();
            }
            return new ObjectResult(channel);
        }
        
        [HttpPost]
        public IActionResult Add([FromBody] Channel channel)
        {
            MongoDBContext dbContext = new MongoDBContext();
            channel.ChannelId =  WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(channel.Title));

            dbContext.Channels.InsertOne(channel);

            return CreatedAtRoute("GetByIdRoute", new { id = channel.ChannelId }, channel);
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(string id, [FromBody] Channel channelPatch) {
            Channel channel = GetChannelById(id);
            foreach(string podcastUrlB64 in channelPatch.Podcasts) {
                string podcastId = podcastUrlB64;
                if (podcastUrlB64.StartsWith("http")) {
                    // in the ws in which we add a new podcast
                    podcastId = Podcast.GetIdFromUrl(podcastUrlB64);
                } 
                channel.Podcasts.Add(podcastId);
            }
            channel.Podcasts = channel.Podcasts.Distinct().ToList();
            // check for a previous bug
            List<string> filteredPodcastsId = new List<string>();
            foreach(string podcastUrlB64 in channel.Podcasts) {
                if (Podcast.GetUrlFromId(podcastUrlB64).StartsWith("http")) {
                    filteredPodcastsId.Add(podcastUrlB64);
                }
            }
            channel.Podcasts = filteredPodcastsId;
            MongoDBContext dbContext = new MongoDBContext();
            dbContext.Channels.UpdateOne(
                Builders<Channel>.Filter.Eq(c => c.ChannelId, id),
                Builders<Channel>.Update.Set(c => c.Podcasts, channel.Podcasts));
            if (channelPatch.ThumbnailUrl != null) {
                dbContext.Channels.UpdateOne(
                    Builders<Channel>.Filter.Eq(c => c.ChannelId, id),
                    Builders<Channel>.Update.Set(c => c.ThumbnailUrl, channelPatch.ThumbnailUrl));
            }
            if (channelPatch.Comment != null) {
                dbContext.Channels.UpdateOne(
                    Builders<Channel>.Filter.Eq(c => c.ChannelId, id),
                    Builders<Channel>.Update.Set(c => c.Comment, channelPatch.Comment));
            }
            return new ObjectResult(GetChannelById(id));
        }

        [HttpDelete("{channelId}/podcasts/{podcastId}")]
        public IActionResult DeletePodcast(string channelId, string podcastId) {
            Channel channel = GetChannelById(channelId);
            channel.Podcasts.Remove(podcastId);
            MongoDBContext dbContext = new MongoDBContext();
            dbContext.Channels.UpdateOne(
                // request by id ".Eq(c => c.Id, channel.Id)" does not work on linux
                Builders<Channel>.Filter.Eq(c => c.ChannelId, channelId),
                Builders<Channel>.Update.Set(c => c.Podcasts, channel.Podcasts));
            return new ObjectResult(GetChannelById(channelId));
        }   
    }
}