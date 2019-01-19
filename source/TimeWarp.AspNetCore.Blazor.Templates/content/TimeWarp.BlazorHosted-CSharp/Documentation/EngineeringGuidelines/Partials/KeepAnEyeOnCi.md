## Keep an eye on the CI
When you merge a commit into a master or release branch you 
should keep an eye on 
[Update Universe](http://aspnetci/viewType.html?buildTypeId=Coherence_UpdateUniverse) 
until your commit has passed it. 
Update Universe takes about 30-45 minutes to run when it passes, 
all you really need to do is ensure that it does.
If Update Universe DOESN'T pass, create a thread in the ASPNet-CI 
Teams channel (regardless of if it was your commits fault) 
and make sure that I see it.
