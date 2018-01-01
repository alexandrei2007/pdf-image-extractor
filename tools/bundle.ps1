# bundle
# download ILMerge at https://www.microsoft.com/en-us/download/details.aspx?id=17630

$msbuildPath = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
$basePath = "$((get-item $PSScriptRoot).parent.FullName)"

cmd.exe /c "`"$($msbuildPath)`" `"$($basePath)\PdfImageExtractor.sln`" /t:Build /p:VisualStudioVersion=14.0"

cmd.exe /c "`"E:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe`" /targetplatform:v4 /target:exe /out:`"$(get-item $PSScriptRoot)\PdfImageExtractor.exe`" `"$($basePath)\bin\Debug\PdfImageExtractor.exe`" `"$($basePath)\bin\Debug\iTextSharp.dll`" `"$($basePath)\bin\Debug\CommandLine.dll`""