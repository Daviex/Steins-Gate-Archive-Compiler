# Steins;Gate Archive Compiler
___________________________________
|      NPA Archives Compiler      |
|---------------------------------|
|   Italian Steins;Gate VN Team   |
|---------------------------------|
|     New Tool Made by Daviex     |
|---------------------------------|
|   Original Tool Made by Nagato  |
|---------------------------------|
|        Version 1.2 Alpha        |
|---------------------------------|
|            CodeName:            |
|---------------------------------|
|         El Psy Congroo          |
|_________________________________|

This tool was re-created from the original one made by Nagato to works with Just USA edition of the game.

This tool was made for the Italian Steins;Gate Project!

If you use it, just mention where you took it from!

# How it works?
After I wrote the decompiler, it was easy to found out invert the process to make
files to be crypted together again. I found initially problems on how I had
to crypt files when I was going to insert them back into the NPA file. After
sometime on it, I made it. It took the folder with files, and scan every single files.
We start by inverting the key. We took all the informations we could from files.
When we have the name file, I multiply his length by 2 because
JUST USA edition using UNICODE on texts. After we have namefile,
offset, filesize and unknow byte that is 0. When we filled our array, we start
creation of NPA file. First of all, we write the header. Then we start
write all the informations we have. One of those is fileCount.
In the for, we write every single part of the file in the same order
it was when I decrypted it. When it will end, will crypt everything and write it
into the final file.

# How to make it works?
Move your folder over the executable, it will compile the NPA file.

Enjoy

# Link to Project Forum
http://steinsgatevn.forumcommunity.net/

# Copyright

Copyright (c) 2015 Davide Iuffrida

License: Academic Free License ("AFL") v. 3.0

AFL License page: http://opensource.org/licenses/AFL-3.0
