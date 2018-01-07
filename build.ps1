$pwd = (pwd).Path;
$artifacts = $pwd + "/artifacts";

Remove-Item $artifacts -Force -Recurse

dotnet build ./src/Interpreter -o $artifacts/interpreter -r win10-x64
