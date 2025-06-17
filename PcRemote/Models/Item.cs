using System;

namespace PcRemote.Models
{
    public class Item
    {
        public required string Id { get; set; }
        public required string Text { get; set; }
        public required string Description { get; set; }
    }
}