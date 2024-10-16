namespace Commands.Samples
{
    // For very complex situations, where you want to separate your commands into different classes and use the class as an identifier,
    // you can create a class-level command by inheriting from the ModuleBase class, and giving the class a Name.
    // 
    // If there are no methods with a Name in the class, the class will not be registered as a command module, and instead as a command on its own.
    [Name("class")]
    public class ClassModule : ModuleBase
    {
        public string ClassCommand()
        {
            return "Hello from a class command!";
        }
    }
}
