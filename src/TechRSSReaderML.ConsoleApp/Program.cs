using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TechRSSReader.Application;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Application.Common.Mappings;
using TechRSSReader.Infrastructure.FeedReader.Maps;
using TechRSSReaderML.ConsoleApp.Services;
using TechRSSReaderML.Model;

namespace TechRSSReaderML.ConsoleApp
{
    class Program
    {
        static AppSettings appSettings = new AppSettings();

        static async Task Main(string[] args)
        {

            IConfiguration configuration = GetConfiguration(args);
            ServiceProvider serviceProvider = ConfigureServices(configuration);
            ConfigurationBinder.Bind(configuration.GetSection("AppSettings"), appSettings);
            Microsoft.Extensions.Logging.ILogger _logger = serviceProvider.GetService<ILogger<Program>>();
            string modelDataFileName = null;
            try
            {
                IModelTrainingService modelTrainingService = serviceProvider.GetService<IModelTrainingService>();
                modelDataFileName = await modelTrainingService.CreateModelDataFileAsync();

                string newModelFile = await modelTrainingService.CreateModel(modelDataFileName);

                // ModelBuilder.CreateModel(modelDataFileName);

                _logger.LogInformation($"=============== Copying Model to the Website ===============");

                
                
                if (File.Exists(appSettings.MLModelFile))
                    File.Delete(appSettings.MLModelFile);
                File.Copy(newModelFile, appSettings.MLModelFile);

                _logger.LogInformation($"=============== Copying the Model for Prediction ===============");

                string executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string modelDirectory = Path.Combine(executablePath, "MLModels");
                if (!Directory.Exists(modelDirectory))
                    Directory.CreateDirectory(modelDirectory);
                string modelFile = Path.Combine(modelDirectory, "MLModel.zip");
                if (File.Exists(modelFile))
                    File.Delete(modelFile);
                File.Copy(newModelFile, modelFile);

                _logger.LogInformation($"=============== Updating FeedItem Predictions ===============");

                await modelTrainingService.UpdateFeedItemPredictions();
                                
            }
            finally
            {
                if (File.Exists(modelDataFileName))
                    File.Delete(modelDataFileName);
            }
           

            // Create single instance of sample data from first line of dataset for model input
            StarRatingInput sampleData = SampleData();

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            WriteSampleOutputToConsole(serviceProvider, sampleData, predictionResult);

            Console.ReadKey();
        }

        private static IConfiguration GetConfiguration(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables()
               .AddCommandLine(args);

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var isDevelopment = string.IsNullOrEmpty(environment) ||
                               environment.ToLower() == "development";

            if (isDevelopment)
                builder.AddUserSecrets<Program>();

            return builder.Build();
        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
        {
            CurrentUserService currentUserService = new CurrentUserService(configuration);

            var services = new ServiceCollection()
                .AddApplication()
                .AddInfrastructure(configuration, null)
                .AddSingleton<ICurrentUserService>(currentUserService)
                .AddAutoMapper(typeof(MappingProfile).Assembly, typeof(RssFeedItemMap).Assembly);


            //var serilogLogger = new LoggerConfiguration()
            //   .ReadFrom.Configuration(configuration)
            //   .CreateLogger();

            var serilogLogger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(serilogLogger);
            });

            return services.BuildServiceProvider();

        }

