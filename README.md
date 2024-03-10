# BankingAssistant

## Install

Download the project as a zip file and extract it (or clone the repository) in a safe place.

No more installation is required.

## Usage

Then open a terminal at "BankingAssistant" and type the following commands:

```bash
cd program
```

```bash
dotnet run
```

### Optional 

You may create a shortcut to the program and place it on your desktop:

Create a shell file named "BankingAssistant.sh" with the following content:

```bash
#!/bin/bash
cd /absolute/path/to/BankingAssistant/program
dotnet run
```

Then make it executable:

```bash
chmod +x BankingAssistant.sh
```

Then make your terminal application the default application to open shell files.

Then you can double click on the file to run the program.

#### Notes

If you encounter an issue with the terminal not being able to find the dotnet command, you may need to add the dotnet directory to your PATH environment variable.

If your shell file cannont find a C# project to execute, make sure that the path you entered goes from the location of the shell to the "program" directory (Most of all: DON'T MOVE THE SHELL FILE, or you would have to update the path).

## Interraction 

The interraction is pretty easy, you just have to follow the commands given on the footer on the screen.

If you update the screen size, go to the home page of the program and select "Update screen size"(third option).

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is under the [MIT](https://choosealicense.com/licenses/mit/) license.