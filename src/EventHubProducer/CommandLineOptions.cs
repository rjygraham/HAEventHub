using CommandLine;

namespace EventHubProducer
{
	internal class CommandLineOptions
	{
		[Option('c', "connectionString", Required = true)]
		public string ConnectionString { get; set; } = "<Replace me to hardcode a default value>";

		[Option('s', "sensorsFile", Required = true)]
		public string SensorsFile { get; set; } = "<Replace me to hardcode a default value>";
	}
}