        static StarRatingInput SampleData()
        {
            return new StarRatingInput()
            {
                Id = 2052F,
                CreatedBy = @"1273228d-89b1-4290-bad6-1ab9a74de8e1",
                Created = @"9/11/2020 10:27:24 AM",
                LastModifiedBy = @"1273228d-89b1-4290-bad6-1ab9a74de8e1",
                LastModified = @"9/17/2020 2:24:25 PM",
                Author = @"",
                BlogId = 1F,
                Categories = @"Gaming",
                Content = "<div><p><img title=\"image\" style=\"float: right; margin: 0px 0px 0px 5px; display: inline\" alt=\"image\" src=\"https://www.hanselman.com/blog/content/binary/Windows-Live-Writer/How-to-use-a-Raspberry-Pi-4-as-a-Minecra_CC25/image_0c566bac-0936-4c3e-b885-929ec037d25c.png\" width=\"450\" align=\"right\" height=\"246\">My 14 year old got tired of paying $7.99 for Minecraft Realm so he could host his friends in their world. He was just hosting on his laptop and then forwarding a port but that means his friends can't connect unless he's actively running. I was running a Minecraft Server in a Docker container on my <a href=\"http://feeds.hanselman.com/~/t/0/0/scotthanselman/~https://www.hanselman.com/blog/SynologyDS1520IsTheSweetSpotForAHomeNASAndAPrivateCloud.aspx\">Synology NAS</a> but I thought teaching him how to run Minecraft Server on a Raspberry Pi 4 we had lying around would be a good learning moment.</p> <p>First, set up your Raspberry Pi. I like <a href=\"http://feeds.hanselman.com/~/t/0/0/scotthanselman/~https://www.raspberrypi.org/downloads/noobs/\">NOOBS</a> as it's super easy to setup. If you want to make things faster for setup and possibly set up your Pi without having to connect a monitor, mouse, or keyboard, mount your SSD card and create a new empty file named ssh, without any extension, inside the boot directory to enable ssh on boot. Remember the default user name is pi and the password is raspberry.</p> <p>SSH over to your Raspberry Pi. You can use Putty, but I like using <a href=\"http://feeds.hanselman.com/~/t/0/0/scotthanselman/~https://www.hanselman.com/blog/HowToUseWindows10sBuiltinOpenSSHToAutomaticallySSHIntoARemoteLinuxMachine.aspx\">Windows 10's built-in SSH</a>. Do your standard update stuff, and also install a JDK:</p><pre>sudo apt update<br>sudo apt upgrade<br>sudo apt install default-jdk</pre><p>There are other Minecraft 3rd party Java Servers you can use, the most popular being Spigot, but the easiest server you can run is the one from Minecraft themselves.</p><p>Go to <a title=\"https://www.minecraft.net/en-us/download/server\" href=\"http://feeds.hanselman.com/~/t/0/0/scotthanselman/~https://www.minecraft.net/en-us/download/server\">https://www.minecraft.net/en-us/download/server</a> in a browser. It'll say something like \"Download <a href=\"http://feeds.hanselman.com/~/t/0/0/scotthanselman/~https://launcher.mojang.com/v1/objects/c5f6fb23c3876461d46ec380421e42b289789530/server.jar\">minecraft_server.1.16.2.jar</a> and run it with the following command.\" That version number and URL will change in the future. Right-click and copy link into your clipboard We are going to PASTE it (right click with your mouse) after the \"wget\" below. So we'll make a folder, download the server.jar, then run it.</p><pre>cd ~<br>mkdir MinecraftServer<br>cd MinecraftServer<br>wget https://launcher.mojang.com/v1/objects/c5f6fb23c3876461d46ec380421e42b289789530/server.jar<br>java -Xmx2500M -Xms2500M -jar server.jar nogui</pre><p>You'll get a warning that you didn't accept the EULA, so now open \"pico eula.txt\" and set eula=true, then hit Ctrl-X and Yes to save the new file. Press the up key and run your command again.</p><pre>java -Xmx2500M -Xms2500M -jar server.jar nogui</pre><p>You could also make a start.sh text file with pico then chmod +x to make it an easier single command way to start your server. Since I have a Raspberry Pi 4 with 4g gigs of RAM and it'll be doing just this one server, I felt 2500 megs of RAM was a sweet spot. Java ran out of memory at 3 gigs.</p><p>You can then run ifconfig at and command line and get your Pi's IP address, or type hostname to get its name. Then you can connect to your world with that name or ip.</p><figure><img title=\"Running Minecraft Servers\" style=\"display: inline\" alt=\"Running Minecraft Servers\" src=\"https://www.hanselman.com/blog/content/binary/Windows-Live-Writer/How-to-use-a-Raspberry-Pi-4-as-a-Minecra_CC25/image_68d26916-6145-42ee-9bf7-4534ec5228ca.png\" width=\"856\" height=\"526\"></figure><h3>Performance Issues with Complex Worlds</h3><p>With very large Minecraft worlds or worlds like my son's with 500+ Iron Golems and Chickens, you may get an error like </p><pre>[Server Watchdog/FATAL]: A single server tick took 60.00 seconds (should be max 0.05)</pre><p>You can workaround this in a few ways. You can gently overclock your Pi4 if it has a fan by adding this to the end of your /boot/config.txt (read articles on overclocking a Pi to be safe)</p><pre>over_voltage=3<br>arm_freq=1850</pre><p>And/or you can disable the Minecraft internal watchdog for ticks by setting max-tick-time to -1 in your server's server.properties file. </p><p>We solved our issue by killing about 480+ Iron Golems with </p><pre>/kill @e[type=minecraft:iron_golem]</pre><p>but that's up to you. Just be aware that the Pi is fast but not thousands of moving entities in Minecraft fast. For us this works great though and is teaching my kids about the command line, editing text files, and ssh'ing into things. </p><hr><p><strong>Sponsor: </strong>Never miss a beat with Seq. Live application logs and health checks. <a href=\"http://feeds.hanselman.com/~/t/0/0/scotthanselman/~https://hnsl.mn/35dKwSn\">Download the Windows installer or pull the Docker image now</a>. </p><br/><hr/>© 2020 Scott Hanselman. All rights reserved. <br/></div><Img align=\"left\" border=\"0\" height=\"1\" width=\"1\" alt=\"\" style=\"border:0;float:left;margin:0;padding:0;width:1px!important;height:1px!important;\" hspace=\"0\" src=\"http://feeds.hanselman.com/~/i/635204885/0/scotthanselman\"><div style=\"clear:both;padding-top:0.2em;\"><a title=\"Like on Facebook\" href=\"http://feeds.hanselman.com/_/28/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/fblike20.png\" style=\"border:0;margin:0;padding:0;\"></a> <a title=\"Share on Google+\" href=\"http://feeds.hanselman.com/_/30/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/googleplus20.png\" style=\"border:0;margin:0;padding:0;\"></a> <a title=\"Tweet This\" href=\"http://feeds.hanselman.com/_/24/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/twitter20.png\" style=\"border:0;margin:0;padding:0;\"></a> <a title=\"Subscribe by email\" href=\"http://feeds.hanselman.com/_/19/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/email20.png\" style=\"border:0;margin:0;padding:0;\"></a> <a title=\"Subscribe by RSS\" href=\"http://feeds.hanselman.com/_/20/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/rss20.png\" style=\"border:0;margin:0;padding:0;\"></a> </div>",
                Description = "<div><p><img title=\"image\" style=\"float: right; margin: 0px 0px 0px 5px; display: inline\" alt=\"image\" src=\"https://www.hanselman.com/blog/content/binary/Windows-Live-Writer/How-to-use-a-Raspberry-Pi-4-as-a-Minecra_CC25/image_0c566bac-0936-4c3e-b885-929ec037d25c.png\" width=\"450\" align=\"right\" height=\"246\">My 14 year old got tired of paying $7.99 for Minecraft Realm so he could host his friends in their world. He was just hosting on his laptop and then forwarding a port but that means his friends can't connect unless he's actively running. I was running a Minecraft Server in a Docker container on my <a href=\"https://www.hanselman.com/blog/SynologyDS1520IsTheSweetSpotForAHomeNASAndAPrivateCloud.aspx\">Synology NAS</a> but I thought teaching him how to run Minecraft Server on a Raspberry Pi 4 we had lying around would be a good learning moment.</p> <p>First, set up your Raspberry Pi. I like <a href=\"https://www.raspberrypi.org/downloads/noobs/\">NOOBS</a> as it's super easy to setup. If you want to make things faster for setup and possibly set up your Pi without having to connect a monitor, mouse, or keyboard, mount your SSD card and create a new empty file named ssh, without any extension, inside the boot directory to enable ssh on boot. Remember the default user name is pi and the password is raspberry.</p> <p>SSH over to your Raspberry Pi. You can use Putty, but I like using <a href=\"https://www.hanselman.com/blog/HowToUseWindows10sBuiltinOpenSSHToAutomaticallySSHIntoARemoteLinuxMachine.aspx\">Windows 10's built-in SSH</a>. Do your standard update stuff, and also install a JDK:</p><pre>sudo apt update<br>sudo apt upgrade<br>sudo apt install default-jdk</pre><p>There are other Minecraft 3rd party Java Servers you can use, the most popular being Spigot, but the easiest server you can run is the one from Minecraft themselves.</p><p>Go to <a title=\"https://www.minecraft.net/en-us/download/server\" href=\"https://www.minecraft.net/en-us/download/server\">https://www.minecraft.net/en-us/download/server</a> in a browser. It'll say something like \"Download <a href=\"https://launcher.mojang.com/v1/objects/c5f6fb23c3876461d46ec380421e42b289789530/server.jar\">minecraft_server.1.16.2.jar</a> and run it with the following command.\" That version number and URL will change in the future. Right-click and copy link into your clipboard We are going to PASTE it (right click with your mouse) after the \"wget\" below. So we'll make a folder, download the server.jar, then run it.</p><pre>cd ~<br>mkdir MinecraftServer<br>cd MinecraftServer<br>wget https://launcher.mojang.com/v1/objects/c5f6fb23c3876461d46ec380421e42b289789530/server.jar<br>java -Xmx2500M -Xms2500M -jar server.jar nogui</pre><p>You'll get a warning that you didn't accept the EULA, so now open \"pico eula.txt\" and set eula=true, then hit Ctrl-X and Yes to save the new file. Press the up key and run your command again.</p><pre>java -Xmx2500M -Xms2500M -jar server.jar nogui</pre><p>You could also make a start.sh text file with pico then chmod +x to make it an easier single command way to start your server. Since I have a Raspberry Pi 4 with 4g gigs of RAM and it'll be doing just this one server, I felt 2500 megs of RAM was a sweet spot. Java ran out of memory at 3 gigs.</p><p>You can then run ifconfig at and command line and get your Pi's IP address, or type hostname to get its name. Then you can connect to your world with that name or ip.</p><figure><img title=\"Running Minecraft Servers\" style=\"display: inline\" alt=\"Running Minecraft Servers\" src=\"https://www.hanselman.com/blog/content/binary/Windows-Live-Writer/How-to-use-a-Raspberry-Pi-4-as-a-Minecra_CC25/image_68d26916-6145-42ee-9bf7-4534ec5228ca.png\" width=\"856\" height=\"526\"></figure><h3>Performance Issues with Complex Worlds</h3><p>With very large Minecraft worlds or worlds like my son's with 500+ Iron Golems and Chickens, you may get an error like </p><pre>[Server Watchdog/FATAL]: A single server tick took 60.00 seconds (should be max 0.05)</pre><p>You can workaround this in a few ways. You can gently overclock your Pi4 if it has a fan by adding this to the end of your /boot/config.txt (read articles on overclocking a Pi to be safe)</p><pre>over_voltage=3<br>arm_freq=1850</pre><p>And/or you can disable the Minecraft internal watchdog for ticks by setting max-tick-time to -1 in your server's server.properties file. </p><p>We solved our issue by killing about 480+ Iron Golems with </p><pre>/kill @e[type=minecraft:iron_golem]</pre><p>but that's up to you. Just be aware that the Pi is fast but not thousands of moving entities in Minecraft fast. For us this works great though and is teaching my kids about the command line, editing text files, and ssh'ing into things. </p><hr><p><strong>Sponsor: </strong>Never miss a beat with Seq. Live application logs and health checks. <a href=\"https://hnsl.mn/35dKwSn\">Download the Windows installer or pull the Docker image now</a>. </p><br/><hr/>© 2020 Scott Hanselman. All rights reserved. <br/></div><div style=\"clear:both;padding-top:0.2em;\"><a title=\"Like on Facebook\" href=\"http://feeds.hanselman.com/_/28/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/fblike20.png\" style=\"border:0;margin:0;padding:0;\"></a>&#160;<a title=\"Share on Google+\" href=\"http://feeds.hanselman.com/_/30/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/googleplus20.png\" style=\"border:0;margin:0;padding:0;\"></a>&#160;<a title=\"Tweet This\" href=\"http://feeds.hanselman.com/_/24/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/twitter20.png\" style=\"border:0;margin:0;padding:0;\"></a>&#160;<a title=\"Subscribe by email\" href=\"http://feeds.hanselman.com/_/19/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/email20.png\" style=\"border:0;margin:0;padding:0;\"></a>&#160;<a title=\"Subscribe by RSS\" href=\"http://feeds.hanselman.com/_/20/635204885/scotthanselman\"><img height=\"20\" src=\"https://assets.feedblitz.com/i/rss20.png\" style=\"border:0;margin:0;padding:0;\"></a>&#160;</div>",
                Link = @"http://feeds.hanselman.com/~/635204885/0/scotthanselman~How-to-use-a-Raspberry-Pi-as-a-Minecraft-Java-Server.aspx",
                PublishingDate = @"9/7/2020 11:36:30 PM",
                PublishingDateString = @"Mon, 07 Sep 2020 23:36:30 GMT",
                RetrievedDateTime = @"9/11/2020 10:27:24 AM",
                RssId = @"https://www.hanselman.com/blog/PermaLink.aspx?guid=bada76c4-2401-4dc8-876f-ca6011cc5891",
                Title = @"How to use a Raspberry Pi 4 as a Minecraft Java Server",
                UserInterested = @"",
                UserInterestPrediction = 0F,
                ReadAlready = true,
                UserRatingPrediction = 0F,
            };
        }

