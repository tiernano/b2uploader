# b2uploader

## What is it?

B2Uploader is a Console app (tested on Windows, may work with Mono or .NET Core on Linux and Mac... need to do some testing) which allows you to upload a folder to [Backblaze B2](https://www.backblaze.com/b2/cloud-storage.html). 

##How do i use it?

Get the source and build in Visual Studio (2013 and 2015 should work). using cmd line, go to the build folder and run a command as follows:

b2uploader --i accountid --a appkey --d directory_to_upload

where accountid and appkey are gotten from BackBlaze's site, and the directory is the folder you want uploaded

##Known issues

* very little logging or details of what is going on
* no error handing (currently crashes with VERY large files and multithreading).
* probably a lot of other stuff... leave issues please!


##What have i used it for?

The app uses multiple threads to do uploads. I am running on a machine in the house with 2 8 core Xeons and 64Gb ram. With this, it kicks of something like 64 threads (the test folder only had 48 files, so all started uploading immediatly!). 
