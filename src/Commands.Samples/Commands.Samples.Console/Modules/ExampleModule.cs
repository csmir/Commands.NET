using Commands.Core;

namespace Commands.Samples
{
    public class ExampleModule : ModuleBase
    {
        [Command("helloworld")]
        public void HelloWorld()
        {
            Console.WriteLine("Hello world!");
        }

        [Command("reply")]
        public void Reply([Remainder] string message)
        {
            Console.WriteLine(message);
        }

        [Command("type-info", "typeinfo", "type")]
        public void TypeInfo(Type type)
        {
            Console.WriteLine($"Information about: {type.Name}");

            Console.WriteLine($"Fullname: {type.FullName}");
            Console.WriteLine($"Assembly: {type.Assembly.FullName}");
        }
    }
}