namespace Commands.Samples
{
    // For very complex situations, where you want to separate your commands into different classes and use the class as an identifier,
    // you can create a class-level command by inheriting from the ModuleBase class, and giving the class a Name.
    // 
    // If there are no methods with a Name in the class, the class will not be registered as a command module, and instead as a command on its own.
    [Name("class")]
    public class ClassModule : ModuleBase
    {
        // Conveniently, commands can also return different types of values, such as strings, integers, or even custom objects.
        // By overriding the ToString method, you can specify the string representation of the custom object when it is sent to the consumer.
        public string ClassCommand()
        {
            return "Hello from a class command!";
        }
    }
}
