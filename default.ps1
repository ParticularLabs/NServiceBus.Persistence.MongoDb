# psake script for NServiceBus.MongoDB

Framework "4.0"
FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))

Properties {
    $baseDir = resolve-path .\.
    $sourceDir = "$baseDir\src"

    $packageDir = "$buildDir\package"
    
	$projectName = "NServiceBus.Persistence.MongoDb"
    $solutionName = "NServiceBus.Persistence.MongoDb"
    $configurations = @("Debug","Release","DebugContracts")
	$projectConfig = $configurations[0]
    

    # if not provided, default to 1.0.0.0
    if(!$version)
    {
        $version = "1.0.0.0"
    }
    # tools
    # change testExecutable as needed, defaults to mstest
    $testExecutable = "$sourceDir\packages\NUnit.Runners.2.6.3\tools\nunit-console-x86.exe"
    
    $unitTestProject = "NServiceBus.Persistence.MognoDb.Tests"
    
    $nugetExecutable = "$sourceDir\.nuget\nuget.exe"
	$nuspecFile = "$sourceDir\NServiceBus.Persistence.MongoDb\NServiceBus.Persistence.MongoDb.nuspec"
	$nugetOutDir = "packaging\"
}

# default task
task default -Depends Compile

task Build -depends Compile {}

task Compile {
    Write-Host "Building main solution ($projectConfig)" -ForegroundColor Green
    exec { msbuild /nologo /m /nr:false /v:m /p:Configuration=$projectConfig $sourceDir\$solutionName.sln }
}

task Test {
	Write-Host "Executing unit tests ($testExecutable)"

	$unitTestAssembly = "$sourceDir\$unitTestProject\bin\$projectConfig\$unitTestProject.dll"
	exec { & $testExecutable $unitTestAssembly /nologo /nodots /xml=$baseDir\tests_results.xml }
}

task BuildPackage -Depends Release{
	exec { & "$nugetExecutable" pack $nuspecFile -OutputDirectory $nugetOutDir }
}

task Release {
    Invoke-psake -nologo -properties @{"projectConfig"="Release"} Compile
}

task Clean {
    Write-Host "Cleaning main solution" -ForegroundColor Green
    foreach ($c in $configurations)
    {
        Write-Host "Cleaning ($c)"
        exec { msbuild /t:Clean /nologo /m /nr:false /v:m /p:Configuration=$c $sourceDir\$solutionName.sln }
    }
	
	Write-Host "Removing nuget packages"
	Remove-Item $sourceDir\packages\* -exclude repositories.config -recurse
	Remove-Item $baseDir\packaging\*.nupkg
    
    Write-Host "Deleting the test directories"
	if (Test-Path $testDir)
    {
		Remove-Item $testDir -recurse -force
	}
}
