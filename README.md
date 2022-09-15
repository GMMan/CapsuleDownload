GMG Capsule Scraper
===================

This program allows you to scrape your game library from Green Man Gaming's
Capsule service (accessed throught the Playfire Client). It records your
Capsule games, purchase info (including associated keys), and download
info, and can generate a downloads list for use with aria2c. After downloading,
it can decrypt the downloaded files.

Tutorial
--------

### Prerequisites

This is a .NET Core 3.1 app, and as such, you need to have the runtime
installed. Download from [here](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
if you do not.

### Dumping account data

Run as follows:

```
CapsuleDownload.exe dump <dir>
```

where `<dir>` is the directory you want to save data to. You will use this same
directory for subsequent commands. The program will then prompt you for your
email address and password. Note there is nothing displayed when you enter your
password; this is normal.

The program will download the following:
- Games list
- For each game:
  - Game info
  - Game description
  - Boxart and banner, if available
  - Download info
  - Purchase info (including keys)

### Generating download links

External download managers are better suited to the task, so this program
will generate a list of URLs to download. In particular, it generates for
[aria2c](https://aria2.github.io/). Run this program as follows:

```
CapsuleDownload.exe generate-links <dir>
```

A `downloads.txt` will be created with a list of URLs and the correct
destination directory for use with aria2c. Call aria2c like this, making sure
that you are running this in the directory you have specified to save things
to:

```
aria2c -c -i downloads.txt
```

### Decrypting games

Capsule downloads are encrypted, and you need to decrypt the files before you
can use them. Run this program as such if you wish to decrypt all games:

```
CapsuleDownload.exe decrypt <dir>
```

This will decrypt the game files to `big_file_decrypted.zip` in the game's
respective folder. If the file already exists and you want to redecrypt the
file, you can add the `--force` option at the end of the command.

To decrypt a single game, run this program as such:

```
CapsuleDownload.exe decrypt <dir> <gameId>
```

where `<gameId>` is the folder name of the game you are trying to decrypt.
If the name has spaces, make sure you put quotes (`"`) around the whole name.

Once the game has been decrypted, you can unzip the file and either run it
(if it's unpacked) or install it (if it's an installer). If you need a serial
key to run a game, check `box.json` inside the game's folder. The key should
be inside the `keys` property.
