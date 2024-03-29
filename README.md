# KeePass to Bitwarden Converter

This program converts a KeePass 2.x unencrypted XML export to the Bitwarden JSON format.

Note: Export the unencrypted XML from KeePass, not KeePassXC.

## Features

* Keeps extra fields/attributes, unlike the Bitwarden-integrated kdbx import.
* Doesn't put everything under a folder, unlike the Bitwarden-integrated kdbx import.
* Automatically moves everything from the "KeePassHttp Passwords" and "KeePassXC-Browser Passwords" folder into the root.
* Filters out the KeePassHttp/KeePassXC-Browser Settings entries and attributes.

## Not-Features

* Doesn't fill out the TOTP field in Bitwarden, because I don't use it in KeePassXC. There's also the small issue that the TOTP implementation in Bitwarden has fixed options that KeePassXC lets you configure.

## Running

Compile it with .NET Core 2.1, then pass it a path to the XML export as the first parameter, and a path for the output JSON file.

Doing anything wrong will crash the application. There is minimal error handling, and no help.

## License

Released under the MIT License.