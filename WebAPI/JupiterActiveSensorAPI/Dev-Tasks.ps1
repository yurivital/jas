Param(
[switch]$cleanup
)


if($cleanup){
 
 $ExtToClean = ("*.html~","*.py~","*.yaml~", "*.pyc", "*.js~")

 foreach ( $filter in $ExtToClean){
   Get-ChildItem -Filter $filter -Recurse | % { 
   Remove-Item $_.FullName
   }
 }
 

}