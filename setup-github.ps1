# GitHub Setup Script for Loyalty Rewards API
# Run this after creating your GitHub repository

# GitHub username and repository name
$githubUsername = "ayushi2311"
$repoName = "-LoyaltyRewardsPlatform-API"

Write-Host "Setting up GitHub repository..." -ForegroundColor Yellow

# Add remote origin (replace with your actual repository URL)
git remote add origin "https://github.com/$githubUsername/$repoName.git"

# Push to GitHub
git branch -M main
git push -u origin main

Write-Host "Repository pushed to GitHub successfully!" -ForegroundColor Green
Write-Host "Repository URL: https://github.com/$githubUsername/$repoName" -ForegroundColor Cyan

# Open repository in browser
Start-Process "https://github.com/$githubUsername/$repoName"

Write-Host "GitHub repository setup complete!" -ForegroundColor Green
