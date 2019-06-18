--导出到文件
exportfile "e:\\test" #noprompt using:FBXEXP
--选择所有Bone骨骼
select $Bone*
exportFile "<path>" #noPrompt selectedOnly:true using:FBXEXP

for i in $* do print i.name

for i in $* do selectmore i.name

FBXImporterGetParam "Import|IncludeGrp|Animation"
select $Bone*
exportFile "e:\\Njn11@Bone" #noPrompt selectedOnly:true using:FBXEXP

FBXExporterGetParam "Cameras" false
FBXExporterGetParam "Animation" true
FBXExporterGetParam "BakeAnimation" true

deselect $Bip*