        static void WriteSampleOutputToConsole(ServiceProvider serviceProvider, StarRatingInput sampleData, StarRatingOutput predictionResult)
        {

            Microsoft.Extensions.Logging.ILogger _logger = serviceProvider.GetService<ILogger<Program>>();


            _logger.LogInformation("Using model to make single prediction -- Comparing actual UserRating with predicted UserRating from sample data...");
            //Console.WriteLine($"Id: {sampleData.Id}");
            //Console.WriteLine($"CreatedBy: {sampleData.CreatedBy}");
            //Console.WriteLine($"Created: {sampleData.Created}");
            //Console.WriteLine($"LastModifiedBy: {sampleData.LastModifiedBy}");
            //Console.WriteLine($"LastModified: {sampleData.LastModified}");
            //Console.WriteLine($"Author: {sampleData.Author}");
            //Console.WriteLine($"BlogId: {sampleData.BlogId}");
            //Console.WriteLine($"Categories: {sampleData.Categories}");
            //Console.WriteLine($"Content: {sampleData.Content}");
            //Console.WriteLine($"Description: {sampleData.Description}");
            //Console.WriteLine($"Link: {sampleData.Link}");
            //Console.WriteLine($"PublishingDate: {sampleData.PublishingDate}");
            //Console.WriteLine($"PublishingDateString: {sampleData.PublishingDateString}");
            //Console.WriteLine($"RetrievedDateTime: {sampleData.RetrievedDateTime}");
            //Console.WriteLine($"RssId: {sampleData.RssId}");
            //Console.WriteLine($"Title: {sampleData.Title}");
            _logger.LogInformation($"UserInterested: {sampleData.UserInterested}");
            //Console.WriteLine($"UserInterestPrediction: {sampleData.UserInterestPrediction}");
            //Console.WriteLine($"ReadAlready: {sampleData.ReadAlready}");
            //Console.WriteLine($"UserRatingPrediction: {sampleData.UserRatingPrediction}");
            _logger.LogInformation($"Predicted UserRating: {predictionResult.Score}");
            _logger.LogInformation("=============== End of process, hit any key to finish ===============");
        }
    }
}
