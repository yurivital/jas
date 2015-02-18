Param(
[switch]$cleanup
)


if($cleanup){
 
 $ExtToClean = ("*.html~","*.py~","*.yaml~", "*.pyc")

 foreach ( $filter in $ExtToClean){
   Get-ChildItem -Filter $filter | % { 
   Remove-Item $_.FullName
   }
 }
 

}