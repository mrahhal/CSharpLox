$pwd = (pwd).Path;
$artifacts = $pwd + "/artifacts";
$interpreter = $artifacts + "/interpreter";

function BuildInterpreter($dir, $runtime)
{
	dotnet build ./src/Interpreter -o $interpreter/$dir -r $runtime
}

Remove-Item $artifacts -Force -Recurse -ErrorAction Ignore

BuildInterpreter "win" "win10-x64"
BuildInterpreter "ubuntu" "ubuntu.16.04-x64"
BuildInterpreter "osx" "osx.10.11-x64"
