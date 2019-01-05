# Display inputs.
Write-Host "DocPath:$env:DocPath"
Write-Host "GitHubUsername:$env:GitHubUsername"
Write-Host "RepositoryName:$env:RepositoryName"
Write-Host "CommitMessage:$env:CommitMessage" 
Write-Host "GitHubEmailSecret:$env:GitHubEmailSecret"
Write-Host "GitHubAccessTokenSecret:$env:GitHubAccessTokenSecret"
Write-Host "DefaultWorkingDirectory:$env:System_DefaultWorkingDirectory" 


    
Write-Host "Cloning existing GitHub Pages branch"

git clone https://${githubusername}:$githubaccesstoken@github.com/$githubusername/$repositoryname.git --branch=gh-pages $env:System_DefaultWorkingDirectory\ghpages --quiet
    
if ($lastexitcode -gt 0)
{
    Write-Host "##vso[task.logissue type=error;]Unable to clone repository - check username, access token and repository name. Error code $lastexitcode"
    [Environment]::Exit(1)
}
    
$to = "$env:System_DefaultWorkingDirectory\ghpages"

Write-Host "Copying new documentation into branch"

Copy-Item env:DocPath $to -recurse -Force

Write-Host "Committing the GitHub Pages Branch"

cd $env:System_DefaultWorkingDirectory\ghpages
git config core.autocrlf false
git config user.email $env:GitHubEmailSecret
git config user.name $env:GitHubUsername
git add *
git commit -m $env:CommitMessage

if ($lastexitcode -gt 0)
{
    Write-Host "##vso[task.logissue type=error;]Error committing - see earlier log, error code $lastexitcode"
    [Environment]::Exit(1)
}

git push

if ($lastexitcode -gt 0)
{
    Write-Host "##vso[task.logissue type=error;]Error pushing to gh-pages branch, probably an incorrect Personal Access Token, error code $lastexitcode"
    [Environment]::Exit(1)
}
