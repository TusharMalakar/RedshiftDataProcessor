git clone -v -b $2 https://github.com/TusharMalakar/RedshiftDataProcessor.git
dotnet restore RedshiftDataProcessor/RedshiftDataProcessor.csproj
echo "Starting build of RedshiftDataProcessor"
dotnet publish RedshiftDataProcessor/RedshiftDataProcessor.csproj -c Release -o .