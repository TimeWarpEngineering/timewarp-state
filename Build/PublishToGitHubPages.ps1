# Display inputs.
Write-Host "DocPath:$env:DocPath"
Write-Host "GitHubUsername:$env:GitHubUsername"
Write-Host "RepositoryName:$env:RepositoryName"
Write-Host "GitHubEmailSecret:$env:GitHubEmailSecret"
#Write-Host "GitHubAccessTokenSecret:$env:GitHubAccessTokenSecret"

#Write-Host "commitMessage:$env:commitMessage" 
#Write-Host "defaultWorkingDirectory:$env:defaultWorkingDirectory" 


#$docPath = Get-VstsInput -Name 'docPath' -Require
#$githubusername = Get-VstsInput -Name 'githubusername' -Require
#$githubemail = Get-VstsInput -Name 'githubemail'
#$githubaccesstoken = Get-VstsInput -Name 'githubaccesstoken' -Require
#$repositoryname = Get-VstsInput -Name 'repositoryname' -Require
#$commitMessage = Get-VstsInput -Name 'commitmessage' -Require

#$defaultWorkingDirectory = Get-VstsTaskVariable -Name 'System.DefaultWorkingDirectory'    
    
#Write-Host "Cloning existing GitHub Pages branch"

#git clone https://${githubusername}:$githubaccesstoken@github.com/$githubusername/$repositoryname.git --branch=gh-pages $defaultWorkingDirectory\ghpages --quiet
    
#if ($lastexitcode -gt 0)
#{
#    Write-Host "##vso[task.logissue type=error;]Unable to clone repository - check username, access token and repository name. Error code $lastexitcode"
#    [Environment]::Exit(1)
#}
    
#$to = "$defaultWorkingDirectory\ghpages"

#Write-Host "Copying new documentation into branch"

#Copy-Item $docPath $to -recurse -Force

#Write-Host "Committing the GitHub Pages Branch"

#cd $defaultWorkingDirectory\ghpages
#git config core.autocrlf false
#git config user.email $githubemail
#git config user.name $githubusername
#git add *
#git commit -m $commitMessage

#if ($lastexitcode -gt 0)
#{
#    Write-Host "##vso[task.logissue type=error;]Error committing - see earlier log, error code $lastexitcode"
#    [Environment]::Exit(1)
#}

#git push

#if ($lastexitcode -gt 0)
#{
#    Write-Host "##vso[task.logissue type=error;]Error pushing to gh-pages branch, probably an incorrect Personal Access Token, error code $lastexitcode"
#    [Environment]::Exit(1)
#}
