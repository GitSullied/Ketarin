# Project #
Ketarin is a package manager for Windows operating systems... because,
apparently they haven't got their act together yet.  They must be too
busy couting thier money or maybe designing a propritary protocol to
replace an open standard that already exists; except they won't (they
couldn't possibly) do the peer-review, QA, and hardening that an open
standard naturally gets.  If you let them do that and then "sell" you
security products too... congratulations, you just got raped by M$.

I digress.  Ketarin is awesome.  You can find the (original project
here)[http://ketarin.org] and (their forum here)[http://ketarin.org/forum].
This is a fork of their software w/ no official connections to the
original developers at the moment, but I hope to faithfully implement
their intentions where I make modifications to the source.

Beyond moving away from sharing bundled archives as a means to distribute
the source code I had some snags that I think I could remove and have
the process of building from source trivial for the next Ketarin fan.


# Background #
Original source downloaded from Ketarin website (http://ketarin.org) via 
the author provided link in the second paragraph under the section entitled
"What is it all about?"
  (http://cdburnerxp.se/downloads/sourcecode/Ketarin.tar.gz)

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
section of the forum (http://ketarin.org/forum)

Let's check on that forum posting...




## Ketarin Server Responding with HTTP 500 ##
No Bueno.  I went to check up on my post from earlier today.  I can see it
cited as a recent topic, but after clicking on the link to view the topic the
Ketarin Forum crashes with a 500 HTTP response code.

Maybe they were updating the site, because things appear to be in better shape
now.  My forum post can be seen here:

https://ketarin.org/forum/topic/852-compiling-ketarin-from-source/page__pid__6282#entry6282

No responses yet; `tis no matter, I proceed on my own.



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
  [...]\Ketarin\Ketarin-src\ApplicationJob.cs(13):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\DbManager.cs(8):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Forms\ApplicationJobDialog.cs(12):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Forms\ChooseAppsToInstallDialog.cs(13):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Forms\ImportFromDatabaseDialog.cs(9):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Forms\InstallingApplicationsDialog.cs(11):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Forms\SettingsDialog.cs(12):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Hotkey.cs(5):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\MainForm.cs(30):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Program.cs(23):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\Updater.cs(11):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\UrlVariable.cs(10):using CDBurnerXP;
  [...]\Ketarin\Ketarin-src\WebClient.cs(6):using CDBurnerXP;
  Matching lines: 13    Matching files: 13    Total files searched: 423

13 files reference the module through the "using" directive.  Let's start by
commenting all those references out.

Then clean and build again.
