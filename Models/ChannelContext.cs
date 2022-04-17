using Microsoft.EntityFrameworkCore;

namespace PodcastsWebapi.Models
{
    public class ChannelContext : DbContext
    {
        public ChannelContext(DbContextOptions<ChannelContext> options)
            : base(options)
        {
        }

        public DbSet<Channel> Channels { get; set; }

    }
}