# SparklES
SparklES is a windows service that allows you to capture information from the [Spark Cloud](http://docs.spark.io/api/) and store it in [ElasticSearch](http://www.elasticsearch.com/).

## Deployment
To deploy the service, simply:
1. Use [Octopus](https://octopusdeploy.com/) to deploy the [nuget package](https://www.myget.org/feed/onecog/package/nuget/OneCog.Spark.Sparkles) [![onecog MyGet Build Status](https://www.myget.org/BuildSource/Badge/onecog?identifier=cd724a82-8cf8-40c9-9cc7-2892c26f50be)](https://www.myget.org/)
2. Download the service (coming soon) and deploy manually (see below)
3. Build [the source](https://github.com/ibebbs/OneCog.Spark.Sparkles) and deploy manually

## Configuration
Before starting the service it needs to be configured to use your access token, spark cores, variables and ElasticSearch instance. To do this, open [OneCog.Spark.Sparkles.exe.config](https://github.com/ibebbs/OneCog.Spark.Sparkles/blob/master/src/OneCog.Spark.Sparkles/App.config) and change the following:

1. #{ElasticSearchHost} - Set this to the address of your ElasticSearch instance; for example 'http://localhost:9200'
2. #{SparkAccessToken} - Set this to the access token for your Spark account. Instructions for finding your access token can be found in the 'Authentication' section [here](http://docs.spark.io/api/).
3. Modify the 'devices' element to contain a 'device' element for each SparkCore you want to capture information from ensuring that the 'id' element is set to the devices unique id (which, like the access token, can be found in the ['Spark Build'](https://www.spark.io/build/) web development portal).
4. Add 'variable' element to the 'variables' element for each variable you would like to capture information from ensuring that the name of the variable matches the name of the variable being published by your spark core; for example, if you have code like this in your core 'Spark.variable("analogvalue", &analogvalue, INT);' then you should name the variable 'analogvalue'.

Other configuration values are detailed below.

## Installation & Running SparklES
Once you have deployed and configured SparklES, you can run it directly simply by double clicking the executable. This will start a command session wherein the executable will report its running status.

If you'd prefer to install SparklES as a service, this can be done by executing the following command at the command prompt: '''OneCog.Spark.Sparkles.exe install --autostart''' (for further information on command line options for configuring SparklES to run as a service see [here](http://topshelf.readthedocs.org/en/latest/overview/commandline.html)). This should install the service so that you can run it via the service applet and will automatically restart the service after a reboot.
