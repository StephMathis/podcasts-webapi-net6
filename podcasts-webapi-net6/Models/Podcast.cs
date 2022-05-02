using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
//using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.WebUtilities;
//using System.ServiceModel.Syndication;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Xml;
using System.IO;
//using Microsoft.SyndicationFeed.Rss;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PodcastsWebapi.Models

{
    public class Podcast
    {
        public ObjectId Id { get; set; }
        public string PodcastId { get; set ;}
        public Uri PodcastUrl { get; set; }
        public string Title {get; set;}
        public string Description { get; set; }
        public List<Episode> Episodes {get; set; }
        public Uri CoverUrl {get; set;}
        public string Link {get; set;}

        public Podcast(string podcastId) {
            this.PodcastId = podcastId;
            this.Episodes = new List<Episode>();
        }

        public static string GetIdFromUrl(string url) {
            return WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(url));
        }
        public static string ? GetUrlFromId(string id) {
            byte[]  bytes_url = WebEncoders.Base64UrlDecode(id);
            string url = System.Text.Encoding.UTF8.GetString(bytes_url, 0, bytes_url.Length);
            return url;
        }

        public static async System.Threading.Tasks.Task<Podcast> parsePodcastAsync(string podcastId, Stream xmlStream) {
            Podcast podcast = new Podcast(podcastId);
            string summary = "", subtitle = "", description = "", desc = "";
            using (var xmlReader = XmlReader.Create(xmlStream, new XmlReaderSettings() { Async = true }))
            {
                var feedReader = new RssFeedReader(xmlReader);

                while(await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        // Read category
                        case SyndicationElementType.Category:
                            ISyndicationCategory category = await feedReader.ReadCategory();
                            desc += " ||| category = [" + category.ToJson().ToString() + "]";
                            break;

                        // Read Image
                        case SyndicationElementType.Image:
                            ISyndicationImage image = await feedReader.ReadImage();
                            podcast.CoverUrl = image.Url;
                            desc += " ||| image = [" + image.ToJson().ToString() + "]";
                            break;

                        // Read Item
                        case SyndicationElementType.Item:
                            try {
                                ISyndicationItem item = await feedReader.ReadItem();
                                long? size = null;
                                long? duration = null;
                                Uri ? uri = null;
                                foreach (SyndicationLink detail in item.Links) {
                                    if (detail.RelationshipType == "enclosure") {
                                        size = detail.Length;
                                        uri = detail.Uri;
                                    }
                                }
                                string pattern = @"^.*durée\s*:\s*(\d\d):(\d\d):(\d\d)\s+.*$";
                                if (!string.IsNullOrEmpty(item.Description)) {
                                    Match m = Regex.Match(item.Description, pattern, RegexOptions.IgnoreCase);
                                    if (m.Success) {
                                        duration = long.Parse(m.Groups[1].Value)*3600;
                                        duration += long.Parse(m.Groups[2].Value)*60;
                                        duration += long.Parse(m.Groups[2].Value);
                                    }
                                }
                                if (uri == null) {
                                    uri = new Uri(item.Id);
                                }

                                Episode episode = new Episode() {
                                    EpisodeId = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(uri.ToString())),
                                    Published = (long)(item.Published.DateTime - new DateTime(1970, 1, 1)).TotalSeconds,
                                    Size = size,
                                    SourceUrl = uri,
                                    Title = item.Title,
                                    Description = item.Description,
                                    Duration = duration
                                };
                                podcast.Episodes.Add(episode);
                            } catch {
                                Console.Write("An episode was not parsed on podcast "+podcastId);
                            }
                            break;

                        // Read link
                        case SyndicationElementType.Link:
                            ISyndicationLink link = await feedReader.ReadLink();
                            desc += " ||| link = [" + link.ToJson().ToString() + "]";
                            break;

                        // Read Person
                        case SyndicationElementType.Person:
                            ISyndicationPerson person = await feedReader.ReadPerson();
                            desc += " ||| person = [" + person.ToJson().ToString() + "]";
                            break;

                        // Read content
                        default:
                            ISyndicationContent content = await feedReader.ReadContent();
                            if (content.Name == "title") {
                                podcast.Title = content.Value;
                            }
                            if (content.Name == "subtitle") {
                                subtitle = content.Value;
                            }
                            if (content.Name == "summary" && content.Value != null) {
                                summary = content.Value;
                            }
                            if (content.Name == "description" && content.Value != null) {
                                description = content.Value;
                            }
                            desc += " ||| content = [" + content.ToJson().ToString() + "]";
                            break;
                    }
                }
            }
            
            podcast.Description = summary;
            return podcast;
        }

    }


}

/*
 {
            "channel_id": "QW5pbWF1eCBkb21lc3RpcXVlcw==",
            "comment": "Ka",
            "podcasts": [
                "aHR0cDovL3JhZGlvZnJhbmNlLXBvZGNhc3QubmV0L3BvZGNhc3QwOS9yc3NfMTM5NTUueG1s",
                "aHR0cDovL3JhZGlvZnJhbmNlLXBvZGNhc3QubmV0L3BvZGNhc3QwOS9yc3NfMTIzNjAueG1s",
                "aHR0cDovL3JhZGlvZnJhbmNlLXBvZGNhc3QubmV0L3BvZGNhc3QwOS9yc3NfMTYyNzQueG1s",
                "aHR0cDovL2NkbjMtZXVyb3BlMS5uZXcyLmxhZG1lZGlhLmZyL3Zhci9leHBvcnRzL3BvZGNhc3RzL3NvdW5kL0F1eC1vcmlnaW5lcy1GcmFuY2stRmVycmFuZC54bWw=",
                "aHR0cDovL2NkbjEtZXVyb3BlMS5uZXcyLmxhZG1lZGlhLmZyL3Zhci9leHBvcnRzL3BvZGNhc3RzL3NvdW5kL2F1LWNvZXVyLWRlLWwtaGlzdG9pcmUueG1s"
            ],
            "thumbnail_url": "http://img.src.ca/2015/01/14/635x357/150114_qy6sr_mlarge_chat_minou_sn635.jpg",
            "title": "Animaux domestiques"
        }
 */