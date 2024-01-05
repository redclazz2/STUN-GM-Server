namespace STUN
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("Made By: Redclazz2. Based on GoodPie's work on Python Server.\n" +
				"Special Thanks to: FatalSheep for BufferStream Class and CinderFire for basic server architecture.\n");

			Console.WriteLine("Starting Server...");
			Server server = new Server();
			server.StartServer(8056);
			Console.WriteLine("Server Started!");
		}
	}
}
