$pwd = (pwd).Path;
$artifacts = $pwd + "/artifacts";
$interpreter = $artifacts + "/interpreter/win/cslox.exe";

if (!(Test-Path $interpreter))
{
	Write-Host "First, run the build script."
}
else
{
	& $interpreter samples/basic.lox
}
