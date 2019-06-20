<<<<<<< HEAD
README.md

<<<<<<< HEAD
https://github.com/cssmagic/blog/issues/22
GitHub 第一坑：换行符自动转换

Git 换行符自动转换问题
https://blog.csdn.net/kongxx/article/details/45391393
=======
https://github.com/cssmagic/blog/issues/22   
GitHub 第一坑：换行符自动转换    
Git 换行符自动转换问题   
https://blog.csdn.net/kongxx/article/details/45391393  
>>>>>>> e66de568f6ceed5162f328297b7ca1ecd2f4151f

導致這個問題的原因是Git自作聰明的“換行符自動轉換”功能。要修復這個問題可以有幾個方式

在安裝“Git for windows”的時候，在“Configuing the line ending conversions”頁面，將默認選中改為“Checkout as-is, commit as-is”

如果已經安裝過了，也可以通過命令行修改，打開“Git Bash”，然後輸入
git config --global core.autocrlf false

也可以在“Git Bash”中修改~/.gitconfig文件，加入或修改下面的行
autocrlf = false

如果使用TortoiseGit，可以在setting的Git配置中取消選中“AutoCrlf”，然後選中“save as Global”


190527
換行句尾使用 < br > ... <br>
或 空格兩個以上 ENTER

The Building Coder  <br>
https://github.com/jeremytammik/tbc <br>

DynamoDS/Dynamo <br>
https://github.com/DynamoDS/Dynamo <br>

DynamoDS/DynamoRevit <br>
https://github.com/DynamoDS/DynamoRevit <br>

DynamoDS/DynamoPrimer <br>
https://github.com/DynamoDS/DynamoPrimer <br>

DynamoDS/RefineryToolkits <br>
https://github.com/DynamoDS/RefineryToolkits <br>

=======
README.md
190527
換行句尾使用 < br > ... <br>

The Building Coder  <br>
https://github.com/jeremytammik/tbc <br>

DynamoDS/Dynamo <br>
https://github.com/DynamoDS/Dynamo <br>

DynamoDS/DynamoRevit <br>
https://github.com/DynamoDS/DynamoRevit <br>

DynamoDS/DynamoPrimer <br>
https://github.com/DynamoDS/DynamoPrimer <br>

DynamoDS/RefineryToolkits <br>
https://github.com/DynamoDS/RefineryToolkits <br>



>>>>>>> 256485d85a849edbb80f7d7f8e727b5c05ede1e8
