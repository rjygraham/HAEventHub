using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace EventHubProducer.Models
{
	public class SampleEvent
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("timestamp")]
		public DateTime Timestamp { get; set; }

		[JsonPropertyName("measurement")]
		public int Measurement { get; set; }
	}
}
