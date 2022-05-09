using Microsoft.AspNetCore.WebUtilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace PodcastsWebapi.Models
{
    public class Channel
    {
        //[BsonId]
        public ObjectId Id { get; set; }
        public string ChannelId { get; set ;}
        public string Title { get; set; }
        public string Comment { get; set; }
        public List<string> Podcasts {get; set; }
        public string ThumbnailUrl {get; set;}

        public Channel() {
            this.Podcasts = new List<string>();
        }

        public static Channel Hydrate(Channel channel) {
            Channel hydrated = channel;
            if (hydrated.Podcasts == null) {
                hydrated.Podcasts = new List<string>();
            }
            if (hydrated.ChannelId == null)
            {
                hydrated.ChannelId = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(hydrated.Comment));
            }
            return hydrated;
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