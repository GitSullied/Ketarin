# Project #
Ketarin is a package manager for Windows operating systems... because,
apparently they haven't got their act together yet.  They must be too
busy couting thier money or maybe designing a propritary protocol to
replace an open standard that already exists; except they won't (they
couldn't possibly) do the peer-review, QA, and hardening that an open
standard naturally gets.  If you let them do that and then "sell" you
security products too... congratulations, you just got raped by M$.

I digress.  Ketarin is awesome.  You can find the [original project
here](http://ketarin.org) and [their forum here](http://ketarin.org/forum).
This is a fork of their software w/ no official connections to the
original developers at the moment, but I hope to faithfully implement
their intentions where I make modifications to the source.

Beyond moving away from sharing bundled archives as a means to distribute
the source code I had some snags that I think I could remove and have
the process of building from source trivial for the next Ketarin fan.




## Tips and Tutorials ##
* [Documentation & Wiki](http://wiki.ketarin.org)
* [Ketarin Tutorial - Italian by MegaLab.it](http://www.megalab.it/4144)
* [Ketarin Tutorial - English by deranjer](http://aproductivelife.blogspot.com/2010/03/using-ketarin-to-automatically-update.html)
* [Tips and Tutorials on the Forum](http://ketarin.org/forum/forum/6-tips-and-tutorials/)

## Templates, App Depots, and Scripting ##
* [Templates on the Forum](http://ketarin.org/forum/forum/7-templates/)
* [PAD File Format](http://ketarin.org/padfile)
* [Batch & C# Scripting](http://wiki.ketarin.org/index.php/Commands)



# Background #
Original source downloaded from Ketarin [website](http://ketarin.org) via 
the author provided link in the second paragraph under the section entitled
"What is it all about?"
  [download source](http://cdburnerxp.se/downloads/sourcecode/Ketarin.tar.gz)

Pre-compiled binaries can also be found on the site.  As of 2012-08-05
the website reports the current version of Ketarin as 1.6.0.434; this does
indeed match the pre-compiled binaries.



# 2012-08-06 #
After decompressing the source files into a folder of their own under my
development directory I opened the ketarin.proj file with VS2008 and as
an initial smoke test ran the commands (from the VS2008 menu bar)
    "Build->Clean Solution" 
followed by
    "Build->Build Solution" (or equivently F7)

Immediatly there were build errors due to a reference to source code
that was not included w/ the Ketarin sources; the source code for
CDBurnerXP, which is indeed referenced by the Ketarin project as being
parallel to the Ketarin parent source directory.
    i.e. ".\..\CDBurnerXP\trunk\[...]" 
        Note: the ".." double-period signifies to go back one directory from
        the current directory "." signified by the single-period, and my use 
        of the ellipses (the triple-period) is to show that the actuall
        content goes on, but I trunkated it here.

I didn't notice until now that the download URL for the Ketarin source files
is actually under the CDBurnerXP dowmain; furthermore the Ketarin website
makes the comment up front that it, Ketarin, was merely created as a supporting
component to the priemier application CDBurnerXP.  Ketarin, as it turns out,
has grown it's own loyal follers interested soley in the functionality offered
by Ketarin all by itself.

I posted in the Ketarin formus asking about the development, maintenance, and
future hosting/distribution plans for Ketarin.  The post was titled "Compiling
Ketarin from Source" posted around 14:04 GMT-6 in the General Discussion
section of the [forum](http://ketarin.org/forum)

Let's check on that forum posting...




## Ketarin Server Responding with HTTP 500 ##
No Bueno.  I went to check up on my post from earlier today.  I can see it
cited as a recent topic, but after clicking on the link to view the topic the
Ketarin Forum crashes with a 500 HTTP response code.

<pre>
    The requested URL "[...]" was not found on this server.

    Additionally, a 500 Internal Server Error error was encountered while trying to use an ErrorDocument to handle the request.
</pre>

Maybe they were updating the site, because things appear to be in better shape
now.  Actually, it's up and down.  Hmmm.... well, anyway.  My forum post can 
be seen here: [Post: Compiling Ketarin from source](https://ketarin.org/forum/topic/852-compiling-ketarin-from-source/page__pid__6282#entry6282)

No responses yet; `tis no matter, I proceed on my own.

## Is the Server Supposed to Tell me That? ##
There is something going on with both the ketarin.org and CDBurnerXP.se
domains; they both worked earlier today, but just can't pull it together
at the moment.  So, I did some poking around to see what cached files or
other resources I might be able to turn up in the meantime.

I don't think that this page is meant for "us" the site visitors.  I think it
is for the administrator, but maybe not.  At a glance the exposed API _might_
be intentionally provided, but if not I would say this might be an avenue that
has given the server a case of the HTTP 500's.  See the exposed [functionality
here](https://ketarin.org/rpc.php).

### Available XMLRPC methods for this server ###
* ketarin.GetMostDownloadedApplications()
* ketarin.GetApplications()
* ketarin.GetSimilarApplications()
* ketarin.GetUpdatedApplications()
* ketarin.GetApplication()
* ketarin.SaveApplication()

### Details ####
(array) ketarin.GetMostDownloadedApplications()
    Returns a list of the most often downloaded applications.  The number of
    apps is currently 50.

(array) ketarin.GetApplications([(string) searchSubject])
    Gets all applications matching a search criteria (application name or GUID)

(array) ketarin.GetSimilarApplications((string) searchSubject, (string) appGuid)
    Gets all applications matching a search criteria (application name) except
    for the application given in the second parameter.
    - searchSubject:
    - appGuid: the application to ignore

(array) ketarin.GetUpdatedApplications((array) applications)
    Gets the XML of all applications that have been apssed as argument and have
    been updated (according to the updatedAt information of the arguments).

(array) ketarin.GetApplication((integer) shareId)
    Returns the data of a particular application by its server ID.

(integer) ketarin.SaveApplication((string) XML, (string) authorGuid)
    Adds an application to the database or updates it.
    - xml: Serialized application as XML
    - authorGuid: UID of the author.  When updating applications, it must be 
        identical to the one used when adding the application.


Seems mostly harmless except for the last call that writes to a DB.  That
and the fact that php may be the worst thing to ever happen to webapp/server
security.  Well, I'll come back to this when I have more time.


## Cached Pages or the Wayback Machine ##
Google doesn't appear to have any cached versions of Ketarin's website; that
would probably be due to the server requesting NOT to be cahced.  Hmmm....

Well, the internet archive does have some snapshots of the site and it appears
to be roughly what I was looking at earlier today.  The declared application
version is still decalred to be the same and that page is almost 2 yr. old.

[Archived Ketarin Website](http://web.archive.org/web/20110725022544/http://ketarin.org/)
 


# To Build or Not to Build #
I am really only interested in Ketarin, but it might be easier to simply 
add the missing source code for the CDBurnerXP application as opposed to 
sugically removing the references to that external project.  

Hmmm... well, maybe the CDBurnerXP source is not available.  I thought that
it might be, but I don't see it yet and the site is going through the same
error scenario that is happening to the Ketarin site.  I hope they are
just working on the site tonight or something.

Well, surgically removing the references (from within the Ketarin project) to 
the external project (CDBurnerXP) might be the only option afterall.  Let's 
get to it.



## Fixing the First Build Error ##
As far as I am aware Ketarin does not depend on CDBurnerXP, so the first 
modification made to the source tree is to remove the reference to that 
external application.  Right-click on the "CDBurnerXP" folder in VS2008's
"Solution Explorer" Window, a context menu will be activated, and then select 
"Exclude from Project".  

The folder melts away with no complaint and now we rebuild the solution; I
prefer to "clean" followed by a "build" to ensure the full run through,
especially since we are still smoke testing and trying to figure out
which way is up.

More output is spewed by VS2008 this time around than the first attempt, but
the ultimate outcome is still a failure.  Scroll to the first line of output
in the VS2008 "Output" window.

The first line mentions the .NET runtime v3.5; I'll have to verify, but I think
I am running v4.0 and I do not know the backwards compatibility issues with
the .NET runtime.  I'll have to look into that.  First, a little closer
look at the error message.



## Fixing the Second Build Error ##
Just beyond the first error message, where the .NET runtime v3.5 is mentioned,
there are 4 references to the "CDBurnerXP" module in source files.  Let's
start with that before taking the .NET runtime clue seriously.



Find all "CDBurnerXP;", Subfolders, Find Results 1, "Entire Solution"
* [...]\Ketarin\Ketarin-src\ApplicationJob.cs(13):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\DbManager.cs(8):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Forms\ApplicationJobDialog.cs(12):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Forms\ChooseAppsToInstallDialog.cs(13):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Forms\ImportFromDatabaseDialog.cs(9):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Forms\InstallingApplicationsDialog.cs(11):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Forms\SettingsDialog.cs(12):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Hotkey.cs(5):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\MainForm.cs(30):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Program.cs(23):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\Updater.cs(11):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\UrlVariable.cs(10):using CDBurnerXP;
* [...]\Ketarin\Ketarin-src\WebClient.cs(6):using CDBurnerXP;

Matching lines: 13    Matching files: 13    Total files searched: 423

13 files reference the module through the "using" directive.  Let's start by
commenting all those references out.

Then clean and build again.
